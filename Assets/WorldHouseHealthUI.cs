using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 사용

public class WorldHouseHealthUI : MonoBehaviour
{
    // 자식 오브젝트에 있는 TextMeshPro 컴포넌트 (인스펙터 또는 코드로 찾기)
    public TextMeshProUGUI healthText;

    // 이 스크립트가 붙어있는 게임 오브젝트의 HouseHealth 컴포넌트
    private HouseHealth targetHouseHealth;

    void Awake()
    {
        // 같은 게임 오브젝트에 있는 HouseHealth 컴포넌트를 찾습니다.
        targetHouseHealth = GetComponent<HouseHealth>();

        // 만약 healthText가 인스펙터에서 할당되지 않았다면 자식에서 찾습니다.
        if (healthText == null)
        {
            healthText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (targetHouseHealth == null)
        {
            Debug.LogWarning("WorldHouseHealthUI: 부모 오브젝트에서 HouseHealth 컴포넌트를 찾을 수 없습니다.", this);
            if (healthText != null) healthText.enabled = false; // 체력 정보 없으면 텍스트 숨김
            enabled = false; // 스크립트 비활성화
        }
        if (healthText == null)
        {
             Debug.LogWarning("WorldHouseHealthUI: 자식 오브젝트에서 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.", this);
             enabled = false; // 스크립트 비활성화
        }
    }

    void OnEnable()
    {
        if (targetHouseHealth != null)
        {
            targetHouseHealth.OnHealthChanged += UpdateHealthText;
            // 초기값 설정
            UpdateHealthText(targetHouseHealth.currentHealth, targetHouseHealth.maxHealth);
        }
    }

    void OnDisable()
    {
        if (targetHouseHealth != null)
        {
            targetHouseHealth.OnHealthChanged -= UpdateHealthText;
        }
    }

    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            // 예: "75 / 100" 형식으로 표시
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
}