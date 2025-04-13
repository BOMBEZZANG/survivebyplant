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

    // ===>>> 추가: 게임 시작 시 초기 시간 설정 <<<===
    [Header("시작 시간")]
    [Range(0f, 1f)]
    [Tooltip("게임 시작 시 초기 시간 (0.0 ~ 1.0, 예: 0.3 = 오전 7시 12분쯤)")]
    public float initialTimeOfDay = 0.3f; // 예: 아침 7시 12분

    // --- 현재 시간 정보 ---
    [Header("현재 상태 (읽기 전용)")]
    [Range(0f, 1f)]
    public float currentTimeOfDay01; // 초기값 설정을 Awake에서 하므로 여기선 제거
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
        else { Instance = this; /* DontDestroyOnLoad(gameObject); */ }

        // ===>>> 수정: 초기 시간을 Inspector 값으로 설정 <<<===
        currentTimeOfDay01 = initialTimeOfDay;

        // 초기 isNight 상태 계산
        isNight = IsNightTime(currentTimeOfDay01);
        wasNight = isNight;

        // ===>>> 추가: 초기 상태 확인 로그 <<<===
        Debug.Log($"TimeManager Initialized. currentDay = {currentDay}, Initial timeOfDay01 = {currentTimeOfDay01}, isNight = {isNight}");
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

        if (currentTimeOfDay01 >= 1f)
        {
            currentTimeOfDay01 -= 1f;
            currentDay++;
            // isNight = IsNightTime(currentTimeOfDay01); // 상태 변경은 CheckStateChange에서
            Debug.Log($"Day {currentDay} Started.");
            OnDayStart?.Invoke(); // 날짜 변경 시 OnDayStart 호출
        }
    }

    void CheckStateChange()
    {
        isNight = IsNightTime(currentTimeOfDay01);

        if (!wasNight && isNight) // 낮 -> 밤
        {
            Debug.Log($"Night Started (Day {currentDay}).");
            OnNightStart?.Invoke();
        }
        else if (wasNight && !isNight) // 밤 -> 낮
        {
             // 날짜 변경 시 OnDayStart가 호출되므로 여기서 또 호출할 필요 없음
             // Debug.Log($"Day Started (Day {currentDay}).");
        }
        wasNight = isNight; // 이전 상태 업데이트
    }

    bool IsNightTime(float time01)
    {
        // 밤 시작 시간 이후 이거나, 낮 시작 시간 이전이면 밤
        return time01 >= nightStartTime || time01 < dayStartTime;
    }

    public string GetTimeAsString()
    {
         float timeInMinutes = currentTimeOfDay01 * 24 * 60;
         int hour = Mathf.FloorToInt(timeInMinutes / 60) % 24;
         int minute = Mathf.FloorToInt(timeInMinutes % 60);
         return $"{hour:00}:{minute:00}";
    }
}