using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class CarnivorousPlant : MonoBehaviour
{
    // --- Inspector 설정 변수들 ---
    [Header("Attack Stats")]
    [Tooltip("공격 당 데미지")] public int damage = 5;
    [Tooltip("공격/탐지 범위")] public float attackRange = 1.5f;
    [Tooltip("공격 후 다음 공격까지의 대기 시간(초)")] public float attackCooldown = 2.0f;
    [Tooltip("공격 1회당 소모될 내구도 (0이면 소모 없음)")] public float durabilityCostPerAttack = 5f;

    [Header("Growth & Visuals")]
    [Tooltip("씨앗 상태일 때의 스프라이트")] public Sprite seedSprite;
    [Tooltip("성장 완료 후의 스프라이트")] public Sprite adultSprite;
    [Tooltip("성장 완료까지 걸리는 시간(초)")] public float growthTime = 5.0f;
    [Tooltip("최종 성장 단계 (0=씨앗, 1=성체)")] public int maxGrowthStage = 1;

    [Header("Durability (Health)")]
    [Tooltip("식물의 최대 내구도 (시작 시 내구도)")] public float maxDurability = 100f;
    [Tooltip("1초당 감소할 내구도 (0이면 시간에 따라 감소 안 함)")] public float durabilityDecayPerSecond = 0.1f;
    public float currentDurability;

    [Header("Durability UI")]
    [Tooltip("각 식물 아래에 생성될 내구도 바 UI 프리팹 (Slider 및 PlantDurabilityUI 스크립트 포함)")]
    public GameObject durabilityBarPrefab;
    [Tooltip("식물 중심 기준으로 UI가 표시될 상대적 위치 오프셋")]
    public Vector3 durabilityBarOffset = new Vector3(0, -0.5f, 0);
    [Tooltip("UI 크기 스케일 (x, y 값만 사용)")]
    public Vector2 durabilityBarScale = new Vector2(1f, 0.2f);

    [Header("Required Components (Auto-find or Assign)")]
    [Tooltip("공격 범위를 감지할 CircleCollider2D (IsTrigger=ON 이어야 함)")]
    public CircleCollider2D attackRangeCollider;

    // --- 내부 상태 변수들 ---
    private float lastAttackTime;
    private GameObject currentTarget = null;
    private int currentGrowthStage = 0;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isAlive = true;
    public GameObject durabilityBarInstance;
    private Canvas durabilityUICanvas; // UI 캔버스 참조 추가

    // --- 애니메이터 파라미터를 문자열로 변경 (해시 대신) ---
    private readonly string attackTriggerParam = "AttackTrigger";
    private readonly string dieTriggerParam = "DieTrigger";
    private readonly string growthStageParam = "GrowthStage";

    // 내구도 변경 이벤트 선언
    public event Action<float> OnDurabilityChanged;

    void Awake()
    {
        // 컴포넌트 참조
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // 필수 콜라이더 자동 찾기
        if (attackRangeCollider == null)
        {
            attackRangeCollider = GetComponentInChildren<CircleCollider2D>();
            if (attackRangeCollider != null && !attackRangeCollider.isTrigger)
            {
                Debug.LogWarning($"[{gameObject.name}] Found a CircleCollider2D but it's not set to 'Is Trigger'. Setting it to Trigger mode.", gameObject);
                attackRangeCollider.isTrigger = true;
            }
        }
    }

    void Start()
    {
        // 필수 컴포넌트 최종 확인
        if (spriteRenderer == null) {
            Debug.LogError($"[{gameObject.name}] 필수 컴포넌트 누락: SpriteRenderer!", gameObject);
            isAlive = false; enabled = false; return;
        }
        if (animator == null) {
            Debug.LogWarning($"[{gameObject.name}] Animator 컴포넌트가 없습니다!", gameObject);
        }
        if (attackRangeCollider == null) {
            Debug.LogError($"[{gameObject.name}] 필수 컴포넌트 누락: Attack Range CircleCollider2D (IsTrigger=true)!", gameObject);
            isAlive = false; enabled = false; return;
        }

        // 상태 초기화
        currentDurability = maxDurability;
        isAlive = true;
        currentGrowthStage = 0;
        lastAttackTime = -attackCooldown;
        UpdateVisuals();

        // 공격 콜라이더 초기 설정
        if (attackRangeCollider != null) {
            attackRangeCollider.isTrigger = true;
            attackRangeCollider.radius = attackRange;
            attackRangeCollider.enabled = false;
        } else {
            Debug.LogWarning($"[{gameObject.name}] Start: attackRangeCollider가 없습니다.");
        }

        // === 내구도 UI 생성 및 초기화 ===
        CreateDurabilityBar();

        // 성장 로직 시작
        if (growthTime > 0 && currentGrowthStage < maxGrowthStage) {
            StartCoroutine(GrowTimer());
        }

        // 명시적으로 내구도 이벤트 발생 (UI 초기 업데이트)
        float ratio = GetCurrentDurabilityRatio();
        Debug.Log($"[{gameObject.name}] Start: 초기 내구도 비율 {ratio:F2} 이벤트 발생");
        OnDurabilityChanged?.Invoke(ratio);
    }

    // 내구도 바 UI 생성 함수
    private void CreateDurabilityBar()
    {
        if (durabilityBarPrefab == null) {
            Debug.LogWarning($"[{gameObject.name}] Durability Bar Prefab이 연결되지 않았습니다.", gameObject);
            return;
        }

        // 기존 UI가 있으면 제거
        if (durabilityBarInstance != null) {
            Destroy(durabilityBarInstance);
        }

        Debug.Log($"[{gameObject.name}] CreateDurabilityBar: 내구도 바 생성 시작");

        // 월드 스페이스 좌표로 UI 위치 계산
        Vector3 spawnPos = transform.position + durabilityBarOffset;
        
        // UI 오브젝트 생성
        durabilityBarInstance = Instantiate(durabilityBarPrefab, spawnPos, Quaternion.identity);
        durabilityBarInstance.name = $"{gameObject.name}_DurabilityBar";
        
        // 캔버스 설정 확인
        Canvas canvas = durabilityBarInstance.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = durabilityBarInstance.GetComponentInChildren<Canvas>();
        }
        
        if (canvas != null)
        {
            // 월드 스페이스 캔버스 설정 확인/조정
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                Debug.LogWarning($"[{gameObject.name}] CreateDurabilityBar: Canvas의 Render Mode가 World Space가 아닙니다. World Space로 변경합니다.", durabilityBarInstance);
                canvas.renderMode = RenderMode.WorldSpace;
            }
            
            // 캔버스 스케일 조정 (크기가 적절하도록)
            canvas.transform.localScale = new Vector3(durabilityBarScale.x, durabilityBarScale.y, 1f) * 0.01f;
            Debug.Log($"[{gameObject.name}] CreateDurabilityBar: Canvas 스케일 설정: {canvas.transform.localScale}");
            
            // Canvas 상태 확인
            Debug.Log($"[{gameObject.name}] CreateDurabilityBar: Canvas 렌더 모드: {canvas.renderMode}, worldCamera: {canvas.worldCamera}");
            
            // 월드 카메라 설정 (없으면 메인 카메라 사용)
            if (canvas.worldCamera == null && Camera.main != null)
            {
                canvas.worldCamera = Camera.main;
                Debug.Log($"[{gameObject.name}] CreateDurabilityBar: Canvas 월드 카메라를 메인 카메라로 설정");
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] CreateDurabilityBar: 내구도 바에 Canvas 컴포넌트가 없습니다!", durabilityBarInstance);
        }
        
        // UI가 항상 카메라를 향하도록 설정
        if (durabilityBarInstance != null && Camera.main != null)
        {
            durabilityBarInstance.transform.forward = Camera.main.transform.forward;
            Debug.Log($"[{gameObject.name}] CreateDurabilityBar: UI를 카메라 방향으로 회전");
        }
        
        // 고정 위치 설정을 위한 Follow 스크립트 추가
        DurabilityBarFollow followScript = durabilityBarInstance.AddComponent<DurabilityBarFollow>();
        if (followScript != null)
        {
            followScript.targetTransform = transform;
            followScript.offset = durabilityBarOffset;
            Debug.Log($"[{gameObject.name}] CreateDurabilityBar: Follow 스크립트 추가 및 설정 완료");
        }
        
        // UI 스크립트 초기화
        PlantDurabilityUI uiScript = durabilityBarInstance.GetComponent<PlantDurabilityUI>();
        if (uiScript == null)
        {
            uiScript = durabilityBarInstance.GetComponentInChildren<PlantDurabilityUI>();
        }
        
        if (uiScript != null)
        {
            Debug.Log($"[{gameObject.name}] CreateDurabilityBar: PlantDurabilityUI 스크립트 찾음, 초기화 시작");
            
            // 초기화 전에 현재 내구도 값 확인 (디버깅)
            float ratio = GetCurrentDurabilityRatio();
            Debug.Log($"[{gameObject.name}] CreateDurabilityBar: 초기화 전 내구도 비율: {ratio:F2} (현재: {currentDurability}/{maxDurability})");
            
            // UI 스크립트 초기화
            uiScript.Initialize(this);
            
            // 이벤트 발생 (UI 업데이트)
            OnDurabilityChanged?.Invoke(ratio);
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] CreateDurabilityBar: Durability Bar Prefab에 PlantDurabilityUI 스크립트가 없습니다!", durabilityBarPrefab);
            Destroy(durabilityBarInstance);
        }
    }

    void Update()
    {
        if (!isAlive) return;

        // 시간당 내구도 감소 (성장 단계가 최대일 때만)
        if (currentGrowthStage >= maxGrowthStage && durabilityDecayPerSecond > 0f)
        {
            // 이전 값 저장 (변경 감지용)
            float prevDurability = currentDurability;
            
            // 내구도 감소
            currentDurability -= durabilityDecayPerSecond * Time.deltaTime;
            currentDurability = Mathf.Max(0f, currentDurability); // 0 미만으로 떨어지지 않도록
            
            // 값이 변경되었다면 이벤트 발생 (최적화)
            if (Mathf.Abs(prevDurability - currentDurability) > 0.01f)
            {
                float ratio = GetCurrentDurabilityRatio();
                Debug.Log($"[{gameObject.name}] Update: 내구도 감소됨: {currentDurability:F2}/{maxDurability:F2}, 비율: {ratio:F2}");
                OnDurabilityChanged?.Invoke(ratio);
            }
            
            // 내구도가 0 이하면 사망
            if (currentDurability <= 0f) { 
                Debug.Log($"[{gameObject.name}] Update: 내구도 0 이하로 사망 처리");
                Die(); 
                return;
            }
        }

        // 공격 로직
        if (currentGrowthStage >= maxGrowthStage && attackRangeCollider != null && attackRangeCollider.enabled)
        {
            if (currentTarget != null && Time.time >= lastAttackTime + attackCooldown) { 
                Attack(); 
            }
            if (currentTarget != null && !currentTarget.activeInHierarchy) { 
                currentTarget = null; 
            }
        }
    }

    // --- Trigger 관련 메서드 ---
    void OnTriggerStay2D(Collider2D other)
    {
        if (!isAlive || currentGrowthStage < maxGrowthStage || attackRangeCollider == null || !attackRangeCollider.enabled) return;
        if (other.CompareTag("Enemy")) { 
            if (currentTarget == null) { 
                currentTarget = other.gameObject; 
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (!isAlive) return;
        if (other.gameObject == currentTarget) { 
            currentTarget = null; 
        }
    }

    // --- 성장 관련 메서드 ---
    IEnumerator GrowTimer() { 
        yield return new WaitForSeconds(growthTime); 
        GrowPlant(); 
    }
    
    public void GrowPlant() {
        if (!isAlive) return;
        if (currentGrowthStage < maxGrowthStage) { 
            currentGrowthStage++; 
            UpdateVisuals(); 
            EnableAttackCollider(); 
            Debug.Log($"[{gameObject.name}] 식물이 성장했습니다."); 
        }
    }
    
    void EnableAttackCollider() { 
        if (attackRangeCollider != null) { 
            attackRangeCollider.enabled = true; 
        }
    }
    
   public void UpdateVisuals() {
    if (spriteRenderer != null) {
        Sprite targetSprite = (currentGrowthStage == 0) ? seedSprite : adultSprite;
        if (targetSprite != null && spriteRenderer.sprite != targetSprite) { 
            spriteRenderer.sprite = targetSprite; 
        }
    }
    
    if (animator != null) {
        // 더 간단하게 수정 - 파라미터 존재 여부 검사
        bool hasGrowthParam = false;
        foreach (var param in animator.parameters) {
            if (param.name == growthStageParam) {
                hasGrowthParam = true;
                break;
            }
        }
        
        if (hasGrowthParam) {
            animator.SetInteger(growthStageParam, currentGrowthStage);
        }
        // 경고 메시지 제거 - 파라미터가 없으면 조용히 무시함
    }
}
    // --- 공격 및 피격 함수 ---
    public void TakeDamage(float damageAmount)
    {
        if (!isAlive || damageAmount <= 0) return;
        currentDurability -= damageAmount;
        OnDurabilityChanged?.Invoke(GetCurrentDurabilityRatio());
        Debug.Log($"[{gameObject.name}] Took {damageAmount} damage. Current Durability: {currentDurability}/{maxDurability}");

        if (currentDurability <= 0f) { 
            Die(); 
        }
    }

    public float GetCurrentDurabilityRatio()
    {
        // 0으로 나누기 방지
        if (maxDurability <= 0.001f) {
            Debug.LogWarning($"[{gameObject.name}] GetCurrentDurabilityRatio: maxDurability가 0에 가깝습니다. ({maxDurability})");
            return 0f;
        }
        
        // 현재 내구도가 음수일 경우 방지
        float safeCurrentDurability = Mathf.Max(0f, currentDurability);
        
        // 내구도 비율 계산 (0~1 사이 값)
        float ratio = Mathf.Clamp01(safeCurrentDurability / maxDurability);
        
        // 디버그 로그 (값 확인)
        Debug.Log($"[{gameObject.name}] GetCurrentDurabilityRatio: {safeCurrentDurability}/{maxDurability} = {ratio:F2}");
        
        return ratio;
    }

// Die 함수 내의 애니메이터 코드 수정
private void Die() {
    if (!isAlive) return;
    isAlive = false;
    Debug.Log($"[{gameObject.name}] 식물이 죽었습니다! (내구도 고갈)");

    // 상호작용 비활성화
    if(attackRangeCollider != null) attackRangeCollider.enabled = false;
    Collider2D mainCollider = GetComponent<Collider2D>();
    if(mainCollider != null && !mainCollider.isTrigger) { mainCollider.enabled = false; }

    // 죽는 애니메이션 (안전 검사 추가) - 파라미터가 없으면 경고 없이 넘어감
    if (animator != null) {
        // 더 간단하게 수정 - 파라미터 존재 여부 검사
        bool hasDieParam = false;
        foreach (var param in animator.parameters) {
            if (param.name == dieTriggerParam) {
                hasDieParam = true;
                break;
            }
        }
        
        if (hasDieParam) {
            animator.SetTrigger(dieTriggerParam);
        }
        // 경고 메시지 제거 - 파라미터가 없으면 조용히 무시함
    }

    // 내구도 바 UI 제거
    if (durabilityBarInstance != null) { 
        Destroy(durabilityBarInstance); 
    }

    // 오브젝트 제거
    Destroy(gameObject, 2f);
}

// Attack 함수 내의 애니메이터 코드 수정
void Attack()
{
    if (currentTarget == null || !isAlive) return;

    if (animator != null) { 
        // 더 간단하게 수정 - 파라미터 존재 여부 검사
        bool hasAttackParam = false;
        foreach (var param in animator.parameters) {
            if (param.name == attackTriggerParam) {
                hasAttackParam = true;
                break;
            }
        }
        
        if (hasAttackParam) {
            animator.SetTrigger(attackTriggerParam); 
        }
        // 경고 메시지 제거 - 파라미터가 없으면 조용히 무시함
    }
    
    lastAttackTime = Time.time;

    // 공격 시 내구도 소모
    if (durabilityCostPerAttack > 0f)
    {
        currentDurability -= durabilityCostPerAttack;
        OnDurabilityChanged?.Invoke(GetCurrentDurabilityRatio());
        Debug.Log($"[{gameObject.name}] Attacked! Durability cost: {durabilityCostPerAttack}. Current: {currentDurability}/{maxDurability}");
        if (currentDurability <= 0f) { Die(); return; }
    }

    // 데미지 전달
    AntHealth antHealth = currentTarget.GetComponent<AntHealth>();
    if (antHealth != null) {
         antHealth.TakeDamage(damage);
         if (currentTarget != null && (!currentTarget.activeInHierarchy || antHealth.currentHealth <= 0)) { 
             currentTarget = null; 
         }
    } else { 
        currentTarget = null; 
    }
}
    void OnDestroy()
    {
        // UI 제거
        if (durabilityBarInstance != null)
        {
            Destroy(durabilityBarInstance);
        }
        
        // 이벤트 구독자 해제
        OnDurabilityChanged = null;
    }
}

// 내구도 바가 식물을 따라다니게 하는 컴포넌트
public class DurabilityBarFollow : MonoBehaviour
{
    public Transform targetTransform;
    public Vector3 offset;
    
    // 매 프레임마다 타겟 위치 추적
    void LateUpdate()
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + offset;
            
            // 카메라를 향해 바라보게 설정 (3D 환경에서 유용)
            if (Camera.main != null)
            {
                transform.forward = Camera.main.transform.forward;
            }
        }
        else
        {
            // 타겟이 없으면 자신을 파괴
            Destroy(gameObject);
        }
    }
}