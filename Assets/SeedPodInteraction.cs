using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

// RequireComponent는 그대로 유지, IInteractable 인터페이스 구현 추가
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SeedPodInteraction : MonoBehaviour, IInteractable // , IInteractable 추가
{
    // --- 기존 변수들은 대부분 그대로 사용 ---
    [Header("Interaction Settings")]
    // [Tooltip("플레이어가 상호작용 가능한 최대 거리")] // 이제 PlayerInteraction.cs 에서 관리
    // public float interactionRange = 1.0f;
    [Tooltip("플레이어가 범위 내에 있을 때 하이라이트할 색상")]
    public Color highlightColor = Color.yellow;

    [Header("Shaking Effect")]
    [Tooltip("식물이 흔들리는 시간 (초)")]
    public float shakeDuration = 0.5f;
    [Tooltip("식물이 흔들리는 강도")]
    public float shakeMagnitude = 0.1f;

    [Header("Seed Spawning")]
    [Tooltip("생성할 씨앗 줍기 아이템 프리팹")]
    public GameObject seedPickupPrefab;
    [Tooltip("씨앗 아이템이 튀어나오는 힘의 크기")]
    public float spawnForce = 1.5f;
    [Tooltip("이 주머니에서 생성될 최대 씨앗 개수")]
    public int maxSeedsToProduce = 5;

    [Header("Cooldown")]
    [Tooltip("한 번 상호작용 후 다음 가능할 때까지 걸리는 시간 (초)")]
    public float cooldownTime = 3.0f;

    [Header("Visuals")]
    [Tooltip("씨앗이 다 떨어졌을 때 표시할 스프라이트")]
    public Sprite emptyPodSprite;
     // (선택) 흔들기 성공 시 사운드
    [Tooltip("씨앗 획득 성공 시 재생할 사운드 (선택 사항)")]
    public AudioClip shakeSound;
    [Range(0f, 1f)]
    public float shakeSoundVolume = 1.0f;

    // --- 내부 작동 변수들 ---
    private Vector3 originalPosition;
    private bool canShake = true;
    // private bool isPlayerInRange = false; // 이제 PlayerInteraction이 거리 관리
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private int seedsProducedCount = 0;
    private bool isEmpty = false;

    // --- IInteractable 인터페이스 구현 ---
    public string InteractionPrompt
    {
        get
        {
            if (isEmpty) return "Empty Seed Pod";
            if (!canShake) return "Seed Pod (Recharging)";
            return "Shake for Seeds"; // 상호작용 가능 시 표시될 텍스트
        }
    }

    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] SpriteRenderer not found!", gameObject);
            canShake = false;
        }

        if (seedPickupPrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] SeedPickup Prefab is not assigned!", gameObject);
            canShake = false;
        }

        if (seedsProducedCount >= maxSeedsToProduce)
        {
            SetEmptyState();
        }
        else
        {
            isEmpty = false;
            canShake = true;
        }

        // 콜라이더 설정 확인 (Trigger여야 하이라이트 작동)
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) {
             Debug.LogError($"[{gameObject.name}] Collider2D가 없습니다! 하이라이트 및 상호작용이 불가능합니다.", gameObject);
             enabled = false;
        } else if (!col.isTrigger) {
             Debug.LogWarning($"[{gameObject.name}] Collider2D의 IsTrigger가 꺼져있습니다. 하이라이트(OnTriggerEnter/Exit)가 작동하지 않을 수 있습니다.", gameObject);
        }
    }

    // --- 추가: IInteractable 인터페이스의 Interact 메서드 ---
    public void Interact(GameObject interactor) // PlayerInteraction 스크립트가 호출
    {
         Debug.Log($"[{gameObject.name}] Interact() called by {interactor.name}");

         // 비어있거나 쿨다운 중이면 실행 안 함
         if (isEmpty)
         {
             Debug.Log($"[{gameObject.name}] Interaction ignored: Pod is empty.");
             // 여기에 '빈 주머니' 효과음 등 추가 가능
             return;
         }
         if (!canShake)
         {
             Debug.Log($"[{gameObject.name}] Interaction ignored: Pod is on cooldown.");
             // 여기에 '쿨다운 중' 효과음 등 추가 가능
             return;
         }

         // 거리 체크는 PlayerInteraction.cs에서 이미 했으므로 여기서는 생략

         // 씨앗 생성 및 흔들림 코루틴 시작
         if (seedPickupPrefab != null)
         {
             Debug.Log($"[{gameObject.name}] Starting shake sequence via E key interaction...");
             canShake = false; // 즉시 다시 못 흔들게
             StartCoroutine(ShakeSequence());
         }
         else
         {
             Debug.LogError($"[{gameObject.name}] Cannot start shake sequence, seedPickupPrefab is null!", gameObject);
         }
    }

    // 수정된 ShakeSequence 코루틴 - 효과음 재생 타이밍 변경
    IEnumerator ShakeSequence()
    {
        // --- 흔들기 시작 시 효과음 재생 (여기로 이동) ---
        if (shakeSound != null)
        {
            AudioSource.PlayClipAtPoint(shakeSound, transform.position, shakeSoundVolume);
            Debug.Log($"[{gameObject.name}] 흔들기 효과음 재생 시작");
        }
        // ------------------------------------------

        // --- 흔들림 효과 부분 ---
        float elapsed = 0.0f;
        Vector3 startPos = transform.position; // 흔들기 전 위치 저장
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            float yOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            // originalPosition 대신 startPos 기준으로 흔들리도록 수정 (더 자연스러움)
            transform.position = startPos + new Vector3(xOffset, yOffset, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = startPos; // 원래 위치로 복구
        // --- 흔들림 효과 끝 ---

        // --- 씨앗 생성 및 상태 업데이트 ---
        if (seedPickupPrefab != null)
        {
             Vector3 spawnPosition = transform.position + Vector3.up * 0.2f; // 스폰 위치 조정 가능
             GameObject pickupInstance = Instantiate(seedPickupPrefab, spawnPosition, Quaternion.identity);
             Rigidbody2D pickupRb = pickupInstance.GetComponent<Rigidbody2D>();

             if (pickupRb != null)
             {
                  float randomAngle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
                  Vector2 forceDirection = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle));
                  pickupRb.AddForce(forceDirection * spawnForce, ForceMode2D.Impulse);
             }

             // --- 효과음 재생 코드 제거 (위로 이동) ---

             seedsProducedCount++;
             Debug.Log($"[{gameObject.name}] Seed produced. Count: {seedsProducedCount}/{maxSeedsToProduce}");

             if (seedsProducedCount >= maxSeedsToProduce)
             {
                 Debug.Log($"[{gameObject.name}] Max seeds produced. Setting to empty state.");
                 SetEmptyState(); // 여기서 쿨다운 코루틴 시작 안 함
             }
             else
             {
                 StartCoroutine(Cooldown()); // 아직 씨앗 남았으면 쿨다운 시작
             }
        }
        else
        {
             Debug.LogError($"[{gameObject.name}] Cannot instantiate SeedPickup, prefab is null!", gameObject);
        }
    }

    // Cooldown 코루틴 (변경 없음)
    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        if (!isEmpty) { canShake = true; }
    }

    // SetEmptyState 함수 (변경 없음)
    void SetEmptyState()
    {
        isEmpty = true;
        canShake = false;
        if (spriteRenderer != null && emptyPodSprite != null) { spriteRenderer.sprite = emptyPodSprite; }
        // 하이라이트 제거는 OnTriggerExit2D에서도 처리되므로 여기서 중복될 수 있음
        // if (spriteRenderer != null) { spriteRenderer.color = originalColor; }
    }

    // OnTriggerEnter2D (하이라이트 시작 - 변경 없음)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // isPlayerInRange = true; // 이 플래그는 더 이상 사용하지 않음
            // 비어있지 않고 스프라이트 렌더러가 있을 때만 하이라이트
            if (!isEmpty && spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
    }

    // OnTriggerExit2D (하이라이트 종료 - isPlayerInRange 제거)
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // isPlayerInRange = false; // 이 플래그는 더 이상 사용하지 않음
            // 원래 색상으로 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor; // 비어있는 상태 포함 원래 색상으로 (빈 스프라이트 기준)
            }
        }
    }

    // 디버깅용 Gizmos 추가 (선택 사항)
    void OnDrawGizmos()
    {
        // 인터랙션 범위 표시 (삭제된 변수라 비활성화)
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // 콜라이더 시각화
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = col.isTrigger ? Color.green : Color.red;
            if (col is BoxCollider2D)
            {
                BoxCollider2D boxCol = col as BoxCollider2D;
                Vector3 size = new Vector3(boxCol.size.x, boxCol.size.y, 0.1f);
                Vector3 center = transform.position + (Vector3)boxCol.offset;
                Gizmos.DrawWireCube(center, size);
            }
            else if (col is CircleCollider2D)
            {
                CircleCollider2D circleCol = col as CircleCollider2D;
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }
    }
}