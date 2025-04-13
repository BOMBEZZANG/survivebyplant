using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 필요

public class TimeIndicatorFill : MonoBehaviour
{
    [Tooltip("채워질 UI Image 컴포넌트 - Inspector에서 연결 필수")]
    public Image fillImage;

    void Start()
    {
        // 시작 시 fillImage 할당 확인
        if (fillImage == null)
        {
            Debug.LogError("TimeIndicatorFill: Fill Image가 Inspector에 연결되지 않았습니다!", this.gameObject);
            enabled = false; // 비활성화
            return;
        }
        // Image Type이 Filled인지 확인 (선택 사항)
        if (fillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("TimeIndicatorFill: 연결된 Image의 Image Type이 'Filled'가 아닙니다. Fill Amount가 작동하지 않을 수 있습니다.", this.gameObject);
        }
    }

    void Update()
    {
        // TimeManager 인스턴스가 있고, fillImage가 연결되어 있을 때만 실행
        if (TimeManager.Instance != null && fillImage != null)
        {
            // TimeManager의 현재 시간(0~1) 값을 가져와서 Fill Amount에 직접 할당
            fillImage.fillAmount = TimeManager.Instance.currentTimeOfDay01;
        }
    }
}