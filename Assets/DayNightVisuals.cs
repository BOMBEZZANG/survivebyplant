using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D 사용을 위해 필요

public class DayNightVisuals : MonoBehaviour
{
    [Header("References")]
    [Tooltip("제어할 전역 조명 (Global Light 2D) - Inspector에서 연결 필수")]
    public Light2D globalLight;

    [Header("Time-Based Settings")]
    [Tooltip("시간대별 조명 색상 변화를 정의합니다.")]
    public Gradient dayNightGradient; // 시간대별 색상

    [Tooltip("시간대별 조명 밝기(Intensity) 변화를 정의합니다.")]
    public AnimationCurve intensityCurve; // 시간대별 밝기 (0~1 범위로 사용 권장)

    [Range(0f, 1f)] // 인스펙터 편의를 위해 추가 (값을 0~1로 제한)
    [Tooltip("밝기 커브에서 가져온 값에 곱해질 최대 밝기 값")]
    public float maxIntensity = 1.0f; // 최종 밝기 조절용

    void Start()
    {
        // 시작 시 globalLight 할당 확인
        if (globalLight == null)
        {
            Debug.LogError("DayNightVisuals: Global Light 2D가 Inspector에 연결되지 않았습니다!", this.gameObject);
            enabled = false; // 비활성화
        }
    }

    void Update()
    {
        // TimeManager 인스턴스가 없거나 globalLight가 없으면 중단
        if (TimeManager.Instance == null || globalLight == null) return;

        // TimeManager에서 현재 시간(0~1) 값을 가져옴
        float time01 = TimeManager.Instance.currentTimeOfDay01;

        // Gradient에서 현재 시간에 맞는 색상을 가져와 적용
        globalLight.color = dayNightGradient.Evaluate(time01);

        // AnimationCurve에서 현재 시간에 맞는 밝기 비율(0~1)을 가져와 최종 밝기 적용
        // intensityCurve가 null이 아닐 경우에만 실행 (선택적 기능이므로)
        if (intensityCurve != null)
        {
            globalLight.intensity = intensityCurve.Evaluate(time01) * maxIntensity;
        }
        // 만약 intensityCurve를 사용하지 않으려면 위 if 블록을 삭제하거나 주석 처리하고,
        // 필요하다면 아래처럼 isNight 상태에 따라 간단히 밝기를 조절할 수도 있습니다.
        // else {
        //     globalLight.intensity = TimeManager.Instance.isNight ? 0.5f : 1.0f; // 예시: 밤이면 밝기 0.5
        // }
    }
}