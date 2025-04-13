using UnityEngine;

public class AntHealth : MonoBehaviour
{
    [Header("기본 설정")]
    public int maxHealth = 10; // 개미의 최대 체력
    public int currentHealth;  // 현재 체력

    [Header("사망 시 드롭 아이템")]
    [Tooltip("죽을 때 떨어뜨릴 키틴 조각 프리팹")]
    public GameObject chitinScrapPrefab; // Inspector에서 ChitinScrapPickup 프리팹 연결
    [Tooltip("떨어뜨릴 최소 개수")]
    public int minScrapDrop = 1;
    [Tooltip("떨어뜨릴 최대 개수")]
    public int maxScrapDrop = 1; // 일단 1개만 떨어뜨리도록 설정 (조절 가능)

    void Start()
    {
        currentHealth = maxHealth;
        // Debug.Log($"Ant {gameObject.name} Initialized. Health: {currentHealth}"); // 필요시 주석 해제
    }

    // 데미지를 받는 함수 (식물로부터 호출됨)
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // 이미 죽었으면 무시

        currentHealth -= damage;
        // 데미지 로그는 PlayerInventory에서 확인하므로 여기선 생략 가능
        // Debug.Log($"[{gameObject.name}] 개미가 {damage} 피해를 입음. 남은 체력: {currentHealth}/{maxHealth}", gameObject);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 개미 죽음 처리
    void Die()
    {
        // 사망 로그 (어떤 개미가 죽는지 확인)
        Debug.LogError($"ANT {gameObject.name} IS DYING!", gameObject);

        // ===>>> 추가: 아이템 드롭 로직 <<<===
        if (chitinScrapPrefab != null)
        {
            // 떨어뜨릴 개수 랜덤 결정 (min ~ max 사이)
            int dropAmount = Random.Range(minScrapDrop, maxScrapDrop + 1);
            Debug.Log($"[{gameObject.name}] Dropping {dropAmount} Chitin Scrap(s).");

            for (int i = 0; i < dropAmount; i++)
            {
                // 죽은 위치 주변에 약간 랜덤하게 생성
                Vector3 dropPosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                Instantiate(chitinScrapPrefab, dropPosition, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Chitin Scrap Prefab is not assigned, cannot drop item.");
        }
        // ===>>> 추가 끝 <<<===

        // 여기에 죽는 이펙트(파티클 등)나 사운드 추가 가능

        Destroy(gameObject); // 개미 오브젝트 제거
    }
}