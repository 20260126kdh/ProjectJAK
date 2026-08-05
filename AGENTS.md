# Project: 작살잡이 (ProjectJAK)

이 문서는 이 저장소에서 작업하는 Codex와 개발자가 가장 먼저 따라야 하는 프로젝트 전용 지침이다.

## Project Overview

- 장르: 2D 덱 빌딩 로그라이크
- 엔진: Unity 6 (`6000.0.78f1`)
- 언어: C#
- 렌더 파이프라인: Universal Render Pipeline 17.0.4
- 주요 UI: uGUI
- 애니메이션: Spine Runtime
- 입력: 기존 `UnityEngine.Input` 기반 코드와 Input System 패키지가 함께 존재하므로, 변경 전 해당 씬의 방식을 확인한다.

## Source of Truth

작업 전 아래 순서로 문맥을 확인한다.

1. 이 `AGENTS.md`
2. `Docs/PROJECT_RULE.md`
3. `Docs/CODING_RULE.md`
4. `Docs/GAME_DESIGN.md`
5. `Docs/DEVLOG.md`
6. 실제 씬, 프리팹, ScriptableObject, CSV 및 코드

문서와 구현이 다르면 임의로 한쪽을 맞다고 가정하지 않는다. 실제 동작을 확인하고, 불일치를 결과에 알리며 필요한 경우 문서도 함께 갱신한다.

## Priority

1. 완성 가능성
2. 안정성
3. 개발 속도
4. 유지보수성
5. 확장성
6. 화려한 구조

불필요한 리팩터링보다 기존 구조를 유지한다. 기존 플레이 흐름과 Inspector 연결을 보존하는 것을 새 구조 도입보다 우선한다.

## Required Workflow

모든 기능 작업은 다음 순서를 지킨다.

1. `CURRENT_TASK.md`와 관련 파일을 먼저 분석한다.
2. 현재 구조와 수정이 필요한 파일 목록을 사용자에게 제시한다.
3. 수정 계획, 기존 시스템과의 충돌 가능성 및 예상 부작용을 설명한다.
4. 사용자에게 명시적인 승인을 받는다.
5. 승인된 파일과 범위만 최소한으로 수정한다.
6. 수정 결과, Inspector 설정, 테스트 방법과 점검 항목을 보고한다.

1~3단계에서는 파일을 수정하지 않는다. 사용자가 분석만 요청한 경우에도 승인 없이 구현하지 않는다.

- 한 번에 하나의 기능만 구현한다.
- 큰 기능은 검증 가능한 작은 단계로 나누고 각 단계마다 승인을 받는다.
- 사용자가 `다음`이라고 하기 전에는 다음 단계 구현을 시작하지 않는다.
- 승인 후 요구사항이나 수정 파일이 달라지면 작업을 멈추고 다시 설명한 뒤 승인을 받는다.
- 작업 범위가 `CURRENT_TASK.md`를 벗어나면 먼저 사용자에게 확인한다.
- `CURRENT_TASK.md`가 없거나 현재 작업이 기록되지 않았다면 구현 전에 사용자에게 현재 작업을 확인한다.

## Working Rules

- 요청과 직접 관련된 파일만 수정한다.
- 기존 코드 스타일, 명명, 공개 API와 직렬화 필드를 최대한 유지한다.
- 요청하지 않은 리팩터링, 구조 변경, 로직 삭제, 최적화를 하지 않는다.
- 사용자 승인 없이 클래스명이나 함수명을 변경하지 않는다.
- 승인 범위 밖의 파일을 만들지 않는다.
- 씬, 프리팹, `.asset`, `.meta`, CSV를 이유 없이 재저장하거나 대량 변경하지 않는다.
- Unity가 관리하는 `Library/`, `Logs/`, `Temp/`, `obj/` 산출물은 수정하거나 커밋하지 않는다.
- `Assets/Spine/`, `Assets/Spine Examples/` 등 외부 패키지 코드는 명시적인 요청 없이는 수정하지 않는다.
- 필드명 변경이나 타입 변경 전 기존 Inspector 직렬화 호환성을 검토한다. 이름 변경이 필요하면 `FormerlySerializedAs`를 고려한다.
- Prefab/Scene 참조가 필요한 새 필드는 `[SerializeField] private`를 기본으로 하고 Inspector 연결 방법을 설명한다.
- 정적 게임 데이터는 기존 흐름에 맞춰 ScriptableObject 또는 CSV를 사용하며, 런타임 상태와 원본 데이터를 분리한다.
- 원본 ScriptableObject를 런타임에 직접 변형하지 않는다. 카드 인스턴스처럼 상태가 변하는 데이터는 복제본을 사용한다.
- 카드·적 패턴·상태 효과 enum 또는 CSV 스키마를 바꾸면 로더, 실행기, UI, 저장 데이터에 미치는 영향을 함께 확인한다.
- 저장 형식을 바꾸면 `saveVersion`과 이전 저장 데이터 호환성을 반드시 검토한다.
- 공개 타입과 공개 메서드에는 한국어 XML 문서 주석을 작성한다. 복잡한 private 로직에도 의도를 설명하는 주석을 남긴다.
- 주석은 코드가 무엇을 하는지 반복하기보다 게임 규칙과 예외 처리 이유를 설명한다.
- 기존 텍스트 인코딩은 UTF-8로 보존하며 한글이 깨지지 않도록 확인한다.

## Architecture Guardrails

- 데이터: `Assets/Scripts/Cards`, `Battle`, `Pattern`의 데이터 클래스와 ScriptableObject/CSV
- 로직: `Assets/Scripts/Managers`, 실행기, 컨트롤러
- 표현: `Assets/Scripts/UI`, 애니메이션, 오디오, VFX
- 씬 흐름: `Assets/Scripts/Scenes`, `GameFlowManager`, `StageManager`

새 기능은 가능한 한 데이터·게임 규칙·화면 표현을 분리하되, 작은 수정 때문에 기존 시스템 전체를 재설계하지 않는다.

## Verification

- C# 변경 후 컴파일 오류가 없는지 확인한다.
- 가능하면 관련 Unity Test를 실행한다.
- 자동 검증이 어려우면 대상 씬에서 재현 가능한 수동 테스트 절차를 제시한다.
- Inspector 참조, ScriptableObject/CSV 데이터, 씬 전환, 저장/이어하기 회귀를 변경 범위에 맞춰 확인한다.
- Unity를 직접 실행하지 못했다면 실행했다고 표현하지 않는다.

## Response Rules

승인 전 답변은 다음 순서를 따른다.

1. 현재 구조 분석
2. 수정이 필요한 파일 목록
3. 수정 계획

여기까지는 파일을 수정하지 않는다.

승인 후 답변은 다음 순서를 따른다.

4. 코드 수정: 수정한 파일, 위치와 이유
5. Unity Inspector 설정
6. 테스트 방법과 실제 검증 결과
7. 문제 발생 시 점검 항목과 예상 부작용

확실하지 않은 내용은 추측하지 않고 `확인되지 않았다.`라고 명시한다. `아마`, `될 것이다`, `문제 없을 것이다` 같은 추측성 표현을 사용하지 않는다. 항상 현재 프로젝트의 실제 파일을 근거로 판단한다.

## Git Commit Rules

Commit은 반드시 다음 2단계 승인을 거친다. Push는 사용자가 GitHub Desktop에서 직접 수행한다.

1. 사용자가 `커밋해`라고 말하면 즉시 Commit하지 않는다.
2. 먼저 Git 상태를 확인하고 다음 내용을 보고한다.
   - 변경된 파일
   - 새로 생성되거나 삭제된 파일
   - Git에 포함하면 안 되는 Unity 생성 파일 또는 개인 설정 파일
   - 현재 브랜치
   - 테스트 및 검증 상태
3. 확인 결과에 따라 `현재 상태는 커밋해도 됩니다` 또는 `아직 커밋을 권장하지 않습니다`라고 명확히 답한다.
4. 이 확인 보고 직후 사용자가 `해`라고 말한 경우에만 Commit한다.
5. Commit 성공 후 Commit 해시, 제목, Description, 포함 파일과 GitHub Desktop에서 Push할 브랜치를 보고한다.

다른 질문이나 작업에 대한 `해`는 Commit 승인으로 해석하지 않는다. Codex가 커밋 시점을 제안했더라도 `커밋해`와 `해` 승인을 생략하지 않는다. Codex는 Push를 실행하지 않는다.

### Commit Message

- Commit 제목은 Commit 실행 시점의 한국 시간 기준 `MM/dd HH:mm` 형식을 사용한다.
- 예: `08/06 01:56`
- 같은 분에 제목이 중복될 때만 `MM/dd HH:mm:ss` 형식을 사용한다.
- Description은 변경 내용을 한 줄에 하나씩 개조식으로 작성한다.
- Description은 기능 중심으로 최대 3~5줄을 사용한다.
- 테스트가 남아 있으면 Description 마지막 줄에 간결하게 표시한다.

예시:

```text
08/06 01:56

- 버림 덱 도착 VFX 재생 기능 추가
- Particle System Inspector 연결 추가
- Unity Play 테스트 필요
```

### Failure Handling

- Commit이 실패하면 오류를 보고한다.
- 사용자가 GitHub Desktop에서 Push에 실패하면 오류 내용을 바탕으로 원인을 분석하되 Push를 대신 실행하지 않는다.
- Push 문제를 해결하기 위해 자동으로 Pull, Merge 또는 Rebase하지 않는다.
- Force Push, Branch 생성, Reset, Checkout을 임의로 실행하지 않는다.
- 실패를 해결하기 위해 승인 범위를 벗어나는 Git 작업이 필요하면 먼저 사용자에게 설명하고 승인받는다.

### Recommended Commit Timing

작업 결과를 보고할 때 현재 상태를 기준으로 `커밋 권장: 예` 또는 `커밋 권장: 아니요`를 제시하고 이유를 설명한다.

다음 경우 Commit을 권장한다.

- 하나의 작은 기능 또는 단계가 완료되었다.
- Unity 컴파일, 정적 검증 또는 합의한 테스트가 완료되었다.
- 사용자가 Play Mode 정상 동작을 확인했다.
- 다음 단계에서 기존 코드나 Prefab을 크게 수정하기 전에 복구 기준점이 필요하다.
- 코드와 관련 문서가 함께 정리되었다.
- 작업 종료 또는 다른 기능으로 전환하기 전이다.

다음 경우 Commit을 권장하지 않는다.

- 컴파일 오류나 원인이 확인되지 않은 Console 오류가 남아 있다.
- 기능이 중간 상태이거나 테스트용 임시 변경이 남아 있다.
- 관련 없는 변경이 하나의 Commit 범위에 섞여 있다.
- Unity 생성 파일이나 개인 설정 파일이 포함되어 있다.
- 필요한 Unity Play Mode 또는 Inspector 확인이 남아 있다.

커밋 권장 여부는 다음 형식으로 보고한다.

```text
커밋 권장: 예 또는 아니요
이유: 현재 검증 상태와 남은 작업
추천 범위: 이번 Commit에 포함할 파일
```

## Documentation Maintenance

- 확정된 프로젝트 규칙이 바뀌면 `Docs/PROJECT_RULE.md`를 갱신한다.
- 코딩 규칙이 바뀌면 `Docs/CODING_RULE.md`를 갱신한다.
- 게임 설계가 확정되면 `Docs/GAME_DESIGN.md`를 갱신한다.
- 기능 단위 작업을 마치면 `Docs/DEVLOG.md`에 날짜, 기능, 관련 경로, 검증 결과를 남긴다.
- 추측은 확정 규칙처럼 기록하지 말고 `확인 필요`로 표시한다.
