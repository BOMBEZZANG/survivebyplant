using UnityEngine;

public class AntHealth : MonoBehaviour
{
    [Header("기본 설정")]
    public int maxHealth = 10; // 개미의 최대 체력
    public int currentHealth;  // 현재 체력

    [Header("사망 시 드롭 아이템")]
    [Tooltip("죽을 때 떨어뜨릴 키틴 조각 프리팹")]
    public GameObject chitinScrapPrefab;

    // ===>>> 수정: 확률 변수 추가 <<<===
    [Range(0f, 1f)] // Inspector에서 슬라이더로 0~1 사이 값 조절 가능
    [Tooltip("키틴 조각을 떨어뜨릴 확률 (0.0 = 0%, 0.5 = 50%, 1.0 = 100%)")]
    public float dropChance = 1.0f; // 기본값 100% (기존과 동일하게 시작)

    [Tooltip("확률 성공 시 떨어뜨릴 최소 개수")]
    public int minScrapDrop = 1; // 확률 성공 시 최소 1개
    [Tooltip("확률 성공 시 떨어뜨릴 최대 개수")]
    public int maxScrapDrop = 1; // 확률 성공 시 최대 1개 (조절 가능)

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"ANT {gameObject.name} IS DYING!", gameObject);

        // ===>>> 수정: 확률 체크 로직 추가 <<<===
        if (chitinScrapPrefab != null)
        {
            // 0.0 이상 1.0 미만의 랜덤 소수 생성하여 dropChance와 비교
            if (Random.value <= dropChance) // Random.value는 0.0 <= 값 < 1.0
            {
                // 확률 체크 통과 시, 기존 로직 실행
                int dropAmount = Random.Range(minScrapDrop, maxScrapDrop + 1); // 예: min=1, max=1이면 항상 1개
                Debug.Log($"[{gameObject.name}] Passed drop chance ({dropChance * 100}%). Dropping {dropAmount} Chitin Scrap(s).");

                for (int i = 0; i < dropAmount; i++)
                {
                    Vector3 dropPosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                    Instantiate(chitinScrapPrefab, dropPosition, Quaternion.identity);
                }
            }
            else
            {
                // 확률 체크 실패 시 로그 (필요시 주석 해제)
                // Debug.Log($"[{gameObject.name}] Failed drop chance ({dropChance * 100}%). No scrap dropped.");
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Chitin Scrap Prefab is not assigned, cannot drop item.");
        }
        // ===>>> 수정 끝 <<<===

        // 죽는 이펙트/사운드 등...

        Destroy(gameObject);
    }
}