# Per Player Farm (PPF)

> 🌍 他言語版 README: [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

> **各プレイヤーに一つずつ、並行世界の農場を。**
>
> **Stardew Valley 1.6.15** および **SMAPI 4.3.2** に対応。

---

## ✨ 概要

**Per Player Farm (PPF)** は、同じセーブデータ内で *プレイヤーごとに専用の農場* を生成します。各農場主（farmhand）は `Farm` 型のロケーション `PPF_<UniqueMultiplayerID>` を受け取り、初期清掃済みのマップとキャビンの外観、そして **モッド専用テレポーター** による **メインファーム ↔ PPF** 間の高速移動フローを利用できます。

マルチプレイ（ホストが権限を持つ）を前提に設計され、スプリットスクリーンや再接続でも動作します。

---

## 📦 必須条件

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6**（ソースからビルドする場合）
* （開発者向け） **Pathoschild.Stardew.ModBuildConfig**（NuGet）――自動デプロイ/Zip 用

---

## 🧭 仕組み（概要）

* **PPF の生成**

  * ホスト: セーブを読み込む/作成すると、既知の `PPF_*` をすべて作成・保証し、新規プレイヤー参加時には追加登録します。`PPF_*` は **実際の `Farm`**（`GameLocation` ではない）なので、「農場にのみ設置可能」なバニラ要素も問題なく配置できます。
  * クライアント: ローカルに `Farm` の *スタブ* を生成し、ホスト不在でも warp/UI/移動が機能します。

* **マップ/アセット**

  * `Maps/PPF_*` は `Maps/Farm` を *クローン* したうえで調整しています：`CanBuildHere = T`、`MailboxLocation` を削除し、不要な Action/Warp を除去。
  * `Data/Buildings` に **ファサード** `PPF_CabinFacade` を追加。ドアタイル（`Action: PPF_Door`）とバニラ互換のポストを備えています。
  * `Data/BigCraftables` に **専用テレポーター** `(BC)DerexSV.PPF_Teleporter` を追加。Mini-Obelisk のスプライトを再利用（後から差し替え可能）。

* **移動**

  * **PPF テレポーター** に触れると **旅行メニュー** が開き、「ホストの農場」と `PPF_*` の一覧が表示され、各所有者のオンライン状態も確認できます。
  * ファサードのドア（`PPF_Door`）は所有者を本来の家（FarmHouse）内部へ送るため、バニラの流れを壊しません。

* **テレポーター：賢い配置**

  * まず **優先座標**（`config.json` の `Teleporter.PreferredTileX/PreferredTileY`）を試します。設置できない場合はスパイラル探索＋マップ走査で有効タイルを探します（`GameLocation.CanItemBePlacedHere` ルール）。
  * 設置物を撤去・再配置しても **機能が失われない** よう、イベントで監視し再タグ付けします。

* **清掃とバニラ建築の削除**

  * `PPF_*` にて *軽い初期清掃* を行い、雑草/石/枝・木/草・resource clump を除去。
  * `PPF_*` で *継続的なストリップ* を実施：**常に Farmhouse を削除**、**Greenhouse は未修復なら削除**、**Shipping Bin と Pet Bowl は残す**。壊れた温室を消した際は `Greenhouse` への warp も削除します。

* **warp の同期**

  * イベント側（旧 `PpfWarpHelper` 相当）が **Cabin → PPF** のリンクを同期し、キャビンドアが所有者の PPF へ入り、戻り warp がファサードを指すよう維持します。

* **永続化**

  * 所有者と PPF の対応表を `ppf.locations`（モッドのセーブデータ）に記録。再生成は冪等です。

---

## 🧠 モチベーションと設計判断

Stardew Valley は元々ソロ向けに作られ、マルチプレイでも標準構成（共有農場・少数のキャビン・オプションの別財布）は、独立した進行を重視するプレイヤーには狭すぎます。本モッドの目的は、バニラの流れ（キャビン、Robin、セーブ）を崩さず、各プレイヤーに完全な農場を提供することです。

### 初期試作と問題点

最初のプロトタイプでは、ホスト管理の *テンプレート*（`PPF_Template`）を用いたサイクルを想定していました：

1. セーブ開始時に `PPF_Template` を配置。
2. ホストが Robin の店でそのテンプレにキャビンを建てる。
3. 招待プレイヤーが参加すると、その `PPF_Template` を `PPF_<INVITER_UID>` に昇格。
4. ホストが次の招待者用に新しい `PPF_Template` を受け取る――これを繰り返す。

しかし、この流れには多くの課題がありました：

* **Robin との統合**：バニラのメニューを改造してカスタムマップ上へ建築させるのは、他モッドと競合しやすく、UI フック/タイル検証/コスト計算など大掛かりな改変が必要。
* **脆い永続化**：家を完全オリジナルのロケーションへ移すと、モッドを無効化した際に家の参照が失われ、プレイヤーがスポーン/ベッドを持たない状態になりうる。
* **既存セーブとの互換性**：プレイヤーごとに `GameLocation` ↔ `Farm` を動的変換するのは、アイテム喪失・動物の家なし・クエスト破損などのリスクを伴う。
* **テンプレート管理**：常に空き `PPF_Template` を確保し、ユニーク名を付け残骸を掃除するのは、参加/離脱が多いほどミスの温床になる。

### 最終アーキテクチャ

これらを避けるため、招待プレイヤーのキャビンはメインファームに残し、PPF を **並行農場** として扱います：

* 各プレイヤーは `PPF_<UID>` ロケーション（本物の `Farm`）を持ちますが、バニラのキャビンはメインファームに残ります。モッド無効化時もキャビンとスポーン地点が生き続けます。
* PPF には **ファサード（Log Cabin）** を追加。ドアにカスタム action を仕込み、実際のキャビン内部へ移動します。ポストタイルはバニラ同様（新着メールのアニメーションあり）。
* 全農場に **カスタムオベリスク** (`(BC)DerexSV.PPF_Teleporter`) を配置。メニュー形式で農場間移動ができ、利用可能性とオンライン状態を表示します。
* 農場入口の warp（Bus Stop、Forest、Backwoods など）を各プレイヤーの `PPF_*` へ同期しつつ、メインファームへの出入りも確保します。

これにより原作との互換性を保ちつつ、モッド無効化時の問題を避け、当初の目的であるプレイヤー専用スペースを実現しています。

---

## 🕹️ プレイ方法

1. **ホスト** が通常どおりセーブを読み込みます。既知の PPF が作成／保証されます。
2. **メインファーム** または任意の **PPF** でテレポーターを使うと、農場間移動メニューが開きます。
3. **ファサードのドア** は所有者を本来のキャビン内（FarmHouse）へワープさせます。
4. PPF の郵便受け上に **手紙アイコン** が浮かび、郵便物があることを示します（所有者のみ）。

> メモ：優先タイルが塞がっている場合、テレポーターは自動的に近くの有効タイルへ配置されます。

---

## ⌨️ SMAPI コンソールコマンド

> **ホストのみ** がワールドに変更を加えられます。ゲームを読み込んだ状態で SMAPI コンソールから実行してください。

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**：すべての `PPF_*` と **メインファーム** にテレポーターを保証。
  * **here**：現在のロケーションのみ。
  * **farm**：メインファームのみ。
  * **ppf**：すべての `PPF_*` のみ。
  * **<LocationName>**：指定ロケーションに対して実行。

* `ppf.clean here|all|ppf`

  * 軽度の清掃（残骸/草木/資源 クランプなどの除去）を実行。

* `ppf.strip here|all`

  * `PPF_*` にて **Farmhouse** を常に削除、**Greenhouse は壊れている場合のみ削除**。Shipping Bin/Pet Bowl は維持。壊れた温室を削除すると `Greenhouse` warp も消去。

---

## ⚙️ 設定＆カスタマイズ

* **テレポーターのアンカー**：`config.json` の `Teleporter.PreferredTileX` / `PreferredTileY` を調整。欠落/無効値は 74,15 にフォールバックし、マップ範囲に clamp されます。
* **テレポーターの見た目**：`AssetRequested` 内の `Data/BigCraftables` ブロックで `Texture` / `SpriteIndex` を変更。
* **再清掃**：`ppf.clean`（コンソール）を使って PPF を再度クリーンアップ。

---

## 🧩 互換性

* 想定環境：**Stardew Valley 1.6.15** / **SMAPI 4.3.2**。
* ホストが **権限** を持ちます：クライアント/スタブは永続的な変更を行いません。
* 同じアセット（`Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables`）を変更する他 Mod がある場合、読み込み順の調整が必要になることがあります。本モッドは `AssetRequested` に優先度を設定し、冪等な挙動を目指しています。

---

## 🛠️ 開発

### クイックビルド

1. **.NET 6 SDK** をインストール。
2. プロジェクトに **Pathoschild.Stardew.ModBuildConfig** パッケージが含まれているか確認。
3. `PerPlayerFarm.csproj` で以下を有効化：

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. 必要に応じてユーザーディレクトリに `stardewvalley.targets` を作成し、`GamePath` / `GameModsPath` を設定。
5. ビルド：

   ```bash
   dotnet build -c Release
   ```

   ModBuildConfig が `…/Stardew Valley/Mods/PerPlayerFarm` へコピーし、`.zip` も生成可能。

### コード構成

* **ModEntry**：ハンドラー登録とマネージャ初期化。
* **Events/**

  * `AssetRequested/*`：`Maps/*`、`Data/Buildings`、`Data/BigCraftables` を注入/編集し、カスタム warp をマーク。
  * `ButtonPressed/*`：テレポーター、ファサードドア、旅行メニューの処理。
  * `DayStarted/*`：清掃、テレポーター保証、warp 調整を日の開始時に実行。
  * `LoadStageChanged/*`：既知 PPF をセーブ読み込み時に保証（永続データや既存プレイヤーに基づく）。
  * `ModMessageReceived`：SMAPI メッセージで PPF 登録情報を同期。
  * `ObjectListChanged`：手動で置かれた Mini-Obelisk を再タグ付け。
  * `PeerConnected/*`：ホストが新規プレイヤー用の PPF 資産を整備し、家の warp を更新。
  * `RenderedWorld` / `RenderingWorld`：所有者のファサード上に郵便アイコンを描画・管理。
  * `ReturnedToTitle`：タイトルへ戻った際、クライアントキャッシュをクリア。
  * `SaveLoaded/*`：招待プレイヤー（クライアント）の PPF を読み込み、ホスト側 PPF からバニラ構造物を削除。
  * `Saving`：既知 UID を含む `ppf.locations` を保存。
  * `TouchAction`：カスタム touch action ワープを処理し、各プレイヤーが自分の `PPF_*` に到達できるようにする。
* **Utils/**

  * `Constants`：共有キー/ModData。
  * `ListHelper`：warp 文字列のパースとシリアライズ。
  * `MailboxState`：Rendering/RenderedWorld で使う一時状態。
  * `PpfConsoleCommands`：`ppf.ensure-teleports`、`ppf.clean`、`ppf.strip` コマンド。
* **Types/**

  * `PpfFarmEntry`, `PpfRegistryMessage`, `PpfSaveData`, `WarpLocations`：マルチプレイと永続化のためのデータモデル。
* **Contents/**

  * `Buildings/LogCabin.cs`：PPF ファサード (`PPF_CabinFacade`)。
  * `Itens/PPFTeleporter.cs`：専用テレポーター `(BC)DerexSV.PPF_Teleporter`.
* **i18n/**

  * 各種メッセージ/ログのローカライズ JSON。
* **Configuration/**

  * `ModConfig`: SMAPI の `config.json` から読み込む設定項目。

### 再利用される主な機能

複数のフローで利用されるユーティリティがあるため、変更時はすべての呼び出し元を確認してください：

* `TeleportItem.Initializer`（`Events/DayStarted`, `Utils/PpfConsoleCommands`）：日次処理とコンソールコマンドの双方でテレポーターを保証。
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF`（`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`）：キャビンの標準 warp を所有者の PPF 出口へ置き換え。
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost`（`Events/LoadStageChanged`, `Events/PeerConnected`）：ホスト側に招待者用 PPF 資産を用意。
* `Peerconnected.Locations.TrackOwner`（`Events/LoadStageChanged`, `Events/PeerConnected`）：UID ↔ PPF の紐付けをセーブに保持。
* `SaveLoaded.Locations.LoadPpfFarmsForInvited`（`Events/SaveLoaded`, `Events/DayStarted`）：クライアント側 “シャドウ PPF” のロード。
* `StripAllBuildingsDefault.Strip`（`Events/SaveLoaded`, `Events/DayStarted`）：PPF 上のバニラ建築を継続的に削除。
* `PlayerDataInitializer.CleanLocation`（`Events/DayStarted`, `Utils/PpfConsoleCommands`）：初期清掃ロジック、`ppf.clean` からも再利用。
* `ListHelper.ConvertStringForList`（`Events/AssetRequested/FarmEntries`, `Events/TouchAction`）：注入した warp 文字列のデシリアライズ。
* `PpfBuildingHelper.TryGetOwnerUid` / `GetMailboxTile`（`Events/ButtonPressed`, `Events/RenderedWorld`）：所有者判定とインタラクション／オーバーレイ用タイル計算。

---

## 🧯 トラブルシューティング

* **「テレポーターが出現しない」**

  * 現在地で `ppf.ensure-teleports here` を実行し、SMAPI コンソールで `[PPF]` ログを確認してください。
* **「テレポーターを触ったらバニラ版が開いた」**

  * **PPF 専用テレポーター**（モッドの専用ビッグクラフタブル）を使用しているか確認してください。バニラの Mini-Obelisk は対象外です。
* **「バニラの Mini-Obelisk を PPF に置けない」**

  * PPF は `Farm` タイプである必要があります。本モッドは `PPF_*` を `Farm` として生成します。古いバージョン（`GameLocation`）から移行した場合は、モッドを一度無効化→再有効化してロケーションを再生成するか、手動で変換してください。

---

## 🤝 コントリビューション

コントリビューション（Issue、提案、パッチ、PR）を**歓迎**します！整合性を保つための指針は以下の通りです：

- 大きな機能を実装する前に、まず **Issue** を立ててアイデアを共有し、フィードバックを待ってください。
- Issue でスコープと作業量をすり合わせた後、該当 Issue を参照する PR を送ってください。未承認の作業によるリワークを避けるためです。
- ドキュメント修正やバグフィックスなど小さな変更であれば直接 PR を送って構いませんが、可能なら関連 Issue で一言知らせてください。
- C# コードは .NET 6 / nullable enabled を前提に、`[PPF]` ログの整合性と冪等性を重視し、マルチプレイではホストが権限を持つ前提を守ってください。

> **重要（ライセンス）:** 本モッドは **オープンソースではありません**。**許可なく改造版を公開したりフォークを配布することは禁止** されています。PR やパッチは本リポジトリでレビューを経た上で受け付けます。

---

## 📜 ライセンス

**All rights reserved.（著作権保護下）**

* Mod をインストールし、ゲームで利用することは**可能**です。
* 本リポジトリで Issue/PR を通じた **改善提案** は歓迎します。
* **禁止事項**：フォークの公開、派生 Mod の作成、改造版の再配布（作者の明示的な許可が必要）。

---

## 🙌 クレジット

* **作者:** DerexSV
* **フレームワーク:** SMAPI, Pathoschild.Stardew.ModBuildConfig
* **コミュニティ:** テスターの皆さま、そして Stardew Valley モッディングコミュニティへ感謝。

---

## 📫 サポート

SMAPI のログと再現手順（ホスト/クライアント、マップ、再現ステップ）を添えて *Issue* を作成してください。可能であれば他 Mod を無効化した最小構成での再現例をご提供いただけると助かります。
