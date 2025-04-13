using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // --- Inspector 설정 변수 ---
    [Header("시간 설정")]
    [Tooltip("게임 내 하루가 현실 시간으로 몇 초인지 설정 (예: 12분 = 720초)")]
    public float secondsPerIngameDay = 720f;
    [Tooltip("밤이 시작되는 시간 (0.0 ~ 1.0 사이 값, 예: 0.75 = 저녁 6시)")]
    public float nightStartTime = 0.75f;
    [Tooltip("낮이 시작되는 시간 (0.0 ~ 1.0 사이 값, 예: 0.25 = 오전 6시)")]
    public float dayStartTime = 0.25f;

    [Header("시작 시간")]
    [Range(0f, 1f)]
    [Tooltip("게임 시작 시 초기 시간 (0.0 ~ 1.0, 예: 0.3 = 오전 7시 12분쯤)")]
    public float initialTimeOfDay = 0.3f;

    // --- 현재 시간 정보 ---
    [Header("현재 상태 (읽기 전용)")]
    [Range(0f, 1f)]
    public float currentTimeOfDay01; // 0.0 (자정) ~ 1.0 (다음날 자정 직전)
    public int currentDay = 1;
    public bool isNight = false;

    // --- 싱글톤 인스턴스 ---
    public static TimeManager Instance { get; private set; }

    // --- 상태 변경 이벤트 ---
    public static event Action OnDayStart;
    public static event Action OnNightStart;

    private bool wasNight = false;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; /* DontDestroyOnLoad(gameObject); 선택 사항 */ }

        // 초기 시간 및 상태 설정
        currentTimeOfDay01 = initialTimeOfDay;
        isNight = IsNightTime(currentTimeOfDay01);
        wasNight = isNight;
        Debug.Log($"TimeManager Initialized. Day {currentDay}, Time {GetTimeAsString()}, isNight = {isNight}");
    }

    void Update()
    {
        UpdateTime();
        CheckStateChange();
    }

    void UpdateTime()
    {
        if (secondsPerIngameDay <= 0) return;
        currentTimeOfDay01 += Time.deltaTime / secondsPerIngameDay;

        // 하루가 지났는지 체크
        if (currentTimeOfDay01 >= 1f)
        {
            currentTimeOfDay01 -= 1f; // 시간 재설정 (0.0 ~ 0.999...)
            currentDay++;
            Debug.Log($"Day {currentDay} Started.");
            OnDayStart?.Invoke(); // 날짜 변경 시 OnDayStart 호출 (밤->낮 전환도 여기서 처리될 수 있음)
        }
    }

    void CheckStateChange()
    {
        isNight = IsNightTime(currentTimeOfDay01);

        if (!wasNight && isNight) // 낮 -> 밤 전환 감지
        {
            wasNight = true; // 상태 업데이트 먼저
            Debug.Log($"Night Started (Day {currentDay}). Time: {GetTimeAsString()}");
            OnNightStart?.Invoke();
        }
        else if (wasNight && !isNight) // 밤 -> 낮 전환 감지
        {
            wasNight = false; // 상태 업데이트 먼저
            // OnDayStart는 날짜 변경 시 호출되므로, 여기서 별도 호출은 불필요할 수 있음
            // 필요하다면 여기서도 OnDayStart?.Invoke(); 호출 가능
             Debug.Log($"Day Started (Day {currentDay}). Time: {GetTimeAsString()}");
        }
        // wasNight = isNight; // 상태 변경은 전환 감지 후 마지막에 하는 것이 더 명확할 수 있음
    }

    // 현재 시간(0~1)이 밤 시간대인지 판별
    bool IsNightTime(float time01)
    {
        // 밤 시작 시간(예: 0.75) 이후 이거나, 또는 낮 시작 시간(예: 0.25) 이전이면 밤
        return time01 >= nightStartTime || time01 < dayStartTime;
    }

    /// <summary>
    /// 현재 게임 시간을 "HH:MM" 형식의 문자열로 반환합니다.
    /// </summary>
    /// <returns>시간 문자열 (예: "14:30")</returns>
    public string GetTimeAsString()
    {
         // currentTimeOfDay01 (0~1) 값을 24시간 * 60분 기준으로 변환
         float timeInMinutes = currentTimeOfDay01 * 24 * 60;
         // 시간 계산 (24시간 주기)
         int hour = Mathf.FloorToInt(timeInMinutes / 60) % 24;
         // 분 계산
         int minute = Mathf.FloorToInt(timeInMinutes % 60);
         // "HH:MM" 형식으로 포맷하여 반환 (두 자리 수 보장)
         return $"{hour:00}:{minute:00}";
    }
}