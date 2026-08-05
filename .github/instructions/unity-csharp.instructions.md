---
name: 작살잡이 Unity C# 규칙
description: 작살잡이의 C# 게임 코드와 Editor 스크립트를 변경할 때 적용합니다.
applyTo: "Assets/Scripts/**/*.cs,Assets/Editor/**/*.cs"
---

# Unity C# 작업 규칙

- Unity 버전은 `ProjectSettings/ProjectVersion.txt`에서 확인한다.
- 현재 프로젝트의 렌더 파이프라인과 입력 방식을 실제 설정과 관련 코드에서 확인한다.
- 기존 클래스명, 파일명, 메서드명, 변수명과 public API를 유지한다.
- Inspector 연결이 필요한 새 필드는 `[SerializeField] private`를 기본으로 한다.
- 기획 및 밸런스 값은 기존 Inspector, ScriptableObject 또는 CSV 흐름을 우선한다.
- 기존 ScriptableObject와 CSV 데이터 구조를 유지하고 원본 자산을 런타임에 직접 변경하지 않는다.
- 공개 타입과 공개 메서드의 한국어 XML 주석을 유지한다.
- null 참조, 이벤트 구독과 해제, Coroutine 중복 실행을 점검한다.
- 오브젝트 파괴·비활성화·Scene 전환 이후 참조 유효성을 점검한다.
- 직렬화 필드를 변경하면 기존 Scene과 Prefab 호환성 및 Inspector 설정을 설명한다.
- Save/Continue 관련 변경은 저장 버전과 기존 저장 데이터 호환성을 점검한다.
- 외부 VFX 애니메이션 플러그인과 DOTween을 사용하지 않는다.
- Unity 기본 Coroutine, Particle System과 Transform 이동을 우선한다.
- Unity 컴파일과 Play Mode를 직접 확인하지 못했다면 `확인되지 않았다.`라고 명시한다.
