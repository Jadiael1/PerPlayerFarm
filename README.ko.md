# Per Player Farm (PPF)

> 🌍 다른 언어 README: [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

> **플레이어마다 하나씩, 평행한 농장의 세계.**
>
> **Stardew Valley 1.6.15** 및 **SMAPI 4.3.2** 호환.

---

## ✨ 개요

**Per Player Farm (PPF)** 는 동일한 세이브 안에서 *플레이어별 전용 농장* 을 생성합니다. 각 농부(farmhand)는 `Farm` 타입의 고유 로케이션 `PPF_<UniqueMultiplayerID>` 를 부여받으며, 초기 정리와 캐빈 파사드, 그리고 **모드 전용 텔레포터** 를 통한 **메인 농장 ↔ PPF** 간의 빠른 이동 기능을 갖습니다.

모든 설계는 멀티플레이(호스트 권한 기반)를 염두에 두었으며, 스플릿 스크린과 재접속 상황도 지원합니다.

---

## 📦 요구 사항

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (소스에서 직접 빌드 시 필요)
* (Dev) 자동 배포/ZIP 생성을 위한 **Pathoschild.Stardew.ModBuildConfig** (NuGet)

---

## 🧭 작동 방식 (하이레벨)

* **PPF 생성**

  * 호스트: 세계를 불러오거나 새로 만들 때, 모드는 알려진 모든 `PPF_*` 를 생성/보장하고, 새 플레이어가 들어오면 추가로 등록합니다. `PPF_*` 는 **실제 `Farm`** ( `GameLocation` 아님 ) 이므로 “농장에서만 배치 가능”한 바닐라 아이템도 정상적으로 사용 가능합니다.
  * 클라이언트: 로컬에 `Farm` 타입의 *스텁* 을 생성하여, 호스트가 없어도 워프/인터페이스/이동이 동작하도록 합니다.

* **맵/에셋**

  * `Maps/PPF_*` 는 `Maps/Farm` 을 *클론* 한 후 보정한 버전입니다: `CanBuildHere = T`, `MailboxLocation` 제거, 불필요한 action/warp 제거.
  * `Data/Buildings` 에 **파사드** `PPF_CabinFacade` 를 추가하여 문 타일(`Action: PPF_Door`)과 바닐라 기능의 우체통을 제공합니다.
  * `Data/BigCraftables` 에 **전용 텔레포터** `(BC)DerexSV.PPF_Teleporter` 를 등록합니다. 미니 오벨리스크 스프라이트를 기본으로 사용하지만, 추후 자유롭게 변경할 수 있습니다.

* **이동**

  * **PPF 텔레포터** 와 상호작용하면 **이동 메뉴** 가 열리며, “호스트 농장”과 각 `PPF_*` 엔트리를 나열하고, 소유자의 온라인 여부도 함께 표시합니다.
  * 파사드 문(`PPF_Door`)은 소유자를 실제 집 내부(FarmHouse)로 이동시켜 바닐라 흐름을 유지합니다.

* **텔레포터: 스마트 배치**

  * 먼저 `config.json` 의 `Teleporter.PreferredTileX/PreferredTileY` 로 정의된 **우선 타일**을 시도합니다. 배치가 불가능하면 스파이럴 탐색과 맵 전체 스캔으로 `GameLocation.CanItemBePlacedHere` 규칙에 맞는 타일을 찾습니다.
  * 텔레포터를 제거 후 재배치해도 **기능성이 유지**되도록, 이벤트가 배치를 감시하며 오브젝트를 재태깅합니다.

* **정리 & 바닐라 건물 제거**

  * `PPF_*` 에서 *초기 가벼운 정리*를 수행해 잡초/돌/나뭇가지, 나무/잔디, resource clump 를 제거합니다.
  * `PPF_*` 에서 *지속적인 스트립* 을 실행해 **Farmhouse는 항상 삭제**, **Greenhouse는 파손 상태일 때만 삭제**, **Shipping Bin과 Pet Bowl은 유지**합니다. 파손된 온실을 삭제하면 `Greenhouse` 로 향하는 워프도 함께 제거합니다.

* **워프 동기화**

  * 이벤트 유틸이 **Cabin → PPF** 연결을 동기화하여, 캐빈 문이 소유자의 PPF 로 들어가고, 출구는 해당 파사드로 이어지도록 유지합니다.

* **데이터 저장**

  * 소유자/PPF 목록은 모드 세이브 데이터인 `ppf.locations` 에 저장됩니다. 재생성은 언제나 멱등합니다.

---

## 🧠 모티베이션 & 설계 결정

Stardew Valley 는 원래 솔로 플레이에 맞춰져 있고, 멀티 플레이도 기본 구조(공유 농장, 소수 캐빈, 선택적 분리 자금)가 독립 진행을 원하는 플레이어에게는 여유 공간이 부족합니다. 이 모드는 바닐라 흐름(캐빈, Robin, 세이브)을 유지하면서도 각 초대 플레이어에게 완전한 농장을 제공하는 것을 목표로 합니다.

### 초기 실험과 문제점

초기 프로토타입은 호스트가 관리하는 *템플릿* (`PPF_Template`) 사이클을 상정했습니다:

1. 새 세이브가 `PPF_Template` 하나로 시작.
2. 호스트가 Robin 에게 가서 해당 템플릿에 캐빈/집을 건설.
3. 초대 플레이어가 입장하면 `PPF_Template` 가 `PPF_<INVITER_UID>` 로 전환.
4. 호스트는 다음 초대를 위해 새로운 `PPF_Template` 를 부여받고, 이 과정을 반복.

하지만 다음과 같은 문제에 부딪혔습니다:

* **Robin 메뉴 통합**: 바닐라 carpenter UI 를 수정해 커스텀 맵을 표시하고, 메인 농장 외부에도 건설을 허용하려면 깊은 UI 훅, 타일 검증, 비용 처리 등이 필요하며 다른 모드와의 충돌 위험이 큽니다.
* **취약한 영속성**: 초대 플레이어의 집을 완전한 커스텀 location 으로 옮기면, 모드를 비활성화했을 때 집 참조가 사라져 플레이어가 스폰/침대를 잃을 수 있습니다.
* **기존 세이브 호환성**: 플레이어별 `GameLocation` ↔ `Farm` 동적 변환은 아이템 분실, 가축 무주택화, 퀘스트 손상 등의 리스크가 있습니다.
* **템플릿 관리**: 항상 여분의 `PPF_Template` 를 확보하고, 고유 이름을 부여하며, 잔여물을 정리하는 과정이 꾸준한 실수의 원인이 됩니다. 특히 참가/퇴장이 잦을수록 문제가 커집니다.

### 최종 아키텍처

이러한 리스크를 피하기 위해, 초대 플레이어의 캐빈은 메인 농장에 남겨두고 PPF 를 **평행 농장** 으로 구성했습니다:

* 각 플레이어는 `PPF_<UID>` location(실제 `Farm`) 을 갖지만, 바닐라 캐빈은 메인 농장에 유지됩니다. 모드 비활성화 시에도 집과 스폰이 보존됩니다.
* PPF 에 **파사드 (Log Cabin)** 를 추가하고, 문에 커스텀 action 을 등록해 실제 캐빈 내부로 이동합니다. 우체통 타일은 바닐라와 동일하게 동작하며 새 편지 애니메이션도 표시됩니다.
* 모든 농장에 **커스텀 오벨리스크** (`(BC)DerexSV.PPF_Teleporter`) 를 배치하여 메뉴 기반 이동을 제공하고, 사용 가능 여부/온라인 상태를 표시합니다.
* 농장 입구 워프(Bus Stop, Forest, Backwoods 등)를 각 플레이어의 `PPF_*` 로 동기화하면서, 메인 농장 접근도 유지합니다.

이 구조는 게임 본편과의 호환성을 지키면서 모드 제거 시 부작용을 최소화하고, 플레이어별 전용 공간을 제공합니다.

---

## 🕹️ 사용 방법 (플레이어)

1. **호스트** 가 세이브를 불러옵니다. 이미 알려진 PPF 들이 생성/보장됩니다.
2. **메인 농장** 또는 각 **PPF** 에서 텔레포터를 사용하면 이동 메뉴가 열립니다.
3. **파사드 문** 은 소유자를 실제 집 내부(FarmHouse)로 보내, 바닐라 흐름을 유지합니다.
4. **우편 아이콘** 은 PPF 우체통 상단에 떠서 새 우편을 알립니다(소유자에게만 표시).

> 팁: 우선 배치 타일이 막혀 있으면, 모드는 자동으로 인근의 유효한 타일에 텔레포터를 재배치합니다.

---

## ⌨️ SMAPI 콘솔 명령어

> **호스트만** 월드를 변경할 수 있습니다. 게임 실행 중 SMAPI 콘솔에서 명령을 입력하세요.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all** : 모든 `PPF_*` 와 **메인 농장** 에 텔레포터를 보장.
  * **here** : 현재 위치에만 보장.
  * **farm** : 메인 농장에만 보장.
  * **ppf** : 모든 `PPF_*` 에만 보장.
  * **<LocationName>** : 특정 location 에만 보장.

* `ppf.clean here|all|ppf`

  * 잡초/잔목/자원 덩어리 등을 제거하는 가벼운 청소를 수행.

* `ppf.strip here|all`

  * `PPF_*` 에서 **Farmhouse 는 항상**, **Greenhouse 는 파괴되어 있을 때만** 제거하며, Shipping Bin/Pet Bowl 은 유지합니다. 파괴된 온실을 제거하면 `Greenhouse` 워프도 제거됩니다.

---

## ⚙️ 설정 & 커스터마이즈

* **텔레포터 앵커**: `config.json` 의 `Teleporter.PreferredTileX` / `PreferredTileY` 를 조정하세요. 값이 없거나 잘못된 경우 74,15 로 되돌아가고, 맵 범위에 맞게 clamp 됩니다.
* **텔레포터 외형**: `AssetRequested` 의 `Data/BigCraftables` 블록에서 `Texture` / `SpriteIndex` 를 변경합니다.
* **청소**: 콘솔 명령 `ppf.clean` 으로 언제든 PPF 를 다시 정리할 수 있습니다.

---

## 🧩 호환성

* 대응 버전: **Stardew Valley 1.6.15** / **SMAPI 4.3.2**
* 호스트가 **권한자**: 클라이언트/스텁은 영구 변경을 만들지 않습니다.
* `Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables` 등 **동일 자산** 을 수정하는 다른 모드는 로딩 순서 조정이 필요할 수 있습니다. 본 모드는 우선순위 있는 `AssetRequested` 훅을 사용하며, 멱등성을 목표로 합니다.

---

## 🛠️ 개발

### 빠른 빌드

1. **.NET 6 SDK** 설치
2. 프로젝트에 **Pathoschild.Stardew.ModBuildConfig** 패키지가 포함되었는지 확인
3. `PerPlayerFarm.csproj` 에서 다음을 활성화:

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. 필요 시 사용자 디렉터리에 `stardewvalley.targets` 를 만들고 `GamePath` / `GameModsPath` 지정
5. 빌드 실행:

   ```bash
   dotnet build -c Release
   ```

   ModBuildConfig 가 `…/Stardew Valley/Mods/PerPlayerFarm` 에 복사하고 `.zip` 도 생성할 수 있습니다.

### 코드 구조

* **ModEntry**: 핸들러 등록 및 매니저 초기화
* **Events/**

  * `AssetRequested/*`: `Maps/*`, `Data/Buildings`, `Data/BigCraftables` 수정 및 커스텀 warp 표기
  * `ButtonPressed/*`: 텔레포터, 파사드 도어, 이동 메뉴 처리
  * `DayStarted/*`: 청소, 텔레포터 보장, 워프 조정 (매일 아침)
  * `LoadStageChanged/*`: 저장된 데이터/알려진 플레이어 기반으로 PPF 확보
  * `ModMessageReceived`: SMAPI 메시지로 PPF 레지스트리 동기화
  * `ObjectListChanged`: 수동 배치된 Mini-Obelisk 재태깅
  * `PeerConnected/*`: 신규 플레이어를 위한 PPF 자산 확보 및 하우스 워프 업데이트
  * `RenderedWorld` / `RenderingWorld`: 파사드 위 우편 아이콘 표시 관리
  * `ReturnedToTitle`: 메인 메뉴 복귀 시 클라이언트 캐시 정리
  * `SaveLoaded/*`: 초대받은 플레이어용 PPF 로드 및 호스트 PPF 의 바닐라 건물 제거
  * `Saving`: `ppf.locations` 에 UID 목록 저장
  * `UpdateTicked`: 플레이어가 자신만의 `PPF_*` 에 도착하도록 맵 워프 교체
* **Utils/**

  * `Constants`: 공통 키/ModData
  * `ListHelper`: warp 문자열 파싱/직렬화
  * `MailboxState`: Rendering/RenderedWorld 에서 쓰이는 임시 상태
  * `PpfConsoleCommands`: `ppf.ensure-teleports`, `ppf.clean`, `ppf.strip` 콘솔 명령
* **Types/**

  * `PpfFarmEntry`, `PpfRegistryMessage`, `PpfSaveData`, `WarpLocations`: 멀티플레이 & 영속화 데이터 모델
* **Contents/**

  * `Buildings/LogCabin.cs`: PPF 파사드(`PPF_CabinFacade`)
  * `Itens/PPFTeleporter.cs`: 전용 텔레포터 `(BC)DerexSV.PPF_Teleporter`
* **i18n/**

  * 각 언어의 로그/피드백 메시지를 담은 `*.json`
* **Configuration/**

  * `ModConfig`: SMAPI `config.json` 에서 불러오는 설정

### 주요 재사용 지점

아래 유틸리티는 여러 흐름에서 공통 사용됩니다. 수정 시 모든 호출부를 고려하세요:

* `TeleportItem.Initializer` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): 매일/콘솔 명령 모두에서 텔레포터 보장
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF` (`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`): 캐빈 기본 워프를 PPF 출구로 변경
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost` (`Events/LoadStageChanged`, `Events/PeerConnected`): 호스트 측에서 초대받은 플레이어의 PPF 인프라를 확보
* `Peerconnected.Locations.TrackOwner` (`Events/LoadStageChanged`, `Events/PeerConnected`): UID ↔ PPF 매핑을 세이브에 유지
* `SaveLoaded.Locations.LoadPpfFarmsForInvited` (`Events/SaveLoaded`, `Events/DayStarted`): 클라이언트용 “섀도우 PPF” 로드
* `StripAllBuildingsDefault.Strip` (`Events/SaveLoaded`, `Events/DayStarted`): PPF 의 바닐라 건물 지속 제거
* `PlayerDataInitializer.CleanLocation` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): 초기 청소 로직, `ppf.clean` 명령에서도 사용
* `ListHelper.ConvertStringForList` (`Events/AssetRequested/FarmEntries`, `Events/UpdateTicked`): 주입된 warp 문자열 역직렬화
* `PpfBuildingHelper.TryGetOwnerUid` / `GetMailboxTile` (`Events/ButtonPressed`, `Events/RenderedWorld`): 소유자 판별 및 인터랙션/오버레이 타일 계산

---

## 🧯 문제 해결

* **“텔레포터가 나타나지 않는다.”**

  * 현재 장소에서 `ppf.ensure-teleports here` 를 실행하고, SMAPI 콘솔의 `[PPF]` 로그를 확인하세요.
* **“클릭했더니 바닐라 텔레포터가 열린다.”**

  * **PPF 전용 텔레포터**(모드의 커스텀 빅크래프터블)를 사용 중인지 확인하세요. 바닐라 Mini-Obelisk 는 가로채지 않습니다.
* **“바닐라 Mini-Obelisk 를 PPF 에 두지 못한다.”**

  * PPF 로케이션이 `Farm` 타입인지 확인하세요. 본 모드는 이미 `PPF_*` 를 `Farm` 으로 생성합니다. 과거 버전(`GameLocation`)에서 넘어온 경우, 모드를 비활성화 후 재활성화하여 로케이션을 재생성하거나 수동 변환이 필요할 수 있습니다.

---

## 🤝 기여 가이드

기여(이슈, 제안, 패치, PR)는 언제나 **환영**입니다. 단, 다음 사항을 지켜 주세요:

- 큰 기능을 개발하기 전에, 먼저 **Issue** 를 열어 아이디어를 공유하고 피드백을 기다려 주세요.
- 이슈에서 범위와 작업량을 합의한 다음, 해당 이슈를 참조하는 PR 을 제출하면 재작업을 줄일 수 있습니다.
- 문서/버그 수정 등 작은 변경은 바로 PR 로 제출해도 되지만, 가능하다면 관련 이슈에 짧게 언급해 주세요.
- C# 코드는 .NET 6/nullable 활성화, `[PPF]` 로그 규칙, 멀티플레이 호스트 권한, 멱등 로직을 준수해 주세요.

> **중요 (라이선스):** 본 모드는 **오픈소스가 아닙니다.** 저작권자의 승인 없이 수정본이나 포크를 배포하는 행위는 금지됩니다. 모든 Pull Request/패치는 본 리포지토리에서 검토 후 수용됩니다.

---

## 📜 라이선스

**All rights reserved.**

* 모드를 **설치하고 사용하는 것**은 가능합니다.
* 이 리포지토리에서 Issue/PR 을 통해 **개선 사항을 제안**할 수 있습니다.
* **금지 사항**: 저작권자 명시적 허가 없이 포크 배포, 파생 모드 제작, 수정본 재배포.

---

## 🙌 크레딧

* **제작자:** DerexSV
* **프레임워크:** SMAPI, Pathoschild.Stardew.ModBuildConfig
* **커뮤니티:** 테스터 및 Stardew Valley 모딩 커뮤니티에 감사드립니다.

---

## 📫 지원

SMAPI 로그와 상황 설명(호스트/클라이언트, 맵, 재현 절차)을 포함해 *Issue* 를 열어 주세요. 가능하다면 다른 모드를 끈 최소 구성과 간단한 재현 케이스로 보고해 주시면 도움됩니다.
