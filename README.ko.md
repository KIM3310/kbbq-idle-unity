# KBBQ Idle WebGL (Unity 2022.3 LTS)

KBBQ Idle는 고기 굽기 루프 중심의 방치형/타이쿤 게임 프로젝트입니다. 현재 버전은 Unity WebGL 기준으로 Cloudflare Pages 배포에 맞춰 정리되어 있습니다.

업데이트 기준: **2026년 2월 18일**

## 이번 버전 핵심 변경점
- 4개 그릴 슬롯 기반의 조리 루프 강화
- 손님 큐를 도트 카드 + 말풍선 + 요청 고기 아이콘으로 개선
- 수동 서빙 시 손님 식사 리액션 애니메이션 추가
- 업그레이드 모달 UX + 단계별 그릴 비주얼 변화
- 지글 사운드 레이어 강화(루프 + crackle)
- `돈/서빙수/누적손님/대기열` 지표 가독성 개선
- WebGL 임베딩 기준 모바일/데스크톱 반응형 보정
- 광고/정책 심사 대응용 정적 페이지를 `docs/`에 구성

## 게임 플레이 루프
1. 생고기 구매
2. 그릴 슬롯에 올리기
3. 타이밍 맞춰 뒤집기
4. 익은 고기 수거
5. 대기 손님에게 서빙
6. 업그레이드로 처리량 확장

## 빠른 시작 (Unity)
1. **Unity 2022.3.62f3**로 프로젝트 열기
2. `Assets/Scenes/Main.unity` 실행
3. Play

선택 실행:
- `KBBQ/Run Auto Setup`
- `KBBQ/Validate Data (Portfolio)`

## WebGL 빌드
`docs/`로 빌드:

```bash
./tools/build_webgl_docs.sh
```

메인 진입점:
- `docs/index.html`

## Cloudflare Pages 배포 설정
아래 값으로 고정:
- Framework preset: `None`
- Root directory: `.`
- Build command: `(none)`
- Build output directory: `docs`

배포 전 점검:

```bash
./tools/release_ops.sh check
```

실운영 AdSense 값 반영:

```bash
./tools/release_ops.sh apply-adsense <ca-pub-xxxxxxxxxxxxxxxx> <slot-id>
```

## 품질 게이트
전체 로컬 검증:

```bash
tools/portfolio_quality_gate.sh
```

검증 항목:
- Unity 체크
- 백엔드 테스트
- 결정적 시뮬레이터 테스트

## 저장소 구성
- 게임/UI 코드: `Assets/Scripts/`
- WebGL 배포 산출물: `docs/`
- 선택 백엔드: `server/`
- 시뮬레이터 테스트: `sim/`
- 빌드/운영 스크립트: `tools/`

## 관련 문서
- 메인 README: `README.md`
- 영문 README: `README.en.md`
- Cloudflare 배포 문서: `CLOUDFLARE_PAGES.md`
- 기술 요약: `PROJECT_SUMMARY.ko.md`, `PROJECT_SUMMARY.en.md`

## 라이선스
MIT (`LICENSE`)
