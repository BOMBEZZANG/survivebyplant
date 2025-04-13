using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 사용

public class ClockUI : MonoBehaviour
{
    [Tooltip("시간을 표시할 TextMeshProUGUI 컴포넌트 - Inspector에서 연결 필수")]
    public TextMeshProUGUI clockText;

    void Start()
    {
        // 시작 시 clockText 할당 확인
        if (clockText == null)
        {
            Debug.LogError("ClockUI: Clock Text (TextMeshProUGUI)가 Inspector에 연결되지 않았습니다!", this.gameObject);
            enabled = false; // 비활성화
        }
    }

    void Update()
    {
        // TimeManager 인스턴스가 있고, clockText가 연결되어 있을 때만 실행
        if (TimeManager.Instance != null && clockText != null)
        {
            // TimeManager에서 현재 시간을 "HH:MM" 형식 문자열로 가져옴
            string timeString = TimeManager.Instance.GetTimeAsString();
            // TextMeshProUGUI의 text 속성을 업데이트
            clockText.text = timeString;
        }
    }
}