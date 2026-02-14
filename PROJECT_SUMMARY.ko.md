# KBBQ Idle (Unity) — 프로젝트 요약

## 개요
- Unity 2022.3.62f3 LTS 프로젝트이며 단일 씬 `Assets/Scenes/Main.unity`를 사용합니다.
- K‑BBQ 레스토랑을 운영하는 방치형/타이쿤 게임 구조: 수익을 쌓고, 손님을 서빙하며, 업그레이드/메뉴/매장 티어를 확장하고, 프레스티지로 장기 성장합니다.

## 핵심 루프
- **경제**: `EconomySystem`이 메뉴 수익 + 업그레이드(Income/Menu/Staff/Service/Sizzle) + 매장 티어 + 부스트 + 팁 + 콤보 + 프레스티지 배수로 초당 수익을 계산합니다.
- **손님/큐**: `CustomerSystem`이 큐를 생성하고 만족도/인내심을 관리합니다. 자동 서빙과 수동 “서빙”을 모두 지원하며, 수동 서빙은 팁/콤보 보너스에 영향을 줍니다. Rush 서비스로 일시적으로 처리 속도를 높일 수 있습니다.
- **진행도**: 총 누적 수익 기준으로 레벨이 상승(`ProgressionSystem` + `EconomyTuning`)하며, 레벨에 따라 메뉴/매장 티어가 해금됩니다.
- **프레스티지**: 조건 충족 시 프레스티지 포인트를 받고 진행도를 초기화하여 추가 배수를 얻습니다.
- **세션**: 오프라인 수익(60% 비율, 최대 8시간), 데일리 로그인 보상, 데일리 미션(수익/부스트/업그레이드).
- **튜토리얼**: 부스트 → 업그레이드 → 서빙의 3단계 흐름.

## 구조/시스템
- **오케스트레이터**: `GameManager`가 시스템 초기화, 루프, 저장, UI 업데이트를 관리합니다.
- **세이브**: `SaveSystem`이 PlayerPrefs(`KBBQ_IDLE_SAVE`)에 `SaveData`를 저장합니다.
- **상태 머신**: Boot/Tutorial/MainLoop/Pause/OfflineCalc.
- **UI**: `UIController`가 미션/프레스티지/큐/업그레이드/디버그/퍼포먼스/튜토리얼/리더보드/상점 UI를 바인딩합니다.
- **분석**: `AnalyticsService`는 콘솔 로그 수준의 스텁입니다.

## 데이터/콘텐츠
- `Assets/Data`에 ScriptableObject 기반 데이터:
  - 메뉴/업그레이드/매장 티어/손님 타입
  - 설정: `GameDataCatalog`, `EconomyTuning`, `MonetizationConfig`, `ApiConfig`
- 예시 콘텐츠:
  - 메뉴: Pork Belly, Beef Brisket, Soju, Bingsu 등
  - 업그레이드: Grill Upgrade, Ventilation, 레시피 업그레이드 등
  - 매장 티어: Alley → Hongdae → Gangnam → Hanok → Global
- `GameDataCatalog`가 데이터 묶음을 연결하고, `DefaultDataFactory`가 에셋 누락 시 기본 데이터를 생성합니다.

## 수익화
- `MonetizationConfig`에 보상형 광고 부스트, 전면 광고 보상, IAP 패키지 정의.
- `MonetizationService`는 실제 SDK 연동 없이 보상/통화만 지급하는 스텁입니다.

## 네트워크
- `AuthClient`/`LeaderboardClient`/`FriendsClient`가 `UnityWebRequest` + HMAC 서명 헤더로 통신.
- `ApiConfig` 설정에 따라 네트워크 사용 여부가 결정되며 기본 URL은 예시 값입니다.
- `LeaderboardView`는 현재 모의 데이터로 표시됩니다.

## 에디터 유틸/아티팩트
- `Assets/Editor/KBBQAutoSetup.cs`에서 **KBBQ/Run Auto Setup** 메뉴로 폴더/데이터/프리팹/씬을 자동 생성할 수 있습니다.
  - 참고: 빌드/캐시 아티팩트(`Library/`, `Builds/`)는 레포에서 제외했습니다.

## 비고
- 문서는 `README.md`(및 `README.en.md` / `README.ko.md`)에 정리했습니다.
- 포트폴리오용으로 테스트/검증을 점진적으로 추가하는 중입니다.
