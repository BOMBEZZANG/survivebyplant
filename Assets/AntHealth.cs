using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class AntHealth : MonoBehaviour
{
    [Header("기본 설정")]
    [Tooltip("개미의 최대 체력")]
    public int maxHealth = 10;
    [HideInInspector]
    public int currentHealth;

    [Header("사망 시 드롭 아이템")]
    [Tooltip("죽을 때 떨어뜨릴 키틴 조각 프리팹")]
    public GameObject chitinScrapPrefab;
    [Range(0f, 1f)]
    [Tooltip("키틴 조각을 떨어뜨릴 확률 (0.0 = 0%, 1.0 = 100%)")]
    public float dropChance = 1.0f;
    [Tooltip("확률 성공 시 떨어뜨릴 최소 개수")]
    public int minScrapDrop = 1;
    [Tooltip("확률 성공 시 떨어뜨릴 최대 개수")]
    public int maxScrapDrop = 1;

    [Header("피격 효과")]
    [Tooltip("피격 시 깜빡일 색상")]
    public Color flashColor = Color.red;
    [Tooltip("색상이 깜빡이는 지속 시간 (초)")]
    public float flashDuration = 0.1f;
    [Tooltip("피격 시 생성될 파티클 등의 이펙트 프리팹 (선택 사항)")]
    public GameObject flashEffectPrefab;

    [Header("피격 사운드")]
    [Tooltip("피격 시 재생할 오디오 클립")]
    public AudioClip hurtSound;
    [Range(0f, 1f)]
    [Tooltip("피격음의 볼륨 크기")]
    public float hurtSoundVolume = 1.0f;

    // --- 내부 변수 ---
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private GameObject currentFlashEffect;
    private MaterialPropertyBlock propertyBlock; // 추가: MaterialPropertyBlock 사용

    void Awake()
    {
        // Awake에서 초기화 (Start보다 먼저 실행됨)
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock(); // MaterialPropertyBlock 초기화
        
        if (spriteRenderer == null)
        {
            Debug.LogError($"[{gameObject.name} Awake] SpriteRenderer 컴포넌트를 찾을 수 없습니다!", gameObject);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        
        if (spriteRenderer != null)
        {
            // MaterialPropertyBlock으로 현재 색상 가져오기
            spriteRenderer.GetPropertyBlock(propertyBlock);
            originalColor = spriteRenderer.color;
            
            // 디버그 로그: 시작 색상 확인
            Debug.Log($"[{gameObject.name} Start] 초기 색상: {originalColor}", gameObject);
        }
    }
    
    // 디버깅을 위한 테스트 메서드 - 필요시 사용
    public void TestFlashEffect()
    {
        Debug.Log($"[{gameObject.name} TestFlashEffect] 테스트 피격 효과 실행", gameObject);
        TakeDamage(1);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"[{gameObject.name} TakeDamage] 함수 호출됨. Damage: {damage}, Current Health (Before): {currentHealth}", gameObject);

        if (currentHealth <= 0) return; // 이미 죽었으면 무시

        currentHealth -= damage; // 체력 감소

        // --- 시각적/청각적 피드백 처리 ---
        if (gameObject.activeInHierarchy)
        {
            // 사운드 재생
            if (hurtSound != null)
            {
                AudioSource.PlayClipAtPoint(hurtSound, transform.position, hurtSoundVolume);
            }

            // 색상 깜빡임 처리
            if (spriteRenderer != null)
            {
                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                    // 중지 시 원래 색상으로 복구
                    ResetColor();
                }
                
                // 코루틴 시작 - 색상 변경 시작
                flashCoroutine = StartCoroutine(FlashEffectCoroutine());
                
                // 디버그 로그: 색상 변경 시도 확인
                Debug.Log($"[{gameObject.name} TakeDamage] 색상 변경 시도: {originalColor} -> {flashColor}", gameObject);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name} TakeDamage] SpriteRenderer가 없어 색상 변경 불가", gameObject);
            }

            // 파티클 이펙트 생성
            if (flashEffectPrefab != null)
            {
                ShowFlashEffectPrefab();
                Debug.Log($"[{gameObject.name} TakeDamage] 파티클 생성 시도: {flashEffectPrefab.name}", gameObject);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name} TakeDamage] flashEffectPrefab이 설정되지 않음", gameObject);
            }
        }

        // 사망 체크
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 색상을 원래대로 복구하는 메서드
    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            // 두 가지 방법으로 색상 복구 시도
            spriteRenderer.color = originalColor;
            
            // MaterialPropertyBlock 사용
            propertyBlock.SetColor("_Color", originalColor);
            spriteRenderer.SetPropertyBlock(propertyBlock);
            
            Debug.Log($"[{gameObject.name} ResetColor] 색상 복구: {originalColor}", gameObject);
        }
    }

    // 피격 효과 코루틴 (색상 및 스케일 변경)
    IEnumerator FlashEffectCoroutine()
    {
        // 색상 변경 (MaterialPropertyBlock 사용)
        if (spriteRenderer != null)
        {
            // 두 가지 방법으로 색상 변경 시도
            spriteRenderer.color = flashColor;
            
            // MaterialPropertyBlock 사용
            propertyBlock.SetColor("_Color", flashColor);
            spriteRenderer.SetPropertyBlock(propertyBlock);
            
            Debug.Log($"[{gameObject.name} FlashEffect] 색상 변경됨: {flashColor}", gameObject);
        }

        // 스케일 변경
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.1f;

        // 지속 시간만큼 대기
        yield return new WaitForSeconds(flashDuration);

        // 원래 상태로 복구
        ResetColor();
        transform.localScale = originalScale;

        flashCoroutine = null;
        Debug.Log($"[{gameObject.name} FlashEffect] 효과 종료", gameObject);
    }

    // 이펙트 프리팹 생성 함수 (개선)
    private void ShowFlashEffectPrefab()
    {
        if (currentFlashEffect != null)
        {
            Destroy(currentFlashEffect);
        }
        
        // 파티클 생성 및 위치 지정
        currentFlashEffect = Instantiate(flashEffectPrefab, transform.position, Quaternion.identity);
        
        // 파티클 시스템 컴포넌트 확인 및 재생
        ParticleSystem particleSystem = currentFlashEffect.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
            Debug.Log($"[{gameObject.name} ShowFlashEffectPrefab] 파티클 시스템 재생 시작", gameObject);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name} ShowFlashEffectPrefab] 파티클 시스템 컴포넌트가 없음", gameObject);
        }
        
        // 자동 삭제 (1초 후)
        Destroy(currentFlashEffect, 2.0f);
    }

    // 죽음 처리 함수
    void Die()
    {
        Debug.Log($"ANT {gameObject.name} IS DYING!", gameObject);
        if (chitinScrapPrefab != null && Random.value <= dropChance)
        {
            int dropAmount = Random.Range(minScrapDrop, maxScrapDrop + 1);
            for (int i = 0; i < dropAmount; i++)
            {
                Vector3 dropPosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                Instantiate(chitinScrapPrefab, dropPosition, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }
    
    // 디버깅용: Update 함수에서 키 입력으로 테스트 (필요시 주석 해제)
    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestFlashEffect();
        }
    }
    */
}