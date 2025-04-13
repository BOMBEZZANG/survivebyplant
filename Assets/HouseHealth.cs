using UnityEngine;
using System; // Action을 사용하기 위해 추가

public class HouseHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    // 체력이 변경될 때 호출될 이벤트 정의 (현재 체력, 최대 체력 전달)
    public event Action<int, int> OnHealthChanged;

    void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name}의 초기 체력이 설정되었습니다: {currentHealth}/{maxHealth}");
        // 초기 체력 상태를 UI 등에 알리기 위해 이벤트 호출
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;
        if (damageAmount < 0) damageAmount = 0;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name}이(가) {damageAmount}의 피해를 입었습니다. 현재 체력: {currentHealth}/{maxHealth}");

        // 체력 변경 시 이벤트 호출
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name}이(가) 파괴되었습니다!");
        Destroy(gameObject);
        // 파괴 시에도 체력 변경 이벤트 호출 (0이 된 상태 알림)
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(int healAmount)
    {
         if (currentHealth <= 0 || healAmount <= 0) return;

         currentHealth += healAmount;
         currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
         Debug.Log($"{gameObject.name}이(가) {healAmount}만큼 수리되었습니다. 현재 체력: {currentHealth}/{maxHealth}");
         // 체력 변경 시 이벤트 호출
         OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}