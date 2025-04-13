using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 사용

public class HouseHealthUIUpdater : MonoBehaviour
{
    // 연결할 TextMeshPro UI 요소
    public TextMeshProUGUI healthText;

    // 추적할 HouseHealth 스크립트 (인스펙터에서 연결)
    public HouseHealth targetHouseHealth;

    // 스크립트가 활성화될 때 호출
    void OnEnable()
    {
        if (targetHouseHealth != null)
        {
            // HouseHealth의 OnHealthChanged 이벤트에 UpdateHealthText 함수를 구독(연결)
            targetHouseHealth.OnHealthChanged += UpdateHealthText;
            // 초기 체력 값을 표시하기 위해 즉시 한 번 호출
            UpdateHealthText(targetHouseHealth.currentHealth, targetHouseHealth.maxHealth);
        }
        else
        {
            Debug.LogWarning("HouseHealthUIUpdater: Target House Health가 연결되지 않았습니다.", this);
            // 대상이 없으면 텍스트를 비활성화하거나 기본 메시지 표시
            if (healthText != null) healthText.text = "N/A";
        }
    }

    // 스크립트가 비활성화될 때 호출
    void OnDisable()
    {
        if (targetHouseHealth != null)
        {
            // 메모리 누수 방지를 위해 이벤트 구독 해제
            targetHouseHealth.OnHealthChanged -= UpdateHealthText;
        }
    }

    // 체력 텍스트를 업데이트하는 함수 (이벤트 핸들러)
    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            // 텍스트 형식 지정 (예: "HP: 75 / 100")
            healthText.text = $"HP: {currentHealth} / {maxHealth}";
            // 또는 아이콘 옆에 숫자만 표시: healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
}