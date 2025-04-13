using System.Collections;
using UnityEngine;

// 이 스크립트는 작동을 위해 최소 하나의 Collider2D가 필요합니다.
// 하나는 OnMouseDown용(IsTrigger=OFF), 다른 하나는 근접 감지용(IsTrigger=ON)을 권장합니다.
[RequireComponent(typeof(Collider2D))] // 게임 오브젝트에 Collider2D가 있는지 확인 (없으면 자동 추가 시도)
[RequireComponent(typeof(SpriteRenderer))] // 하이라이트 효과를 위해 SpriteRenderer 필요
public class SeedPodInteraction : MonoBehaviour
{
    // --- Inspector에서 설정할 변수들 ---

    [Header("Interaction Settings")]
    [Tooltip("플레이어가 상호작용 가능한 최대 거리 (Trigger Collider의 Radius와 일치시키는 것이 좋음)")]
    public float interactionRange = 1.0f; // 참고용 값, 실제 범위는 Trigger Collider 크기
    [Tooltip("플레이어가 범위 내에 있을 때 하이라이트(강조)할 색상")]
    public Color highlightColor = Color.yellow; // 예: 노란색으로 강조

    [Header("Shaking Effect")]
    [Tooltip("식물이 흔들리는 시간 (초)")]
    public float shakeDuration = 0.5f;
    [Tooltip("식물이 흔들리는 강도 (좌우/상하 변위 크기)")]
    public float shakeMagnitude = 0.1f;

    [Header("Seed Spawning")]
    [Tooltip("생성할 씨앗 줍기 아이템 프리팹 (ResourcePickup 스크립트 및 Rigidbody2D 포함)")]
    public GameObject seedPickupPrefab;
    [Tooltip("씨앗 아이템이 튀어나오는 힘의 크기")]
    public float spawnForce = 1.5f;

    [Header("Cooldown")]
    [Tooltip("한 번 상호작용 후 다음 가능할 때까지 걸리는 시간 (초)")]
    public float cooldownTime = 3.0f;

    // --- 내부 작동 변수들 ---
    private Vector3 originalPosition;     // 식물의 원래 위치 저장용
    private bool canShake = true;         // 현재 흔들 수 있는지 여부 (쿨다운 제어)
    private bool isPlayerInRange = false; // 플레이어가 상호작용 범위 안에 있는지 여부

    // 하이라이트 효과 및 원래 색상 복원을 위한 변수
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // --- Unity 내장 메서드 ---

    // 게임 오브젝트가 활성화될 때 또는 게임 시작 시 호출됨
    void Start()
    {
        // 초기 위치 저장
        originalPosition = transform.position;

        // 스프라이트 렌더러 컴포넌트 가져오기 및 초기 색상 저장
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; // 원래 색상 기억
        }
        else
        {
            // 하이라이트 기능을 사용할 수 없음을 경고 (선택 사항)
            Debug.LogWarning($"[{gameObject.name}] SpriteRenderer not found on this object. Highlight effect will be disabled.");
        }

        // 필수적인 SeedPickup 프리팹이 Inspector에서 할당되었는지 확인
        if (seedPickupPrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] SeedPickup Prefab is not assigned in the Inspector! Interaction will be disabled.", gameObject);
            canShake = false; // 프리팹 없으면 상호작용 불가 상태로 만듦
        }

        // 이 오브젝트에 필요한 콜라이더들이 제대로 설정되었는지 확인 (디버깅용)
        CheckColliderSetup();
    }

    // 마우스 왼쪽 버튼으로 이 오브젝트의 콜라이더(IsTrigger=OFF)를 클릭했을 때 호출됨
    void OnMouseDown()
    {
        // 디버깅 로그: 클릭 감지 및 현재 상태 (canShake, isPlayerInRange) 출력
        // Debug.Log($"[{gameObject.name}] OnMouseDown detected! canShake = {canShake}, isPlayerInRange = {isPlayerInRange}");

        // 플레이어가 범위 안에 있고, 쿨다운 상태가 아니며, 프리팹이 할당되어 있을 때만 작동
        if (isPlayerInRange && canShake && seedPickupPrefab != null)
        {
            // Debug.Log($"[{gameObject.name}] Starting shake sequence (Player is in range)...");
            canShake = false; // 즉시 다시 못 흔들게 상태 변경 (쿨다운 시작 전)
            StartCoroutine(ShakeSequence()); // 흔들기 및 아이템 생성/발사 코루틴 시작
        }
        else if (!isPlayerInRange)
        {
            // 범위 밖에 있을 때 로그 (필요시 주석 해제)
            // Debug.Log($"[{gameObject.name}] Cannot shake: Player is too far away.");
            // 필요하다면 '너무 멀다'는 피드백(소리 등) 추가 가능
        }
        else if (!canShake)
        {
            // 쿨다운 중일 때 로그 (필요시 주석 해제)
            // Debug.Log($"[{gameObject.name}] Cannot shake yet (on cooldown).");
            // 필요하다면 '쿨다운 중' 피드백 추가 가능
        }
    }

    // --- 코루틴 메서드 ---

    // 흔들림 효과를 주고 아이템을 생성/발사하는 코루틴
    IEnumerator ShakeSequence()
    {
        // Debug.Log($"[{gameObject.name}] ShakeSequence started.");
        float elapsed = 0.0f;

        // 설정된 시간(shakeDuration) 동안 흔들리는 시각 효과
        while (elapsed < shakeDuration)
        {
            // 원래 위치 기준으로 약간의 랜덤 변위 추가
            float xOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            float yOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            transform.position = originalPosition + new Vector3(xOffset, yOffset, 0);

            elapsed += Time.deltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }
        // 흔들림이 끝나면 원래 위치로 정확히 복귀
        transform.position = originalPosition;
        // Debug.Log($"[{gameObject.name}] Shaking visual finished. Preparing to instantiate...");

        // 씨앗 줍기 아이템 생성 및 발사 로직
        if (seedPickupPrefab != null)
        {
             // 생성 위치: 식물의 약간 위 중앙 (Y 오프셋 조절 가능)
             Vector3 spawnPosition = transform.position + Vector3.up * 0.2f;
             // Debug.Log($"[{gameObject.name}] Attempting to Instantiate '{seedPickupPrefab.name}' at {spawnPosition}");

             // 프리팹 인스턴스화 (복제하여 씬에 생성) 하고 변수에 저장
             GameObject pickupInstance = Instantiate(seedPickupPrefab, spawnPosition, Quaternion.identity);

             // 생성된 아이템에서 Rigidbody2D 컴포넌트 가져오기
             Rigidbody2D pickupRb = pickupInstance.GetComponent<Rigidbody2D>();

             if (pickupRb != null) // Rigidbody2D가 있다면 힘 가하기
             {
                  // 위쪽을 기준으로 약간 랜덤한 각도로 방향 설정
                  float randomAngle = Random.Range(-30f, 30f) * Mathf.Deg2Rad; // -30 ~ +30도 사이 랜덤 각도 (라디안 변환)
                  Vector2 forceDirection = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle)); // (0, 1) 벡터를 회전

                  // Impulse(충격량) 모드로 짧고 강한 힘 가하기
                  pickupRb.AddForce(forceDirection * spawnForce, ForceMode2D.Impulse);
                  // Debug.Log($"[{gameObject.name}] Applied impulse force to {pickupInstance.name}");
             }
             else {
                 // Rigidbody2D가 없는 프리팹이면 경고 (물리 효과 적용 불가)
                 Debug.LogWarning($"[{gameObject.name}] Spawned pickup '{pickupInstance.name}' has no Rigidbody2D to apply force.");
             }

             // Debug.Log($"[{gameObject.name}] Instantiate function finished execution.");
             // Debug.Log($"[{gameObject.name}] SeedPickup item instantiated and force applied.");

             // 아이템 생성 및 발사 후 쿨다운 코루틴 시작
             StartCoroutine(Cooldown());
        } else {
             // Start에서 확인했지만, 혹시 모르니 여기서도 null 체크 및 에러 로그
             Debug.LogError($"[{gameObject.name}] Cannot instantiate SeedPickup, prefab is null!", gameObject);
        }
        // Debug.Log($"[{gameObject.name}] ShakeSequence finished.");
    }

    // 쿨다운 타이머를 실행하는 코루틴
    IEnumerator Cooldown()
    {
        // Debug.Log($"[{gameObject.name}] Cooldown started ({cooldownTime}s). canShake = {canShake}");
        // 설정된 시간만큼 대기
        yield return new WaitForSeconds(cooldownTime);
        // 대기 시간이 끝나면 다시 흔들 수 있도록 상태 변경
        canShake = true;
        // Debug.Log($"[{gameObject.name}] Cooldown finished. Setting canShake back to {canShake}.");
    }

    // --- Trigger 콜라이더 관련 함수들 (플레이어 근접 감지) ---

    // 다른 Collider2D가 이 오브젝트의 Trigger(IsTrigger=ON) 안으로 '들어왔을 때' 호출됨
    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트의 태그가 "Player"인지 확인
        if (other.CompareTag("Player"))
        {
            // Debug.Log($"[{gameObject.name}] Player ENTERED interaction range.");
            isPlayerInRange = true; // 플레이어가 범위 안에 있다고 상태 변경

            // (선택 사항) 시각적 피드백: 플레이어가 범위 안에 들어오면 하이라이트
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
    }

    // 다른 Collider2D가 이 오브젝트의 Trigger(IsTrigger=ON) 밖으로 '나갔을 때' 호출됨
    void OnTriggerExit2D(Collider2D other)
    {
        // 나간 오브젝트의 태그가 "Player"인지 확인
        if (other.CompareTag("Player"))
        {
            // Debug.Log($"[{gameObject.name}] Player EXITED interaction range.");
            isPlayerInRange = false; // 플레이어가 범위 밖에 있다고 상태 변경

            // (선택 사항) 시각적 피드백: 플레이어가 범위 밖으로 나가면 원래 색으로 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    // --- 디버깅용 헬퍼 함수 ---

    // Start에서 호출되어 필요한 콜라이더 설정이 되어 있는지 확인
    void CheckColliderSetup()
    {
        bool triggerFound = false;
        bool nonTriggerFound = false;
        Collider2D[] colliders = GetComponents<Collider2D>(); // 이 오브젝트의 모든 Collider2D 가져오기

        if (colliders.Length == 0)
        {
             Debug.LogError($"[{gameObject.name}] No Collider2D found! Both OnMouseDown and Proximity Check need colliders.", gameObject);
             return;
        }

        foreach (Collider2D col in colliders)
        {
            if (col.isTrigger) triggerFound = true; // Trigger 콜라이더 발견
            else nonTriggerFound = true; // Non-Trigger 콜라이더 발견
        }

        if (!triggerFound)
        {
            Debug.LogError($"[{gameObject.name}] No Collider2D set as Trigger found! Proximity check (OnTriggerEnter/Exit) will not work.", gameObject);
        }
        if (!nonTriggerFound)
        {
            Debug.LogError($"[{gameObject.name}] No Collider2D with 'Is Trigger' unchecked found! Click detection (OnMouseDown) might not work.", gameObject);
        }
        if (colliders.Length < 2 && triggerFound && nonTriggerFound) {
             Debug.LogWarning($"[{gameObject.name}] Only one Collider2D found. Ensure it's correctly set up for both click (IsTrigger=OFF) and proximity (IsTrigger=ON), which might not be possible simultaneously with a single collider depending on exact needs.", gameObject);
        }
    }

} // <<< 클래스 정의 끝