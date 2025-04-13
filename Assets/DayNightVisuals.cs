using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D 사용을 위해 필요

public class DayNightVisuals : MonoBehaviour
{
    [Tooltip("제어할 전역 조명 (Global Light 2D)")]
    public Light2D globalLight; // Inspector에서 연결

    [Tooltip("낮의 조명 색상")]
    public Color dayColor = Color.white;
    [Tooltip("밤의 조명 색상")]
    public Color nightColor = new Color(0.2f, 0.2f, 0.4f, 1f); // 약간 어두운 파란/보라색

    [Tooltip("색상 전환 속도")]
    public float transitionSpeed = 1.0f;

    void Start()
    {
        // 시작 시 globalLight 할당 확인
        if (globalLight == null)
        {
            Debug.LogError("DayNightVisuals: Global Light 2D가 Inspector에 연결되지 않았습니다!");
            enabled = false; // 비활성화
        }
    }

    void Update()
    {
        // TimeManager 인스턴스가 없을 경우 중단
        if (TimeManager.Instance == null) return;

        // 목표 색상 결정 (밤이면 nightColor, 낮이면 dayColor)
        Color targetColor = TimeManager.Instance.isNight ? nightColor : dayColor;

        // 현재 색상에서 목표 색상으로 부드럽게 변경 (Lerp 사용)
        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * transitionSpeed);
    }
}