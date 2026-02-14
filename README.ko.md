# KBBQ Idle (Unity 2022)

## 개요
Unity `2022.3.62f3` LTS 기반의 K‑BBQ 방치형/타이쿤 게임 프로젝트입니다. 단일 씬 `Assets/Scenes/Main.unity`를 중심으로 수익 누적, 업그레이드, 장기 성장에 초점을 맞춥니다.

## 게임플레이 하이라이트
- 메뉴 수익, 업그레이드 배수, 매장 티어, 부스트/팁/콤보, 프레스티지를 결합한 경제 시스템.
- 손님 큐와 만족도/인내심 관리, 수동 “서빙”으로 팁/콤보 보너스.
- 총 수익 기반 레벨 상승으로 메뉴/매장 티어 해금.
- 프레스티지 리셋으로 장기 배수 획득.
- 오프라인 수익, 데일리 로그인 보상, 데일리 미션.
- 부스트 → 업그레이드 → 서빙의 짧은 튜토리얼 흐름.

## 프로젝트 구조
- `GameManager`가 시스템 초기화와 메인 루프를 관리.
- `GameStateMachine`이 Boot/Tutorial/MainLoop/Pause/OfflineCalc 상태를 처리.
- `SaveSystem`이 PlayerPrefs(`KBBQ_IDLE_SAVE`)에 저장.
- `UIController`가 미션/프레스티지/큐/업그레이드/리더보드/상점 UI 바인딩.
- ScriptableObject 데이터는 `Assets/Data`에 위치 (`GameDataCatalog`, `EconomyTuning`, `MonetizationConfig`, `ApiConfig`).

## 수익화 & 네트워크
- 보상형 광고/IAP 패키지는 설정되어 있으나 실제 SDK는 연동되지 않은 스텁 상태입니다.
- 네트워크 클라이언트는 존재하지만 기본 URL은 예시이며 리더보드는 모의 데이터로 표시됩니다.

## 빠른 시작
- Unity `2022.3.62f3`로 열기.
- `Assets/Scenes/Main.unity` 실행 후 Play.
- 선택 사항: **KBBQ/Run Auto Setup**으로 데이터/프리팹 재생성.

## 포트폴리오용 보강
- 선택 백엔드(`server/`): 게스트 인증 + HMAC 서명 기반 리더보드/친구 API + 경량 이벤트 수집(FastAPI + SQLite).
- 결정적 시뮬레이터(`sim/`): 경제/진행도 수식 .NET 유닛 테스트.
- WebGL 빌드(`docs/`): GitHub Pages 배포를 위한 WebGL 빌드(`KBBQ/Build WebGL (docs)`).
