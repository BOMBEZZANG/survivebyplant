using UnityEngine;

// 모든 상호작용 가능한 오브젝트가 구현해야 할 인터페이스
public interface IInteractable
{
    // 플레이어가 상호작용했을 때 호출될 메서드
    // interactor: 상호작용을 시도한 게임 오브젝트 (보통 플레이어)
    void Interact(GameObject interactor);

    // (선택 사항) 플레이어가 근처에 있을 때 표시될 상호작용 프롬프트 텍스트
    string InteractionPrompt { get; }
}