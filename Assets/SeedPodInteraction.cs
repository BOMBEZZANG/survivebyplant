using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SeedPodInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("플레이어가 상호작용 가능한 최대 거리")]
    public float interactionRange = 1.0f;
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
    // ===>>> 추가: 최대 생성 개수 <<<===
    [Tooltip("이 주머니에서 생성될 최대 씨앗 개수")]
    public int maxSeedsToProduce = 5;

    [Header("Cooldown")]
    [Tooltip("한 번 상호작용 후 다음 가능할 때까지 걸리는 시간 (초)")]
    public float cooldownTime = 3.0f;

    // ===>>> 추가: 빈 주머니 스프라이트 <<<===
    [Header("Visuals")]
    [Tooltip("씨앗이 다 떨어졌을 때 표시할 스프라이트")]
    public Sprite emptyPodSprite;

    // --- 내부 작동 변수들 ---
    private Vector3 originalPosition;
    private bool canShake = true;
    private bool isPlayerInRange = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    // ===>>> 추가: 생성된 씨앗 개수 카운터 <<<===
    private int seedsProducedCount = 0;
    private bool isEmpty = false; // 주머니가 비었는지 상태 저장

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
            canShake = false; // 스프라이트 없으면 작동 불가
        }

        if (seedPickupPrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] SeedPickup Prefab is not assigned!", gameObject);
            canShake = false; // 프리팹 없으면 작동 불가
        }

        // ===>>> 추가: 초기 상태 업데이트 <<<===
        // 게임 시작 시 이미 최대 개수에 도달했는지 확인 (예: maxSeedsToProduce가 0인 경우)
        if (seedsProducedCount >= maxSeedsToProduce)
        {
            SetEmptyState(); // 비어있는 상태로 즉시 전환
        }
        else
        {
            isEmpty = false;
            canShake = true; // 시작 시 흔들 수 있도록 초기화
        }

        CheckColliderSetup();
    }

    void OnMouseDown()
    {
        // ===>>> 수정: 비어있는지 확인 조건 추가 <<<===
        if (!isEmpty && isPlayerInRange && canShake && seedPickupPrefab != null)
        {
            // Debug.Log($"[{gameObject.name}] Starting shake sequence..."); // 필요시 주석 해제
            canShake = false; // 즉시 다시 못 흔들게 상태 변경
            StartCoroutine(ShakeSequence());
        }
        // else if (isEmpty) Debug.Log("Seed pod is empty."); // 필요시 주석 해제
        // else if (!isPlayerInRange) Debug.Log("Player too far."); // 필요시 주석 해제
        // else if (!canShake) Debug.Log("On cooldown."); // 필요시 주석 해제
    }

    IEnumerator ShakeSequence()
    {
        // --- 흔들림 효과 부분 (변경 없음) ---
        float elapsed = 0.0f;
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            float yOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            transform.position = originalPosition + new Vector3(xOffset, yOffset, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
        // --- 흔들림 효과 끝 ---

        // --- 씨앗 생성 및 상태 업데이트 ---
        if (seedPickupPrefab != null)
        {
             Vector3 spawnPosition = transform.position + Vector3.up * 0.2f;
             GameObject pickupInstance = Instantiate(seedPickupPrefab, spawnPosition, Quaternion.identity);
             Rigidbody2D pickupRb = pickupInstance.GetComponent<Rigidbody2D>();

             if (pickupRb != null)
             {
                  float randomAngle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
                  Vector2 forceDirection = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle));
                  pickupRb.AddForce(forceDirection * spawnForce, ForceMode2D.Impulse);
             }

             // ===>>> 수정: 카운터 증가 및 최대치 확인 <<<===
             seedsProducedCount++;
             Debug.Log($"[{gameObject.name}] Seed produced. Count: {seedsProducedCount}/{maxSeedsToProduce}");

             // 최대 개수에 도달했는지 확인
             if (seedsProducedCount >= maxSeedsToProduce)
             {
                 Debug.Log($"[{gameObject.name}] Max seeds produced. Setting to empty state.");
                 SetEmptyState(); // 비어있는 상태로 전환 (더 이상 Cooldown 시작 안 함)
             }
             else
             {
                 // 최대 개수에 도달하지 않았으면 쿨다운 시작
                 StartCoroutine(Cooldown());
             }
             // ===>>> 수정 끝 <<<===
        } else {
             Debug.LogError($"[{gameObject.name}] Cannot instantiate SeedPickup, prefab is null!", gameObject);
             // 이 경우, 이미 canShake가 false이므로 추가 동작 없음
        }
    }

    IEnumerator Cooldown()
    {
        // Debug.Log($"[{gameObject.name}] Cooldown started ({cooldownTime}s)."); // 필요시 주석 해제
        yield return new WaitForSeconds(cooldownTime);
        // ===>>> 수정: 비어있지 않을 때만 흔들 수 있게 복구 <<<===
        if (!isEmpty)
        {
            canShake = true;
            // Debug.Log($"[{gameObject.name}] Cooldown finished. canShake = true."); // 필요시 주석 해제
        } else {
             // Debug.Log($"[{gameObject.name}] Cooldown finished, but pod is empty. canShake remains false."); // 필요시 주석 해제
        }
    }

    // ===>>> 추가: 비어있는 상태로 전환하는 함수 <<<===
    void SetEmptyState()
    {
        isEmpty = true; // 상태 플래그 설정
        canShake = false; // 영구적으로 흔들기 불가

        // 스프라이트 변경
        if (spriteRenderer != null && emptyPodSprite != null)
        {
            spriteRenderer.sprite = emptyPodSprite;
        }

        // 하이라이트 제거 (만약 플레이어가 범위 안에 있다면)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; // 원래 색상으로 (또는 빈 주머니 스프라이트의 기본 색상)
        }
    }

    // ===>>> 수정: 트리거 로직에서 isEmpty 상태 확인 <<<===
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // 비어있지 않고, 스프라이트 렌더러가 있을 때만 하이라이트
            if (!isEmpty && spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // 비어있는 상태와 관계없이 원래 색상으로 복원 (SetEmptyState에서 색상을 관리할 수도 있음)
            if (spriteRenderer != null)
            {
                // 만약 SetEmptyState에서 색상 변경을 안했다면 여기서 해야함
                // 지금은 SetEmptyState와 여기가 모두 originalColor로 설정하므로 괜찮음
                 if (!isEmpty) // 비어있지 않을때만 원래색 복원 (비어있으면 빈 스프라이트 색 유지)
                 {
                     spriteRenderer.color = originalColor;
                 }
                 // 혹은 간단하게: spriteRenderer.color = originalColor; (SetEmptyState에서 색 변경 안할 경우)
            }
        }
    }

    // 디버깅용 콜라이더 설정 확인 함수 (변경 없음)
    void CheckColliderSetup() { /* ... 이전 코드 ... */ }
}