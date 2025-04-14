using UnityEngine;
using UnityEngine.UI; // Slider 사용을 위해 필요

public class PlantDurabilityUI : MonoBehaviour
{
    private Slider durabilitySlider;
    private CarnivorousPlant targetPlant;
    private Image fillImage; // 슬라이더의 Fill 이미지 참조 추가

    [Tooltip("체력이 낮을 때 색상")]
    public Color lowHealthColor = Color.red;
    [Tooltip("체력이 높을 때 색상")]
    public Color highHealthColor = Color.green;
    [Tooltip("색상 변화 임계값 (0-1 사이)")]
    [Range(0f, 1f)]
    public float colorThreshold = 0.3f;

    void Awake()
    {
        // 자기 자신 또는 자식에서 Slider 컴포넌트 찾기
        durabilitySlider = GetComponent<Slider>();
        if (durabilitySlider == null)
        {
            durabilitySlider = GetComponentInChildren<Slider>();
            if (durabilitySlider == null)
            {
                Debug.LogError($"[{gameObject.name}] PlantDurabilityUI Error: Slider 컴포넌트를 찾을 수 없습니다! 자식에서도 찾지 못했습니다.", gameObject);
                // enabled = false; // 비활성화하지 않고 오류만 보고
                return;
            }
        }

        Debug.Log($"[{gameObject.name}] PlantDurabilityUI: Slider 컴포넌트를 찾았습니다.");

        // Fill 이미지 참조 찾기
        if (durabilitySlider.fillRect != null)
        {
            fillImage = durabilitySlider.fillRect.GetComponent<Image>();
            if (fillImage == null)
            {
                Debug.LogWarning($"[{gameObject.name}] PlantDurabilityUI Warning: 슬라이더의 Fill 이미지를 찾을 수 없습니다!", gameObject);
            }
            else
            {
                Debug.Log($"[{gameObject.name}] PlantDurabilityUI: Fill 이미지를 찾았습니다. 현재 색상: {fillImage.color}");
                // 초기 색상 설정
                fillImage.color = highHealthColor;
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlantDurabilityUI Warning: 슬라이더의 fillRect가 null입니다!", gameObject);
        }

        // 슬라이더 초기 설정
        durabilitySlider.minValue = 0f;
        durabilitySlider.maxValue = 1f;
        durabilitySlider.value = 1f; // 기본값으로 꽉 찬 상태로 설정
        Debug.Log($"[{gameObject.name}] PlantDurabilityUI: 슬라이더 초기화 완료. 값: {durabilitySlider.value}");
    }

    void Start()
    {
        // UI 위치를 카메라 기준으로 고정 (월드 스페이스 캔버스인 경우)
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.Log($"[{gameObject.name}] PlantDurabilityUI: WorldSpace Canvas를 찾았습니다. 회전을 카메라에 맞춥니다.");
            FixUIRotation();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] PlantDurabilityUI: WorldSpace Canvas가 아니거나 Canvas를 찾지 못했습니다.");
        }
    }

    // 카메라 방향으로 UI를 고정하는 함수
    private void FixUIRotation()
    {
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
            Debug.Log($"[{gameObject.name}] PlantDurabilityUI: UI 회전을 카메라 방향으로 설정했습니다.");
        }
    }

    // 매 프레임 UI 회전 고정 (필요한 경우)
    void LateUpdate()
    {
        FixUIRotation();
    }

    /// <summary>
    /// 대상 식물을 설정하고 이벤트 구독 및 초기값 설정을 수행합니다.
    /// CarnivorousPlant 스크립트에서 호출됩니다.
    /// </summary>
    public void Initialize(CarnivorousPlant plant)
    {
        Debug.Log($"[{gameObject.name}] PlantDurabilityUI: Initialize 호출됨");
        
        targetPlant = plant;
        if (targetPlant != null)
        {
            // 식물의 내구도 변경 이벤트 구독
            targetPlant.OnDurabilityChanged += UpdateDurabilityBar;
            
            // 현재 내구도 비율로 슬라이더 초기값 설정
            float ratio = targetPlant.GetCurrentDurabilityRatio();
            UpdateDurabilityBar(ratio);
            
            Debug.Log($"[{gameObject.name}] PlantDurabilityUI: {targetPlant.name}에 초기화됨. 초기 비율: {ratio}");
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] PlantDurabilityUI Initialize Error: Target Plant가 null입니다!", gameObject);
            // gameObject.SetActive(false); // 비활성화하지 않고 오류만 보고
        }
    }

    /// <summary>
    /// 식물의 내구도 변경 이벤트가 발생했을 때 호출될 함수
    /// </summary>
    private void UpdateDurabilityBar(float ratio)
    {
        if (durabilitySlider == null)
        {
            Debug.LogError($"[{gameObject.name}] PlantDurabilityUI Error: durabilitySlider가 null입니다!", gameObject);
            return;
        }
        
        durabilitySlider.value = ratio; // 슬라이더 값 업데이트
        
        // Fill 이미지 색상 변경 (색상 보간)
        if (fillImage != null)
        {
            if (ratio <= colorThreshold)
            {
                fillImage.color = lowHealthColor; // 낮은 체력 색상
            }
            else
            {
                // 체력 비율에 따라 색상 보간
                float t = (ratio - colorThreshold) / (1f - colorThreshold);
                fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, t);
            }
            
            Debug.Log($"[{gameObject.name}] PlantDurabilityUI: Fill 색상 변경: {fillImage.color}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlantDurabilityUI Warning: fillImage가 null입니다!", gameObject);
        }
        
        Debug.Log($"[{gameObject.name}] PlantDurabilityUI: 슬라이더 값 업데이트: {ratio}");
    }

    /// <summary>
    /// 이 UI 오브젝트가 파괴될 때 호출됩니다.
    /// </summary>
    void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 구독 해제
        if (targetPlant != null)
        {
            targetPlant.OnDurabilityChanged -= UpdateDurabilityBar;
            Debug.Log($"[{gameObject.name}] PlantDurabilityUI: {targetPlant.name}의 OnDurabilityChanged 이벤트 구독 해제");
        }
    }
}