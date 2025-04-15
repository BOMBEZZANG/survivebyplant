using UnityEngine;
using System.Linq;
using TMPro; // InteractionPromptUI 사용 시 필요

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("상호작용 가능한 최대 반경")]
    public float interactionRadius = 1.0f; // 상호작용 범위를 적절히 조절하세요
    [Tooltip("상호작용 가능한 오브젝트들이 속한 레이어")]
    public LayerMask interactableLayer; // Inspector에서 "Interactable" 레이어 선택

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
    private Transform _transform;          // 플레이어 Transform 캐시
    private Camera mainCamera;
    private IInteractable currentClosestInteractable; // 현재 가장 가까운 상호작용 가능 객체
    private bool isCursorOverridden = false; // 현재 커서가 상호작용 커서로 변경되었는지 여부

    // --- 추가: 첫 상호작용 여부 플래그 ---
    private static bool hasInteractedBefore = false; // static으로 선언하여 게임 세션 동안 유지

    void Start()
    {
        _transform = transform; // 자신의 Transform 캐시
        playerInventory = GetComponent<PlayerInventory>();
        mainCamera = Camera.main;

        if (playerInventory == null) { Debug.LogError("PlayerInteraction: PlayerInventory 컴포넌트를 찾을 수 없습니다!", gameObject); }
        if (mainCamera == null) { Debug.LogError("PlayerInteraction: 메인 카메라를 찾을 수 없습니다! 카메라 태그를 확인하세요.", gameObject); }
        if (_transform == null) { Debug.LogError("PlayerInteraction: Player Transform을 찾을 수 없습니다!", gameObject); }

        // 시작 시 기본 커서 설정 및 프롬프트 숨기기
        SetCursor(defaultCursor, Vector2.zero); // 기본 커서로 시작
        UpdateInteractionPrompt(null);
    }

    void Update()
    {
        // Null 체크 추가
        if (_transform == null) return;

        // 매 프레임 가장 가까운 상호작용 객체 찾고 프롬프트 업데이트 시도
        FindAndShowInteractionPrompt();

        // E 키를 눌렀을 때 상호작용 시도
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 가장 가까운 객체가 있는지 다시 한번 확인
            if (currentClosestInteractable != null)
            {
                // --- 상호작용 실행 및 커서 피드백 ---
                CancelInvoke(nameof(RevertToDefaultCursor)); // 이전 예약 취소
                SetCursor(interactionCursor, interactionCursorHotspot);
                isCursorOverridden = true;

                string targetName = (currentClosestInteractable is MonoBehaviour mono) ? mono.gameObject.name : "Unknown Interactable";
                Debug.Log($"PlayerInteraction: E 키 입력. 상호작용 실행: {targetName}");
                currentClosestInteractable.Interact(gameObject); // Interact 메서드 호출

                // --- 첫 상호작용 시 플래그 업데이트 ---
                if (!hasInteractedBefore)
                {
                    hasInteractedBefore = true; // 플래그를 true로 설정
                    UpdateInteractionPrompt(null); // 즉시 프롬프트 숨기기
                    Debug.Log("첫 E 키 상호작용 성공. 이제 힌트가 표시되지 않습니다.");
                }
                // ------------------------------------

                Invoke(nameof(RevertToDefaultCursor), interactionCursorDuration); // 커서 복원 예약
            }
            else
            {
                Debug.Log("PlayerInteraction: E 키 입력. 주변에 상호작용 가능한 오브젝트 없음.");
            }
        }
    }

    // 기본 커서로 되돌리는 함수
    void RevertToDefaultCursor()
    {
        if (isCursorOverridden)
        {
            SetCursor(defaultCursor, Vector2.zero);
            isCursorOverridden = false;
        }
    }

    // 커서 설정 함수
    void SetCursor(Texture2D cursorTexture, Vector2 hotspot)
    {
        Texture2D finalCursor = cursorTexture ?? defaultCursor;
        Vector2 finalHotspot = (cursorTexture == null) ? Vector2.zero : hotspot;

        try {
             Cursor.SetCursor(finalCursor, finalHotspot, CursorMode.Auto);
        } catch (System.Exception e) {
             Debug.LogError($"커서 설정 중 오류 발생: {e.Message}", gameObject);
             Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
             isCursorOverridden = false;
        }
    }

    // 가장 가까운 상호작용 객체 찾고 프롬프트 표시
    void FindAndShowInteractionPrompt()
    {
         if (_transform == null) return;
         IInteractable closest = FindClosestInteractable();

         // 가장 가까운 객체 상태가 변경되었는지 확인 후 UI 업데이트
         if (closest != currentClosestInteractable)
         {
             currentClosestInteractable = closest; // 내부 상태 업데이트
             UpdateInteractionPrompt(currentClosestInteractable); // UI 업데이트 호출
         }
         // 만약 가장 가까운 객체가 없어졌다면 (범위 벗어남 등)
         else if (closest == null && currentClosestInteractable != null)
         {
             currentClosestInteractable = null; // 내부 상태 업데이트
             UpdateInteractionPrompt(null); // UI 업데이트 호출 (숨기기)
         }
    }

    // 가장 가까운 상호작용 가능 객체 찾기
    IInteractable FindClosestInteractable()
    {
        if (_transform == null) return null;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(_transform.position, interactionRadius, interactableLayer);
        IInteractable closest = null;
        float minDistanceSqr = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distSqr = (col.transform.position - _transform.position).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    closest = interactable;
                }
            }
        }
        return closest;
    }

    // 상호작용 프롬프트 UI 업데이트 (첫 상호작용 힌트 기능 포함)
    void UpdateInteractionPrompt(IInteractable interactable)
    {
        if (interactionPromptUI == null) return; // UI 없으면 함수 종료

        // 첫 상호작용 전이고, 객체가 있을 때만 힌트 표시
        if (!hasInteractedBefore && interactable != null)
        {
            string promptText = "Press E"; // 첫 힌트는 고정
            // 또는 객체별 프롬프트 사용 시:
            // string promptText = interactable.InteractionPrompt ?? "Press E";
            interactionPromptUI.text = promptText;
            interactionPromptUI.gameObject.SetActive(true);
        }
        else // 첫 상호작용 후이거나, 근처에 객체가 없을 경우
        {
            interactionPromptUI.text = "";
            interactionPromptUI.gameObject.SetActive(false);
        }
    }

    // Gizmo 함수
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    // Disable/Quit 함수
    void OnDisable()
    {
        CancelInvoke(nameof(RevertToDefaultCursor));
        if (Application.isPlaying && isCursorOverridden)
        {
             SetCursor(defaultCursor, Vector2.zero);
        }
        isCursorOverridden = false;
    }

     void OnApplicationQuit() { /* OnDisable에서 처리 */ }
}