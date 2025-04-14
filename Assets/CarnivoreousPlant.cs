using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Animator 컴포넌트도 필요하므로 RequireComponent 추가 (선택 사항)
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))] // Animator 컴포넌트 필요 명시
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

    // ===>>> 추가: Animator 참조 변수 <<<===
    private Animator animator;
    // ===>>> 추가: Animator 파라미터 이름 (오타 방지용) <<<===
    private readonly int attackTriggerHash = Animator.StringToHash("AttackTrigger"); // "AttackTrigger"는 Animator에서 설정한 이름과 일치해야 함

    // --- Unity 내장 메서드 ---

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // ===>>> 추가: Animator 컴포넌트 가져오기 <<<===
        animator = GetComponent<Animator>();

        // 필수 컴포넌트 확인
        if (attackRangeCollider == null) {
            Debug.LogError($"[{gameObject.name}] Attack Range Collider가 Inspector에 연결되지 않았습니다!", gameObject);
            isAlive = false;
        }
        if (spriteRenderer == null) {
            Debug.LogError($"[{gameObject.name}] SpriteRenderer 컴포넌트가 없습니다!", gameObject);
            isAlive = false;
        }
        // ===>>> 추가: Animator 확인 <<<===
        if (animator == null) {
            Debug.LogError($"[{gameObject.name}] Animator 컴포넌트가 없습니다!", gameObject);
            // isAlive = false; // 애니메이션 없어도 공격은 가능할 수 있으므로 주석 처리
        }

        if (!isAlive) return; // 필수 컴포넌트 없으면 초기화 중단

        currentGrowthStage = 0;
        lastAttackTime = -attackCooldown; // 게임 시작하자마자 공격 가능하도록
        UpdateSprite(); // 초기 스프라이트 설정

        if (attackRangeCollider != null) {
            attackRangeCollider.isTrigger = true;
            attackRangeCollider.radius = attackRange;
            attackRangeCollider.enabled = false; // 성체 되기 전까지 비활성화
        }

        if (growthTime > 0 && currentGrowthStage < maxGrowthStage) {
            StartCoroutine(GrowTimer());
        } else if (currentGrowthStage >= maxGrowthStage) {
            EnableAttackCollider();
            // Debug.Log($"[{gameObject.name}] 성체 상태로 시작됨."); // 필요시 주석 해제
        }
         // Debug.Log($"[{gameObject.name}] 식물 초기화 완료. 현재 단계: {currentGrowthStage}"); // 필요시 주석 해제
    }

     void Update()
     {
         if (!isAlive || currentGrowthStage < maxGrowthStage || attackRangeCollider == null || !attackRangeCollider.enabled) return;

         if (currentTarget != null)
         {
             if (Time.time >= lastAttackTime + attackCooldown)
             {
                 Attack();
             }
         }

         // 타겟 유효성 검사 (비활성화/파괴된 경우)
         if (currentTarget != null && !currentTarget.activeInHierarchy)
         {
              // Debug.Log($"[{gameObject.name}] 타겟 ({currentTarget.name}) 비활성화됨. 타겟 해제."); // 필요시 주석 해제
              currentTarget = null;
         }
     }

     // Trigger 관련 함수들 (OnTriggerStay2D, OnTriggerExit2D)은 변경 없음
     void OnTriggerStay2D(Collider2D other)
     {
         if (!isAlive || currentGrowthStage < maxGrowthStage || attackRangeCollider == null || !attackRangeCollider.enabled) return;

         if (other.CompareTag("Enemy"))
         {
             if (currentTarget == null) // 현재 타겟이 없을 때만 새 타겟으로 설정
             {
                 // Debug.Log($"[{gameObject.name}] OnTriggerStay2D: Setting currentTarget to {other.name}"); // 필요시 주석 해제
                 currentTarget = other.gameObject;
             }
         }
     }

     void OnTriggerExit2D(Collider2D other)
     {
         if (!isAlive) return;

         if (other.gameObject == currentTarget)
         {
             // Debug.Log($"[{gameObject.name}] 타겟 ({other.name}) 범위 이탈. 타겟 해제."); // 필요시 주석 해제
             currentTarget = null;
         }
     }


    // 성장 관련 함수들 (GrowTimer, GrowPlant, EnableAttackCollider, UpdateSprite)은 변경 없음
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
            EnableAttackCollider();
            // Debug.Log($"[{gameObject.name}] 식물이 성장했습니다. 현재 단계: {currentGrowthStage}"); // 필요시 주석 해제
        }
    }

    void EnableAttackCollider() {
         if (attackRangeCollider != null) {
            // Debug.Log($"[{gameObject.name}] EnableAttackCollider(): Attempting to enable collider."); // 필요시 주석 해제
            attackRangeCollider.enabled = true;
            // Debug.Log($"[{gameObject.name}] EnableAttackCollider(): Collider enabled state is: {attackRangeCollider.enabled}."); // 필요시 주석 해제
         } else {
             // Debug.LogWarning($"[{gameObject.name}] EnableAttackCollider(): attackRangeCollider is NULL."); // 필요시 주석 해제
         }
    }

    public void UpdateSprite()
    {
         if (spriteRenderer == null) return;
         try {
             // === 수정: 성장 단계에 따라 스프라이트 대신 Animator 상태를 제어할 수도 있음 ===
             // 여기서는 여전히 직접 스프라이트를 바꾸지만, Animator로 Idle 상태를 관리하는 것이 더 일반적
             Sprite targetSprite = (currentGrowthStage == 0) ? seedSprite : adultSprite;
             if (targetSprite != null) {
                 if(spriteRenderer.sprite != targetSprite) {
                     spriteRenderer.sprite = targetSprite;
                 }
             }
         } catch (System.Exception e) {
             Debug.LogError($"[{gameObject.name}] UpdateSprite 중 오류 발생: {e.Message}\n{e.StackTrace}");
         }
    }

    // 공격 함수 수정
    void Attack()
    {
        if (currentTarget == null) {
             // Debug.LogWarning($"[{gameObject.name}] Attack() called but currentTarget is null."); // 필요시 주석 해제
             return;
        }

        // Debug.Log($"[{gameObject.name}] 개미 ({currentTarget.name}) 공격!"); // 필요시 주석 해제
        lastAttackTime = Time.time;

        // ===>>> 추가: 공격 애니메이션 트리거 <<<===
        if (animator != null)
        {
            // Animator Controller에서 설정한 Trigger 파라미터 이름("AttackTrigger")과 일치해야 함
            animator.SetTrigger(attackTriggerHash); // StringToHash 사용이 성능에 더 좋음
            // 또는 animator.SetTrigger("AttackTrigger");
             Debug.Log($"[{gameObject.name}] Attack Animation Triggered!"); // 애니메이션 트리거 확인 로그
        }
        // ===>>> 애니메이션 트리거 끝 <<<===

        // --- 여기에 공격 시점 사운드 재생 추가 가능 ---
        // if (attackSoundClip != null && audioSource != null) { /* ... Play sound ... */ }

        AntHealth antHealth = currentTarget.GetComponent<AntHealth>();
        if (antHealth != null)
        {
            antHealth.TakeDamage(damage);

            // 공격 후 타겟 생존 여부 확인 및 타겟 해제
            if (currentTarget != null && (!currentTarget.activeInHierarchy || antHealth.currentHealth <= 0))
            {
                // Debug.Log($"[{gameObject.name}] Target {currentTarget.name} confirmed dead after attack. Clearing target."); // 필요시 주석 해제
                currentTarget = null;
            }
        }
        else
        {
            // Debug.LogWarning($"[{gameObject.name}] 타겟 {currentTarget.name}의 AntHealth를 찾을 수 없습니다. 타겟 해제."); // 필요시 주석 해제
            currentTarget = null;
        }
    }

    // TakeDamage, Die 함수는 변경 없음
    public void TakeDamage(int damageAmount) {
        if (!isAlive) return;
        health -= damageAmount;
        if (health <= 0) {
            Die();
        }
    }

    private void Die() {
        isAlive = false;
        // Debug.Log($"[{gameObject.name}] 식물이 죽었습니다."); // 필요시 주석 해제
        if(attackRangeCollider != null) attackRangeCollider.enabled = false;
        // 콜라이더 비활성화 등...
        // Destroy(gameObject, 2f);
    }
}