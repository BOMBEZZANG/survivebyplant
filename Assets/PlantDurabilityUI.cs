using UnityEngine;
using UnityEngine.UI;

public class PlantDurabilityUI : MonoBehaviour
{
    private Slider durabilitySlider;
    private Image fillImage;
    private CarnivorousPlant targetPlant;

    [Header("색상 설정")]
    public Color lowHealthColor = Color.red;
    public Color highHealthColor = Color.green;
    [Range(0f, 1f)]
    public float colorThreshold = 0.3f;

    [Header("디버그")]
    public bool enableDetailedLogs = true;

    void Awake()
    {
        // 슬라이더 컴포넌트 찾기
        durabilitySlider = GetComponentInChildren<Slider>();
        if (durabilitySlider == null)
        {
            Debug.LogError($"[{gameObject.name}] Slider 컴포넌트를 찾을 수 없습니다!", gameObject);
            enabled = false;
            return;
        }

        LogMessage($"Slider 컴포넌트 찾음: {durabilitySlider.name}");

        // Fill 이미지 찾기
        if (durabilitySlider.fillRect != null)
        {
            fillImage = durabilitySlider.fillRect.GetComponent<Image>();
            LogMessage($"fillRect 확인: {durabilitySlider.fillRect.gameObject.name}");
        }
        else
        {
            LogMessage("fillRect가 null입니다. 슬라이더의 Fill Rect 설정을 확인하세요.");
        }

        if (fillImage == null)
        {
            // 대체 방법: Fill Area에서 Fill 오브젝트 찾기
            Transform fillArea = durabilitySlider.transform.Find("Fill Area");
            if (fillArea != null)
            {
                Transform fill = fillArea.Find("Fill");
                if (fill != null)
                {
                    fillImage = fill.GetComponent<Image>();
                    LogMessage($"대체 방법으로 Fill 이미지 찾음: {(fillImage != null ? fillImage.gameObject.name : "null")}");
                }
            }

            if (fillImage == null)
            {
                Debug.LogError($"[{gameObject.name}] Fill 이미지를 찾을 수 없습니다! Slider의 Fill Rect와 계층 구조(Fill Area/Fill)를 확인하세요.", gameObject);
            }
        }
        else
        {
            LogMessage($"Fill 이미지 찾음: {fillImage.gameObject.name}");
        }

        // 슬라이더 초기 설정
        durabilitySlider.minValue = 0f;
        durabilitySlider.maxValue = 1f;
        durabilitySlider.value = 1f;

        if (fillImage != null)
        {
            fillImage.color = highHealthColor;
            fillImage.enabled = true; // Fill 이미지가 비활성화되어 있을 경우 강제 활성화
            LogMessage($"Fill 이미지 초기 색상 설정: {highHealthColor}");
        }
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }

    public void Initialize(CarnivorousPlant plant)
    {
        LogMessage($"Initialize 호출됨: {(plant != null ? plant.name : "null")}");

        if (targetPlant != null)
        {
            targetPlant.OnDurabilityChanged -= UpdateDurabilityBar;
            LogMessage($"기존 식물 {targetPlant.name}과의 연결 해제");
        }

        targetPlant = plant;
        if (targetPlant != null)
        {
            targetPlant.OnDurabilityChanged += UpdateDurabilityBar;
            float ratio = targetPlant.GetCurrentDurabilityRatio();
            LogMessage($"식물 {targetPlant.name}에 연결 완료, 초기 내구도 비율: {ratio:F2}");
            UpdateDurabilityBar(ratio);
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Initialize에 null 식물이 전달되었습니다!", gameObject);
        }
    }

    private void UpdateDurabilityBar(float ratio)
    {
        LogMessage($"UpdateDurabilityBar 호출됨: {ratio:F2}");

        if (durabilitySlider == null)
        {
            Debug.LogError($"[{gameObject.name}] Slider가 null입니다!", gameObject);
            return;
        }

        // 슬라이더 값 업데이트
        durabilitySlider.value = ratio;
        LogMessage($"슬라이더 value 설정: {ratio:F2}");

        // Fill 이미지 색상 업데이트
        if (fillImage != null)
        {
            Color newColor;
            if (ratio <= colorThreshold)
            {
                newColor = lowHealthColor;
            }
            else
            {
                float t = (ratio - colorThreshold) / (1f - colorThreshold);
                newColor = Color.Lerp(lowHealthColor, highHealthColor, t);
            }
            fillImage.color = newColor;
            LogMessage($"Fill 색상 변경: {newColor}, 비율: {ratio:F2}");

            // 디버깅: Fill Rect 크기 확인
            RectTransform fillRect = durabilitySlider.fillRect;
            if (fillRect != null)
            {
                LogMessage($"Fill Rect 크기: {fillRect.rect.width}x{fillRect.rect.height}, 스케일: {fillRect.localScale}");
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Fill Image가 null입니다!", gameObject);
        }
    }

    private void LogMessage(string message)
    {
        if (enableDetailedLogs)
        {
            Debug.Log($"[{gameObject.name}] PlantDurabilityUI: {message}");
        }
    }

    void OnDestroy()
    {
        if (targetPlant != null)
        {
            targetPlant.OnDurabilityChanged -= UpdateDurabilityBar;
            LogMessage($"{targetPlant.name}의 이벤트 구독 해제");
        }
    }
}