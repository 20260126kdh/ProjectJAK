# 완료된 이전 작업

## 작업명

Assets/Scripts 폴더 구조 정리 1단계 — Managers 역할별 분류

## 목표

- `Assets/Scripts/Managers`에 혼재한 스크립트를 역할별 기존 영역으로 이동한다.
- 스크립트와 `.meta`를 함께 이동하여 Unity GUID 참조를 보존한다.
- 코드, 클래스명, namespace와 공개 API는 변경하지 않는다.

## 현재 상태

완료 — 2026-08-06

## 변경 범위

- `Assets/Scripts/Core`
- `Assets/Scripts/Audio`
- `Assets/Scripts/Battle/Managers`
- `Assets/Scripts/Cards/Managers`
- `Assets/Scripts/Player/Managers`
- `Assets/Scripts/Scenes/Managers`

## 검증

- 16개 스크립트와 각 `.meta` 파일의 이동을 확인했다.
- 이동 전후 스크립트 GUID가 모두 동일함을 확인했다.
- Unity 6000.0.78f1에서 16개 이동을 인식하고 `Assembly-CSharp`와 `Assembly-CSharp-Editor` 컴파일 및 도메인 재로드가 완료됐다.
- `Editor.log`에서 C# 컴파일 오류와 Missing Script 관련 오류가 없음을 확인했다.
- 이동한 스크립트 15개의 기존 Scene 직렬화 참조가 유지됨을 확인했다. `GameFlowManager`는 직렬화 참조가 없다.
- Play Mode 기능 회귀 테스트는 실행하지 않았다.

## 다음 작업

사용자가 `다음`이라고 하면 `Assets/Scripts/UI` 정리 범위를 분석한다. 승인 전에는 추가 이동을 진행하지 않는다.

# 현재 작업

## 작업명

카드 더미 이동 VFX

## 목표

1. 턴 종료 시 보존되지 않은 손패 카드가 버림 덱 위치로 이동한다.
2. 기존 `CardUI` 오브젝트를 이동하고 회전시킨다.
3. 드로우 파일이 비었을 때 버림 파일을 재셔플한다.
4. 버림 카드가 소용돌이에 흡수되는 연출을 재생한다.
5. 소용돌이가 드로우 덱 위치로 이동한다.
6. 물기둥과 비말을 재생한다.
7. 연출 후 실제 드로우 파일을 재생성한다.

## 구현 원칙

- 한 번에 한 단계만 진행한다.
- 사용자가 `다음`이라고 하기 전에는 다음 단계를 구현하지 않는다.
- 외부 애니메이션 플러그인과 DOTween을 사용하지 않는다.
- Unity 기본 Coroutine, Particle System과 Transform 이동을 우선 사용한다.
- `Assets/NamuFX`의 외부 원본 Prefab과 Material은 직접 수정하지 않는다.
- 프로젝트용 복제본은 `Assets/Art/VFX/Water`에서 관리한다.
- 기존 카드, 덱, 손패, 보존 카드 및 턴 종료 로직을 임의로 변경하지 않는다.

## 단계

1. `VFX_DiscardArrivalSplash` 튜닝
2. 버림 덱 위치 `Transform` 확보
3. VFX 테스트 재생 메서드 구현
4. 카드 1장 이동 연출
5. 여러 카드 순차 이동
6. 마지막 카드 도착 시 Splash 재생
7. 실제 턴 종료 로직과 연결
8. 재셔플 연출 설계
9. 재셔플 카드 이동
10. 물기둥 재생 및 드로우 파일 재생성
11. 전체 회귀 테스트

## 현재 단계

7단계 — 실제 턴 종료 로직과 연결

## 현재 상태

7단계 완료 — 사용자 Unity Play Mode 확인 완료

## 확인된 사실

- 프로젝트용 Prefab은 `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`에 존재한다.
- 외부 원본은 `Assets/NamuFX/StylizedWaterEffects/Prefabs/Water_Splash_A.prefab`이다.
- 프로젝트용 Prefab은 원본과 비교했을 때 루트 이름만 다르고 Particle System 설정은 동일하다.
- 루트와 여섯 자식 Particle System은 `Looping`이 꺼져 있고 `Play On Awake`가 켜져 있다.
- 프로젝트용 도착 Prefab은 세로 물기둥을 끄고 낮은 물보라·링·버블 중심으로 조정했다.
- Battle Scene의 `BattleCanvas/BattlePanel/Discard deck`은 `RectTransform`이고 손패 부모와 같은 `BattlePanel` 아래에 있다.
- `HandManager`의 `discardPileTarget`에 `Discard deck` RectTransform이 연결되어 있다.
- 카드 UI가 순차적으로 물빛 변환 후 버림 덱으로 가속 이동하는 테스트를 사용자가 확인했다.
- 이동 카드는 원본의 25% 크기를 유지하고 프로젝트용 물 VFX가 카드 위치를 따라간다.
- 보존 확정 시 선택한 카드 UI를 제외한 나머지 카드에 연출을 실행하도록 연결했다.
- 연출 완료 후 기존 `DiscardUnpreservedCards`, Jinx 초기화와 턴 전환을 실행한다.
- 실제 보존 확정 흐름에서 카드 이동, 버림 처리와 턴 전환이 정상 동작함을 사용자가 Play Mode에서 확인했다.
- 보존 유무와 손패 수별 모든 경계 조건의 개별 검증 여부는 확인되지 않았다.

## 현재 단계 수정 대상

- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 현재 단계 금지 범위

- `Assets/NamuFX` 외부 원본 수정
- 기존 보존 카드 판정과 버림 덱 데이터 규칙 변경
- `BattleScene`의 승인된 VFX Inspector 참조 외 Scene 수정
- 다른 Prefab 수정
- 다른 카드 더미 VFX 수정
- 재셔플 연출 구현

## 현재 단계 완료 조건

- 보존 카드는 이동 연출 대상에서 제외된다.
- 나머지 카드 연출이 끝난 후 실제 버림 덱 데이터가 갱신된다.
- 연출 중 중복 확정과 카드 선택이 차단된다.
- 보존 카드 없음과 버릴 카드 없음 경로가 정상 종료된다.
- 연출 후 적 턴이 한 번만 시작된다.
- Unity 컴파일 오류와 Missing Reference가 없다.

## 다음 작업

사용자가 `다음`이라고 하면 8단계 재셔플 연출을 설계한다. 승인 전에는 재셔플 코드와 VFX를 수정하지 않는다.

# 현재 작업 변경

## 작업명

뽑을 더미에서 손패로 순차 드로우 연출

## 목표

- 새로 뽑히는 카드가 `Decktodrawfrom`에서 각 카드의 최종 손패 위치로 이동한다.
- 여러 장은 오른쪽 손패 자리부터 왼쪽 방향으로 한 장씩 이동한다.
- 드로우 이동 시간은 `0.27초`로, 버림 카드 `0.54초`보다 2배 빠르게 재생한다.
- 기존 드로우, 덱 재생성, Jinx와 턴 시작 규칙을 보존한다.

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 수정 대상

- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 최초 4장과 새 턴 부족분이 오른쪽 자리부터 한 장씩 이동한다.
- 카드 효과 드로우에도 같은 연출이 적용된다.
- 연출 중 카드 선택과 턴 종료가 차단된다.
- Jinx 상태가 드로우 완료 후 정상 표시된다.
- Unity 컴파일 오류와 Missing Reference가 없다.

# 현재 작업 변경

## 작업명

보존 모드 UX 1단계 — ESC 취소

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 수정 대상

- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scripts/Core/PauseManager.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 보존 모드에서 ESC를 누르면 선택을 해제하고 일반 전투 상태로 복귀한다.
- 같은 ESC 입력으로 Pause 패널이 열리지 않는다.
- 기존 보존 카드 데이터와 턴 상태는 변경되지 않는다.

# 현재 작업 변경

## 작업명

보존 모드 UX 2단계 — 손패 제외 화면 70% 암전

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 완료 조건

- 보존 모드 진입 시 손패를 제외한 화면이 검정 70% 불투명도로 어두워진다.
- ESC 취소와 보존 확정 시 암전이 해제된다.
- 암전 UI가 카드 및 단축키 입력을 막지 않는다.
- 암전 Canvas는 Sorting Order `100`, 손패 Canvas는 `101`로 고정하여 다른 전투 UI와 무관하게 암전과 손패 표시 순서를 보장한다.
- 보존 확정 버튼도 Canvas Sorting Order `101`로 표시하여 암전에서 제외한다.
- 보존 확정 버튼은 보존 모드에서만 표시하고 시작 덱·일반 전투·취소·확정 상태에서는 숨긴다.

# 현재 작업 변경

## 작업명

보존 모드 UX 3단계 — 선택 카드 중앙 확대

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 완료 조건

- 보존 카드 선택 시 드로우와 동일한 `0.27초`에 화면 중앙으로 이동한다.
- 선택 카드는 원본의 `1.4배`로 확대되고 회전이 제거된다.
- 재선택·다른 카드 선택·ESC 취소·보존 확정 시 원래 손패 배치로 복원된다.

# 현재 작업 변경

## 작업명

버프/디버프 아이콘 Hover 설명 표시

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 수정 대상

- `Assets/Scripts/UI/EnemyStatusIconUI.cs`
- `Assets/Prefabs/Enemy/EnemyStatusIcon.prefab`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 플레이어와 적 상태 효과 아이콘에 마우스를 올리면 이름, 설명, 현재 수치와 지속 정보가 표시된다.
- 마우스가 벗어나거나 아이콘이 제거되면 툴팁이 숨겨진다.
- 툴팁이 화면 가장자리 밖으로 나가지 않는다.
- 툴팁은 다른 UI 입력을 가로막지 않는다.
- 버프·디버프 이름과 설명은 기획 정리표의 공식 한글 표기를 사용한다.
- 툴팁은 상태 아이콘과 동일한 `Pretendard-Regular SDF` 폰트와 Material을 사용한다.
- 사용자 확인 결과 툴팁 한글명과 폰트가 정상 표시되며 Console 오류가 없다.

# 현재 작업 변경

## 작업명

버림 덱 셔플 VFX 1단계 — 소용돌이와 난류 경로 테스트

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 버림 덱 위치에서 소용돌이 VFX가 먼저 재생된다.
- 이후 난류 VFX가 버림 덱에서 뽑을 덱 위치로 이동한다.
- 테스트 중 실제 버림 덱, 뽑을 덱과 손패 데이터는 변경되지 않는다.
- 기존 Base VFX 프리팹은 수정하지 않는다.
- 작은 물살 3개가 서로 다른 곡선으로 버림 덱에서 뽑을 덱까지 순차 이동한다.
- 이동 물살은 작은 물 입자 꼬리를 남기고, 마지막 도착 시 낮은 물결 Splash가 재생된다.
- 물 폭탄과 높은 물기둥은 사용하지 않는다.
- 기존 범용 물 VFX 테스트 대신 전용 `VFX_DeckShuffleTransfer` 프리팹을 사용한다.
- 응축 파동, 카드 잔상 교차 셔플, 이중 S자 물빛 리본, 도착 재구성의 네 단계로 재생한다.
- 전용 연출 테스트에서도 실제 덱 데이터는 변경하지 않는다.

## 2026-08-10 셔플 이동 VFX 물 표현 조정

- 전용 물 Trail Material을 이동 물줄기에 적용한다.
- 버림 덱에서 먼저 위로 솟은 후 뽑을 덱으로 흐르는 2단계 이동 경로를 사용한다.
- 실제 덱 셔플 로직 연결은 이번 단계에서 변경하지 않는다.
- Unity Play Mode 시각 확인이 필요하다.
- 상승 높이를 6배로 확대하고 초반 40% 동안 위로 상승한다.
- 손패 위쪽 높이를 유지하다가 뽑을 덱 가까이에서 하강한다.
- 이동 물줄기 주변에 작은 물방울을 함께 표시한다.
- 이동 시간을 0.65초로 조정하고 상승 높이를 7.5배로 확대한다.
- 이동 경로는 유지하고 물줄기, 파동, 물방울과 카드 잔상 크기를 1.35배로 확대한다.
- 전체 색상을 채도 낮은 딥 블루·청록 계열로 낮춰 어두운 전투 분위기에 맞춘다.

# 현재 작업 변경

## 작업명

카드 강화 망치 VFX 1단계 — 선택 카드 타격 테스트

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Art/VFX/CardUpgrade/GoldenAnvil.png`
- `Assets/Scripts/UI/VFX/CardUpgradeHammerVfx.cs`
- `Assets/Prefabs/VFX/VFX_CardUpgradeHammer.prefab`
- `Assets/Scripts/UI/UpgradePanelUI.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`

## 완료 조건

- 선택한 강화 카드 전체를 덮는 망치가 위에서 내려찍는다.
- 타격 순간 카드 눌림, 흔들림, 충격 섬광과 불꽃 파편을 표시한다.
- 테스트 중 실제 카드 강화 데이터는 변경하지 않는다.
- 망치 VFX는 강화 패널 UI 앞에 표시된다.
- 망치 손잡이가 위를 향한 상태에서 머리로 카드를 수직 타격한다.
- 강화 패널이 열려 있는 동안 전투 손패를 숨기고 패널이 닫히면 이전 상태로 복구한다.
- 망치 이미지는 타격면이 아래를 향하는 정면 구도로 교체한다.
- 0.32초 예비 동작 후 0.13초 급가속 낙하와 0.1초 히트 스톱으로 무게감을 표현한다.
- 참고 GIF처럼 선택 카드를 중앙에 1.45배 확대하고 나머지 화면을 어둡게 표시한다.
- 망치 타격 전 금색 방사광을 표시하고 타격 순간 카드보다 큰 주황·청록 원형 폭발을 재생한다.
- 연출 종료 후 선택 카드를 원래 Grid 부모, 순서, 위치와 크기로 복구한다.
- 망치는 참고 GIF처럼 두꺼운 사각 머리와 짧은 손잡이를 가진 해양 대장장이 망치로 교체한다.
- 일반 금속과 금빛 발광 버전을 겹쳐 오른쪽 진입 중 달아오르고 카드에 휘둘러 충돌하도록 한다.

## 2026-08-10 최초 강화 망치 VFX로 복구

- 최초 제작한 긴 손잡이와 닻 문양의 3/4 구도 망치 이미지를 복원한다.
- 중앙 카드 확대, 대형 폭발과 발광 망치 교차 연출을 제거한다.
- 강화 패널 진입 중 전투 손패 숨김 기능은 유지한다.
- 최초 망치 이미지는 유지하고 참고 GIF의 금빛 예고, 오른쪽 대형 진입, 히트 스톱, 방사형 불꽃과 잔불 타이밍을 적용한다.

# 현재 작업 변경

## 작업명

카드 강화 황금 모루 VFX

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 완료 조건

- 기존 망치 대신 정면형 황금 모루가 카드 정중앙 위에서 나타난다.
- 모루의 X 위치와 회전값을 고정하고 수직으로 급가속 낙하한다.
- 타격 순간 카드 눌림, 금빛 충격광과 절제된 금속 파편을 표시한다.
- 측면 진입, 주황 발광 복제본과 잔불은 표시하지 않는다.
- 강화 패널 표시 중 전투 손패 숨김과 F10 테스트 단축키는 유지한다.

# 현재 작업 변경

## 작업명

새 게임 스테이지 진행도 초기화 오류 수정

## 현재 상태

구현 완료 — Unity Play Mode 회귀 확인 필요

## 수정 대상

- `Assets/Scripts/Scenes/Managers/TitleManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 저장 후 종료로 타이틀에 돌아온 뒤 새 게임을 시작하면 Stage 1 일반 전투부터 시작한다.
- 이전 런의 스테이지, 전투 횟수, 진행 단계와 선택 보스 상태가 남지 않는다.
- 이어하기는 저장된 진행도를 기존과 동일하게 복원한다.

# 현재 작업 변경

## 작업명

게임 마우스 커서 이미지 교체

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Resources/Cursor/Nomal.png`
- `Assets/Resources/Cursor/Click.png`
- `Assets/Scripts/UI/GameCursorController.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 기본 상태에서 금색 테두리의 작살촉 커서를 표시한다.
- 좌클릭 중에는 작살촉 끝에 청록색 반짝임이 있는 커서로 전환한다.
- 두 이미지의 흰 배경은 투명 처리한다.
- 일반·클릭 상태 전환 중 실제 클릭 지점은 작살촉 끝에 고정한다.
- 일반·클릭 커서 본체는 5%씩 두 차례 축소해 최초 적용 크기의 90.25%로 표시한다.

# 현재 작업 변경

## 작업명

게임 화면 전환 페이드

## 현재 상태

구현 완료 — Unity 컴파일 및 Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scripts/UI/ScreenFadeController.cs`
- `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`
- `Assets/Scripts/Battle/Managers/BattleManager.cs`
- `Assets/Scripts/Core/PauseManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 최초 실행 시 0.64초 페이드 아웃 후 3프레임 페이드 인을 재생한다.
- 클래스 선택 후 전투 진입과 다음 전투 진입 시 0.32초 페이드 아웃, 0.24초 페이드 인을 재생한다.
- 타이틀 복귀 시 0.15초 페이드 아웃, 1.56초 페이드 인을 재생한다.
- 페이드 중 중복 입력을 차단하고 일시정지 상태에서도 전환을 완료한다.

# 현재 작업 변경

## 작업명

클래스 선택 카드 확장 UI 1단계

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scripts/UI/ClassSelectionCardUI.cs`
- `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`
- `Assets/Scripts/Scenes/ClassSelectShortcutController.cs`
- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 기본 화면에서 세 클래스 카드가 기존 위치에 표시된다.
- 선택 카드는 0.16초 동안 중앙으로 이동하며 확대된다.
- 나머지 두 카드는 좌우 끝으로 이동하며 축소된다.
- 선택 해제 시 모든 카드가 원래 위치와 크기로 복원된다.
- 프레임 PNG가 없으면 클래스별 임시 색상을 사용한다.
- 추후 Inspector에 9-slice Sprite를 연결하면 코드 변경 없이 프레임을 교체할 수 있다.
- 클릭, 숫자키 1·2·3, Enter와 Esc 입력이 기존 선택·확정·취소 규칙과 함께 동작한다.

# 현재 작업 변경

## 작업명

클래스 상세 정보 및 가로형 비율 조정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`
- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 선택 카드와 상세 패널이 기획서 기준 약 1.78:1 가로 비율로 표시된다.
- 캐릭터 영역과 정보 영역이 약 25:75 비율로 표시된다.
- 피지크 최대 체력 95, 전투 종료 회복량 7이 표시된다.
- 테크니션 최대 체력 75, 작살잡이 스택 4회 기준 패시브가 표시된다.
- 캡틴 최대 체력 75, 선원 2명·체력 12·공격 시 작살잡이 스택 2 패시브가 표시된다.
- 실제 전투 패시브 로직과 시작 데이터는 이번 단계에서 변경하지 않는다.

# 현재 작업 변경

## 작업명

클래스 상세 패시브 설명 영역 넘침 수정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 테크니션과 캡틴의 패시브 설명이 패시브 패널 밖으로 나오지 않는다.
- 긴 패시브 설명은 패널 안에서 줄바꿈되고 26~42 범위에서 자동으로 크기가 조정된다.
- 피지크, 테크니션, 캡틴 모두 동일한 텍스트 표시 규칙을 사용한다.
- 패시브 설명과 클래스 로그라인 영역이 겹치지 않는다.

# 현재 작업 변경

## 작업명

클래스 상세 패시브 칸 비율 재조정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 클래스명, 체력과 패시브 칸의 세로 높이가 서로 어울리게 표시된다.
- 패시브 칸은 오른쪽 가로 폭을 넓혀 긴 설명을 수용한다.
- 테크니션과 캡틴 패시브가 24~34 글자 크기 범위에서 읽을 수 있게 표시된다.
- 패시브 칸과 체력 칸, 로그라인 영역이 서로 겹치지 않는다.

# 현재 작업 변경

## 작업명

클래스 상세 패시브 설명 축약

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 피지크 문구는 기존 간결한 설명을 유지한다.
- 테크니션과 캡틴 패시브 설명은 효과 수치를 유지하면서 짧게 표시한다.
- 실제 전투 패시브 로직과 UI 비율은 변경하지 않는다.

# 현재 작업 변경

## 작업명

클래스 설명 칸 위치 및 글자 크기 조정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 클래스 설명 패널이 패시브 영역보다 아래에 분리되어 표시된다.
- 설명 텍스트가 설명 패널 중앙에 표시된다.
- 패시브와 설명 텍스트가 동일한 24~34 자동 글자 크기 규칙을 사용한다.
- 긴 클래스 설명이 설명 칸 안에서 줄바꿈되어 모두 표시된다.

# 현재 작업 변경

## 작업명

패시브·클래스 설명 글자 크기 방향 정정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 패시브 글자가 기존 클래스 설명과 동일한 고정 크기 50으로 표시된다.
- 클래스 설명 글자는 기존 고정 크기 50으로 복원된다.
- 패시브 텍스트 영역은 높이 150에서 줄바꿈된다.
- 아래로 내린 클래스 설명 패널 위치는 유지한다.

# 현재 작업 변경

## 작업명

클래스 상세 체력·패시브 표기 칸 확대

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 체력 표기 칸이 330×140 크기로 표시된다.
- 패시브 표기 칸이 680×140 크기로 표시된다.
- 내부 텍스트 영역도 확대된 칸에 맞춰 조정된다.
- 기존 글자 크기, 줄바꿈 규칙과 클래스 설명 위치는 유지한다.

# 현재 작업 변경

## 작업명

클래스 상세 정보 칸 비율 및 정렬 통일

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 클래스명과 설명 칸의 좌우 경계가 동일하게 정렬된다.
- 체력 칸 폭은 240으로 축소된다.
- 패시브 칸은 남은 가로 공간을 사용하는 약 731 폭으로 확대된다.
- 체력 칸과 패시브 칸 사이에 10의 간격이 유지된다.
- 각 칸의 내부 텍스트 좌우 여백은 15로 통일된다.

# 현재 작업 변경

## 작업명

체력·패시브 칸 가로 비율 추가 조정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 체력 칸 폭은 190으로 축소된다.
- 패시브 칸 폭은 약 781로 확대된다.
- 두 칸 사이 간격 10과 전체 행 폭 약 981은 유지된다.
- 클래스명 및 설명 칸과의 좌우 정렬은 유지된다.

# 현재 작업 변경

## 작업명

체력 줄바꿈 및 캡틴 패시브 넘침 수정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 피지크 체력 표기가 한 줄로 표시된다.
- 캡틴 패시브 설명이 패시브 칸 안에 표시된다.
- 체력과 패시브 글자 크기 50은 유지된다.
- 패널 크기와 전체 정렬은 변경되지 않는다.

# 현재 작업 변경

## 작업명

체력 및 피지크 패시브 텍스트 정렬

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 모든 클래스의 체력 텍스트가 칸 중앙에 정렬된다.
- 피지크 패시브 텍스트는 칸의 왼쪽·세로 중앙에 정렬된다.
- 테크니션과 캡틴 패시브 텍스트는 왼쪽·위쪽 정렬을 유지한다.
- 클래스 전환 시 이전 클래스의 세로 정렬이 남지 않는다.
- 폰트 크기와 패널 비율은 변경되지 않는다.

# 현재 작업 변경

## 작업명

클래스 설명 가독성 줄바꿈 조정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 피지크 설명이 계약 전후와 마지막 강조 문장 단위로 구분된다.
- 테크니션 설명이 기술 연마와 작살 공격 묘사 단위로 구분된다.
- 캡틴 설명이 유물 획득, 선원 소환과 선원 행동 단위로 구분된다.
- 설명 문구, 글자 크기 50과 설명 패널 비율은 유지된다.

# 현재 작업 변경

## 작업명

인게임 기준 클래스 설명 줄바꿈 재조정

## 현재 상태

구현 완료 — Unity Play Mode 재확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 인게임 표시 폭에서 조사나 어미가 한 글자만 다음 줄로 밀리지 않는다.
- 피지크 설명은 6개의 의미 단위 줄로 표시된다.
- 테크니션 설명은 4개의 의미 단위 줄로 표시된다.
- 캡틴 설명은 6개의 의미 단위 줄로 표시된다.
- 폰트 크기와 설명 패널 비율은 변경되지 않는다.

# 현재 작업 변경

## 작업명

클래스 설명 패널 및 텍스트 위치 재조정

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 설명 패널이 기존 위치보다 위로 20 이동한다.
- 설명 텍스트가 패널 내부에서 추가로 위로 15 이동한다.
- 마지막 설명 줄이 배경 칸 안에 표시된다.
- 클래스명, 체력, 패시브 칸과 버튼 위치는 유지된다.
- 문구, 줄바꿈, 글자 크기와 패널 크기는 변경되지 않는다.

# 현재 작업 변경

## 작업명

인게임 기본·클릭 커서 적용

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Resources/Cursor/Nomal.png`
- `Assets/Resources/Cursor/Click.png`
- `Assets/Scripts/UI/GameCursorController.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 게임 시작 시 Nomal 커서가 표시된다.
- 마우스 왼쪽 버튼을 누르는 동안 Click 커서가 표시된다.
- 버튼을 놓으면 Nomal 커서로 복귀한다.
- 씬 전환 이후에도 커서가 유지된다.
- 이미지의 화살촉 위치가 실제 클릭 지점과 일치한다.

# 현재 작업 변경

## 작업명

첫 일반 전투 손패 좌표 간헐 오류 수정

## 현재 상태

구현 완료 — Unity Play Mode 반복 확인 필요

## 수정 대상

- `Assets/Scripts/UI/StartingDeckUI.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 시작 덱 확인 후 전투 패널이 활성화된 상태에서 첫 손패 좌표를 계산한다.
- 첫 손패 4장이 항상 부채꼴 최종 위치와 회전값에 도착한다.
- 첫 일반 전투를 반복 진입해도 카드가 화면 하단의 중간 좌표에 남지 않는다.
- 일반 드로우, 다음 전투와 이어하기 손패 복원 흐름은 변경되지 않는다.

# 현재 작업 변경

## 작업명

클래스 설명 영역 하단 확장

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 설명 패널의 위쪽 경계는 유지된다.
- 설명 패널이 아래쪽으로 60 확장되어 약 417 높이가 된다.
- 설명 텍스트 영역이 아래쪽으로 60 확장되어 320 높이가 된다.
- 마지막 설명 줄이 배경 칸 안에 표시된다.
- 버튼과 다른 정보 칸의 위치는 변경되지 않는다.

# 현재 작업 변경

## 작업명

테크니션 로그라인 세로 중앙 정렬

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scripts/Scenes/Managers/ClassSelectManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 테크니션 로그라인 텍스트가 칸의 왼쪽·세로 중앙에 표시된다.
- 피지크와 캡틴 로그라인은 왼쪽·위쪽 정렬을 유지한다.
- 클래스 전환 시 이전 클래스의 정렬 상태가 남지 않는다.
- 문구, 줄바꿈, 글자 크기와 패널 크기는 변경되지 않는다.
