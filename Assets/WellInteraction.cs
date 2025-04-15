using UnityEngine;
using System.Collections;

// 필수 컴포넌트 명시
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))] // SpriteRenderer 추가
public class WellInteraction : MonoBehaviour, IInteractable
{
    [Header("Resource Settings")]
    public string waterResourceName = "Water";
    public int waterAmountPerInteraction = 1;

    [Header("Feedback")]
    public AudioClip collectSound;
    [Range(0f, 1f)]
    public float collectSoundVolume = 1.0f;

    [Header("Cooldown (Optional)")]
    public float cooldownSeconds = 1.0f;
    private float lastInteractionTime = -Mathf.Infinity;

    // --- 하이라이트 관련 변수 추가 ---
    [Header("Interaction Visuals")]
    [Tooltip("플레이어가 범위 내에 있을 때 하이라이트할 색상")]
    public Color highlightColor = Color.yellow;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool canInteractNow = true; // 쿨다운 상태 반영용 (하이라이트 제어)
    // ---------------------------------

    // 상호작용 가능 상태 + 쿨다운 상태를 고려한 프롬프트
    public string InteractionPrompt
    {
        get
        {
             if (Time.time < lastInteractionTime + cooldownSeconds)
                 return "Well (Recharging)"; // 쿨다운 중 메시지
             else
                 return $"Collect {waterResourceName}"; // 상호작용 가능 메시지
        }
    }


void Start()
    {
         // --- 하이라이트 관련 초기화 ---
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            originalColor = spriteRenderer.color;
        } else {
            Debug.LogError($"[{gameObject.name}] SpriteRenderer 컴포넌트가 없습니다!", gameObject);
        }

        // --- Collider 확인 및 Trigger 강제 변경 로직 *제거* ---
        bool hasTriggerCollider = false;
        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length == 0) {
             Debug.LogError($"[{gameObject.name}] Collider2D 컴포넌트가 없습니다!", gameObject);
             enabled = false;
             return;
        }
        foreach(Collider2D c in colliders) {
            if (c.isTrigger) { hasTriggerCollider = true; break; }
        }
        if (!hasTriggerCollider) {
             Debug.LogWarning($"[{gameObject.name}] WellInteraction: 하이라이트 기능을 위한 Trigger Collider (IsTrigger=true)가 없습니다.", gameObject);
        }
        // -------------------------------------------------------

        lastInteractionTime = -cooldownSeconds;
        canInteractNow = true;
    }


    public void Interact(GameObject interactor)
    {
        // 쿨다운 체크
        if (Time.time < lastInteractionTime + cooldownSeconds)
        {
            Debug.Log($"[{gameObject.name}] 우물이 아직 재충전 중입니다...");
            return;
        }

        PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
        if (playerInventory == null) return;

        bool added = playerInventory.AddResource(waterResourceName, waterAmountPerInteraction);

        if (added)
        {
            lastInteractionTime = Time.time;
            canInteractNow = false; // 상호작용했으니 잠시 쿨다운 상태
            StartCoroutine(CooldownTimer()); // 쿨다운 타이머 시작

            if (collectSound != null) { AudioSource.PlayClipAtPoint(collectSound, transform.position, collectSoundVolume); }

            // 하이라이트 즉시 제거 (쿨다운 시작 시) - 선택적
             if (spriteRenderer != null) { spriteRenderer.color = originalColor; }
        }
    }

    // 쿨다운 완료 후 상태 업데이트 코루틴
    IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(cooldownSeconds);
        canInteractNow = true; // 쿨다운 끝나면 다시 상호작용 가능
        // 플레이어가 여전히 범위 안에 있다면 다시 하이라이트 (OnTriggerStay2D 대체)
        // 이 방식 대신 OnTriggerEnter/Exit만 사용해도 충분함
    }

    // --- 하이라이트 로직 추가 ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 상호작용 가능하고(쿨다운 아님), 렌더러가 있다면 하이라이트
            if (canInteractNow && spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 렌더러가 있다면 원래 색으로 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
    // ---------------------------

     // (선택 사항) 쿨다운 중에도 계속 범위 안에 있을 때 색상 업데이트
     void OnTriggerStay2D(Collider2D other) {
         if (other.CompareTag("Player")) {
             // 쿨다운 끝나면 다시 하이라이트, 쿨다운 중이면 원래 색상 유지
             if (canInteractNow && spriteRenderer != null && spriteRenderer.color != highlightColor) {
                 spriteRenderer.color = highlightColor;
             } else if (!canInteractNow && spriteRenderer != null && spriteRenderer.color != originalColor) {
                 spriteRenderer.color = originalColor;
             }
         }
     }
}