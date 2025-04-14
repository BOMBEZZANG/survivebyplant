using UnityEngine;
using System.Linq;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("상호작용 가능한 최대 반경")]
    public float interactionRadius = 1.5f;
    [Tooltip("상호작용 가능한 오브젝트들이 속한 레이어")]
    public LayerMask interactableLayer;

    [Header("Cursor Settings")]
    [Tooltip("기본 마우스 커서 (Texture Type = Cursor)")]
    public Texture2D defaultCursor;
    [Tooltip("상호작용 시 잠시 표시될 커서 (손 모양 등, Texture Type = Cursor)")]
    public Texture2D interactionCursor; // 범용 상호작용 커서
    [Tooltip("상호작용 커서의 클릭 지점 (핫스팟)")]
    public Vector2 interactionCursorHotspot = Vector2.zero;
    [Tooltip("상호작용 커서가 표시될 시간 (초)")]
    public float interactionCursorDuration = 0.2f; // 짧게 깜빡이는 시간

    [Header("UI (Optional)")]
    [Tooltip("상호작용 프롬프트를 표시할 TextMeshProUGUI 요소")]
    public TextMeshProUGUI interactionPromptUI;

    // 내부 변수
    private PlayerInventory playerInventory;
    private Transform _transform;
    private Camera mainCamera;
    private IInteractable currentClosestInteractable;
    private bool isCursorOverridden = false; // 현재 커서가 상호작용 커서로 변경되었는지 여부

    void Start()
    {
        _transform = transform;
        playerInventory = GetComponent<PlayerInventory>();
        mainCamera = Camera.main;

        if (playerInventory == null) { Debug.LogError("PlayerInteraction: PlayerInventory 컴포넌트를 찾을 수 없습니다!", gameObject); }
        if (mainCamera == null) { Debug.LogError("PlayerInteraction: 메인 카메라를 찾을 수 없습니다!", gameObject); }

        // 시작 시 기본 커서 설정 및 프롬프트 숨기기
        SetCursor(defaultCursor, Vector2.zero); // 기본 커서로 시작
        UpdateInteractionPrompt(null);
    }

    void Update()
    {
        // 매 프레임 가장 가까운 상호작용 객체 찾기 (UI 프롬프트 용도)
        FindAndShowInteractionPrompt();

        // E 키를 눌렀을 때 상호작용 시도
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentClosestInteractable != null)
            {
                // --- 상호작용 실행 및 커서 피드백 ---
                // 1. 상호작용 커서로 변경
                SetCursor(interactionCursor, interactionCursorHotspot);
                isCursorOverridden = true;

                // 2. 상호작용 실행
                Debug.Log($"PlayerInteraction: E 키 입력. 상호작용 실행: {((MonoBehaviour)currentClosestInteractable).gameObject.name}");
                currentClosestInteractable.Interact(gameObject);

                // 3. 짧은 시간 후 기본 커서로 복원 예약
                // 기존 Invoke 취소 (연속 E키 입력 시 중복 방지)
                CancelInvoke(nameof(RevertToDefaultCursor));
                Invoke(nameof(RevertToDefaultCursor), interactionCursorDuration);
                // ------------------------------------
            }
            else
            {
                Debug.Log("PlayerInteraction: E 키 입력. 주변에 상호작용 가능한 오브젝트 없음.");
                // 상호작용 대상 없을 때도 커서 피드백? (선택적: 짧게 손모양 보였다 사라지게)
                // SetCursor(interactionCursor, interactionCursorHotspot);
                // isCursorOverridden = true;
                // CancelInvoke(nameof(RevertToDefaultCursor));
                // Invoke(nameof(RevertToDefaultCursor), interactionCursorDuration * 0.5f); // 더 짧게
            }
        }
    }

    // 기본 커서로 되돌리는 함수 (Invoke로 호출됨)
    void RevertToDefaultCursor()
    {
        SetCursor(defaultCursor, Vector2.zero);
        isCursorOverridden = false;
    }

    // 커서 설정 함수
    void SetCursor(Texture2D cursorTexture, Vector2 hotspot)
    {
        // 커서 텍스처가 할당되었는지 확인
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
        else
        {
            // 지정된 커서가 없으면 시스템 기본 커서 사용
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
             if (cursorTexture == defaultCursor && defaultCursor == null)
                 Debug.LogWarning("PlayerInteraction: Default Cursor 텍스처가 할당되지 않았습니다.");
             else if (cursorTexture == interactionCursor && interactionCursor == null)
                  Debug.LogWarning("PlayerInteraction: Interaction Cursor 텍스처가 할당되지 않았습니다.");
        }
    }

    // 가장 가까운 상호작용 객체 찾고 프롬프트 표시
    void FindAndShowInteractionPrompt()
    {
         if (interactionPromptUI == null && currentClosestInteractable == null) return;

         Collider2D[] colliders = Physics2D.OverlapCircleAll(_transform.position, interactionRadius, interactableLayer);
         IInteractable closest = FindClosestInteractableInColliders(colliders);

         if (closest != currentClosestInteractable)
         {
             currentClosestInteractable = closest;
             UpdateInteractionPrompt(currentClosestInteractable);
         }
    }

    // 콜라이더 목록에서 가장 가까운 IInteractable 찾기 (변경 없음)
    IInteractable FindClosestInteractableInColliders(Collider2D[] colliders)
    {
        IInteractable closest = null;
        float minDistanceSqr = float.MaxValue;
        foreach (Collider2D col in colliders) {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null && col.gameObject != gameObject) {
                float distSqr = (col.transform.position - _transform.position).sqrMagnitude;
                if (distSqr < minDistanceSqr) {
                    minDistanceSqr = distSqr;
                    closest = interactable;
                }
            }
        }
        return closest;
    }

    // 상호작용 프롬프트 UI 업데이트 (변경 없음)
    void UpdateInteractionPrompt(IInteractable interactable)
    {
        if (interactionPromptUI == null) return;
        if (interactable != null) {
            interactionPromptUI.text = interactable.InteractionPrompt;
            interactionPromptUI.gameObject.SetActive(true);
        } else {
            interactionPromptUI.text = "";
            interactionPromptUI.gameObject.SetActive(false);
        }
    }

    // (디버깅용) 상호작용 반경 시각화 (변경 없음)
    void OnDrawGizmosSelected() { /* ... */ }

    // 게임 종료 또는 오브젝트 파괴 시 기본 커서로 복원
    void OnDisable()
    {
        // Invoke 예약된 것이 있다면 취소하고 즉시 기본 커서로 설정
        CancelInvoke(nameof(RevertToDefaultCursor));
        if (isCursorOverridden || Cursor.visible) // 커서가 변경되었거나 보이는 상태일 때만 복원 시도
        {
             SetCursor(defaultCursor, Vector2.zero);
        }
    }

     void OnApplicationQuit()
    {
         SetCursor(defaultCursor, Vector2.zero);
    }
}