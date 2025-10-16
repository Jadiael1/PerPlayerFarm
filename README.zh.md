# Per Player Farm (PPF)

> 🌍 其他语言版本： [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md)

> **并行农场的世界——每位玩家拥有自己的农场。**
>
> 兼容 **Stardew Valley 1.6.15** 与 **SMAPI 4.3.2**。

---

## ✨ 总览

**Per Player Farm (PPF)** 会在同一个存档中为*每位玩家*创建一座专属农场。每个农场助手（farmhand）都会获得一种 `Farm` 类型的自有位置，命名为 `PPF_<UniqueMultiplayerID>`，并带有初始清理、外观小屋以及在 **主农场** ↔ **PPF** 之间进行快速旅行的 **专属传送装置**。

一切均以多人游戏（主机拥有最终权限）为设计核心，同样支持分屏和断线重连。

---

## 📦 系统需求

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6**（用于从源码编译）
* （开发者）**Pathoschild.Stardew.ModBuildConfig**（NuGet），用于自动部署/打包

---

## 🧭 工作原理（高层）

* **PPF 创建**

  * 主机：在加载/创建存档时，模组会创建或确保所有已知的 `PPF_*`（并在有人加入时自动注册新 PPF）。`PPF_*` 是 **真正的 `Farm`**（而非 `GameLocation`），因此可以放置所有“仅限农场”类型的原版物品。
  * 客户端：会在本地生成 `Farm` 类型的 *stub*，以便在没有主机时也能使用 warp、界面和旅行功能。

* **地图 / 资源**

  * `Maps/PPF_*` 以 `Maps/Farm` 的 *克隆* 为基础，并进行调整：`CanBuildHere = T`，移除 `MailboxLocation`，去除不需要的 action/warp。
  * `Data/Buildings` 注入一座 **外观小屋** `PPF_CabinFacade`，包含门口 tile（`Action: PPF_Door`）和使用原版行为的邮箱。
  * `Data/BigCraftables` 注入 **专属传送器** `(BC)DerexSV.PPF_Teleporter`，默认复用迷你方尖碑的贴图（可自行替换）。

* **旅行**

  * 与 **PPF 传送器** 交互会打开一个 **旅行菜单**，列出“主机农场”以及每个 `PPF_*` 条目，并显示农场主是否在线。
  * 外观小屋的门（`PPF_Door`）会把农场主直接送至真实小屋（FarmHouse）内部——保持原版体验。

* **传送器：智能落点**

  * 首先尝试 **首选坐标**（在 `config.json` 的 `Teleporter.PreferredTileX/PreferredTileY` 中配置）。若该位置不可用，会通过螺旋搜索 + 地图扫描，在 `GameLocation.CanItemBePlacedHere` 规则下找到最近的有效 tile。
  * 传送器在被移除后再次放置时依旧 **保持可用**：事件系统会监控放置行为并自动 **重新标记** 对象。

* **清理与移除原版建筑**

  * `PPF_*` 上的 *初始轻量清理*：移除杂草/石头/树枝、树木/草地以及 resource clump。
  * `PPF_*` 上的 *持续 strip*：**始终移除** **Farmhouse**；**仅在温室破损（未解锁）时移除 Greenhouse**；**不会影响** **Shipping Bin** 与 **Pet Bowl**。当移除破损温室时，同步删除通往 `Greenhouse` 的 warp。

* **warp 同步**

  * 事件逻辑会将 **Cabin → PPF** 的连接保持一致（农场主 cabin 的门会进入自己的 PPF，离开时回到相应外观小屋）。

* **持久化**

  * 农场主与 PPF 的映射会存储在 `ppf.locations`（模组存档数据）中，重复创建具有幂等性。

---

## 🧠 动机与设计决策

Stardew Valley 最初是一款单人体验；即使在多人模式中，标准配置（共享农场、少量 cabin、可选的货币分离）对于想要独立发展的玩家来说也过于局促。本模组旨在在不破坏原版流程（cabin、Robin、存档）的前提下，为每位受邀玩家提供一座完整的独立农场。

### 最初的设想与阻碍

原型阶段曾设想使用主机管理的 *模板*（`PPF_Template`）循环：

1. 存档以一个 `PPF_Template` 开始。
2. 主机到 Robin 处，在该模板上建设 cabin/房屋。
3. 当受邀玩家加入时，该 `PPF_Template` 变为 `PPF_<INVITER_UID>`。
4. 主机再获得一个新的 `PPF_Template`，供未来邀请者使用，如此往复。

该流程遇到了多重难题：

* **与 Robin 的整合**：修改原版木工菜单以显示自定义地图、允许在主农场外建造，会与其他模组冲突，并需要深入的 UI hook、tile 校验和费用逻辑。
* **脆弱的持久化**：若将 cabin 完全迁移到自定义位置，一旦禁用模组，游戏就会失去对玩家住所的引用，导致没有有效的出生点/床位。
* **保存兼容性**：为每位玩家动态转换 `GameLocation` ↔ `Farm` 存在风险（物品丢失、动物无家可归、任务损坏等）。
* **多模板维护**：确保随时有空闲 `PPF_Template`、保证命名唯一、清理残留数据，在多玩家频繁进出时容易出错。

### 最终架构

为避免上述问题，玩家 cabin 保留在主农场，而 PPF 仅作为一座 **并行农场**：

* 每位玩家拥有 `PPF_<UID>` 位置（真实 `Farm`），但原版 cabin 仍留在主农场。即使禁用模组，住所与出生点仍然有效。
* PPF 中加入 **外观小屋（Log Cabin）**；门的自定义 action 会传送至真实 cabin 内部。邮箱与原版一致，会显示新邮件动画。
* 所有农场上都会生成 **专属 obelisk** (`(BC)DerexSV.PPF_Teleporter`)，通过菜单在各农场间跳转，并显示可用状态与玩家在线情况。
* 农场入口 warp（Bus Stop、Forest、Backwoods 等）会同步指向各自的 `PPF_*`，同时主农场依旧可达。

此架构既保持与原版的兼容性，又能避免模组禁用后的问题，同时实现“每位玩家一座农场”的目标。

---

## 🕹️ 玩家指南

1. **主机** 正常加载存档，模组会创建/确保所有已知的 PPF。
2. 在 **主农场** 或任意 **PPF** 中，与专属传送器互动即可打开旅行菜单。
3. **外观小屋的门** 会把农场主传送到自己真实小屋（FarmHouse）内部。
4. 当邮箱有新信件时，**漂浮的信件图标** 会出现在 PPF 邮箱上方（仅对其所有者可见）。

> 提示：如果首选位置被占用，模组会自动在附近的有效 tile 上放置传送器。

---

## ⌨️ SMAPI 控制台指令

> **只有主机** 能对世界进行变更。请在游戏运行时于 SMAPI 控制台输入命令。

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**：确保**所有** `PPF_*` 及 **主农场** 都有传送器。
  * **here**：仅确保当前所在位置。
  * **farm**：仅确保主农场。
  * **ppf**：仅确保所有 `PPF_*`。
  * **<LocationName>**：确保指定名称的某个位置。

* `ppf.clean here|all|ppf`

  * 对杂物/草木/资源块进行轻度清理。

* `ppf.strip here|all`

  * 在 `PPF_*` 内 **始终移除 Farmhouse**，并且 **仅在温室仍破损时** 移除 Greenhouse；不影响 Shipping Bin、Pet Bowl。移除破损温室时，会同步删除指向 `Greenhouse` 的 warp。

---

## ⚙️ 配置与自定义

* **传送器锚点**：在 `config.json` 中调整 `Teleporter.PreferredTileX` / `Teleporter.PreferredTileY`。若缺失或设置无效，会回退到 74,15，并自动限制在地图范围内。
* **传送器外观**：在 `AssetRequested` 中的 `Data/BigCraftables` 区块修改 `Texture`/`SpriteIndex` 即可。
* **清理**：使用控制台命令 `ppf.clean` 可在任意时刻重新清理 PPF。

---

## 🧩 兼容性

* 设计目标：**Stardew Valley 1.6.15** / **SMAPI 4.3.2**
* 主机拥有 **最终权限**：客户端/stub 不会写入持久修改。
* 若其他模组也修改 **相同资源**（如 `Maps/Farm`、`Maps/BusStop`、`Data/Buildings`、`Data/BigCraftables`），可能需要调整加载顺序。PPF 使用带优先级的 `AssetRequested` 并尽力保证幂等。

---

## 🛠️ 开发

### 快速构建

1. 安装 **.NET 6 SDK**。
2. 确认项目中引用了 **Pathoschild.Stardew.ModBuildConfig**。
3. 在 `PerPlayerFarm.csproj` 中启用：

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. （如有需要）在用户目录创建 `stardewvalley.targets` 并设置 `GamePath` / `GameModsPath`。
5. 构建：

   ```bash
   dotnet build -c Release
   ```

   ModBuildConfig 会复制至 `…/Stardew Valley/Mods/PerPlayerFarm`，并可生成 `.zip`。

### 代码结构

* **ModEntry**：注册事件处理器并初始化管理器。
* **Events/**

  * `AssetRequested/*`：注入/修改 `Maps/*`、`Data/Buildings`、`Data/BigCraftables`，并标记自定义 warp。
  * `ButtonPressed/*`：处理传送器交互、外观小屋门和旅行菜单。
  * `DayStarted/*`：日初执行清理、保证传送器、同步 warp。
  * `LoadStageChanged/*`：在加载存档时，根据持久数据/已知玩家确保 PPF 存在。
  * `ModMessageReceived`：通过 SMAPI 消息同步 PPF 注册表。
  * `ObjectListChanged`：重新标记手动放置的 Mini-Obelisk，使其保持传送功能。
  * `PeerConnected/*`：主机为新玩家准备 PPF 资源，并更新房屋 warp。
  * `RenderedWorld` / `RenderingWorld`：管理并绘制外观小屋上的邮件提示。
  * `ReturnedToTitle`：返回标题界面时清理客户端缓存。
  * `SaveLoaded/*`：加载客户端 PPF 并在主机端移除 PPF 内的原版建筑。
  * `Saving`：将 `ppf.locations` 与 UID 映射写入存档。
  * `TouchAction`：处理自定义 touch action warp，确保每位玩家进入自己的 `PPF_*`。
* **Utils/**

  * `Constants`：共享常量与 modData 键名。
  * `ListHelper`：warp 字符串的解析/序列化。
  * `MailboxState`：Rendering/RenderedWorld 使用的临时状态。
  * `PpfConsoleCommands`：提供 `ppf.ensure-teleports`、`ppf.clean`、`ppf.strip` 控制台命令。
* **Types/**

  * `PpfFarmEntry`、`PpfRegistryMessage`、`PpfSaveData`、`WarpLocations`：多人与持久化所需的数据模型。
* **Contents/**

  * `Buildings/LogCabin.cs`：PPF 外观 (`PPF_CabinFacade`)。
  * `Itens/PPFTeleporter.cs`：专属传送器 `(BC)DerexSV.PPF_Teleporter`。
* **i18n/**

  * `*.json` 中包含日志、提示等本地化文本。
* **Configuration/**

  * `ModConfig`：由 SMAPI 的 `config.json` 读取的参数。

### 关键复用点

部分工具在多处使用，修改时请确认所有调用点：

* `TeleportItem.Initializer`（`Events/DayStarted`, `Utils/PpfConsoleCommands`）：既用于每日循环，也用于命令式保障传送器存在。
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF`（`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`）：将 cabin 的默认 warp 改为 PPF 出口。
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost`（`Events/LoadStageChanged`, `Events/PeerConnected`）：确保主机持有受邀玩家的 PPF 资源。
* `Peerconnected.Locations.TrackOwner`（`Events/LoadStageChanged`, `Events/PeerConnected`）：维护 UID ↔ PPF 的持久映射。
* `SaveLoaded.Locations.LoadPpfFarmsForInvited`（`Events/SaveLoaded`, `Events/DayStarted`）：为客户端加载 “shadow PPF”。
* `StripAllBuildingsDefault.Strip`（`Events/SaveLoaded`, `Events/DayStarted`）：持续清除 PPF 内的原版建筑。
* `PlayerDataInitializer.CleanLocation`（`Events/DayStarted`, `Utils/PpfConsoleCommands`）：初始清理逻辑，也在 `ppf.clean` 中复用。
* `ListHelper.ConvertStringForList`（`Events/AssetRequested/FarmEntries`, `Events/TouchAction`）：反序列化注入的 warp 字符串。
* `PpfBuildingHelper.TryGetOwnerUid` / `GetMailboxTile`（`Events/ButtonPressed`, `Events/RenderedWorld`）：计算所有者与交互/覆盖图的目标 tile。

---

## 🧯 故障排查

* **“传送器没有出现。”**

  * 在当前地图执行 `ppf.ensure-teleports here`；查看 SMAPI 控制台内 `[PPF]` 日志。
* **“点击传送器却打开了原版的。”**

  * 确认使用的是 **PPF 专属传送器**（模组的 big craftable），而非原版迷你方尖碑。
* **“无法在 PPF 放置原版迷你方尖碑。”**

  * PPF 必须为 `Farm` 类型。本模组已经按 `Farm` 创建 `PPF_*`。如果是旧版本（`GameLocation`）升级而来，请移除后重新启用模组以重建，或手动转换。

---

## 🤝 贡献指南

我们**欢迎贡献**（issue、建议、补丁、PR）！为保持沟通高效，请遵循以下流程：

- 在实现大型功能前，先 **提交 issue** 说明想法，并等待维护者反馈。
- 在 issue 中确认范围/工作量，再提交对应的 PR，可避免未获批准的重复劳动。
- 对于文档或 bug 小修，可以直接提交 PR，但最好在相关 issue 中提及。
- C# 代码请保持 .NET 6 / nullable enabled 风格，日志使用 `[PPF]` 前缀，逻辑尽量幂等，并遵循“主机权威”模型。

> **重要（许可）**：本模组 **不是开源软件**。**未经作者授权，禁止发布 fork 或修改版**。Pull Request 和补丁仅会在本仓库中经过审核后被接受。

---

## 📜 许可协议

**版权所有，保留一切权利。**

* 你可以在自己的游戏中 **安装并使用** 该模组。
* 你可以通过本仓库的 issue/PR **提出改进建议**。
* **禁止** 未经作者明确许可发布 fork、制作衍生模组或分发修改版。

---

## 🙌 鸣谢

* **作者**：DerexSV
* **框架**：SMAPI，Pathoschild.Stardew.ModBuildConfig
* **社区**：感谢所有测试人员以及 Stardew Valley 模组社区。

---

## 📫 支持

请提交 *issue*，并附上 SMAPI 日志与场景描述（主机/客户端、地图、复现步骤）。建议提供最小化复现（尽量关闭其他模组），以方便定位问题。
