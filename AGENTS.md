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

기존 플레이 흐름과 Inspector 연결을 보존하는 것을 새 구조 도입보다 우선한다.

## Working Rules

- 요청과 직접 관련된 파일만 수정한다.
- 기존 코드 스타일, 명명, 공개 API와 직렬화 필드를 최대한 유지한다.
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

작업 결과에는 다음을 간결하게 포함한다.

- 무엇을 변경했는지
- 수정한 파일과 핵심 위치
- Unity Inspector에서 연결하거나 확인할 항목
- 테스트 방법과 실제 검증 결과
- 남은 위험이나 확인하지 못한 사항

## Documentation Maintenance

- 확정된 프로젝트 규칙이 바뀌면 `Docs/PROJECT_RULE.md`를 갱신한다.
- 코딩 규칙이 바뀌면 `Docs/CODING_RULE.md`를 갱신한다.
- 게임 설계가 확정되면 `Docs/GAME_DESIGN.md`를 갱신한다.
- 기능 단위 작업을 마치면 `Docs/DEVLOG.md`에 날짜, 기능, 관련 경로, 검증 결과를 남긴다.
- 추측은 확정 규칙처럼 기록하지 말고 `확인 필요`로 표시한다.
