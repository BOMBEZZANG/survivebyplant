using System.Collections;
using System.Collections.Generic; // Needed for IEnumerator
using UnityEngine;

// 필요한 컴포넌트 자동 추가
[RequireComponent(typeof(SpriteRenderer))]
public class CarnivorousPlant : MonoBehaviour
{
    // --- Inspector에서 할당/설정할 변수들 ---
    [Header("Attack Stats")]
    [Tooltip("공격 당 데미지")] public int damage = 5;
    [Tooltip("공격/탐지 범위 (아래 Attack Range Collider의 Radius와 일치 권장)")] public float attackRange = 1.5f;
    [Tooltip("공격 후 다음 공격까지의 대기 시간(초)")] public float attackCooldown = 2.0f;

    [Header("Growth & Visuals")]
    [Tooltip("씨앗 상태일 때의 스프라이트")] public Sprite seedSprite;
    [Tooltip("성장 완료 후의 스프라이트")] public Sprite adultSprite;
    [Tooltip("성장 완료까지 걸리는 시간(초)")] public float growthTime = 5.0f;
    [Tooltip("최종 성장 단계 (0=씨앗, 1=성체)")] public int maxGrowthStage = 1;

    [Header("Health")]
    [Tooltip("식물의 시작 체력")] public int health = 50;

    [Header("Required Components (Assign in Inspector)")]
    [Tooltip("공격 범위를 감지할 CircleCollider2D (IsTrigger=ON 이어야 함)")] public CircleCollider2D attackRangeCollider;

    // --- 내부 상태 변수들 ---
    private float lastAttackTime;
    private GameObject currentTarget = null;
    private int currentGrowthStage = 0;
    private SpriteRenderer spriteRenderer;
    private bool isAlive = true;

    // --- Unity 내장 메서드 ---

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (attackRangeCollider == null) {
            Debug.LogError($"[{gameObject.name}] Attack Range Collider가 Inspector에 연결되지 않았습니다! 공격 기능을 사용할 수 없습니다.", gameObject);
            isAlive = false;
        }
        if (spriteRenderer == null) {
            Debug.LogError($"[{gameObject.name}] SpriteRenderer 컴포넌트가 없습니다!", gameObject);
            isAlive = false;
            return;
        }

        isAlive = true;
        currentGrowthStage = 0;
        lastAttackTime = -attackCooldown;
        UpdateSprite();

        if (attackRangeCollider != null) {
            attackRangeCollider.isTrigger = true;
            attackRangeCollider.radius = attackRange;
            attackRangeCollider.enabled = false; // 처음엔 비활성화
        }

        if (growthTime > 0 && currentGrowthStage < maxGrowthStage) {
            StartCoroutine(GrowTimer());
        } else if (currentGrowthStage >= maxGrowthStage) {
            EnableAttackCollider();
            Debug.Log($"[{gameObject.name}] 성체 상태로 시작됨.");
        }
         Debug.Log($"[{gameObject.name}] 식물 초기화 완료. 현재 단계: {currentGrowthStage}");
    }

     void Update()
     {
         // --- Update 함수 진입 조건 확인 로그 ---
         bool isUpdateBlocked = !isAlive || currentGrowthStage < maxGrowthStage || attackRangeCollider == null || !attackRangeCollider.enabled;
         // if(isUpdateBlocked) Debug.Log($"Update returning early: isAlive={isAlive}, stage={currentGrowthStage}<{maxGrowthStage}={(currentGrowthStage < maxGrowthStage)}, colliderNull={attackRangeCollider == null}, colliderEnabled={attackRangeCollider?.enabled ?? false}");
         // --- 로그 끝 ---

         if (isUpdateBlocked) return; // 조건 안 맞으면 실행 중지

         // --- 공격 쿨다운 및 실행 로직 ---
         if (currentTarget != null) // 타겟이 있어야 함
         {
             // --- 공격 준비 상태 확인 로그 (활성화!) ---
             // 현재 시간, 마지막 공격 시간, 쿨다운, 다음 공격 가능 시간, 타겟 이름 출력
            // Debug.Log($"[{gameObject.name}] Update Attack Check: Time={Time.time}, LastAttack={lastAttackTime}, Cooldown={attackCooldown}, TimeToAttack={lastAttackTime + attackCooldown}, Target={currentTarget.name}");
             // --- 로그 끝 ---

             if (Time.time >= lastAttackTime + attackCooldown) // 쿨다운 시간 지났는지 확인
             {
                 Attack(); // 공격 실행
             }
         }

         // --- 타겟 유효성 검사 ---
         if (currentTarget != null && !currentTarget.activeInHierarchy) // 타겟이 비활성화되었는지 (죽었는지) 확인
         {
              Debug.Log($"[{gameObject.name}] 타겟 ({currentTarget.name}) 비활성화됨. 타겟 해제.");
              currentTarget = null; // 타겟 정리
         }
     }

     // --- Trigger 관련 메서드 ---

     void OnTriggerStay2D(Collider2D other)
     {
         // --- OnTriggerStay2D 함수 진입 조건 확인 로그 ---
         bool isTriggerBlocked = !isAlive || currentGrowthStage < maxGrowthStage || attackRangeCollider == null || !attackRangeCollider.enabled;
         // if(isTriggerBlocked) Debug.Log($"OnTriggerStay2D returning early: isAlive={isAlive}, stage={currentGrowthStage}<{maxGrowthStage}={(currentGrowthStage < maxGrowthStage)}, colliderNull={attackRangeCollider == null}, colliderEnabled={attackRangeCollider?.enabled ?? false}");
         // --- 로그 끝 ---

         if (isTriggerBlocked) return; // 조건 안 맞으면 실행 중지

         // --- 상세 탐지 로그 (활성화!) ---
         // 범위 안에 머무는 모든 객체 정보 출력
         Debug.Log($"[{gameObject.name}] OnTriggerStay2D ACTIVE: Detected '{other.name}' with tag '{other.tag}'");
         // --- 로그 끝 ---

         if (other.CompareTag("Enemy")) // 태그가 "Enemy"인지 확인
         {
             // --- 상세 타겟 설정 로그 (활성화!) ---
             // Enemy 태그 감지 및 현재 타겟 상태 로그 출력
             Debug.Log($"[{gameObject.name}] OnTriggerStay2D: Detected Enemy '{other.name}'. Current target is {(currentTarget == null ? "NULL" : currentTarget.name)}");
             // --- 로그 끝 ---
             if (currentTarget == null) // 현재 타겟이 없을 때만 새 타겟으로 설정
             {
                 // --- 상세 타겟 설정 로그 (활성화!) ---
                 // 타겟 설정 로그 출력
                 Debug.Log($"[{gameObject.name}] OnTriggerStay2D: Setting currentTarget to {other.name}");
                 // --- 로그 끝 ---
                 currentTarget = other.gameObject; // 타겟 설정
             }
         }
     }

     void OnTriggerExit2D(Collider2D other)
     {
         if (!isAlive) return;

         if (other.gameObject == currentTarget) // 현재 타겟이 범위를 벗어났다면
         {
             // --- 상세 타겟 해제 로그 (활성화!) ---
             Debug.Log($"[{gameObject.name}] 타겟 ({other.name}) 범위 이탈. 타겟 해제.");
             // --- 로그 끝 ---
             currentTarget = null; // 타겟 해제
         }
     }


    // --- 성장 관련 메서드 ---

    IEnumerator GrowTimer()
    {
        yield return new WaitForSeconds(growthTime);
        GrowPlant();
    }

    public void GrowPlant()
    {
        if (!isAlive) return;
        if (currentGrowthStage < maxGrowthStage)
        {
            currentGrowthStage++;
            UpdateSprite();
            // 활성화 시도 및 결과 확인 로그
            Debug.Log($"[{gameObject.name}] GrowPlant: Calling EnableAttackCollider(). attackRangeCollider is {(attackRangeCollider == null ? "NULL" : "Assigned")}");
            EnableAttackCollider();
            if(attackRangeCollider != null) {
                 Debug.Log($"[{gameObject.name}] GrowPlant: After EnableAttackCollider() call, collider enabled state is: {attackRangeCollider.enabled}");
            }
            Debug.Log($"[{gameObject.name}] 식물이 성장했습니다. 현재 단계: {currentGrowthStage}");
        }
    }

    void EnableAttackCollider() {
         if (attackRangeCollider != null) {
            Debug.Log($"[{gameObject.name}] EnableAttackCollider(): Attempting to enable collider.");
            attackRangeCollider.enabled = true;
            Debug.Log($"[{gameObject.name}] EnableAttackCollider(): Tried setting enabled. Current state: {attackRangeCollider.enabled}."); // 활성화 후 상태 확인
         } else {
             Debug.LogWarning($"[{gameObject.name}] EnableAttackCollider(): attackRangeCollider is NULL.");
         }
    }

    public void UpdateSprite()
    {
         if (spriteRenderer == null) return;
         try {
             Sprite targetSprite = (currentGrowthStage == 0) ? seedSprite : adultSprite;
             if (targetSprite != null) {
                 if(spriteRenderer.sprite != targetSprite) {
                     spriteRenderer.sprite = targetSprite;
                 }
             } else {
                 // Debug.LogWarning($"[{gameObject.name}] UpdateSprite: 단계 {currentGrowthStage}에 해당하는 스프라이트가 없습니다.");
             }
         } catch (System.Exception e) {
             Debug.LogError($"[{gameObject.name}] UpdateSprite 중 오류 발생: {e.Message}\n{e.StackTrace}");
         }
    }

    // --- 공격 및 체력 관련 메서드 ---

// CarnivorousPlant.cs의 Attack 함수 수정
void Attack()
{
    if (currentTarget == null) {
         Debug.LogWarning($"[{gameObject.name}] Attack() called but currentTarget is null.");
         return;
    }

    Debug.Log($"[{gameObject.name}] 개미 ({currentTarget.name}) 공격!");
    lastAttackTime = Time.time; // 쿨다운 타이머 리셋

    AntHealth antHealth = currentTarget.GetComponent<AntHealth>();
    if (antHealth != null)
    {
        antHealth.TakeDamage(damage);
        // --- 여기에 공격 애니메이션/이펙트/사운드 재생 ---

        // === 추가: 공격 후 타겟이 죽었는지 확인하고 즉시 타겟 해제 ===
        // TakeDamage 호출 후 AntHealth 상태를 다시 확인하거나,
        // currentTarget이 비활성화되었는지 확인하여 죽었는지 판단할 수 있습니다.
        if (currentTarget != null && (!currentTarget.activeInHierarchy || antHealth.currentHealth <= 0))
        {
            Debug.Log($"[{gameObject.name}] Target {currentTarget.name} confirmed dead after attack. Clearing target.");
            currentTarget = null; // 즉시 타겟 해제
        }
        // === 추가 끝 ===
    }
    else
    {
        Debug.LogWarning($"[{gameObject.name}] 타겟 {currentTarget.name}의 AntHealth를 찾을 수 없습니다. 타겟 해제.");
        currentTarget = null; // 타겟 정리
    }
}
    public void TakeDamage(int damageAmount) {
        if (!isAlive) return;
        health -= damageAmount;
        if (health <= 0) {
            Die();
        }
    }

    private void Die() {
        isAlive = false;
        Debug.Log($"[{gameObject.name}] 식물이 죽었습니다.");
        if(attackRangeCollider != null) attackRangeCollider.enabled = false;
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach(Collider2D col in colliders){
            if(col != null && !col.isTrigger){
                col.enabled = false;
                break;
            }
        }
        // --- 여기에 죽는 모습(스프라이트 변경), 파티클 효과 등 추가 ---
        // Destroy(gameObject, 2f); // 선택 사항: 일정 시간 후 오브젝트 제거
    }
}