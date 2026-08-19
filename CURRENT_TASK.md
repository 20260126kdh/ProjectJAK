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
사망 UI 타이틀 씬 이름 수정

## 현재 상태

구현 및 이미지 연결 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/UI/PlayerDeathTransitionController.cs`
- `Assets/Art/UI/Death`
- `Assets/Prefabs/Player/Captain.prefab`
- `Assets/Prefabs/Player/Physique.prefab`
- `Assets/Prefabs/Player/Technician.prefab`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 사망 UI의 타이틀 복귀가 Build Scene List에 등록된 Main_TitleScene을 사용한다.
- 기존 페이드 이동과 직접 이동 대체 경로가 같은 씬 이름을 사용한다.
- 존재하지 않는 TitleScene 로드 오류가 발생하지 않는다.

# 현재 작업 변경
## 작업명
사망 UI 타이틀 복귀 보완

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/UI/PlayerDeathTransitionController.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 사망 UI의 타이틀 복귀 버튼을 누르면 중복 입력 없이 한 번만 이동을 시작한다.
- 기존 화면 페이드가 사용 가능하면 정상 페이드로 TitleScene에 이동한다.
- 기존 전환이 0.5초 이상 점유 중이면 TitleScene을 직접 불러온다.
- 타이틀 이동 전에 Time.timeScale이 1로 복구된다.

# 현재 작업 변경
## 작업명
플레이어 사망 작살 지퍼 전환 1단계

## 현재 상태

구현 완료 / Unity Play Mode 시각 확인 필요

## 수정 대상
- `Assets/Scripts/Player/PlayerCombat.cs`
- `Assets/Scripts/UI/PlayerDeathTransitionController.cs`
- `Assets/Scripts/UI/DeathZipperGraphic.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 불사 판정 후 플레이어 HP가 실제 0일 때만 사망 전환이 한 번 시작된다.
- 작살 앞쪽은 기존 전투 화면, 작살 뒤쪽은 시간차로 벌어진 붉은 영역이 표시된다.
- 작살 퇴장 후 화면 전체가 붉게 열린 뒤 사망 UI가 표시된다.
- 연출 중 전투 시간이 정지하고 UI 연출은 실시간 기준으로 계속 진행된다.
- 연출 시간, 색상, 작살 크기·높이와 교체 이미지가 PlayerCombat Inspector에 노출된다.

# 현재 작업 변경

## 작업명

Button_UI 미적용 에셋 연결

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 완료 조건

- 휴식 회복·강화 버튼에 Normal, Hover, Pressed 이미지를 적용한다.
- 타이틀과 전투 환경설정 버튼에 Setting 상태 이미지를 적용한다.
- 뽑을 덱과 버림 덱 버튼에 각 상태 이미지를 적용한다.
- 타이틀과 전투의 Master, BGM, SFX 슬라이더에 Case, Bar, NOV 이미지를 적용한다.
- 기존 버튼 이벤트와 슬라이더 값 저장 로직은 유지한다.

# 현재 작업 변경

## 작업명

튜토리얼 대상 역삼각형 위치와 적 범위 수정

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 완료 조건

- 대상의 모든 활성 렌더러 경계를 합산해 캐릭터 이미지 최상단을 계산한다.
- 역삼각형은 캐릭터 이미지 최상단보다 위에 표시된다.
- 적 대상 카드는 생성 순서상 첫 번째 생존 적에게만 역삼각형을 표시한다.
- 플레이어 대상 카드의 기존 표시 흐름은 유지한다.

# 현재 작업 변경

## 작업명

첫 전투 튜토리얼 조작 안내 개선

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 완료 조건

- 대화 중 스페이스바가 다음 버튼과 동일하게 한 단계만 진행한다.
- 지정 카드를 선택하면 적용 대상 머리 위에 빨간 역삼각형을 표시한다.
- 카드 사용 또는 선택 해제 시 대상 표시를 제거한다.
- 첫 스킬 사용 뒤 턴 종료/보존 버튼, 보존 카드, 보존 확정 버튼 순서로 강조한다.
- 정해진 보존 순서를 건너뛸 수 없다.

# 현재 작업 변경

## 작업명

클래스별 첫 전투 튜토리얼 3단계 — 보존과 마무리

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 완료 조건

- 첫 번째 스킬 사용 뒤 공격·방어 카드 사용 횟수와 보존 설명을 표시한다.
- 클래스별 두 번째 스킬 카드만 보존 대상으로 선택할 수 있다.
- 보존 확정 뒤 기존 버림 연출, 적 턴과 다음 턴 드로우를 진행한다.
- 다음 플레이어 턴에 일반 초상화 대사 뒤 클래스별 변화 초상화와 마무리 대사를 표시한다.
- 마무리 대사 종료 뒤 모든 튜토리얼 입력 제한을 해제한다.

# 현재 작업 변경

## 작업명

클래스별 첫 전투 튜토리얼 2단계 — 카드 강조와 행동 조건

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 완료 조건

- 도입 대사 뒤 투창만 빨간 테두리로 1초 간격 점멸하고 사용할 수 있다.
- 투창 사용 뒤 회피 설명과 강조가 표시되며 회피만 사용할 수 있다.
- 회피 사용 뒤 클래스별 첫 번째 스킬 설명과 강조가 표시된다.
- 지정 카드 외 손패, 턴 종료, 보존, 덱 보기, F10과 ESC 퍼즈 입력은 차단된다.
- 지정 카드가 실제로 사용 완료된 뒤에만 다음 단계로 진행한다.
- 각 단계 종료 시 카드 강조 테두리를 제거한다.

# 현재 작업 변경

## 작업명

클래스별 첫 전투 튜토리얼 — 첫 손패 고정

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 완료 조건

- 튜토리얼 첫 손패는 투창 1장, 회피 1장, 선택 클래스의 시작 스킬 카드 2장으로 구성된다.
- 손패 순서는 투창, 회피, 스킬 1, 스킬 2 순서다.
- 지정 카드 네 장을 제외한 시작 덱 여섯 장만 섞여 드로우 파일에 남는다.
- 기존 첫 손패 드로우 이동 연출과 카드 드로우 SFX를 유지한다.
- 이어하기와 다음 전투의 기존 덱 복원·셔플 규칙은 변경하지 않는다.

# 현재 작업 변경

## 작업명

클래스별 첫 전투 튜토리얼 1단계 — 기반 UI와 도입 대사

## 현재 상태

구현 완료 — Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Art/UI/Tutorial`
- `Assets/Scripts/Tutorial/TutorialDialogueData.cs`
- `Assets/Scripts/Tutorial/TutorialManager.cs`
- `Assets/Scripts/UI/StartingDeckUI.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 새 게임 시작 덱 확인 후 선택 클래스에 맞는 초상화와 도입 대사가 표시된다.
- 화면은 검은색 40%로 암전되고 대사는 흰색 Pretendard 폰트를 사용한다.
- 다음 버튼은 제공된 Normal, Hover, Pressed 이미지를 사용한다.
- 대화 중 다른 전투 UI 입력은 차단된다.
- 이어하기에서는 튜토리얼이 다시 시작되지 않는다.
- 1단계 대사가 끝나면 전투 입력이 복구된다.

# 현재 작업 변경

## 작업명

퍼즈 환경 설정 화면 위로 손패가 표시되는 문제 수정

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 퍼즈 메뉴에서 환경 설정을 열면 손패가 환경 설정 화면 위로 표시되지 않는다.
- 환경 설정의 버튼, 슬라이더와 토글 입력이 정상 동작한다.
- ESC로 환경 설정에서 퍼즈 메뉴로 돌아가고 다시 게임을 재개할 수 있다.
- 보존 모드용 손패 Canvas 정렬 순서와 동작은 유지한다.

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

전투 SFX 1단계 — 공격·피격·방어 판정

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Audio/SFX`
- `Assets/Scripts/Audio/SFXManager.cs`
- `Assets/Scripts/Cards/CardEffectExecutor.cs`
- `Assets/Scripts/Player/PlayerCombat.cs`
- `Assets/Scenes/PlayScene/Main_TitleScene.unity`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 플레이어와 선원 공격은 `attack` 재생 0.1초 뒤 판정 피격음을 재생한다.
- 공격으로 방어도가 정확히 0이 되더라도 실제 HP 피해가 없으면 완전 방어로 판정해 `blocked_hit`을 재생한다.
- 방어도가 0이 된 뒤 실제 HP 피해가 발생하기 시작한 공격부터 기존 체력 피해 판정음을 재생한다.
- 작살 보유 적도 실제 HP 피해가 발생한 공격부터 `harpoonstack_hit`을 재생한다.
- 일반 실제 체력 피해 20 미만은 `hit ver.1`, 20 이상은 `hit ver.2`를 재생한다.
- 선원 공격과 선원 피격은 피해량과 관계없이 `hit ver.1`을 재생한다.
- 중독과 자해에는 공격 SFX를 재생하지 않는다.

# 현재 작업 변경

## 작업명

전투 SFX 2단계 — 카드 복합 효과 재생 순서

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 완료 조건

- 카드 효과 로직은 기존 데이터 순서대로 즉시 처리하고 SFX 순서만 별도로 제어한다.
- 공격/피격 시작 0.3초 후 방어도, 방어도 시작 0.6초 후 버프를 재생한다.
- 버프 시작 0.6초 후 디버프, 디버프 시작 0.7초 후 작살 스택음을 재생한다.
- 앞 카테고리가 없는 경우 첫 번째 존재하는 효과음은 즉시 재생한다.
- 서로 다른 버프·디버프가 2종 이상이면 0.2초 간격으로 각각 최대 2회 재생한다.
- 같은 상태를 여러 대상에게 부여해도 해당 종류의 효과음은 한 번만 계산한다.
- `Exit`은 버프음 대상에서 제외한다.
- 실제 획득 방어도와 실제 추가 작살 스택이 0이면 해당 효과음을 재생하지 않는다.

# 현재 작업 변경

## 작업명

전투 SFX 3단계 — 카드·UI·승리 효과음

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 완료 조건

- 실제 손패 드로우 이동을 시작하는 카드마다 `card_draw`를 한 번 재생한다.
- 손패·보상·강화 카드에 마우스가 진입할 때 `card_hover`를 한 번 재생한다.
- 휴식 화면에서 강화 패널이 정상적으로 열릴 때 `card_upgrade`를 재생한다.
- 휴식 회복 버튼 처리가 성공할 때만 `heal`을 재생하고 전투 후 자동 회복에는 재생하지 않는다.
- 마지막 적 처치 후 보상 패널이 활성화될 때 `victory`를 재생한다.
- 마우스 버튼 또는 키보드 키를 누른 프레임마다 `click`을 한 번 재생한다.
- 기존 드로우 속도, 강화, 회복량과 보상 생성 로직은 변경하지 않는다.

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

전투 SFX 4단계 적 행동 효과음

## 현재 상태

구현 완료 — Unity Play Mode 청음 확인 필요

## 수정 대상

- `Assets/Scripts/Audio/SFXManager.cs`
- `Assets/Scripts/Pattern/EnemyPatternController.cs`
- `Assets/Scripts/Enemy/Enemy.cs`
- `Assets/Scripts/Enemy/HRevelationController.cs`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 성공한 적 패턴 행동의 방어도, 버프와 디버프 효과음만 재생한다.
- 공격 이후 효과는 공격/피격 시작으로부터 0.3초 뒤 순서 재생을 시작한다.
- 서로 다른 상태 효과가 여러 개면 같은 종류의 효과음을 0.2초 간격으로 최대 2회 재생한다.
- 독오름, 불어터진 피부, 모독받은 후광과 심해의 속삭임에도 디버프음을 재생한다.
- 적 패턴의 실제 효과 실행 순서와 전투 수치는 변경하지 않는다.

# 현재 작업 변경

## 작업명

피지크 공격 카드 즉시 흡혈 수정

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Scripts/Cards/CardEffectExecutor.cs`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- `PHY_ATK_005`는 해당 카드가 실제로 입힌 피해만큼 즉시 회복한다.
- `PHY_ATK_007`은 모든 적에게 실제로 입힌 피해의 합계만큼 즉시 회복한다.
- 두 카드 사용 후 흡혈 상태가 남아 다음 공격에서 회복하지 않는다.
- `PHY_SKL_005`의 이번 턴 흡혈 상태는 기존대로 유지된다.
- 방어도에 막힌 피해와 적의 남은 체력을 초과한 피해는 회복량에 포함하지 않는다.

# 현재 작업 변경

## 작업명

상태 효과 enum 직렬화 번호 복구

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Scripts/Battle/StatusEffectType.cs`
- `Assets/Data/ScriptableObjects/Cards/PHY_SKL_006.asset`
- `Assets/Data/ScriptableObjects/Cards/TEC_SKL_006.asset`
- `Assets/Data/ScriptableObjects/Cards/CAP_SKL_002.asset`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- `CAP_ATK_003` 영혼 파괴가 악마의 힘이 아닌 약화 1을 부여한다.
- 기존 약화·취약 카드가 CSV에 기록된 상태 효과를 부여한다.
- 소멸 카드 3종의 `Exit` 효과가 유지된다.
- 악마의 힘과 힘 감소 효과가 기존 로직대로 동작한다.
- 이후 enum 선언 순서 변경에도 기존 직렬화 번호가 변하지 않는다.

# 현재 작업 변경

## 작업명

클래스 기획서 기준 시작 스킬 에셋 수정

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Data/ScriptableObjects/Cards/TEC_SKL_002.asset`
- `Assets/Data/ScriptableObjects/Cards/CAP_SKL_001.asset`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 테크니션 시작 스킬 `엄폐`가 방어도 12를 획득한다.
- 엄폐 카드 설명에도 방어도 12가 표시된다.
- 캡틴 시작 스킬 `CAP_SKL_001`의 이름은 `길잡이`로 유지한다.
- 길잡이의 피해 10과 전체 약화·취약 1 효과는 유지된다.
- CSV와 시작 덱 구성은 변경하지 않는다.

# 현재 작업 변경

## 작업명

악마의 힘 상태 아이콘 연결

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Data/StatusEffectIconDatabase.asset`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- `DevilPower = 24` 상태에 `DevilPower.png` 아이콘이 표시된다.
- 약화·취약을 포함한 기존 상태 효과 아이콘 연결은 유지된다.
- 상태 효과 아이콘 데이터베이스에 중복 번호가 없다.

# 현재 작업 변경

## 작업명

악마와의 거래 강화 설명 표기 수정

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Scripts/Cards/CardUpgradeUtility.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- `PHY_SKL_002` 강화 설명에 체력 손실 7과 힘 획득 7이 표시된다.
- 강화 후 실제 체력 손실과 힘 획득이 모두 7이 된다.
- 다른 카드의 강화 수치와 설명은 변경하지 않는다.
- CSV와 카드 ScriptableObject는 변경하지 않는다.

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
# 현재 작업 변경
## 작업명
튜토리얼 대사 글자 크기 및 중앙 배치 조정

## 현재 상태

구현 완료 / Unity Play Mode 시각 확인 필요

## 수정 대상
- `Assets/Scripts/Tutorial/TutorialManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 튜토리얼 대사 글자가 기존 36pt보다 7pt 큰 43pt로 표시된다.
- 튜토리얼 대사 텍스트 영역이 대사 프레임의 세로 중앙에 배치된다.
- 초상화와 다음 버튼의 위치는 변경되지 않는다.

# 현재 작업 변경
## 작업명
전투 덱 보기 버튼 상태 전환 수정

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 우측 상단 덱 보기 버튼을 누르는 동안 Pressed 이미지가 표시된다.
- 덱 보기 버튼을 누른 뒤 다시 마우스를 올리면 Hover 이미지가 표시된다.
- 덱 패널 노출과 카드 목록 동작은 변경되지 않는다.

# 현재 작업 변경
## 작업명
캡틴 튜토리얼 배신 및 선원 1 지정

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Tutorial/TutorialManager.cs`
- `Assets/Scripts/Battle/Managers/BattleManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 캡틴 튜토리얼 스킬 사용 단계에서 CAP_SKL_002 배신을 안내한다.
- 배신 카드 선택 시 선원 1 머리 위에 대상 표시가 출력된다.
- 선원 1만 튜토리얼 지정 대상으로 사용할 수 있다.
- 배신 사용 후 보존 단계에서는 남아 있는 CAP_SKL_001 길잡이를 지정한다.
- 다른 클래스의 튜토리얼 스킬 단계는 변경되지 않는다.

# 현재 작업 변경
## 작업명
캡틴 튜토리얼 선원 대상 마커 위치 수정

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Tutorial/TutorialTargetMarker.cs`
- `Assets/Scripts/Tutorial/TutorialManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- CAP_SKL_002 선택 시 역삼각형이 선원 1 이미지의 가로 중앙 위에 표시된다.
- 선원 이동 및 재배치 시 마커가 선원 이미지 머리 위를 계속 추적한다.
- 플레이어와 적 대상 마커의 기존 가로 위치는 변경되지 않는다.

# 현재 작업 변경
## 작업명
선원 PNG 기준 튜토리얼 마커 배치

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Tutorial/TutorialTargetMarker.cs`
- `Assets/Scripts/Tutorial/TutorialManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 선원 대상 마커가 CrewPrefab/Visual의 SpriteRenderer 경계를 직접 기준으로 사용한다.
- 마커가 선원 PNG의 가로 중앙 및 최상단 위에 표시된다.
- 다른 Renderer와 루트 오프셋은 선원 마커 위치 계산에 영향을 주지 않는다.
- 적과 플레이어 대상 마커는 기존 위치 계산을 유지한다.

# 현재 작업 변경
## 작업명
캡틴 및 선원 튜토리얼 마커 미세 조정

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Tutorial/TutorialTargetMarker.cs`
- `Assets/Scripts/Tutorial/TutorialManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 캡틴 대상 마커가 기존 위치보다 오른쪽으로 0.25 이동한다.
- 선원 대상 마커가 PNG 기준 위치보다 왼쪽과 아래로 각각 0.20 이동한다.
- 적 대상 마커 위치는 변경되지 않는다.
- 대상 이동 및 재배치 중에도 오프셋이 유지된다.

# 현재 작업 변경
## 작업명
아스피도켈 HP 0 침몰 사망 및 보상 전환 수정

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Enemy/EnemySpawner.cs`
- `Assets/Scripts/Battle/Managers/TurnManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 아스피도켈이 HP 0에서 침몰 상태로 진입한 뒤 적 행동 목록에 유지된다.
- 다음 적 턴에 침몰 피해를 실행하고 기존 Die 흐름으로 사망한다.
- 아스피도켈이 마지막 적이면 사망 통지 후 보상 화면으로 전환된다.
- 침몰 상태가 아닌 HP 0 적은 기존처럼 행동 목록에서 제외된다.

# 현재 작업 변경
## 작업명
일시정지 중 전투 카드 선택 및 사용 차단

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scripts/Battle/Managers/BattleManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 일시정지 중 손패 카드를 새로 선택할 수 없다.
- 일시정지 전에 선택한 카드도 적, 플레이어, 선원에게 사용할 수 없다.
- 환경설정 및 전투 포기 확인 화면에서도 전투 카드 입력이 차단된다.
- 게임 재개 후 기존 카드 선택과 사용 흐름이 정상 동작한다.

# 현재 작업 변경
## 작업명
클래스별 튜토리얼 엑셀 스크립트 및 지정 색상 반영

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Tutorial/TutorialManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 캡틴, 테크니션, 피지크 튜토리얼 대사가 각 엑셀 원문과 일치한다.
- 엑셀의 문장부호, 띄어쓰기와 줄바꿈이 그대로 표시된다.
- 세 클래스의 마지막 17번 대사 전체만 #ff0000으로 표시된다.
- 기존 카드 지정, 대상 제한, 보존 진행 로직은 유지된다.

# 현재 작업 변경
## 작업명
선원 Collider 중첩 클릭 대상 판정 수정

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Player/CrewClickHandler.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 선원 Collider가 겹친 영역에서도 클릭 지점에 가장 가까운 선원만 선택된다.
- 거리 판정은 각 선원의 Visual SpriteRenderer 중심을 사용한다.
- 선원 1 PNG 위를 클릭하면 선원 2가 대신 선택되지 않는다.
- 단독 선원 클릭과 기존 카드 사용 흐름은 유지된다.

# 현재 작업 변경
## 작업명
적 상단 UI 가시성 배경 칸 추가

## 현재 상태

구현 완료 / Unity Play Mode 시각 확인 필요

## 수정 대상
- `Assets/Scripts/UI/EnemyStatusUI.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 모든 적의 `TopBar` 280×55 영역 뒤에 300×65 크기의 중간 회색 배경 칸을 표시한다.
- 기존 상단 UI 위치는 유지하고 배경만 좌우 10, 위아래 5만큼 확장한다.
- 배경 칸은 상태 아이콘, 작살, Intent와 방어도 UI만 감싼다.
- 적 체력바에는 배경 칸을 적용하지 않는다.
- 배경 칸은 Raycast를 받지 않아 상태 아이콘 Hover와 전투 입력을 방해하지 않는다.
- 일반 적과 보스를 포함한 기존 적 Prefab에 공통 적용한다.

# 현재 작업 변경
## 작업명
메인 타이틀 배경 영상 적용

## 현재 상태

구현 완료 / Unity Play Mode 시각 확인 필요

## 수정 대상
- `Assets/Resources/Video/Main_Title.mp4`
- `Assets/Scripts/Scenes/Managers/TitleManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- `Main_Title.mp4`가 메인 타이틀의 전체 화면 배경으로 자연스럽게 반복 재생된다.
- 영상은 기존 타이틀 버튼과 설정 UI보다 뒤에 표시된다.
- 화면 비율이 달라도 빈 여백 없이 화면을 채운다.
- 영상 오디오는 재생하지 않고 기존 `Main_Title.wav` BGM을 유지한다.
- 영상은 H.264 Constrained Baseline, 고정 24fps와 BT.709 색 공간으로 인코딩한다.
- Unity 임포트 시 프레임 타임스탬프와 Color Primaries 경고가 발생하지 않는다.
- 원본 영상의 마지막 0.5초와 시작 0.5초를 교차 전환해 반복 경계의 화면 단절을 줄인다.
- 교차 전환을 포함한 최종 재생 길이는 3.5초로 유지한다.
- 새 게임, 이어하기, 설정과 종료 버튼의 기존 입력 흐름을 유지한다.

# 현재 작업 변경
## 작업명
적이 부여하는 동일 디버프의 지속 턴 중첩

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상
- `Assets/Scripts/Battle/StatusEffectHandler.cs`
- `Assets/Scripts/Pattern/EnemyPatternExecutor.cs`
- `Assets/Scripts/Enemy/HRevelationController.cs`
- `Assets/Scripts/Enemy/Enemy.cs`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 여러 적이 플레이어에게 같은 지속형 디버프를 부여하면 디버프 수치는 처음 적용된 값을 유지한다.
- 같은 디버프의 지속 턴은 각 적용량을 합산한다.
- 약화 수치 1·1턴을 두 번 받으면 약화 수치 1·2턴이 된다.
- 중독, 영구 효과, 버프와 플레이어가 적에게 부여하는 상태 효과의 기존 중첩 규칙은 유지한다.

# 현재 작업 변경
## 작업명
3줄 이상 튜토리얼 대사 칸 높이 확장

## 현재 상태

구현 완료 / Unity Play Mode 시각 확인 필요

## 수정 대상
- `Assets/Scripts/Tutorial/TutorialManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 실제 표시 줄 수가 3줄 이상이면 대사 프레임과 텍스트 영역의 높이가 60만큼 확장된다.
- 1~2줄 대사로 전환하면 기존 높이로 자동 복구된다.
- 확장 여부와 관계없이 대사는 프레임 중앙에 표시된다.
- 초상화와 다음 버튼의 위치는 변경되지 않는다.
# 현재 작업 변경
## 작업명
Rest 버튼 휴식 이미지 연출

## 현재 상태

구현 완료 / Unity Play Mode 시각 확인 필요

## 수정 범위

- `Assets/Art/UI/Rest`
- `Assets/Scripts/UI/RestPanelUI.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 휴식 단계 진입 즉시 `HandCardParent`를 비활성화하고 다음 전투 시작 직전에 복구한다.
- 휴식 패널은 Sorting Order 150으로 핸드 Canvas(101)보다 위에 표시한다.
- 휴식 패널의 버튼 화면은 현재 직업의 `*_Rest` 이미지로 전투 UI와 핸드를 가린다.
- Rest와 Upgrade 글씨는 각 버튼 이미지 아래에 표시한다.
- 휴식 화면 최초 진입 시 직업별 `*_Rest` 이미지와 버튼을 표시한다.
- Rest 버튼을 누르면 버튼을 숨기고 현재 직업의 `*_Rest` 이미지를 표시한다.
- 1초 후 0.3초 페이드 아웃으로 화면을 검게 가리고 `*_Rest_End` 이미지로 교체한 뒤, 0.3초 페이드 인한다.
- 페이드 인 완료 후 0.5초 뒤 사용 가능한 버튼을 다시 표시한다.
- 강화 버튼과 강화 취소는 휴식 이미지 연출을 실행하지 않는다.
- 다음 휴식 단계 진입 시 휴식 이미지를 숨긴 상태로 초기화한다.
- Rest와 Upgrade 버튼은 기존 중앙 위치보다 330px 아래에 표시한다.
- Rest와 Upgrade 라벨은 Font Weight 700의 흰색 글씨로 표시한다.
# 현재 작업 변경
## 작업명
전투 턴 전환 두루마리 배너

## 현재 상태

구현 완료 / Unity Play Mode 시각 확인 필요

## 수정 범위

- `Assets/Scripts/UI/TurnBannerUI.cs`
- `Assets/Scripts/Battle/Managers/TurnManager.cs`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 적 턴과 플레이어 턴 시작 시 화면 중앙에 각각 `적 턴`, `내 턴` 문구를 표시한다.
- 배너는 좌측에서 우측으로 0.4초 동안 펼쳐지고 0.5초 유지 후 0.4초 동안 접힌다.
- 배너 재생 중 마우스와 키보드 전투 입력을 차단한다.
- 적 턴 행동은 적 턴 배너가 완전히 종료된 뒤 실행한다.
- UI 생성에 실패해도 기존 턴 흐름은 중단되지 않는다.
# 현재 작업 변경
## 작업명
Editor 전용 플레이어 사망 테스트 단축키

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 범위

- `Assets/Scripts/Scenes/BattleShortcutController.cs`
- `Assets/Scripts/Player/PlayerCombat.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- Unity Editor 전투 중 F9를 누르면 플레이어 HP가 즉시 0이 된다.
- 방어도, 선원, 무감각과 불사 효과를 우회하고 기존 사망 전환을 실행한다.
- 사망 연출 재생 중에는 중복 실행하지 않는다.
- 일시정지와 주요 전투 외 패널이 열린 상태에서는 실행하지 않는다.
- 빌드 버전에는 테스트 단축키를 포함하지 않는다.
# 현재 작업 변경
## 작업명
전체 카드 클릭 대상 검증

## 현재 상태

구현 완료 / Unity Play Mode 카드별 확인 필요

## 수정 범위

- `Assets/Scripts/Battle/Managers/BattleManager.cs`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 카드 53장의 CSV와 CardData 대상 설정은 기존 값을 유지한다.
- Enemy, AllEnemies, RandomEnemy 효과가 하나라도 있는 카드는 적 클릭으로만 사용한다.
- Self 및 직접 선택이 필요 없는 아군 전체 효과 카드는 플레이어 클릭으로만 사용한다.
- 단일 선원 희생 카드는 선원 클릭으로만 사용한다.
- 잘못된 대상 클릭 시 카드, 사용 횟수와 상태 효과를 소모하지 않는다.
# 현재 작업 변경
## 작업명
사망 UI 재배치 및 사망 캐릭터 이미지 연결 준비

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Scripts/UI/PlayerDeathTransitionController.cs`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 사망 제목, 최종 진행 스테이지, 타이틀 복귀 버튼이 기획 시안의 세로 배치로 표시된다.
- CAP/PHY/TEC 사망 이미지가 각 클래스 Prefab에 연결되어 화면 왼쪽 아래에 표시된다.
- 사망 문구와 최종 스테이지 칸은 `Death_UI.png` 프레임을 사용한다.
- 프레임 위 글자는 밝은 아이보리색과 검은 외곽선으로 선명하게 표시된다.
- 이미지 참조가 누락된 상태에서도 사망 UI가 오류 없이 표시된다.

# 현재 작업 변경
## 작업명
플레이어 충돌형 사망 전환 연출

## 현재 상태

구현 완료 / Unity Play Mode 시각 확인 필요

## 수정 대상

- `Assets/Scripts/Player/PlayerCombat.cs`
- `Assets/Scripts/UI/PlayerDeathTransitionController.cs`
- `Assets/Prefabs/Player/Captain.prefab`
- `Assets/Prefabs/Player/Physique.prefab`
- `Assets/Prefabs/Player/Technician.prefab`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 신규 작살 이미지가 화면 우측 위에서 0.3초 동안 이동하여 플레이어 위치에 정지한다.
- 작살 충돌 지점에서 붉은 원이 퍼져 화면 전체를 덮는다.
- 붉은 확산 시간은 0.68초를 사용한다.
- 작살 크기는 560×280으로 표시한다.
- 비행 중 현재 클래스의 공격 궤적 색상으로 꼬리를 표시한다.
- 도착 시 클래스별 공격 충돌 섬광을 표시하고 작살과 꼬리를 0.12초 안에 제거한다.
- 자동 분할 Sprite Pivot과 작살 PNG의 투명 여백을 보정해 작살촉, 충돌 섬광과 붉은 확산 중심을 일치시킨다.
- 충돌 섬광은 `HarpoonTipPoint`의 자식으로, 꼬리는 작살의 자식으로 배치해 독립 좌표 계산을 사용하지 않는다.
- 충돌 임팩트는 별도 위치 보정 없이 `HarpoonTipPoint` 로컬 원점에 고정하고, 붉은 확산도 같은 지점을 사용한다.
- 비행 중 클래스 트레일은 원본 PNG의 가시 중심 편차를 반영해 작살 로컬 Y `+82` 지점에 배치한다.
- 클래스 트레일의 오른쪽 끝은 `GetHarpoonTipInset` 값을 사용해 실제 작살촉과 같은 X 지점에 고정한다.
- 충돌과 확산 이후 기존 사망 UI가 표시된다.
- 타이틀 복귀 버튼은 메인 타이틀의 게임 시작 버튼과 같은 상태 이미지를 사용한다.
# 현재 작업 변경
## 작업명
게임 클리어 엔딩 영상 재생

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 범위

- `Assets/Resources/Video/Ending.mp4`
- `Assets/Scripts/Scenes/GameClearController.cs`
- `Assets/Scripts/Battle/Managers/BattleManager.cs`
- `Assets/Scenes/PlayScene/GameClearScene.unity`
- `ProjectSettings/EditorBuildSettings.asset`
- `CURRENT_TASK.md`
- `Docs/GAME_DESIGN.md`
- `Docs/DEVLOG.md`

## 완료 조건

- Stage 3 최종 보스 아리엘 처치 시 보상 화면 없이 `GameClearScene`으로 이동한다.
- 엔딩 영상은 전체 화면으로 한 번 재생하며 기존 전투 BGM은 정지한다.
- 영상 재생이 끝나면 페이드 후 `Main_TitleScene`으로 이동한다.
- Stage 1·2 보스와 모르바엘의 기존 보상 흐름은 유지한다.
- 엔딩 영상은 H.264 Baseline, 24fps, BT.709 규격을 사용한다.
# 현재 작업 변경
## 작업명
클래스 선택 상세 창 전체 화면 확장

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 범위

- `Assets/Scenes/PlayScene/Class_SelectScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 클래스 선택 카드를 누르면 상세 패널이 Canvas 전체 화면을 채운다.
- 상세 패널 내부 UI는 원본 `1580×880` 비율을 유지하는 `DetailContent` 아래에서 동일 배율로 확대한다.
- 선택한 클래스 버튼은 `1920×1080`으로 확대되어 상세 UI의 전체 화면 배경으로 표시된다.
- 상세 화면에서는 선택하지 않은 두 클래스 버튼을 숨기고, 뒤로 가면 원래 계층과 배치를 복원한다.
- 선택 카드는 `0.36초` 동안 확대되고, 완료 후 `0.2초`를 더 기다린 뒤 상세 내용과 확정 버튼을 표시한다.
- 상세 화면 왼쪽 이미지 칸에는 선택 클래스별 CAP, TEC, PHY 영상을 음소거 반복 재생한다.
- 클래스 영상은 원본 비율을 유지하며 왼쪽 칸을 채우도록 중앙 기준으로 잘라 표시한다.
- 왼쪽 선명 영상은 RectTransform 이동 대신 `RawImage.uvRect`로 원본 표시 구간을 직접 선택한다.
- PHY는 원본 가로 60%, TEC는 59%, CAP은 50% 지점을 표시 중심으로 사용해 얼굴 구도를 조정한다.
- 왼쪽 영상 칸은 스텐실 마스크를 사용해 확대 영상이 네모 칸 밖으로 노출되지 않게 한다.
- 같은 클래스 영상을 전체 화면에 채워 재생하고 블러와 반투명 검정 오버레이를 적용해 상세 UI 배경으로 사용한다.
- PNG 프레임 대신 영상 가장자리에 Unity UI `Image` 4개로 구성한 4px 테두리를 표시한다.
- 테두리는 클래스 선택 버튼 색상의 보색을 사용하며 영상과 마스크의 `340×750` 크기를 유지한다.
- Back, Confirm 버튼과 ESC, Enter 단축키 동작은 유지한다.

# 현재 작업 변경
## 작업명
이어하기 손패 강화 카드 복원 수정

## 현재 상태

구현 완료 / Unity Play Mode 확인 필요

## 수정 범위

- `Assets/Scripts/Cards/Managers/DackManager.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 이어하기 덱 복원 시 저장된 카드 순서를 그대로 유지한다.
- 같은 카드가 여러 장이고 일부만 강화된 경우에도 저장된 손패 인덱스가 정확한 강화 카드 인스턴스를 가리킨다.
- 신규 게임과 카드 획득 시의 기존 이름 정렬은 유지한다.
- 저장 스키마와 기존 저장 파일 호환성을 유지한다.

# 현재 작업 변경
## 작업명
몬스터 Spine 소스 적용 1단계 - 원본 Import

## 현재 상태

원본 Import 및 자동 생성 에셋 확인 완료 / 몬스터 프리팹 연결 전

## 수정 범위

- `Assets/Art/Enemy/Spine`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- ZIP의 PNG, SKEL, ATLAS 파일 17세트를 적 전용 Spine 폴더에 배치한다.
- Atlas 확장자를 Unity Spine Importer가 인식하는 `.atlas.txt`로 사용한다.
- Skeleton 확장자를 Unity Spine Importer가 인식하는 `.skel.bytes`로 사용한다.
- 모든 SKEL 데이터가 프로젝트 Spine Runtime 4.3과 호환되는지 확인한다.
- Atlas Asset, Material, SkeletonData Asset이 각각 17개 생성됐는지 확인한다.
- 이 단계에서는 기존 몬스터 프리팹을 변경하지 않는다.

# 현재 작업 변경
## 작업명
몬스터 Spine 소스 적용 2단계 - 1스테이지 일반 몬스터

## 현재 상태

프리팹 9개 Spine 연결 완료 / Unity Play Mode 시각 확인 필요

## 수정 범위

- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Goby1.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Goby2.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Mermaid.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Mimic1.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Mimic2.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/SeaCrab1.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/SeaCrab2.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Thief1.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Thief2.prefab`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 1스테이지 일반 몬스터 9개가 각 종에 맞는 Spine Idle을 반복 재생한다.
- 기존 SpriteRenderer의 표시 범위를 기준으로 Spine 크기와 중심을 맞춘다.
- 기존 SpriteRenderer만 비활성화하고 전투 로직, 콜라이더와 UI 참조는 유지한다.
- 같은 종의 1·2 변형 프리팹은 동일한 SkeletonData를 사용한다.

# 현재 작업 변경
## 작업명
모르바엘보다 장송의 원혼을 나중에 처치할 때 전투가 종료되지 않는 문제 수정

## 현재 상태

구현 완료 / Unity 컴파일 및 Play Mode 확인 필요

## 수정 범위

- `Assets/Scripts/Enemy/Enemy.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 모르바엘을 먼저 처치한 뒤 남은 장송의 원혼을 모두 처치하면 전투가 정상 종료된다.
- 장송의 원혼을 먼저 처치하고 모르바엘을 나중에 처치하는 기존 진행은 유지된다.
- 살아 있는 다른 적이 있으면 장송의 원혼 사망만으로 전투가 종료되지 않는다.

# 현재 작업 변경
## 작업명
1스테이지 일반 몬스터 Spine 시각 보정 마무리

## 현재 상태

구현 완료 / 사용자 Unity Play Mode 확인 완료

## 최종 수정 범위

- `Assets/Art/Enemy/Spine`
- `Assets/Editor/SpineSettings.asset`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Goby1.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Goby2.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Mermaid.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Mimic1.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Mimic2.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/SeaCrab1.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/SeaCrab2.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Thief1.prefab`
- `Assets/Prefabs/Enemy/NormalBattle/1Stage/Thief2.prefab`
- `Assets/Scripts/Tutorial/TutorialTargetMarker.cs`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 결과

- 1스테이지 일반 몬스터 9개에 종별 Spine Idle을 적용했다.
- 몬스터별 크기와 바닥 위치를 BattleScene의 나무 floor에 맞췄다.
- 몬스터별 UI 위치를 개별 보정하고 머리 위에 간격을 두어 배치했다.
- 튜토리얼 역삼각형이 적 UI 전체 영역의 상단 중앙을 따라가도록 수정했다.
- 같은 종 몬스터가 겹쳐도 지지직거리지 않도록 렌더 순서를 분리했다.
- 임시 Editor 자동 적용 도구를 제거해 프리팹 값이 다시 덮어써지지 않게 했다.
# 현재 작업 - 2스테이지 일반 몬스터 Spine 적용

## 현재 단계

- 2스테이지 일반 몬스터 프리팹 7종에 종별 Spine Idle 연결 및 초기 배치
- F8로 2스테이지 첫 일반 전투에 진입하는 Editor 전용 테스트 단축키 추가
- 기존 루트 SpriteRenderer의 높이와 바닥 위치를 기준으로 비율 왜곡 없이 배치
- 누적 확대 문제를 제거하고 종별 고정 Scale, 바닥 위치와 EnemyUIRoot 위치로 2차 보정
- 2스테이지 Play 화면 기준으로 모든 몬스터 접지점을 약 10~15px 위로 맞추고 화면 밖 UI를 종별 머리 위 위치로 하향 보정
- Turtle 1·2와 Drowned 1·2만 몸과 UI를 함께 아래로 내려 앞쪽 나무 바닥에 접지
- OldMermaid, JellyfishMermaid와 Drowned 1·2의 UI만 머리 위로 추가 이동
- Drowned 1·2 UI를 머리 위 간격에 맞게 소폭 하향 미세 조정

## 완료 상태

- 2스테이지 일반 몬스터 Spine 적용 및 Play Mode 시각 확인 완료
- 몬스터별 크기, floor 접지점, UI 위치와 1·2번 렌더 순서 저장 완료
- 임시 `EnemySpinePrefabSetup` Editor 도구 제거 완료
- F8 2스테이지 테스트 단축키 유지
- 다음 단계에서 Unity Play 화면을 기준으로 몬스터별 크기, floor 위치와 UI 위치를 개별 보정

## 수정 대상

- `Assets/Prefabs/Enemy/NormalBattle/2Stage`
- `Assets/Scripts/Scenes/BattleShortcutController.cs`
- `Assets/Editor/EnemySpinePrefabSetup.cs` (적용 완료 후 제거할 임시 도구)
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`
