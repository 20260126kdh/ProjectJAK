# 개발 로그

## 2026-08-18 — 적 상단 UI 가시성 배경 칸 추가

- 모든 적 Prefab이 공유하는 `EnemyStatusUI`에서 `TopBar` 배경 이미지를 자동 생성하도록 했다.
- 상태 아이콘, 작살, Intent와 방어도 뒤에 85% 불투명도의 중간 회색 배경을 적용했다.
- 기존 280×55 상단 UI 배치는 유지하고 배경만 300×65로 확장했다.
- 별도 루트인 체력바는 제외하고 배경 Raycast를 비활성화해 기존 Hover와 전투 입력을 유지했다.
- 검증: 정적 코드 및 diff 검사 필요, Unity 컴파일과 Play Mode 시각 확인 필요.

## 2026-08-18 — 메인 타이틀 배경 영상 적용

- 전달받은 4초 분량 `Main_Title.mp4`를 타이틀 전용 Resources 영상으로 추가했다.
- `TitleManager`가 영상을 카메라 Far Plane에 빈 여백 없이 반복 출력하도록 연결했다.
- 영상 오디오는 비활성화해 기존 `Main_Title.wav` BGM과 타이틀 UI 흐름을 유지했다.
- 원본 H.264 High 영상을 Constrained Baseline, 고정 24fps, BT.709 색 공간과 정상 타임스탬프로 재인코딩했다.
- 마지막 0.5초와 시작 0.5초를 교차 전환하고 영상 순서를 재배치해 3.5초 순환 영상으로 만들었다.
- 반복 경계의 프레임 변화량이 10.71에서 4.84로 약 55% 감소한 것을 확인했다.
- 검증: 1920×1080, 3.5초, 84프레임과 인코딩 메타데이터 및 반복 디코딩 확인 완료. Unity 재임포트와 Play Mode 시각 확인 필요.

## 2026-08-18 — 적이 부여하는 동일 디버프의 지속 턴 중첩

- 여러 적이 플레이어에게 같은 지속형 디버프를 부여할 때 수치는 유지하고 지속 턴만 합산하도록 전용 적용 경로를 추가했다.
- 일반 적 패턴과 심해의 속삭임, 불어터진 피부의 지속형 디버프에 같은 규칙을 적용했다.
- 중독, 영구 효과, 버프와 플레이어가 적에게 부여하는 상태 효과의 기존 중첩 규칙은 유지했다.
- 검증: 정적 코드 및 diff 검사 완료, Unity 컴파일과 Play Mode 확인 필요.

## 2026-08-18 — 빌드 5.3 방어도 피격 SFX 설명 수정

- 공격으로 방어도가 정확히 0이 되더라도 실제 HP 피해가 없으면 `blocked_hit`을 재생하는 것으로 설명을 명확히 했다.
- 기존 `hit ver.1`, `hit ver.2`, `harpoonstack_hit`은 방어도 소진 자체가 아니라 실제 HP 피해가 발생하기 시작한 공격부터 재생한다고 기록했다.
- 현재 코드의 실제 체력 피해 판정이 이 규칙과 일치해 코드와 Inspector 설정은 변경하지 않았다.

기능을 완료할 때 날짜, 변경 내용, 관련 경로, 검증 결과를 기록한다. 아래 초기 목록은 2026-08-06 저장소에서 코드와 자산의 존재를 확인해 작성했으며, 최초 구현일은 Git 커밋 메시지만으로 확정할 수 없어 별도로 표기하지 않았다.

## 2026-08-06 — 프로젝트 문서화 시작

- 루트 `AGENTS.md` 추가
- 프로젝트/코딩/게임 디자인/개발 로그 문서 추가
- Unity 버전과 현재 주요 시스템 구조 정리
- 검증: 파일 및 주요 코드 구조 확인

## 구현 확인된 기능 (최초 구현일 확인 필요)

- 카드 데이터, 카드 효과 실행, 강화, CSV Import
- 덱/손패/뽑기·버리기 흐름과 시작 덱 UI
- 적 생성, CSV 기반 적 패턴, 조건 검사와 Intent UI
- 플레이어/적 상태 효과와 상태 아이콘 UI
- 선원(Crew) 소환, 체력, 공격, 회복과 희생 효과
- 스테이지 일반 전투/휴식/보스 진행
- 전투 보상과 카드 강화 UI
- BGM, SFX, 오디오 설정
- 저장, 타이틀 이어하기, 전투 시작 상태 및 카드 순서 복원
- 전투/클래스 선택 단축키와 일시정지
- Spine Runtime과 애니메이션 컨트롤러
- VFX 자산 및 작업 구조

## 과거 이력 후보 (사용자 확인 필요)

다음 날짜는 기존 대화에서 제시된 예시이므로 확인 후 확정 이력으로 이동한다.

- 2026-07-08: Spine 적용
- 2026-07-15: Reward Panel
- 2026-07-28: Intent Icon
- 2026-08-05: VFX 작업 시작

## 2026-08-06 — Scripts Managers 역할별 분류

- `Managers`에 혼재한 16개 스크립트를 Core, Audio, Battle, Cards, Player, Scenes 영역으로 이동
- 관련 경로: `Assets/Scripts/Core`, `Audio`, `Battle/Managers`, `Cards/Managers`, `Player/Managers`, `Scenes/Managers`
- Inspector/데이터 변경: 없음, 기존 `.meta`와 GUID 유지
- 검증: 이동 전후 GUID·내용 해시 일치, 누락·고아 `.meta` 없음, Unity 6000.0.78f1 컴파일·도메인 재로드 성공, C# 및 Missing Script 오류 없음
- 남은 작업: Play Mode 기능 회귀 확인

## 2026-08-06 — 버림 도착 물보라 튜닝 및 위치 참조 확보

- `VFX_DiscardArrivalSplash` 루트 Scale을 `0.45`로 조정하고 사용자 화면 확인 완료
- `HandManager`에 버림 더미 도착 위치용 `RectTransform` 필드 추가
- Battle Scene의 기존 `Discard deck` RectTransform을 Inspector 참조로 연결
- 관련 경로: `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`, `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- 검증: 직렬화 참조 일치, Unity 컴파일·도메인 재로드·Battle Scene 재임포트 성공, Missing Reference 오류 없음
- 남은 작업: VFX 테스트 재생 메서드 구현

## 2026-08-07 — 보존 확정 버림 카드 이동 연출 연결

- 보존하지 않은 손패 카드가 순차적으로 물빛 변환 후 버림 덱으로 가속 이동하는 연출 구현
- 선택한 보존 카드 UI를 연출에서 제외하고 완료 후 기존 버림 데이터와 턴 전환 흐름에 연결
- 연출 중 중복 확정과 카드 선택을 차단하고 설정 누락 시 기존 턴 처리를 계속하는 예외 처리 추가
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`, `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`
- 검증: 단독 이동 연출과 실제 보존 확정 흐름에서 카드 이동·버림 처리·턴 전환이 정상 동작함을 사용자 Play Mode에서 확인
- 남은 작업: 보존 유무·손패 수별 경계 조건 회귀 테스트, 재셔플 연출 설계

## 기록 형식

## 2026-08-10 — 손패 버림 덱 이동 속도 조정

- 손패 카드가 버림 덱까지 이동하는 시간을 `0.7초`에서 `0.54초`로 단축하여 이동 속도를 약 30% 높임
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`
- Inspector/데이터 변경 없음
- 검증: 사용자 Unity Play Mode 확인 완료

## 2026-08-10 — 뽑을 더미에서 손패로 순차 드로우 연출

- 새로 뽑은 카드 UI를 `Decktodrawfrom`에서 최종 손패 위치로 한 장씩 이동하도록 연결
- 카드 데이터와 최종 손패 순서는 유지하고 오른쪽 손패 자리부터 왼쪽 방향으로 이동 연출
- 드로우 이동 시간을 `0.27초`로 분리하여 버림 카드 `0.54초`보다 2배 빠르게 재생하고 연출 중 카드 선택과 턴 종료를 차단
- 턴 시작 Jinx 표시는 드로우 연출 완료 후 적용되도록 연결
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- Inspector/데이터 변경: `HandManager.drawPileTarget`에 `Decktodrawfrom` RectTransform 연결
- 검증: 사용자 Unity Play Mode에서 드로우 속도·순서·동작 확인 완료

## 2026-08-10 — TEC_SKL_006 소멸 데이터 동기화

- `CardEffects.csv`의 `Exit` 설정에 맞춰 `TEC_SKL_006.asset` 두 번째 효과를 `Toxic`에서 `Exit`으로 동기화
- 관련 경로: `Assets/Data/CSV/CardEffects.csv`, `Assets/Data/ScriptableObjects/Cards/TEC_SKL_006.asset`
- Inspector/데이터 변경: `statusEffectType` enum 직렬화 값 `23` → `24`
- 검증: CSV와 ScriptableObject 일치, Unity 오류 로그 0건, 사용자 Play Mode 소멸 동작 확인 완료

## 2026-08-10 — 보존 모드 ESC 취소

- ESC 입력을 전담하는 `PauseManager`에서 일반 Pause보다 보존 모드 취소를 우선 처리
- 보존 선택 표시를 해제하고 턴 종료 버튼을 복원하되 보존 카드 데이터와 턴 진행은 유지
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scripts/Core/PauseManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- Inspector/데이터 변경: `PauseManager.handManager`에 BattleScene `HandManager` 연결
- 검증: 사용자 Unity Play Mode에서 보존 취소, 선택 해제, Pause 미진입과 재진입 확인 완료

## 2026-08-10 — 보존 모드 화면 70% 암전

- BattleCanvas 전체를 덮는 검정 70% 불투명도의 `PreserveDimOverlay` 추가
- `HandCardParent`를 별도 Canvas로 정렬하여 암전 위에 손패만 표시
- 암전 밝기 차이가 표시되지 않던 Canvas 정렬 문제를 막기 위해 Overlay Order `100`, 손패 Order `101`로 렌더링 순서 고정
- `PreserveDimOverlay` Image의 잘못된 스크립트 GUID로 Missing Script가 발생한 원인을 확인하고 Unity UI Image GUID로 수정
- 보존 확정 버튼에 Canvas·GraphicRaycaster를 추가하고 Order `101`로 설정하여 손패와 함께 암전에서 제외
- `EndTurnButton`을 보존 확정 버튼으로 잘못 판단한 설정을 복구하고, `ConfirmPreserveCard`가 연결된 실제 `PresrveConfirmButton`으로 Canvas·GraphicRaycaster 이동
- `HandManager`uc5d0 실제 보존 확정 버튼 참조를 추가하고 보존 모드 진입 중에만 표시하도록 상태 연결

## 2026-08-10 — 보존 선택 카드 중앙 확대

- 보존 카드를 드로우와 동일한 `0.27초`에 화면 중앙으로 이동하고 `1.4배`로 확대
- 중앙 표시 시 카드 회전을 제거하고 손패 최상단에 표시
- 재선택, 다른 카드 선택, ESC 취소, 보존 확정과 전투 초기화 경로에서 원래 위치·회전·크기·형제 순서 복원
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`
- Inspector/데이터 변경: `preserveFocusedCardScale` 기본값 `1.4`
- 검증: Unity 컴파일 및 Play Mode 확인 필요
- 버프·디버프 정리표를 기준으로 모든 현재 상태 효과 enum의 한글명과 툴팁 설명을 동기화
- 문서의 `ProfandHalo`는 실제 enum `ProfanedHalo`에 `장송의 가호`로 연결하고, enum에 없는 EOL/EOD는 제외
- 툴팁 폰트를 상태 아이콘의 `Pretendard-Regular SDF`와 동일하게 연결하고 TMP 구형 줄바꿈 API 경고 제거
- 검증: 사용자 Unity Play Mode에서 툴팁 표시와 폰트 정상 동작 및 Console 오류 없음 확인 완료

## 2026-08-10 — 버림 덱 셔플 VFX 1단계

- 버림 덱 위치의 소용돌이 이후 난류가 뽑을 덱 위치로 이동하는 수동 테스트 추가
- 기존 `VFX_VortexCore_Base`, `VFX_CardWaterStream`을 수정하지 않고 Scene 참조로 연결
- 실제 덱 데이터와 자동 셔플 로직은 변경하지 않음
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- 검증: Unity 컴파일 및 Play Mode VFX 크기·위치 확인 필요
- 물 폭탄과 단일 직선 난류 구성을 제거하고 작은 물살 3개의 순차 곡선 이동으로 교체
- 각 물살에 서로 다른 간격과 곡률을 적용해 카드 묶음이 물살에 섞여 이동하는 느낌으로 조정
- 마지막 도착 시 `VFX_DrawPileSplash`의 높은 물기둥을 제외한 낮은 물결과 입자만 재생
- 기존 범용 VFX 조합을 대체하는 전용 `ShuffleTransferVfx`와 `VFX_DeckShuffleTransfer.prefab` 추가
- 응축 원형 파동, 카드 비율 잔상 5개의 교차 셔플, 3중 S자 리본 이동, 도착 파동과 카드 재구성으로 시퀀스 구성
- LineRenderer의 외곽광·본체·하이라이트를 분리해 화면을 가리지 않는 청록색 심해 물살 표현
- 보존 진입, ESC 취소, 보존 확정과 전투 초기화 경로에 암전 표시 상태 연결
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- Inspector/데이터 변경: `HandManager.preserveDimOverlay` 연결, HandCardParent Canvas·GraphicRaycaster 추가
- 검증: 사용자 Unity Play Mode에서 시작 덱 표시, 보존 모드 70% 암전, 손패·보존 확정 버튼 제외와 버튼 상태 전환 확인 완료

```text
## YYYY-MM-DD — 기능명

- 변경 내용
- 관련 경로: Assets/...
- Inspector/데이터 변경: ...
- 검증: ...
- 남은 작업: ...
```

## 2026-08-10 — 상태 효과 아이콘 Hover 툴팁

- 공용 `EnemyStatusIconUI`에 마우스 진입·이탈 처리를 추가해 플레이어와 적 상태 아이콘 모두에 툴팁 표시
- 상태 효과 이름, 실제 로직 기준 설명, 현재 수치와 지속 정보를 표시
- 화면 경계 위치 보정과 Raycast 비활성화로 화면 이탈과 입력 방해를 방지
- 관련 경로: `Assets/Scripts/UI/EnemyStatusIconUI.cs`, `Assets/Prefabs/Enemy/EnemyStatusIcon.prefab`
- 검증: Unity 컴파일 및 Play Mode 확인 필요

## 2026-08-10 — 셔플 이동 VFX 물 표현 조정

- 전용 셔플 VFX 본체에 `M_WaterTrail` 재질을 연결해 물결 질감을 적용
- 이동 경로를 버림 덱 위로 먼저 상승한 후 뽑을 덱으로 휘어지는 2단계 곡선으로 변경
- 본체 주변에 서로 다른 진폭으로 흔들리는 보조 물줄기 2개를 추가
- 관련 경로: `Assets/Scripts/UI/VFX/ShuffleTransferVfx.cs`, `Assets/Prefabs/VFX/VFX_DeckShuffleTransfer.prefab`
- 검증: 정적 참조 및 Unity 컴파일 로그 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 클래스 상세 체력·패시브 표기 칸 확대

- 체력 표기 칸을 약 296×120에서 330×140으로 확대하고 내부 텍스트 영역을 300×115로 조정
- 패시브 표기 칸을 620×120에서 680×140으로 확대하고 내부 텍스트 영역을 650×130으로 조정
- 두 칸의 높이를 맞추고 서로 겹치지 않도록 패시브 칸의 가로 위치를 소폭 조정
- 기존 글자 크기, 줄바꿈 규칙과 클래스 설명 위치는 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: 씬 YAML 값 점검, Unity Play Mode 시각 확인 필요

## 2026-08-11 — 클래스 상세 정보 칸 비율 및 정렬 통일

- 설명 칸의 약 981 폭과 중심선을 기준으로 클래스명 칸의 좌우 경계를 정렬
- 체력 칸 폭을 330에서 240으로 축소하고 패시브 칸 폭을 약 731로 확대
- 체력과 패시브 칸 사이에 10 간격을 두고 전체 행이 설명 칸의 좌우 경계와 일치하도록 배치
- 클래스명, 체력과 패시브 텍스트의 좌우 내부 여백을 15로 통일
- 기존 글자 크기, 문구와 클래스 설명 위치는 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: 씬 YAML 값 및 Unity 오류 로그 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 체력·패시브 칸 가로 비율 추가 조정

- 체력 칸 폭을 240에서 190으로 추가 축소하고 내부 텍스트 폭을 160으로 조정
- 패시브 칸 폭을 약 731에서 약 781로 확대하고 내부 텍스트 폭을 751로 조정
- 두 칸 사이 간격 10과 전체 행 폭 약 981을 유지
- 클래스명 및 설명 칸과의 좌우 경계 정렬과 기존 글자 크기는 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: 씬 YAML 값 및 Unity 오류 로그 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 체력 줄바꿈 및 캡틴 패시브 넘침 수정

- 체력 텍스트 폭을 160에서 180으로 확대해 피지크 체력 표기의 한 줄 공간 확보
- 패시브 텍스트 폭을 751에서 771로 확대해 패널 내부 여백을 활용
- 캡틴 패시브의 `체력 12의 선원`을 `체력 12 선원`으로 축약
- 고정 글자 크기 50과 체력·패시브 패널 크기 및 전체 정렬은 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: 씬 YAML 값 및 Unity 오류 로그 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 체력 및 피지크 패시브 텍스트 정렬

- 공통 체력 TMP 텍스트를 가로·세로 중앙 정렬로 변경
- 클래스 상세 정보 갱신 시 패시브 텍스트의 왼쪽 정렬을 명시적으로 적용
- 피지크 패시브만 세로 중앙, 테크니션과 캡틴 패시브는 위쪽 정렬로 적용
- 클래스 전환마다 세로 정렬을 다시 지정해 이전 선택의 정렬 상태가 남지 않도록 처리
- 기존 폰트 크기, 패널 크기, 문구와 줄바꿈 설정은 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`, `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`
- 검증: 정적 코드 및 Unity 오류 로그 점검, Play Mode 클래스 전환 확인 필요

## 2026-08-11 — 클래스 설명 가독성 줄바꿈 조정

- 피지크 설명을 계약 전후와 마지막 강조 문장 기준으로 줄바꿈
- 테크니션 설명을 기술 연마와 작살 공격 묘사 기준으로 줄바꿈
- 캡틴 설명을 유물 획득, 선원 소환과 선원 행동 기준으로 줄바꿈
- 기존 설명 문구, 고정 글자 크기 50, 설명 패널 크기와 정렬은 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: 씬 YAML 값 및 Unity 오류 로그 점검, Play Mode 설명 표시 확인 필요

## 2026-08-11 — 인게임 기준 클래스 설명 줄바꿈 재조정

- 실제 인게임 캡처에서 캡틴의 `인식하여,`가 한 글자 단위로 다시 줄바꿈되는 문제 확인
- 피지크 설명을 인게임 폭 기준 6개 의미 단위로 재배치
- 테크니션 설명을 인게임 폭 기준 4개 의미 단위로 재배치
- 캡틴 설명을 유물 획득, 선원 소환과 행동 기준의 짧은 6개 줄로 재배치
- 기존 문구, 고정 글자 크기 50, 설명 패널 크기와 정렬은 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: 씬 YAML 및 Unity 오류 로그 점검, Play Mode 인게임 표시 재확인 필요

## 2026-08-11 — 클래스 설명 패널 및 텍스트 위치 재조정

- 인게임 캡처에서 마지막 설명 줄이 배경 칸 아래로 내려오는 상태 확인
- 설명 패널 Y 위치를 -140에서 -120으로 올려 패시브 칸과의 빈 간격 축소
- 설명 텍스트 내부 Y 위치를 0에서 15로 올려 마지막 줄의 패널 내부 공간 확보
- 클래스명, 체력, 패시브 칸과 버튼 위치 및 기존 문구·줄바꿈·글자 크기는 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: 씬 YAML 및 Unity 오류 로그 점검, Play Mode 인게임 위치 확인 필요

## 2026-08-11 — 클래스 설명 영역 하단 확장

- 인게임에서 위치 이동 후에도 6줄 설명이 패널 높이를 초과하는 상태 확인
- 설명 패널 높이를 약 357에서 417로 늘리고 중심 Y를 -120에서 -150으로 조정
- 설명 텍스트 영역 높이를 260에서 320으로 확대
- 패널과 텍스트의 위쪽 경계를 유지한 상태로 아래쪽만 60 확장
- 버튼과 다른 정보 칸의 위치, 기존 문구·줄바꿈·글자 크기 50은 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: 씬 YAML 및 Unity 오류 로그 점검, Play Mode 설명 하단 표시 확인 필요

## 2026-08-11 — 테크니션 로그라인 세로 중앙 정렬

- 클래스 상세 정보 갱신 시 로그라인 텍스트의 왼쪽 정렬을 명시적으로 적용
- 테크니션 로그라인만 세로 중앙 정렬로 표시
- 피지크와 캡틴은 기존 위쪽 정렬을 유지하고 클래스 전환마다 정렬 상태를 다시 지정
- 기존 문구, 줄바꿈, 글자 크기와 패널 위치·크기는 유지
- 관련 경로: `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`
- 검증: 정적 코드 및 Unity 오류 로그 점검, Play Mode 클래스 전환 확인 필요

## 2026-08-11 — 인게임 기본·클릭 커서 적용

- 기본 `Nomal.png`와 클릭 `Click.png`를 런타임 로드용 `Assets/Resources/Cursor` 경로로 이동
- 커서 이미지를 최대 64×64, Read/Write 활성화와 Cursor 타입으로 설정
- 게임 시작 시 자동 생성되어 씬 전환 후에도 유지되는 `GameCursorController` 추가
- 마우스 왼쪽 버튼을 누르는 동안 클릭 커서로 전환하고 버튼 해제 시 기본 커서로 복귀
- 원본 이미지의 투명 여백을 반영한 hotspot을 적용해 화살촉과 실제 클릭 지점을 정렬
- 관련 경로: `Assets/Resources/Cursor`, `Assets/Scripts/UI/GameCursorController.cs`
- 검증: 정적 코드 및 Unity 오류 로그 점검, Editor·Windows 빌드 커서 크기와 hotspot 확인 필요

## 2026-08-11 — 첫 일반 전투 손패 좌표 간헐 오류 수정

- 시작 덱 확인 시 비활성 전투 패널에서 드로우 애니메이션의 월드 목적 좌표를 저장하던 순서 확인
- 전투 패널 활성화와 시작 덱 패널 비활성화를 손패 생성보다 먼저 실행하도록 순서 변경
- `Canvas.ForceUpdateCanvases()`로 전투 UI 최종 좌표를 확정한 뒤 첫 손패 4장을 드로우
- 일반 드로우, 다음 전투와 이어하기 손패 복원 로직은 변경하지 않음
- 관련 경로: `Assets/Scripts/UI/StartingDeckUI.cs`
- 검증: 정적 호출 순서 및 Unity 오류 로그 점검, 새 게임 첫 일반 전투 반복 진입 확인 필요

## 2026-08-11 — 화면 전환 페이드 속도 조정

- 전체 화면 전환이 빠르게 느껴지는 문제를 완화하기 위해 페이드 시간을 약 20% 연장
- 최초 실행을 0.64초 페이드 아웃과 3프레임 페이드 인으로 조정
- 전투 진입을 0.32초 페이드 아웃과 0.24초 페이드 인으로 조정
- 타이틀 복귀를 0.15초 페이드 아웃과 1.56초 페이드 인으로 조정
- 관련 경로: `Assets/Scripts/UI/ScreenFadeController.cs`
- 검증: Unity 컴파일 로그 점검 및 Play Mode 시각 확인 필요

## 2026-08-11 — 클래스 선택 카드 확장 UI 1단계

- 참고 시뮬레이터를 기준으로 선택 카드 중앙 확대와 미선택 카드 좌우 축소·이동 동작 추가
- 기존 클래스 버튼과 상세 패널을 유지하고 0.16초 비스케일 보간 애니메이션 연결
- 클래스 프레임 PNG가 없는 동안 피지크·테크니션·캡틴에 빨강·초록·보라 임시 프레임 색상 적용
- 추후 Inspector의 클래스별 Sprite 필드에 PNG를 연결하면 `Image.Type.Sliced`로 자동 전환되도록 구성
- 숫자키 1·2·3 및 키패드 입력으로 클래스 선택 기능 추가, 기존 Enter·Esc 동작 유지
- 관련 경로: `Assets/Scripts/UI/ClassSelectionCardUI.cs`, `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`, `Assets/Scripts/Scenes/ClassSelectShortcutController.cs`, `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: Unity C# 컴파일 및 Missing Reference 로그 점검 완료, Play Mode 시각 확인 필요

## 2026-08-11 — 클래스 상세 정보 및 가로형 비율 조정

- 클래스 세부 기획서의 상세 화면 시안을 기준으로 선택 카드 크기를 1180×660, 상세 패널 배율을 0.75로 조정
- 측면 카드를 240×540으로 축소하고 중앙 기준 좌우 간격을 760으로 조정
- 피지크 표시 패시브의 전투 종료 회복량을 7로 변경
- 테크니션 표시 최대 체력을 75, 추가 작살잡이 스택 기준을 4로 변경
- 캡틴 표시 패시브를 선원 2명, 최대 체력 12, 공격 시 작살잡이 스택 2로 변경
- 실제 전투 패시브 실행기와 시작 덱 데이터는 변경하지 않음
- 관련 경로: `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`, `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: Unity 컴파일 로그 및 씬 참조 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 클래스 상세 패시브 설명 영역 넘침 수정

- 패시브 패널 높이를 약 120에서 190으로 확대
- 패시브 텍스트 높이를 약 100에서 170으로 확대
- TMP 자동 크기 조절을 활성화하고 글자 크기 범위를 26~42로 제한
- 기존 패시브 문구와 클래스 데이터, 로그라인 영역은 변경하지 않음
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: Unity 씬 참조 및 오류 로그 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 클래스 상세 패시브 칸 비율 재조정

- 세로로 늘어났던 패시브 패널과 텍스트 높이를 기존 비율로 복원
- 패시브 패널 폭을 약 620으로 확대하고 오른쪽으로 이동
- 패시브 텍스트 폭을 600으로 확대하고 중앙 정렬
- TMP 자동 글자 크기 범위를 24~34로 조정하여 긴 설명의 가독성 확보
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: Unity 씬 참조와 오류 로그 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 패시브·클래스 설명 글자 크기 방향 정정

- 요청 해석을 바로잡아 클래스 설명 글자를 기존 고정 크기 50으로 복원
- 패시브 글자도 클래스 설명과 동일한 고정 크기 50으로 변경
- 패시브 텍스트 높이만 100에서 150으로 확대하여 줄바꿈 공간 확보
- 클래스 설명 패널의 아래쪽 이동 위치와 나머지 UI 비율은 유지
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: Unity 씬 참조와 오류 로그 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 클래스 설명 칸 위치 및 글자 크기 조정

- 클래스 설명 패널 Y 위치를 -116에서 -140으로 내려 패시브 영역과 간격 확보
- 설명 텍스트 내부 Y 위치를 116에서 0으로 변경하여 패널 중앙 정렬
- 설명 텍스트 높이를 약 100에서 260으로 확대해 긴 설명의 줄바꿈 공간 확보
- 패시브와 동일한 TMP 자동 글자 크기 24~34 적용
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: Unity 씬 참조와 오류 로그 점검, Play Mode 시각 확인 필요

## 2026-08-11 — 클래스 상세 패시브 설명 축약

- 테크니션 설명을 `작살잡이 4스택 부여마다 1스택을 추가 부여합니다.`로 축약
- 캡틴 설명을 선원 체력·소환 수·공격 시 스택 효과가 두 문장에 들어가도록 축약
- 피지크 설명과 패시브 수치, 실제 전투 로직, UI 비율은 변경하지 않음
- 관련 경로: `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- 검증: Unity 씬 참조와 오류 로그 점검, Play Mode 시각 확인 필요
- 상승 배율을 6으로 높이고 상승 구간을 전체 이동의 40%로 확대
- 손패 상단을 통과한 뒤 뽑을 덱 가까이에서 하강하도록 곡선 제어점 수정
- 물줄기 주변에 물 재질을 공유하는 작은 물방울 5개 추가
- 이동 시간을 0.65초로 늦추고 상승 배율을 7.5로 높여 손패 위쪽 경로를 강조
- 경로 높이는 유지하면서 물줄기, 파동, 물방울과 카드 잔상 시각 크기를 1.35배 확대
- 밝은 청록과 흰색 하이라이트를 딥 블루·탁한 청록 계열로 낮추고 투명도를 조정
- 공유 `M_WaterTrail` 원본 재질은 변경하지 않고 전용 프리팹 색상만 수정

## 2026-08-10 — 카드 강화 망치 VFX 1단계

- 어두운 해양 분위기의 닻 문양 대장장이 망치 스프라이트 제작 및 투명 배경 처리
- 선택 카드의 화면 크기와 중심을 기준으로 망치가 예비 동작 후 카드 전체를 내려찍는 UI 연출 추가
- 타격 순간 카드 눌림·흔들림, 청록 충격 섬광과 금빛 불꽃 파편 표시
- `UpgradePanelUI` Context Menu 테스트 연결, 실제 강화 데이터 변경은 제외
- 관련 경로: `Assets/Art/VFX/CardUpgrade`, `Assets/Scripts/UI/VFX/CardUpgradeHammerVfx.cs`, `Assets/Prefabs/VFX/VFX_CardUpgradeHammer.prefab`
- 검증: Unity 컴파일 및 Play Mode 시각 확인 필요
- 망치 타격 각도를 180도로 전환해 손잡이가 위를 향하고 망치 머리가 카드에 수평으로 닿도록 수정
- 강화 패널 진입 전 `HandCardParent` 활성 상태를 저장하고 패널 표시 중 숨긴 뒤 종료 시 복구
- 망치 이미지를 넓은 타격면이 아래로 향하고 손잡이가 위로 선 정면 구도로 교체
- 0.32초 예비 동작, 0.13초 급가속 낙하, 0.1초 히트 스톱과 느린 복원으로 타격 무게 강화
- 참고 GIF의 대상 확대·예고광·대형 원형 폭발·잔불 정리 순서를 강화 망치 연출에 적용
- 선택 카드를 강화 Grid에서 임시 분리해 중앙에 1.45배 확대하고 종료 후 부모·순서·Transform 복구
- 주황·금색 방사 폭발과 청록 내부 고리가 결합된 `UpgradeImpactBurst.png` 추가
- 참고 GIF의 사각 망치 실루엣·짧은 손잡이·오른쪽 진입 구도를 기준으로 망치 이미지 전면 교체
- 검은 청동 기본형과 금빛 발광형을 동일 실루엣으로 제작해 접근·타격 중 교차 페이드
- 망치 머리의 왼쪽 타격면을 피벗으로 사용해 오른쪽에서 확대되며 휘둘러 들어오는 동작 적용

## 2026-08-10 — 최초 카드 강화 망치 VFX로 복구

- 사용자 요청에 따라 최초 생성한 긴 손잡이·닻 문양·3/4 구도의 검은 망치 이미지로 복원
- 중앙 카드 확대, 방사형 대형 폭발과 금빛 발광 망치 교차 연출 제거
- 0.2초 예비 동작, 0.16초 타격, 작은 청록 섬광과 금빛 파편 구성으로 복귀
- 강화 패널 표시 중 전투 손패를 숨기고 종료 시 복구하는 기능은 유지
- 최초 망치 이미지는 유지하면서 참고 GIF처럼 카드 금빛 강조 후 망치가 오른쪽에서 확대 진입하도록 동작 변경
- 동일 망치 복제본을 주황색으로 확대 배치해 접근 중 달아오르는 외곽광 표현
- 0.08초 히트 스톱, 16방향 방사광, 18개 잔불 파편으로 충돌 규모와 잔상 강화

## 2026-08-10 — 일반 전투 F10 테스트 단축키

- `BattleShortcutController`에서 Unity Editor 전용 F10 입력 처리 추가
- 진행 중인 일반 전투에서만 `BattleManager`의 기존 정상 승리 처리 경로를 호출
- 적 정리, 전투 후 회복, 클래스 패시브, 진행도 증가와 카드 보상 표시 유지
- 시작 덱 확인, Pause, 주요 UI 패널과 보스 전투에서는 실행 차단
- 관련 경로: `Assets/Scripts/Scenes/BattleShortcutController.cs`, `Assets/Scripts/Battle/Managers/BattleManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- 검증: Unity 컴파일 및 Play Mode 확인 필요

## 2026-08-10 — 카드 강화 황금 모루 VFX

- 기존 망치 스프라이트를 해양 문양이 새겨진 정면형 황금 모루 스프라이트로 교체
- 모루가 카드 정중앙 위에서 회전 없이 짧게 대기한 뒤 수직 급가속 낙하하도록 동작 변경
- 측면 진입, 주황 발광 복제본과 잔불을 제거하고 금빛 충격광·금속 파편 중심으로 정리
- 강화 패널의 손패 숨김과 실제 강화 콜백 연결 구조는 유지
- 관련 경로: `Assets/Art/VFX/CardUpgrade`, `Assets/Scripts/UI/VFX/CardUpgradeHammerVfx.cs`, `Assets/Prefabs/VFX/VFX_CardUpgradeHammer.prefab`
- 검증: 정적 참조와 Unity 컴파일 로그 점검, Play Mode 시각 확인 필요

## 2026-08-10 — 새 게임 스테이지 진행도 초기화 수정

- 저장 후 종료 시 `DontDestroyOnLoad` 상태로 남은 `StageManager` 진행도가 새 게임에 이어지는 원인 확인
- 새 게임에서 저장 파일과 이어하기 문맥을 제거한 뒤 런타임 스테이지 진행도도 최초 상태로 초기화
- 이어하기와 전투 포기의 기존 저장·초기화 흐름은 변경하지 않음
- 관련 경로: `Assets/Scripts/Scenes/Managers/TitleManager.cs`
- 검증: 정적 호출 경로 및 Unity 컴파일 로그 점검, 새 게임·이어하기 Play Mode 회귀 확인 필요

## 2026-08-12 — 게임 마우스 커서 이미지 교체

- 기본 커서를 금색 테두리와 가죽 손잡이가 있는 작살촉 이미지로 교체
- 좌클릭 중에는 작살촉 끝에 청록색 반짝임이 표시되는 별도 커서 적용
- 두 원본의 흰 배경을 투명 처리하고 256×256 커서 텍스처로 정리
- 일반·클릭 이미지의 여백 차이를 반영해 각각 작살촉 끝으로 Hotspot 재설정
- 일반·클릭 커서 본체를 5% 축소하고 변경된 작살촉 위치에 맞춰 Hotspot 재보정
- 축소된 커서에서 다시 5% 축소해 최초 크기의 90.25%로 조정하고 Hotspot 재보정
- 관련 경로: `Assets/Resources/Cursor`, `Assets/Scripts/UI/GameCursorController.cs`
- 검증: 이미지 알파 및 정적 Hotspot 점검, Unity Play Mode 시각 확인 필요

## 2026-08-12 — 전투 SFX 1단계 공격·피격·방어

- 플레이어·선원 공격에 `attack` 후 0.1초 지연된 피격음 조합 추가
- 완전 방어, 방어 관통, 작살 보유 여부와 실제 체력 피해 20 기준으로 피격음 분기
- 선원 공격 및 선원 피격은 약한 피격음으로 통일
- 적 패턴 공격의 플레이어 방어·선원 흡수·플레이어 체력 피해 결과에 피격음 연결
- 중독과 자해 피해는 공격 SFX 대상에서 제외
- 관련 경로: `Assets/Audio/SFX`, `Assets/Scripts/Audio/SFXManager.cs`, `Assets/Scripts/Cards/CardEffectExecutor.cs`, `Assets/Scripts/Player/PlayerCombat.cs`, `Assets/Scenes/PlayScene/Main_TitleScene.unity`
- 검증: Unity 컴파일 및 Play Mode 조합 확인 필요

## 2026-08-12 — 전투 SFX 2단계 카드 복합 효과 순서

- 카드 효과 실행 결과를 공격, 방어도, 버프, 디버프와 작살 카테고리로 수집
- 실제 게임 효과 순서는 변경하지 않고 SFX만 0.3/0.6/0.6/0.7초 간격으로 예약 재생
- 서로 다른 버프·디버프는 0.2초 간격으로 각 최대 2회 재생하고 동일 상태의 다중 대상은 한 번으로 통합
- 실제 방어도 또는 작살 증가가 없는 경우와 `Exit` 상태는 해당 효과음에서 제외
- 관련 경로: `Assets/Scripts/Audio/SFXManager.cs`, `Assets/Scripts/Cards/CardEffectExecutor.cs`, `Assets/Scenes/PlayScene/Main_TitleScene.unity`
- 검증: Unity 컴파일 및 Play Mode 복합 카드 청음 확인 필요

## 2026-08-12 — 전투 SFX 3단계 카드·UI·승리 효과음

- 드로우 이동을 시작하는 카드마다 `card_draw`를 한 번씩 재생하도록 연결
- 손패, 보상과 강화 카드의 Pointer Enter에 `card_hover` 연결
- 휴식 회복 성공 시 `heal`, 강화 패널 진입 시 `card_upgrade` 재생
- 마지막 적 처치 후 보상 패널 활성화와 동시에 `victory` 재생
- SFXManager에서 마우스·키보드 입력 시작 프레임마다 공통 `click` 재생
- 관련 경로: `Assets/Scripts/Audio/SFXManager.cs`, `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scripts/UI/CardUI.cs`, `Assets/Scripts/UI/RestPanelUI.cs`, `Assets/Scripts/UI/RewardPanelUI.cs`, `Assets/Scenes/PlayScene/Main_TitleScene.unity`
- 검증: Unity 컴파일 및 Play Mode UI 청음 확인 필요

## 2026-08-12 — 전투 SFX 4단계 적 행동 효과음

- 성공한 적 CSV 패턴 행동을 공격, 방어도, 버프와 디버프로 집계해 기존 SFX 간격 규칙 적용
- 서로 다른 적 버프·디버프를 0.2초 간격으로 각 최대 2회 재생
- 기본 공격 뒤 독오름·불어터진 피부 발동을 공격 이후 디버프음 순서에 연결
- 모독받은 후광과 심해의 속삭임의 실제 상태이상 부여에 디버프음 연결
- 관련 경로: `Assets/Scripts/Audio/SFXManager.cs`, `Assets/Scripts/Pattern/EnemyPatternController.cs`, `Assets/Scripts/Enemy/Enemy.cs`, `Assets/Scripts/Enemy/HRevelationController.cs`
- 검증: 정적 diff 점검 완료, Unity 컴파일 및 Play Mode 청음 확인 필요

## 2026-08-12 — 피지크 공격 카드 즉시 흡혈 수정

- `PHY_ATK_005`, `PHY_ATK_007`의 공격 후 흡혈 상태 부여로 다음 공격에서 회복되던 원인 수정
- 공격과 자기 흡혈이 함께 있는 카드는 각 대상의 실제 체력 피해만큼 즉시 회복하도록 처리
- 카드 한정 흡혈 효과는 지속 상태로 남기지 않고 `PHY_SKL_005`의 턴 유지형 흡혈은 보존
- 관련 경로: `Assets/Scripts/Cards/CardEffectExecutor.cs`
- 검증: Unity 컴파일 및 Play Mode 단일·전체 공격 회복량 확인 필요

## 2026-08-12 — 상태 효과 enum 직렬화 번호 복구

- `DevilPower` 중간 삽입으로 기존 약화가 악마의 힘으로 해석되던 원인 수정
- 기존 상태 효과 번호를 명시적으로 고정해 카드와 아이콘 ScriptableObject 호환성 복구
- 번호 고정에 맞춰 피지크·테크니션·캡틴 소멸 카드 3종의 `Exit` 값을 교정
- CSV와 카드 에셋의 전체 ApplyStatus 항목 대조 예정
- 관련 경로: `Assets/Scripts/Battle/StatusEffectType.cs`, `Assets/Data/ScriptableObjects/Cards`
- 검증: Unity 컴파일 및 Play Mode 영혼 파괴·소멸 카드 확인 필요

## 2026-08-12 — 클래스 기획서 기준 시작 스킬 에셋 수정

- 최신 클래스 세부 기획서를 기준으로 테크니션 `엄폐` 방어도를 15에서 12로 변경
- 캡틴 시작 스킬 `CAP_SKL_001` 이름은 사용자 확인에 따라 `길잡이`로 유지
- 카드 효과와 시작 덱 구성은 유지하고 CSV는 변경하지 않음
- 관련 경로: `Assets/Data/ScriptableObjects/Cards/TEC_SKL_002.asset`, `Assets/Data/ScriptableObjects/Cards/CAP_SKL_001.asset`
- 검증: 정적 에셋 값 점검 완료, Unity Play Mode 확인 필요

## 2026-08-12 — 악마의 힘 상태 아이콘 연결

- 상태 효과 enum 번호 복구에 맞춰 `DevilPower = 24` 아이콘 항목 추가
- 기존 `DevilPower.png` Sprite GUID와 fileID를 상태 효과 아이콘 데이터베이스에 연결
- 기존 약화·취약 및 다른 상태 효과 아이콘 항목은 유지
- 관련 경로: `Assets/Data/StatusEffectIconDatabase.asset`
- 검증: 번호·GUID 정적 점검 완료, Unity Play Mode 아이콘 표시 확인 필요

## 2026-08-12 — 악마와의 거래 강화 설명 표기 수정

- `PHY_SKL_002` 강화 시 체력 7, 힘 5로 표시되던 설명 숫자를 체력 7, 힘 7로 보정
- 사용자 확인에 따라 강화 후 실제 체력 손실도 5에서 7로 변경해 표시와 일치시킴
- 다른 카드 강화 규칙과 CSV·카드 ScriptableObject는 변경하지 않음
- 관련 경로: `Assets/Scripts/Cards/CardUpgradeUtility.cs`
- 검증: Unity 컴파일 및 강화 화면 표시 확인 필요

## 2026-08-11 — 게임 화면 전환 페이드

- 런타임 전역 검은색 오버레이와 코루틴 기반 `ScreenFadeController` 추가
- 최초 실행에 0.53초 페이드 아웃과 2프레임 페이드 인 적용
- 클래스 선택 후 전투 진입과 같은 씬의 다음 전투 진입에 0.27초 페이드 아웃과 0.20초 페이드 인 적용
- 클래스 선택 취소, 전투 포기와 저장 후 종료의 타이틀 복귀에 0.12초 페이드 아웃과 1.30초 페이드 인 적용
- 비스케일 시간을 사용하고 페이드 중 UI Raycast를 차단하여 Pause 상태와 중복 입력 처리
- 관련 경로: `Assets/Scripts/UI/ScreenFadeController.cs`, `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`, `Assets/Scripts/Battle/Managers/BattleManager.cs`, `Assets/Scripts/Core/PauseManager.cs`
- 검증: 정적 참조 및 Unity 컴파일 로그 점검, Play Mode 시각 확인 필요

# 2026-08-12 퍼즈 환경 설정 UI 정렬 수정

- `BattleScene`의 `SettingsPanel`에 독립 Canvas와 GraphicRaycaster를 추가했다.
- 환경 설정 Canvas의 Sorting Order를 `200`으로 지정해 보존 모드용 손패 Canvas(`101`)보다 위에 표시되도록 했다.
- 손패와 보존 모드의 기존 정렬값 및 게임 규칙은 변경하지 않았다.
- Unity Play Mode에서 퍼즈 → 환경 설정 표시 및 입력 회귀 확인이 필요하다.
# 2026-08-12 클래스별 첫 전투 튜토리얼 1단계

- 제공된 클래스별 초상화, 대화 프레임과 다음 버튼 이미지를 `Assets/Art/UI/Tutorial`에 추가했다.
- 클래스별 도입 대사와 화면 암전, 초상화, 대화창, 다음 버튼을 런타임에 구성하는 `TutorialManager`를 추가했다.
- 새 게임 시작 덱 확인 완료 후 튜토리얼을 시작하고 이어하기 경로에서는 재실행하지 않도록 연결했다.
- 특정 카드 강조와 행동 조건 연결은 다음 단계 범위로 남겼다.
- 제공 이미지를 Sprite로 임포트하고 BattleScene 참조와 Pretendard 폰트를 연결했다.
- Unity 컴파일·임포트 오류가 없음을 확인했으며 Play Mode 시각 확인이 필요하다.

# 2026-08-12 튜토리얼 첫 손패 고정

- 새 게임 튜토리얼 첫 손패를 투창, 회피, 클래스별 시작 스킬 2장으로 고정했다.
- 지정 카드 인스턴스를 시작 덱에서 한 장씩 분리하고 남은 6장만 섞어 드로우 파일을 구성했다.
- 기존 `HandManager.DrawCards(4)`를 유지해 드로우 이동 연출과 SFX가 그대로 재생된다.
- 이어하기와 다음 전투의 기존 드로우 파일 준비 로직은 변경하지 않았다.

# 2026-08-12 튜토리얼 카드 행동 2단계

- 투창, 회피, 클래스별 첫 번째 스킬 순서로 지정 카드 사용 조건을 연결했다.
- 지정 카드에 빨간 Outline을 1초 간격으로 점멸하고 다른 손패 선택을 차단했다.
- `BattleManager.FinishCardUse`에서 성공한 카드 사용을 튜토리얼에 알리도록 연결했다.
- 튜토리얼 진행 중 턴 종료, 보존, 덱 보기, F10과 퍼즈 입력을 차단했다.
- Unity Play Mode에서 세 클래스의 대상 선택과 단계 전환 확인이 필요하다.

# 2026-08-12 튜토리얼 보존과 마무리 3단계

- 첫 번째 클래스 스킬 사용 뒤 사용 횟수와 보존 안내 대사를 연결했다.
- 클래스별 두 번째 스킬만 보존 선택할 수 있고 확정 뒤 기존 버림 연출과 적 턴이 진행되도록 했다.
- 다음 플레이어 턴 시작 시 일반 초상화에서 변화 초상화로 전환되는 클래스별 마무리 대사를 추가했다.
- 보존 단계 외 턴 종료와 보존 입력 차단 규칙은 유지했다.
- Unity 컴파일과 세 클래스의 전체 튜토리얼 Play Mode 확인이 필요하다.

# 2026-08-12 튜토리얼 조작 안내 개선

- 대화 진행에 스페이스바 입력을 추가하고 같은 프레임 중복 진행을 차단했다.
- 지정 카드 선택 시 적 또는 플레이어 머리 위에 빨간 역삼각형 대상 표시를 생성한다.
- 첫 스킬 뒤 턴 종료/보존 버튼, 두 번째 스킬 카드, 보존 확정 버튼 순서로 강조하도록 변경했다.
- 기존 일반 전투의 카드 대상 선택과 보존 입력 규칙은 변경하지 않았다.
- Unity 컴파일과 세 클래스 대상 표시·보존 순서 Play Mode 확인이 필요하다.

# 2026-08-12 튜토리얼 대상 표시 위치 수정

- 대상의 첫 렌더러가 아닌 모든 활성 렌더러의 합산 경계를 기준으로 역삼각형 위치를 계산한다.
- 역삼각형을 합산 이미지 최상단보다 위쪽에 배치하도록 높이 여백을 조정했다.
- 적 대상 카드는 EnemySpawner 생성 순서의 첫 번째 생존 적에게만 표시하도록 변경했다.
- Unity Play Mode에서 각 클래스 캐릭터와 1번 적의 실제 표시 위치 확인이 필요하다.
- 클래스 마커의 가로 위치는 무기 렌더러에 영향받지 않도록 클래스 루트 중심으로 고정했다.
- 이미지 최상단과 마커 사이 높이 여백을 0.75에서 0.25로 줄였다.

# 2026-08-12 Button_UI 미적용 에셋 연결

- 휴식 회복·강화, 환경설정, 뽑을 덱과 버림 덱 버튼에 제공된 상태 이미지를 연결했다.
- 버튼 Transition을 Sprite Swap으로 지정하고 기존 클릭 이벤트는 유지했다.
- 타이틀과 전투 환경설정의 Master, BGM, SFX 슬라이더에 제공된 프레임, 바와 손잡이 이미지를 연결했다.
- ZIP의 이미지 30개가 프로젝트 내부 파일과 동일함을 해시로 확인해 에셋 복사는 수행하지 않았다.
- Unity Play Mode에서 두 씬의 크기, 비율과 입력 확인이 필요하다.
- 사용자 확인에 따라 메인 타이틀의 환경설정 버튼과 사운드 슬라이더는 기존 이미지로 복원했다.
- 제공 Button_UI의 신규 환경설정 이미지는 전투 씬에만 유지한다.
- 사용자 확인에 따라 배틀 씬 퍼즈 환경설정 버튼과 사운드 슬라이더도 기존 이미지로 복원했다.
- 제공 Button_UI는 휴식 회복·강화와 뽑을 덱·버림 덱 버튼에만 유지한다.
# 2026-08-13 튜토리얼 대사 가독성 조정

- 튜토리얼 대사 글자 크기를 36pt에서 43pt로 확대했다.
- 텍스트 영역의 피벗과 위치를 대사 프레임의 세로 중앙에 맞췄다.
- 기존 가로 영역과 중앙 정렬을 유지해 초상화 및 다음 버튼 영역을 침범하지 않도록 했다.
- Unity Play Mode에서 해상도별 대사 줄바꿈과 중앙 배치 시각 확인이 필요하다.

# 2026-08-13 튜토리얼 장문 대사 칸 확장

- TMP가 계산한 실제 표시 줄 수를 기준으로 3줄 이상인 대사를 판별하도록 했다.
- 3줄 이상이면 대사 프레임과 텍스트 영역을 위쪽으로 60 확장하도록 했다.
- 1~2줄 대사에서는 기존 크기로 자동 복구하고, 두 상태 모두 텍스트 중앙 배치를 유지한다.
- Unity Play Mode에서 2줄과 3줄 대사를 오갈 때 크기 복구 및 줄바꿈 시각 확인이 필요하다.

# 2026-08-13 덱 보기 버튼 상태 전환 수정

- 전투 우측 상단 덱 보기 버튼의 Navigation을 None으로 변경해 클릭 후 선택 상태가 남지 않도록 했다.
- Selected Sprite에 기존 Hover 이미지를 연결해 선택 상태가 발생해도 Hover 표현이 유지되도록 했다.
- Normal, Hover, Pressed 이미지와 기존 덱 패널 호출 연결은 유지했다.
- Unity Play Mode에서 클릭 중 Pressed 표시와 덱 패널 종료 후 Hover 복귀 확인이 필요하다.

# 2026-08-13 캡틴 튜토리얼 스킬 대상 변경

- 캡틴 튜토리얼 스킬 사용 카드를 CAP_SKL_001에서 CAP_SKL_002 배신으로 변경했다.
- 배신 선택 시 CrewManager 목록의 첫 번째 선원을 선원 1로 지정해 대상 표시를 생성한다.
- 튜토리얼 배신 단계에서는 선원 1 이외의 선원 클릭을 카드 실행 전에 차단한다.
- 배신의 소멸로 보존 단계가 막히지 않도록 캡틴 보존 지정 카드를 CAP_SKL_001 길잡이로 교체했다.
- Unity Play Mode에서 선원 1 표시와 다른 선원 클릭 차단을 확인해야 한다.

# 2026-08-13 선원 대상 튜토리얼 마커 위치 수정

- 선원 대상 마커에만 Renderer 통합 경계의 가로 중앙을 사용하도록 선택 인자를 추가했다.
- 마커 높이는 기존처럼 Renderer 최상단과 여백을 기준으로 계산한다.
- 플레이어 및 적 대상 표시는 기존 루트 중심 가로 좌표를 유지한다.
- Unity Play Mode에서 선원 1 머리 위 중앙 배치와 이동 추적을 확인해야 한다.

# 2026-08-13 선원 PNG 기준 대상 마커 배치

- 캡틴 튜토리얼의 선원 마커에 CrewPrefab/Visual SpriteRenderer를 직접 전달하도록 변경했다.
- 마커 좌표를 선원 PNG bounds의 가로 중앙과 최상단을 기준으로 계산한다.
- PNG Renderer를 찾지 못한 경우에는 기존 통합 Renderer 높이 계산으로 대체한다.
- Unity Play Mode에서 선원 PNG 머리 위 중앙 위치를 확인해야 한다.

# 2026-08-13 캡틴·선원 대상 마커 오프셋 조정

- 캡틴 대상 튜토리얼 마커를 기존 위치에서 오른쪽으로 0.25 이동했다.
- 선원 대상 마커를 PNG 기준 위치에서 왼쪽과 아래로 각각 0.20 이동했다.
- 적 대상 마커는 별도 오프셋 없이 기존 위치를 유지한다.
- Unity Play Mode에서 캡틴과 선원 마커의 최종 위치를 시각 확인해야 한다.

# 2026-08-13 아스피도켈 침몰 사망 흐름 수정

- HP 0의 침몰 상태 아스피도켈이 EnemySpawner 행동 목록에서 누락되던 문제를 수정했다.
- TurnManager가 침몰 폭발 가능 상태는 HP 0이어도 TakeTurn을 호출하도록 예외 처리했다.
- 침몰 폭발 후 기존 Die, NotifyEnemyDefeated, CheckBattleEnd 흐름으로 보상 화면을 열도록 유지했다.
- Unity Play Mode에서 침몰 피해 후 보스 사망 및 보상 화면 전환을 확인해야 한다.

# 2026-08-13 일시정지 중 전투 카드 입력 차단

- HandManager 카드 선택 요청에서 PauseManager의 일시정지 상태를 검사하도록 했다.
- BattleManager 카드 실행 직전에도 일시정지 상태를 검사해 기존 선택 카드의 사용을 차단했다.
- 적, 플레이어, 선원 대상 실행 경로가 공통 CanUseSelectedCard 검사를 통과하도록 기존 구조를 유지했다.
- Unity Play Mode에서 퍼즈, 환경설정, 전투 포기 확인 화면과 재개 후 입력을 확인해야 한다.

# 2026-08-13 클래스별 튜토리얼 엑셀 스크립트 반영

- 캡틴, 테크니션, 피지크 튜토리얼의 전체 대사를 제공된 클래스별 엑셀 원문으로 교체했다.
- 클래스별 회피, 스킬, 보존 안내 문구를 분리해 엑셀의 줄바꿈과 문장부호를 유지했다.
- 각 클래스의 마지막 17번 환영 대사 전체에만 TMP #ff0000 색상 태그를 적용했다.
- Unity Play Mode에서 클래스별 전체 대사 순서, 줄바꿈, 마지막 대사 색상을 확인해야 한다.

# 2026-08-13 선원 Collider 중첩 클릭 판정 수정

- 선원 Collider가 서로 겹친 경우 클릭 지점과 PNG 중심이 가장 가까운 선원만 입력을 처리하도록 했다.
- 선원 위치 기준은 CrewPrefab/Visual SpriteRenderer의 화면 좌표 중심을 사용한다.
- 다른 선원의 겹친 OnMouseDown 호출은 카드 사용 요청 전에 차단한다.
- Unity Play Mode에서 선원 사이 중첩 영역과 각 선원 중심 클릭을 확인해야 한다.

# 2026-08-13 플레이어 사망 작살 지퍼 전환 1단계

- 불사 효과 처리 이후에도 플레이어 HP가 0이면 사망 전환을 한 번만 호출하도록 연결했다.
- 작살 진행 위치와 각 화면 가로 지점의 통과 시간을 기준으로 붉은 영역이 위아래로 벌어지는 UI 메시를 추가했다.
- 작살 퇴장 후 남은 영역을 완전히 열고 임시 사망 문구, 최종 스테이지와 타이틀 복귀 버튼을 표시한다.
- 사망 연출 중 전투 시간은 정지하며 모든 전환 코루틴은 unscaledDeltaTime으로 재생한다.
- PlayerCombat Inspector에서 교체할 작살·사망 UI 이미지와 연출 시간을 조정할 수 있다.
- 실제 제공 이미지 연결 전 Unity Play Mode에서 지퍼 방향, 속도와 입력 차단을 확인해야 한다.

# 2026-08-13 사망 UI 타이틀 복귀 보완

- 사망 UI 타이틀 복귀 버튼의 중복 입력을 차단하고 입력 로그를 추가했다.
- 기존 ScreenFadeController가 사용 가능하면 기존 페이드 전환으로 TitleScene에 이동한다.
- 다른 화면 전환이 0.5초 이상 점유된 경우에는 TitleScene을 직접 불러오는 대체 경로를 추가했다.
- 모든 타이틀 이동 경로에서 Time.timeScale을 먼저 1로 복구한다.

# 2026-08-13 사망 UI 타이틀 씬 이름 수정

- 사망 UI가 존재하지 않는 TitleScene을 요청하던 값을 Main_TitleScene으로 변경했다.
- ScreenFadeController 경로와 SceneManager 직접 이동 경로가 Build Scene List의 동일한 씬을 사용한다.
# 2026-08-18 - Rest 버튼 휴식 이미지 연출

- 이미지 교차 페이드를 제거하고 검은 화면을 이용한 0.3초 페이드 아웃·0.3초 페이드 인 전환으로 변경했다.
- Rest와 Upgrade 버튼을 추가로 90px 내려 버튼 라벨을 화면 하단에 가깝게 배치했다.
- Rest와 Upgrade 버튼을 추가로 80px 내리고 라벨을 흰색 Font Weight 700으로 변경했다.
- 휴식 단계 진입 시 HandCardParent를 비활성화하고 다음 전투 시작 직전에 복구하도록 변경했다.
- 흰색 버튼 배경을 제거하고 직업별 `*_Rest` 이미지를 휴식 패널 기본 배경으로 사용하도록 변경했다.
- Rest와 Upgrade 버튼을 기존 위치보다 160px 아래로 이동했다.
- RestPanel 전용 Canvas를 Sorting Order 150으로 설정해 핸드 Canvas(101)를 완전히 가리도록 했다.
- 전용 GraphicRaycaster를 함께 보장해 휴식 버튼 입력을 유지했다.
- 버튼 화면에 불투명한 흰색 배경을 적용해 핸드와 전투 UI를 가렸다.
- Rest와 Upgrade 라벨을 버튼 이미지 아래로 이동했다.
- 완료 이미지 전환 후 버튼 표시 전 0.5초 대기 시간을 추가했다.
- 경로: `Assets/Art/UI/Rest`, `Assets/Scripts/UI/RestPanelUI.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- 직업별 기본/완료 휴식 이미지 6장을 Sprite로 추가했다.
- 휴식 화면 최초 진입 시 이미지를 숨기고 기존 버튼을 표시하도록 변경했다.
- Rest 버튼을 누르면 버튼을 숨긴 채 기본 이미지를 표시하고, 1초 후 완료 이미지와 사용 가능한 버튼을 표시하도록 연결했다.
- 카드 강화와 강화 취소에서는 휴식 이미지 연출이 실행되지 않도록 분리했다.
- 정적 검증 및 C# 컴파일을 진행했으며 Unity Play Mode 시각 확인이 필요하다.
# 2026-08-18 - 전투 턴 전환 두루마리 배너

- 경로: `Assets/Scripts/UI/TurnBannerUI.cs`, `Assets/Scripts/Battle/Managers/TurnManager.cs`
- 적 턴과 내 턴 문구를 화면 중앙의 런타임 두루마리 배너로 표시하도록 추가했다.
- 좌→우 펼침 0.4초, 유지 0.5초, 접힘 0.4초 연출과 입력 차단을 연결했다.
- 적 행동은 적 턴 배너 종료 후 실행하며 플레이어 턴 로직은 배너 뒤에서 준비되도록 유지했다.
- 정적 검증과 C# 컴파일 후 Unity Play Mode 시각 확인이 필요하다.
# 2026-08-18 - Editor 전용 사망 테스트 단축키

- 경로: `Assets/Scripts/Scenes/BattleShortcutController.cs`, `Assets/Scripts/Player/PlayerCombat.cs`
- 전투 중 F9로 플레이어 HP를 0으로 만들고 기존 사망 전환을 즉시 실행하도록 추가했다.
- 테스트 신뢰성을 위해 방어도, 선원, 무감각과 불사 효과를 우회한다.
- 일시정지와 주요 패널에서는 실행하지 않고 빌드에서는 코드를 제외한다.
# 2026-08-18 - 전체 카드 클릭 대상 검증

- 경로: `Assets/Scripts/Battle/Managers/BattleManager.cs`
- 카드 53장의 CSV, CardData와 설명을 대조하고 데이터 대상 값이 일치함을 확인했다.
- 카드 효과 전체를 기준으로 적, 플레이어, 단일 선원 클릭 대상을 판정하도록 추가했다.
- 잘못된 대상 클릭을 카드 실행과 상태 소비 전에 차단하도록 수정했다.
- C# 컴파일 및 대표 카드 Play Mode 확인이 필요하다.
# 2026-08-18 - 사망 UI 재배치 및 이미지 연결 준비

- 관련 경로: `Assets/Scripts/UI/PlayerDeathTransitionController.cs`
- 사망 제목과 최종 진행 스테이지를 검은 패널로 분리하고 타이틀 복귀 버튼을 하단으로 재배치했다.
- 추후 제공되는 사망 이미지를 Inspector에 연결하면 왼쪽 아래에 표시되도록 선택 필드를 추가했다.
- 제공된 CAP/PHY/TEC 사망 이미지를 `Assets/Art/UI/Death`로 가져오고 각 클래스 Prefab에 연결했다.
- 제공된 `Death_UI.png`를 사망 문구와 최종 스테이지 공통 프레임으로 적용하고 글자를 밝은 아이보리색으로 조정했다.
- 사망 작살 이동 시간을 0.3초로 단축하고 플레이어 실제 화면 위치에서 정지하도록 변경했다.
- 기존 좌우 지퍼 전환을 충돌점 중심의 붉은 원형 확산으로 교체했다.
- 타이틀 복귀 버튼에 메인 타이틀 게임 시작 버튼의 상태 이미지를 적용했다.
- ImageGen으로 기존 캐릭터 화풍에 맞는 사망 연출용 작살 이미지를 제작하고 투명 PNG로 적용했다.
- 작살 진입 방향을 화면 우측 위에서 플레이어로 향하는 대각선으로 변경하고 진행 방향에 맞춰 회전시켰다.
- 붉은 배경 확산 시간을 0.48초에서 0.68초로 늦췄다.
- 사망 작살 크기를 560×280으로 줄였다.
- 클래스별 공격 Atlas에서 원본 궤적과 충돌 섬광을 추출해 비행 꼬리와 도착 임팩트로 연결했다.
- 충돌 후 작살, 꼬리와 섬광이 0.12초 안에 페이드 아웃되도록 변경했다.
- 사망 효과 Sprite Pivot을 중앙으로 수정하고 작살 PNG의 우측 투명 여백을 보정해 작살촉과 충돌 효과 위치를 일치시켰다.
- 작살 내부에 실제 촉 기준점 `HarpoonTipPoint`를 추가하고 충돌 섬광과 붉은 확산 중심이 해당 Transform을 직접 사용하도록 변경했다.
- 비행 꼬리를 작살 자식으로 변경해 이동과 회전 시 위치가 분리되지 않도록 고정했다.
- 충돌 임팩트의 별도 Y 보정을 제거하고 `HarpoonTipPoint` 로컬 원점에 직접 고정했다. 붉은 확산도 동일한 충돌 지점을 사용한다.
- 사용자 녹화의 16.40~16.55초 구간과 트레일 PNG 알파 중심을 비교해, 비행 중 클래스 이펙트를 작살 로컬 Y `+82` 지점으로 보정했다.
- 추가 녹화에서 트레일이 로컬 X `-360`으로 밀려 있음을 확인하고, 트레일의 오른쪽 끝을 `GetHarpoonTipInset` 기준의 실제 작살촉 X 위치에 고정했다.
- 자동 검증: C# 컴파일 확인 필요
- 수동 검증: F9 사망 테스트로 UI 배치와 이미지 비연결 상태 확인 필요
# 2026-08-18 게임 클리어 엔딩 영상

- 제공된 8초 엔딩 영상을 H.264 Baseline, 24fps, BT.709 규격으로 변환해 `Assets/Resources/Video/Ending.mp4`에 추가했다.
- 빈 `GameClearScene`에 `GameClearController`를 연결하고 전체 화면 영상 재생과 전투 BGM 정지를 구현했다.
- 아리엘 처치 후 보상 화면 대신 게임 클리어 씬으로 이동하고, 영상 종료 후 메인 타이틀로 복귀하도록 연결했다.
- `GameClearScene`을 Build Scene List에 등록했다.
# 2026-08-18 클래스 상세 창 전체 화면 확장

- `ClassDetailPanel`의 Stretch 여백을 제거해 Canvas 전체 영역을 사용하도록 변경했다.
- 상세 패널 열기 배율을 0.75에서 1로 변경했다.
- 내부 UI를 원본 `1580×880` 비율의 `DetailContent`로 묶고 1920 화면 너비에 맞춰 약 1.215배 균일 확대했다.
- 선택한 클래스 버튼을 1920×1080 전체 화면으로 확대하고 상세 UI가 그 위에 표시되도록 계층 순서를 유지했다.
- 상세 화면에서는 미선택 카드를 숨기며, 뒤로 가기 시 표시 상태와 원래 계층 순서를 복원한다.
- 카드 확대 시간만큼 기다린 뒤 상세 정보 패널을 표시하고, 대기 중 뒤로 가면 예약된 표시를 취소하도록 처리했다.
- 카드 확대 시간을 0.36초로 조정하고 확대 완료 후 0.2초의 추가 대기 시간을 적용했다.
- 클래스 선택 영상 3종을 Resources에 추가하고 상세 화면 왼쪽 이미지 칸에서 클래스별로 반복 재생하도록 연결했다.
- 영상은 음소거하고 원본 비율을 유지한 채 왼쪽 영역을 채우도록 중앙 크롭하며, 선택 취소 시 재생을 정리한다.
- 클래스 선택 영상 3종을 H.264 Constrained Baseline, 1280×720, 24fps, BT.709로 재인코딩해 WindowsMediaFoundation 경고 원인을 제거했다.
- 레이아웃 시스템 때문에 반영되지 않던 RectTransform 위치 조정을 제거하고 `RawImage.uvRect` 기반 크롭으로 교체했다.
- PHY는 원본 가로 60%, TEC는 59%, CAP은 50%를 표시 중심으로 사용해 클래스별 얼굴 구도를 분리했다.
- 클래스 선택 영상용 PNG 프레임 적용을 제거하고 Unity UI Image 4개로 구성한 4px 테두리로 교체했다.
- PHY는 청록, TEC는 자홍, CAP은 연두 계열의 버튼 보색을 사용하고 영상과 마스크의 340×750 크기를 유지했다.

# 2026-08-19 이어하기 손패 강화 카드 복원 수정

- 이어하기 덱 복원 후 이름 정렬이 저장된 카드 인덱스와 카드 인스턴스의 대응을 바꾸는 원인을 확인했다.
- `RestoreDeck`에서는 저장된 카드 순서를 그대로 유지해 손패와 드로우 파일 인덱스가 정확한 강화 카드 인스턴스를 가리키도록 수정했다.
- 신규 게임과 카드 획득 시의 기존 덱 이름 정렬은 유지했으며 저장 형식과 `saveVersion`은 변경하지 않았다.
- 왼쪽 영상 영역을 스텐실 마스크로 제한해 확대 영상이 칸 바깥으로 노출되는 문제를 수정했다.
- 동일한 영상 텍스처를 전체 화면 블러 배경으로 재사용하고 어두운 오버레이를 추가해 상세 정보 가독성을 높였다.
- 전체 화면 선택 방식에서 사용하지 않는 측면 카드 크기·간격 필드를 제거해 CS0414 경고를 정리했다.
- 기존 상세 정보 UI와 Back, Confirm, ESC, Enter 입력 흐름은 유지했다.

# 2026-08-19 몬스터 Spine 원본 Import

- 관련 경로: `Assets/Art/Enemy/Spine`
- `Monster_Spine.zip`의 PNG, SKEL, ATLAS 17세트를 프로젝트에 배치했다.
- Atlas 파일은 Unity Spine Importer용 `.atlas.txt` 확장자로 정리했다.
- Skeleton 파일은 Unity Spine Importer용 `.skel.bytes` 확장자로 정리했다.
- SKEL 17개가 모두 Spine 4.3.23 데이터이며 프로젝트 Runtime 4.3 지원 범위에 포함되는 것을 확인했다.
- PMA Atlas에 맞게 Spine Auto-Import Preset을 전환했다.
- Atlas Asset, Material, SkeletonData Asset이 각각 17개 생성됐고 누락된 Meta와 고아 Meta가 없음을 확인했다.
- 기존 몬스터 프리팹 연결은 변경하지 않았다.

# 2026-08-19 1스테이지 일반 몬스터 Spine 적용

- 관련 경로: `Assets/Prefabs/Enemy/NormalBattle/1Stage`
- Goby 2개, Mermaid 1개, Mimic 2개, SeaCrab 2개, Thief 2개 프리팹에 종별 Spine Idle을 연결했다.
- 기존 SpriteRenderer의 표시 Bounds와 Sorting Order를 기준으로 Spine 렌더러의 크기, 중심과 정렬 순서를 설정했다.
- 기존 SpriteRenderer는 비활성화하고 Enemy 컴포넌트, 콜라이더와 UI 자식 참조는 유지했다.
- 프리팹 9개 모두 SpineVisual, SkeletonData, SkeletonAnimation 1개와 비활성 SpriteRenderer 1개를 정적으로 확인했다.
- 임시 Editor 적용 도구는 프리팹 저장 완료 후 제거했다.
- Unity Play Mode에서 몬스터별 크기, 위치와 Idle 반복을 시각적으로 확인해야 한다.

# 2026-08-19 모르바엘 전투 종료 순서 버그 수정

- 관련 경로: `Assets/Scripts/Enemy/Enemy.cs`
- 모르바엘이 먼저 사망해 전투 종료가 보류된 뒤 마지막 장송의 원혼이 사망하면 전투 종료 조건을 다시 검사하도록 수정했다.
- 장송의 원혼을 비활성화한 다음 검사하여 사망한 원혼이 생존 적으로 계산되지 않도록 했다.
- Unity 컴파일 및 Play Mode 재현 테스트가 필요하다.

# 2026-08-19 1스테이지 일반 몬스터 Spine 시각 보정 완료

- 1스테이지 Goby, Mermaid, Mimic, SeaCrab, Thief 프리팹 9개의 Spine 비율과 크기를 보정했다.
- 종별 발, 꼬리와 상자 하단을 BattleScene 나무 floor에 맞추도록 개별 위치를 저장했다.
- `EnemyUIRoot` 위치를 프리팹별로 조정해 몬스터 머리 위에 일정한 간격으로 표시했다.
- 두 몬스터가 겹칠 때 Spine 메시 정렬이 흔들리지 않도록 1·2번 프리팹의 Sorting Order를 분리했다.
- 튜토리얼 대상 역삼각형이 적 UI의 실제 활성 Graphic Bounds 상단 중앙을 따라가도록 변경했다.
- 사용자 Unity Play Mode 확인으로 몬스터 표시, UI 위치와 튜토리얼 화살표 동작을 검증했다.
- 최종 프리팹 저장 후 임시 `EnemySpinePrefabSetup` Editor 도구를 제거했다.
# 2026-08-19 2스테이지 일반 몬스터 Spine 초기 연결

- 관련 경로: `Assets/Prefabs/Enemy/NormalBattle/2Stage`
- Drowned 2개, JellyfishMermaid, OldMermaid, ShortFinnedSandfish, Turtle 2개 프리팹에 종별 Spine Idle을 연결하는 임시 Editor 도구를 추가했다.
- 기존 루트 SpriteRenderer의 실제 높이와 하단을 기준으로 Spine 비율과 초기 위치를 계산하며 UI, 콜라이더와 전투 컴포넌트는 유지한다.
- Unity Refresh 후 7개 프리팹 적용 결과와 Play Mode 시각 검증이 필요하다.
- Editor Play Mode에서 F8을 누르면 진행도를 2스테이지 첫 일반 전투로 설정하고 기존 다음 전투 준비 흐름으로 즉시 전환하도록 테스트 단축키를 추가했다.
- 첫 Play 화면에서 모든 2스테이지 Spine이 지나치게 작고 UI가 분리된 것을 확인해 종별 크기를 4~5.5배 확대하고, 바닥 접지와 UI 상단 간격을 실제 Spine Bounds 기준으로 다시 계산하도록 보정했다.
- Bounds 기반 확대값이 누적되어 몬스터와 UI가 화면을 벗어난 문제를 확인하고, 반복 실행해도 변하지 않는 종별 고정 Transform 값으로 교체했다.
- 2스테이지 floor의 실제 Play 화면을 기준으로 몬스터 접지점을 위로 통일하고 화면 위로 잘린 EnemyUIRoot를 종별 머리 높이에 맞게 낮췄다.
- Play 화면에서 뒤쪽 난간 선에 떠 있던 Turtle과 Drowned만 몸과 UI를 함께 아래로 내려 앞쪽 나무 floor에 맞췄다.
- OldMermaid, JellyfishMermaid와 Drowned의 UI가 머리와 겹치는 화면을 기준으로 해당 종의 EnemyUIRoot만 추가로 위로 이동했다.
- Drowned UI가 머리에서 조금 멀어진 최종 화면을 기준으로 EnemyUIRoot만 소폭 아래로 미세 조정했다.
- 사용자 Play Mode 확인 후 2스테이지 몬스터 7종의 크기, floor 접지점과 UI 위치를 확정하고 임시 Editor 적용 도구를 제거했다.
- F8 테스트 키를 고정 2스테이지 이동에서 현재 기준 다음 스테이지 첫 일반 전투 이동으로 변경하고, 마지막 스테이지에서는 이동을 차단했다.
# 2026-08-19 3스테이지 일반 몬스터 Spine 초기 연결

- 관련 경로: `Assets/Prefabs/Enemy/NormalBattle/3Stage`
- Edward, SeaBeast, Shipcollector, Templeguardian, Undeadcommander 프리팹에 각각의 Spine Idle을 연결하는 임시 Editor 도구를 추가했다.
- 기존 루트 SpriteRenderer의 하단과 높이를 기준으로 Spine 비율을 유지한 초기 크기와 위치를 계산하며 UI, 콜라이더와 전투 컴포넌트는 유지한다.
- Unity Refresh 후 5개 프리팹 적용 결과와 F8 3스테이지 Play Mode 시각 검증이 필요하다.
- 사용자 Play 화면에서 크기와 위치가 정상임을 확인하고, 몬스터 본체는 유지한 채 5종 EnemyUIRoot만 실제 Spine 상단 Bounds 위로 재배치했다.
- Spine 상단 Bounds가 5종 모두 같은 잘못된 UI 높이를 만든 문제를 확인해 종별 고정 UI 위치로 교체하고, Undeadcommander만 접지점을 유지하며 약 35% 확대했다.
- Shipcollector UI가 머리에서 여전히 크게 떨어진 Play 화면을 기준으로 해당 EnemyUIRoot만 추가로 낮췄다.
- 사용자 제공 이미지 순서에 따라 Shipcollector UI는 오른쪽, Templeguardian과 Edward UI는 아래, SeaBeast UI는 왼쪽 아래로 미세 조정하고 Undeadcommander는 유지했다.
- 후속 Play 화면을 기준으로 Shipcollector UI는 왼쪽으로 재조정하고, Templeguardian과 Edward는 추가 하향, SeaBeast는 실제 머리 위치까지 크게 왼쪽 아래로 이동했다.
- 사용자 Play Mode 확인 후 3스테이지 일반 몬스터 5종의 크기, floor 접지점과 UI 위치를 확정하고 임시 Editor 적용 도구를 제거했다.
# 2026-08-21 사슬 투창 취약 지속 턴 중첩 수정

- `PHY_ATK_002`를 같은 적에게 재사용할 때 내부 수치만 증가하고 UI에 표시되는 지속 턴은 2로 유지되는 원인을 확인했다.
- 효과 강도가 고정된 약화, 취약, 손상, 미끄러짐과 부러짐은 수치를 유지하고 지속 턴을 합산하는 공통 상태 효과 경로를 추가했다.
- 기존 적 디버프 지속 턴 중첩 메서드는 새 공통 경로를 호출하도록 유지해 기존 호출부 호환성을 보존했다.
# 2026-08-21 적 약화 종료 후 Intent 피해 표시 갱신

- 적 행동 직후 다음 패턴 Intent가 먼저 계산되고 그 뒤 약화 지속 턴이 감소해, 약화 제거 후에도 이전 피해 수치가 화면에 남는 원인을 확인했다.
- `EnemyIntentUI`가 해당 적의 `StatusEffectsChanged` 이벤트를 구독해 힘·약화 추가, 중첩, 감소와 제거 직후 현재 Intent를 다시 계산하도록 수정했다.
- 패턴 진행 순서와 실제 공격 피해 계산은 변경하지 않았다.
# 2026-08-21 전체 핵심 로직 50회 회귀 테스트 준비

- 카드 에셋, 적 패턴 CSV, 1~3스테이지 일반 적 프리팹 구성을 검사하는 임시 Editor 회귀 도구를 추가했다.
- 고정 강도 디버프 지속 턴 중첩, 힘·약화 피해 계산, 상태 변경 이벤트와 EnemyIntentUI 구독을 독립 초기화하여 각각 50회 반복하도록 구성했다.
- Unity Refresh 후 6개 항목 총 300회 결과 확인이 필요하다.
- 1차 실행에서 패턴 CSV enum 대소문자와 Edit Mode 생명주기 차이로 발생한 테스트 자체의 오탐 2건을 확인했다.
- CSV 검증을 실제 런타임 로더와 같은 대소문자 무시 방식으로 맞추고 Intent 구독 테스트가 OnEnable 경로를 명시적으로 실행하도록 수정했다.
- 모르바엘 4턴의 원혼 유무별 조건부 공격이 같은 실행 순서를 공유하는 정상 데이터를 중복으로 판정한 오탐을 확인했다.
- 실행 순서 중복 검사를 조건 종류와 조건 값까지 포함하도록 변경해 서로 다른 조건 분기는 허용하고 동일 조건 중복만 실패 처리한다.
- Unity Editor에서 6개 핵심 항목을 각 50회 실행해 전체 300/300 통과를 확인했다.
- 검증 완료 후 임시 `ProjectRegressionTestRunner` Editor 도구와 메타 파일을 제거했다.
# 2026-08-21 상태 효과 Hover 팝업 가독성 개선

- 플레이어와 적이 공유하는 상태 효과 아이콘 프리팹에 Blue/Red 팝업 Sprite를 연결했다.
- 디버프는 Red, 버프와 패시브는 Blue로 구분하고 9-Slice로 내용 길이에 맞춰 높이가 확장되도록 구성했다.
- Hover 설명에 고정 비율, 공격 피해, 획득 방어도와 지속 시간을 게임 효과 기준으로 명시했다.
- Play 화면 기준으로 팝업 글자가 테두리에 붙지 않도록 좌우·상하 여백과 글자 크기를 조정하고 자동 높이 계산에 동일한 여백을 반영했다.
# 2026-08-22 1스테이지 이동 맵 1단계

- 첨부된 깨끗한 항구 맵을 이미지 편집 도구로 원형을 유지해 복원하고 `Stage1_Map.png` 배경 에셋으로 추가했다.
- 전체 맵을 표시하는 고정 직교 카메라와 좌측 하단 시작 지점을 갖는 `StageMapScene` 생성 도구를 추가했다.
- 선택한 클래스의 기존 Spine 외형만 재사용하고 전투 컴포넌트를 비활성화한 맵 캐릭터가 WASD로 이동하도록 구성했다.
- 이동 경계, 차단물과 전투 진입은 다음 단계로 분리했다.
- 사용자 Play Mode 확인으로 선택 클래스 캐릭터 생성과 WASD 이동이 정상 동작함을 확인했다.
- `StageMapScene`과 Build Settings 저장 후 임시 `StageMapSceneSetup` Editor 도구를 제거해 1단계를 마무리했다.
# 2026-08-22 인간형 150 Run 폴리싱 테스트 기반

- 클래스별 50 Run의 ID와 재현용 Seed를 순서대로 발급하는 테스트 Coordinator 기반을 추가했다.
- Run, 전투, 턴, 사망 원인과 최근 행동을 직렬화할 수 있는 기록 구조를 추가했다.
- Run별 JSON과 전체 요약 CSV를 `Application.persistentDataPath/PolishTestResults`에 저장하도록 구성했다.
- 이번 단계에서는 AutoPlayer와 실제 게임 진행을 연결하지 않았으며 밸런스 및 전투 규칙을 변경하지 않았다.
# 2026-08-22 폴리싱 테스트 Seed·기록 재현 검증

- 같은 입력의 Seed 생성, 150 Run Seed 고유성과 Unity 난수열 재현을 확인하는 EditMode 테스트를 추가했다.
- Run, 전투, 턴과 사망 정보의 JSON 저장 및 CSV 헤더·행 누적을 검증하도록 구성했다.
- Logger가 EditMode 테스트 중 사용자 기록 폴더 대신 고유한 임시 폴더를 사용하도록 테스트 전용 출력 경로를 추가했다.
- `PHY-001`의 Run ID와 Seed가 새 Coordinator에서도 동일하게 발급되는지 검증한다.
# 2026-08-22 폴리싱 테스트 공개 정보 관찰 계층

- 현재 HP·Block, 손패 설명과 사용 가능 여부, 살아 있는 적, 화면 표시 Intent, 상태 효과, 작살과 선원 정보를 복사하는 관찰 계층을 추가했다.
- `EnemyIntentUI`가 실제 생성한 Intent 아이콘 문자열을 읽기 전용 목록으로 제공해 AutoPlayer가 UI와 같은 수치를 사용하도록 했다.
- 관찰 스냅샷에는 다음 드로우, 보상, Seed, RNG와 미래 패턴 필드를 두지 않았으며 이를 EditMode 테스트로 검사한다.
- 이번 단계에서는 AutoPlayer의 카드 선택과 턴 진행을 구현하지 않았다.
# 2026-08-22 폴리싱 테스트 공통 인간형 판단

- 공개 스냅샷의 Intent 피해를 단일·다단히트 표기에서 해석하고 HP·Block 기준 위험도를 네 단계로 분류했다.
- 카드 화면 설명의 피해·방어도·회복 키워드를 기준으로 완전 탐색 없이 한 행동을 점수화하도록 구성했다.
- 명백한 사망 위험에서는 생존 행동을 우선하되 처치로 피해를 제거할 수 있으면 공격을 선택하도록 했다.
- 비슷한 선택은 게임 RNG와 분리된 결정용 난수로 고르며 같은 Seed와 화면 상태에서는 같은 판단을 재현한다.
- 실제 카드 클릭과 턴 종료는 연결하지 않았다.

## 2026-08-22 - 인간형 150 Run 폴리싱 테스트 5단계

- 판단 결과를 기존 카드 선택, 대상 지정, 턴 종료 API로 전달하는 실행기를 추가했다.
- 손패 인덱스, 적 인덱스와 선원 순서가 유효하지 않으면 실패 결과를 반환하도록 했다.
- null 판단과 필수 전투 컴포넌트 누락에 대한 Editor 테스트를 추가했다.
- Unity 컴파일 및 Test Runner 실행 결과는 확인 필요 상태다.

## 2026-08-22 - 인간형 150 Run 폴리싱 테스트 8단계

- BattleScene에서 F7을 눌러 현재 전투의 인간형 자동 테스트를 시작하도록 연결했다.
- 현재 클래스, 스테이지, 전투 번호와 재현 Seed를 기록하고 종료 시 JSON과 CSV를 저장하도록 했다.
- F7 중복 실행을 차단하고 보상 선택과 다음 전투 이동은 수행하지 않도록 제한했다.
- Unity 컴파일, EditMode 테스트와 실제 F7 Play Mode 검증은 확인 필요 상태다.

## 2026-08-23 - F7 자동 전투 카드 대상 판정 반복 수정

- 카드 효과의 실제 클릭 대상을 공개 손패 정보에 포함하고 판단 엔진이 이를 우선하도록 수정했다.
- 적 대상 스킬과 선원 희생 카드가 각각 유효한 적과 최저 체력 선원을 선택하도록 보완했다.
- 카드 소비와 턴 전환이 실제로 일어나지 않으면 실행 성공 대신 거부 결과를 반환하도록 했다.
- Unity 컴파일, EditMode 테스트와 실제 F7 Play Mode 재검증은 확인 필요 상태다.

## 2026-08-23 - 타이틀 F7 전체 폴리싱 캠페인 1단계

- 타이틀 F7로 총 150 Run 세션과 첫 Physique Run을 발급하는 캠페인 컨트롤러를 추가했다.
- 사용자 저장 파일은 보존하고 런타임 진행도만 초기화하는 테스트 전용 타이틀 진입 경로를 추가했다.
- 클래스 선택 씬에서 현재 Run 클래스를 자동 선택·확정하고 BattleScene 준비 상태까지 유지하도록 연결했다.
- 전투와 튜토리얼 자동 진행은 후속 단계로 분리했으며 Unity 검증이 필요하다.

## 2026-08-23 - F7 클래스 선택 및 튜토리얼 자동화

- 클래스 선택 씬 이름 문자열 불일치 영향을 받지 않도록 실제 ClassSelectManager 존재 여부로 자동 선택을 시작하게 수정했다.
- 시작 덱 확인과 튜토리얼 대화, 지정 카드 사용, 보존, 턴 종료를 기존 공개 입력 경로로 자동 처리하도록 추가했다.
- 튜토리얼 완료 뒤 기존 단일 전투 자동 컨트롤러가 현재 Run Seed로 전투를 이어받도록 연결했다.
- Unity 컴파일과 실제 타이틀 F7 Play Mode 검증이 필요하다.

## 2026-08-23 - F7 일반 전투 보상 자동화

- 자동 전투 승리 결과를 기록하고 보상 패널 준비 상태를 감시하도록 캠페인 흐름을 확장했다.
- 공개된 보상 중 희귀도가 가장 높은 카드를 실제 선택·확정 경로로 획득하도록 추가했다.
- 일반 구간은 다음 전투 생성 완료 후 자동 전투를 재시작하고 휴식·보스 분기는 후속 단계 대기로 남겼다.
- 보상 우선순위 Editor 테스트를 추가했으며 Unity 컴파일과 Play Mode 검증이 필요하다.

## 2026-08-23 - F7 휴식·강화·보스전 진입 자동화

- 체력이 부족할 때 휴식 회복을 실행하고 이미지 전환 연출이 끝날 때까지 입력을 대기하도록 추가했다.
- 강화 가능한 공개 카드 중 희귀도가 높은 카드를 실제 선택·확정 경로로 강화하도록 연결했다.
- 휴식 완료 후 실제 Next Battle 경로로 보스전에 진입하고 몬스터 생성 뒤 자동 전투를 재시작하도록 했다.
- 강화 선택 Editor 테스트를 추가했으며 Unity 컴파일과 Play Mode 검증이 필요하다.

## 2026-08-23 - F7 실제 보존 입력 경로 적용

- 자동 턴 종료가 TurnManager를 직접 호출해 손패 버림을 건너뛰던 원인을 수정했다.
- 자동 판단이 공개 손패에서 다음 턴에 남길 카드 최대 한 장을 선택하도록 추가했다.
- 실제 보존 모드 진입, 카드 선택, 확정과 나머지 카드 버림 연출을 그대로 사용하도록 실행기를 변경했다.
- 튜토리얼 보존 확정 뒤에는 HandManager가 수행하는 턴 종료를 기다리도록 중복 입력을 제거했다.

## 2026-08-23 - F7 상황 판단 보존

- 공개 손패 스냅샷에 카드 희귀도를 추가하고 보존 판단에서 강화 여부, 종류와 예상 피해를 함께 평가하도록 했다.
- 절대 보존 점수가 기준 미만이면 남은 카드가 있어도 빈 보존을 선택하도록 변경했다.
- 같은 점수의 후보는 Run 판단 Seed를 사용해 재현 가능한 범위에서 선택이 달라지도록 했다.
- 기본 일반 카드는 보존하지 않고 강화 스킬·에픽 카드가 보존되는 Editor 테스트를 추가했다.

## 2026-08-23 - F7 클래스별 50 Run 자동 반복 및 보고서

- Run 클리어·사망·오류를 확정한 뒤 런타임을 초기화하고 같은 클래스 다음 Run을 자동 시작하도록 연결했다.
- 클래스별 50회 완료 후 Physique, Technician, Captain 순서로 전환하고 총 150회 완료 알림을 추가했다.
- 세션별 Runs JSON, RunSummary, ClassSummary, IssueSummary와 FinalReport 생성을 추가했다.
- Error, Exception, Assert 로그를 Run ID와 Seed가 포함된 버그 후보 기록으로 저장하도록 했다.

## 2026-08-23 - F7 특수 선택 패널 자동화

- 자동 전투 루프가 계시 패널 활성화를 우선 감지하고 일반 카드 입력을 일시 정지하도록 했다.
- 현재 체력과 이미 선택한 계시를 기준으로 실제 계시 버튼 경로를 호출하도록 추가했다.
- 선택한 계시와 당시 체력을 Console에 기록하고 생존 우선 선택 Editor 테스트를 추가했다.

## 2026-08-23 - F7 자동 테스트 4배속

- F7 전체 캠페인 시작 시 Time.timeScale을 4로 설정해 전투·턴·연출을 가속하도록 했다.
- UI 자동 입력의 실시간 대기는 유지하고 캠페인 완료·중단·제거 시 기존 배속을 복원하도록 했다.
- 일반 플레이에는 영향을 주지 않도록 Editor 전용 캠페인 Controller 내부로 제한했다.

## 2026-08-22 - 인간형 150 Run 폴리싱 테스트 7단계

- 플레이어 턴 시작 시 공개 HP, 방어도, 손패, 적 상태와 Intent를 턴 기록으로 생성하도록 연결했다.
- 한 턴의 카드 사용과 턴 종료를 순서대로 누적하고 판단 이유, 위험도와 실행 결과를 함께 기록하도록 했다.
- Logger가 연결되지 않은 일반 플레이에서는 기록을 생략하고 자동 전투는 유지한다.
- Unity 컴파일 및 Test Runner 실행 결과는 확인 필요 상태다.

## 2026-08-22 - 인간형 150 Run 폴리싱 테스트 6단계

- 플레이어 턴과 카드 흐름의 입력 가능 시점을 기다리며 단일 전투를 자동 진행하는 컨트롤러를 추가했다.
- 승리, 사망, 제한 시간과 최대 행동 수를 기준으로 자동 진행을 안전하게 종료하도록 했다.
- 보상 선택과 다음 전투 진입은 연결하지 않았다.
- Unity 컴파일 및 Test Runner 실행 결과는 확인 필요 상태다.
# 2026-08-23 자동 플레이 카드 조합 판단 1단계

- `PolishBattleObserver`가 현재 손패 카드의 실제 효과 종류, 수치, 대상과 반복 횟수를 공개 스냅샷에 복사하도록 확장했다.
- `PolishHumanDecisionEngine`에 최대 4장, 빔 폭 24의 제한 조합 탐색을 추가했다.
- 취약·약화·힘·방어·회복·다단 공격·작살 부여/소비의 순서 효과를 가상 상태에 반영한다.
- 조합의 첫 카드만 실행하고 실제 사용 후 전체 판단을 다시 수행해 예측 오차가 누적되지 않도록 유지했다.
- 취약 후 공격과 힘 후 다단 공격 순서를 검증하는 Editor Test를 추가했다. Unity 실행 검증은 필요하다.
# 2026-08-23 30분 목표 고속 자동 테스트 모드

- 타이틀의 `F7`은 기존 4배속 일반 모드, `Shift+F7`은 20배속 고속 모드로 분리했다.
- 고속 모드에서는 전투와 튜토리얼 자동 입력을 프레임 단위로 진행하고 다음 Run의 0.5초 고정 대기를 제거했다.
- 150 Run, 판단 로직, Seed와 오류 기록 방식은 일반 모드와 동일하게 유지했다.
- 최종 보고서에 실행 모드와 실제 경과 시간을 기록하도록 확장했다.
- 씬 로딩 시간은 배속 대상이 아니므로 30분 완료 여부는 실제 Play Mode 측정이 필요하다.
# 2026-08-23 클래스별 50회 고속 테스트 1단계

- Shift+F7 피지크, Shift+F8 테크니션, Shift+F9 캡틴으로 고속 테스트 입력을 분리했다.
- `PolishTestRunCoordinator`가 지정 클래스만 50회 발급한 뒤 완료 상태가 되도록 확장했다.
- 고속 씬에서는 카메라, VideoPlayer, AudioListener와 Renderer 출력만 비활성화하고 UI 및 전투 로직은 유지했다.
- 완료 팝업과 최종 보고서에 대상 클래스, 50회 진행도와 실제 경과 시간을 기록한다.
- 실제 Play Mode에서 클래스별 소요 시간과 UI 의존 로직 정상 동작 확인이 필요하다.
# 2026-08-23 자동 플레이 기록 기반 안정화

- 체력 소모 카드가 플레이어를 즉시 사망시키는 경우 후보에서 제외하고 생존 불가 상황에서는 큰 감점을 적용했다.
- 예정 피해가 없을 때 순수 즉시 방어 카드가 낭비되지 않도록 후보에서 제외했다.
- 조합 탐색 점수에도 실제 체력 소모 비용과 사망 경로 제외를 반영했다.
- 단일 전투 Controller가 사망 시 공개 상태, 직전 턴과 Intent를 `PolishDeathRecord`로 저장하도록 연결했다.
- Victory 감지 후 `BattleManager` 전투 종료 완료를 확인하고 보상 자동화를 시작하도록 경합을 차단했다.
- 보상 패널 Timeout 메시지에 진행 Phase, 패널 상태, 카드 수, 전투 상태와 Encounter 진단을 추가했다.
- Unity Play Mode에서 기존 실패 Seed `2127192874`, 사망 Seed `2127192878` 재검증이 필요하다.
# 2026-08-24 고속 테스트 PHY-003 정지 수정

- 고속 모드에서 Main Camera를 비활성화해 `Camera.main`이 사라지던 문제를 수정했다.
- Main Camera는 활성 상태로 유지하고 cullingMask 0과 검은 Clear만 적용해 렌더링 비용을 줄였다.
- 고속 폴리싱 테스트에서는 카드 버림 VFX를 생략하고 일반 `CompletePreserveConfirmation` 경로로 즉시 처리한다.
- `PolishTestLogger`가 동일 Run의 같은 오류 메시지를 한 번만 `IssueSummary.csv`에 기록하도록 중복 방지했다.
- 기존 실패 세션은 PHY-001~002까지만 유효하며 PHY-003부터 재검증이 필요하다.
# 2026-08-24 모르바엘 이후 아리엘 자동 전투 연결

- 보상 자동화가 `NormalBattle`뿐 아니라 `BossBattle`에서도 다음 적 생성을 기다리도록 확장했다.
- Stage 3 모르바엘 보상 후 동일 BossBattle 단계에서 생성되는 아리엘에 자동 전투를 연결한다.
- Campaign Controller에도 BossBattle 대기 콜백을 처리하는 안전 경로를 추가했다.
- 일반 전투와 휴식 분기는 기존 동작을 유지했다.
- PHY-003에서 `후속 자동화 단계 대기: BossBattle` 이후 멈추던 기록을 기준으로 재검증이 필요하다.
# 2026-08-24 자동 테스트 클래스별 30회 조정

- 자동 테스트의 클래스별 Run 횟수를 50회에서 30회로 줄였다.
- 클래스 단독 고속 테스트는 30회 후 정지하고 일반 F7은 총 90회 후 종료한다.
- 기존 Run ID와 Seed 생성 공식은 유지해 각 클래스 1~30번 결과를 이전 기록과 비교할 수 있다.
- 세션 상태, 최종 보고서, 완료 팝업과 회귀 테스트의 예상 횟수를 함께 갱신했다.
# 2026-08-24 - 폴리싱 자동 테스트 전투 승리 조기 판정 수정

- 관련 경로: `Assets/Scripts/Testing/Polish/PolishSingleBattleController.cs`
- 관련 테스트: `Assets/Tests/Editor/Polish/PolishSingleBattleControllerTests.cs`
- 적이 잠시 0명인 프레임을 즉시 승리로 확정하지 않고 `BattleManager.IsBattleStarted`를 함께 확인하도록 수정했다.
- 후속 적 생성 또는 사망 처리 중에는 자동 전투를 유지하고, 실제 전투 종료 후에만 승리를 기록한다.
- 런타임 및 Editor 어셈블리 정적 빌드 오류 0개를 확인했다. Unity Editor Test와 Physique Play Mode 재실행은 필요하다.
# 2026-08-24 - 독립 시뮬레이터 런타임 경로 1단계

- `POLISH_SIMULATION_BUILD` 조건에서 자동 캠페인 컨트롤러가 Player 빌드에 포함되도록 분리했다.
- `-simulate -class -runs -speed -output` 명령줄 설정과 완료 후 자동 종료를 추가했다.
- 시뮬레이터 저장 파일은 `simulation_game_save.json`, 결과는 별도 `PolishSimulationResults` 루트에 기록하도록 분리했다.
- 클래스별 실행 횟수, 배속, 명령줄 파싱 Editor 테스트를 추가했다.
- `Assembly-CSharp` 정적 빌드는 기존 직렬화 경고 4개, 오류 0개다. 새 파일 Unity Import 후 Editor 테스트 확인이 필요하다.
# 2026-08-24 - 독립 시뮬레이터 Editor 빌드 메뉴 2단계

- `Tools > Build > Simulation Build` 메뉴를 추가했다.
- 활성화된 Build Settings Scene으로 Windows 64비트 시뮬레이터를 생성하도록 구성했다.
- `BuildPlayerOptions.extraScriptingDefines`로 해당 빌드에만 `POLISH_SIMULATION_BUILD`을 적용했다.
- 빌드 성공과 실패 결과, 실행 파일 위치 및 명령줄 예시를 Editor에서 표시하도록 구성했다.
# 2026-08-24 - 독립 시뮬레이터 창 모드

- `-simulate` 명령줄 실행 시 화면을 1280×720 창 모드로 전환하도록 적용했다.
- 일반 게임 빌드와 Unity Play Mode의 화면 설정은 변경하지 않는다.
# 2026-08-24 - Shipcollector 본체 위치 조정

- Stage 3 Shipcollector의 UI 위치를 유지하고 Spine 본체만 왼쪽으로 0.35 이동했다.
- 이동한 본체와 클릭 판정이 일치하도록 CircleCollider2D X 오프셋도 -0.35로 조정했다.
- 몬스터 크기와 세로 위치는 변경하지 않았다.
