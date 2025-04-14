using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq 사용을 위해 추가 (FirstOrDefault 등)

// 날짜별 스폰 설정을 저장할 클래스
[System.Serializable]
public class NightSpawnSettings
{
    [Tooltip("이 설정이 적용될 시작 날짜")]
    public int startDay = 1;
    [Tooltip("이 설정이 적용될 마지막 날짜 (startDay와 같으면 해당 날짜 하루만 적용)")]
    public int endDay = 1;

    [Tooltip("이 밤에 스폰할 총 개미 수")]
    public int totalAntsToSpawn = 10;
    [Tooltip("이 밤의 개미 스폰 간격 (초)")]
    public float spawnInterval = 5.0f;
    [Tooltip("이 밤에 사용할 스폰 위치들 (Transform 리스트)")]
    public List<Transform> spawnPoints; // 기존 Transform[] 대신 List<Transform> 사용
}
public class BugSpawner : MonoBehaviour
{
    [Header("기본 설정")]
    [Tooltip("생성할 개미 프리팹")]
    public GameObject antPrefab;

    // --- 수정: 단일 설정 대신 날짜별 설정 리스트 사용 ---
    [Header("날짜별 스폰 설정")]
    [Tooltip("날짜별 스폰 설정을 추가하세요. 날짜 범위가 겹치지 않도록 주의하세요.")]
    public List<NightSpawnSettings> nightSettingsList;

    [Tooltip("위 리스트에 해당하지 않는 밤에 적용될 기본 설정")]
    public NightSpawnSettings defaultNightSettings; // 기본값 설정

    // --- Private Variables ---
    private NightSpawnSettings currentNightSettings; // 현재 밤에 적용될 설정
    private int antsSpawnedThisNight = 0;   // 이번 밤에 스폰된 개미 수
    private float nextSpawnTime = 0f;     // 다음 스폰 시간
    private int antCounter = 0;           // 생성된 개미 고유 번호 카운터

    void Start()
    {
        // TimeManager 이벤트 구독
        TimeManager.OnNightStart += HandleNightStart;
        TimeManager.OnDayStart += HandleDayStart; // 낮 시작 시 스폰 중지 및 리셋

        // 초기 상태 설정 (게임 시작 시 낮일 수 있으므로)
        HandleDayStart(); // 초기에는 스폰하지 않도록 설정
    }

    void OnDestroy() // 또는 OnDisable
    {
        // 게임 오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
        TimeManager.OnNightStart -= HandleNightStart;
        TimeManager.OnDayStart -= HandleDayStart;
    }

    // 밤 시작 시 호출될 함수
    void HandleNightStart()
    {
        Debug.Log($"[{gameObject.name}] Night started for Day {TimeManager.Instance.currentDay}. Applying spawn settings.");
        antsSpawnedThisNight = 0; // 밤 시작 시 스폰 카운트 초기화
        currentNightSettings = GetSettingsForDay(TimeManager.Instance.currentDay); // 현재 날짜에 맞는 설정 가져오기

        if (currentNightSettings == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Day {TimeManager.Instance.currentDay}에 해당하는 NightSpawnSettings를 찾을 수 없습니다. 기본 설정을 사용합니다.");
            currentNightSettings = defaultNightSettings; // 해당 날짜 설정 없으면 기본 설정 사용
        }

        if (currentNightSettings != null && currentNightSettings.totalAntsToSpawn > 0)
        {
            // 첫 스폰은 즉시 또는 약간의 딜레이 후 시작하도록 설정 (선택 사항)
            // nextSpawnTime = Time.time; // 즉시 시작
            nextSpawnTime = Time.time + Random.Range(0f, currentNightSettings.spawnInterval * 0.5f); // 약간 랜덤한 첫 스폰 딜레이
            Debug.Log($"[{gameObject.name}] Settings applied: TotalAnts={currentNightSettings.totalAntsToSpawn}, Interval={currentNightSettings.spawnInterval}, SpawnPoints Count={currentNightSettings.spawnPoints?.Count ?? 0}");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 현재 밤({TimeManager.Instance.currentDay}일차) 또는 기본 설정에 스폰할 개미가 없거나 설정이 없습니다.");
            currentNightSettings = null; // 스폰할 필요 없으면 null로 설정
        }
    }

    // 낮 시작 시 호출될 함수
    void HandleDayStart()
    {
        Debug.Log($"[{gameObject.name}] Day started. Stopping ant spawn for now.");
        currentNightSettings = null; // 낮에는 스폰 설정 비활성화
        antsSpawnedThisNight = 0;   // 스폰 카운트 리셋
    }

    // 현재 날짜에 맞는 설정을 찾는 함수
    NightSpawnSettings GetSettingsForDay(int currentDay)
    {
        // Linq를 사용하여 현재 날짜가 startDay와 endDay 사이에 있는 첫 번째 설정을 찾음
        return nightSettingsList.FirstOrDefault(settings => currentDay >= settings.startDay && currentDay <= settings.endDay);
        // 만약 Linq를 사용하지 않으려면:
        /*
        foreach (NightSpawnSettings settings in nightSettingsList)
        {
            if (currentDay >= settings.startDay && currentDay <= settings.endDay)
            {
                return settings;
            }
        }
        return null; // 맞는 설정이 없으면 null 반환
        */
    }

    void Update()
    {
        // 현재 밤이고, 적용할 설정이 있으며, 아직 스폰할 개미가 남았는지 확인
        if (TimeManager.Instance != null && TimeManager.Instance.isNight && currentNightSettings != null && antsSpawnedThisNight < currentNightSettings.totalAntsToSpawn)
        {
            // 다음 스폰 시간이 되었는지 확인
            if (Time.time >= nextSpawnTime)
            {
                SpawnAnt(); // 개미 생성
                antsSpawnedThisNight++; // 스폰된 개미 수 증가
                // 다음 스폰 시간 계산
                nextSpawnTime = Time.time + currentNightSettings.spawnInterval;

                // 이번 스폰으로 목표치를 채웠는지 확인
                if (antsSpawnedThisNight >= currentNightSettings.totalAntsToSpawn)
                {
                     Debug.Log($"[{gameObject.name}] Day {TimeManager.Instance.currentDay} 밤의 목표 개미 수({currentNightSettings.totalAntsToSpawn}) 스폰 완료.");
                     // 선택: 여기서 currentNightSettings = null; 로 설정하여 더 이상 Update에서 체크 안하게 할 수도 있음
                }
            }
        }
    }

    void SpawnAnt()
    {
        // 현재 밤 설정과 스폰 위치 리스트 유효성 검사
        if (antPrefab == null || currentNightSettings == null || currentNightSettings.spawnPoints == null || currentNightSettings.spawnPoints.Count == 0)
        {
            Debug.LogError($"[{gameObject.name}] SpawnAnt 실패: Prefab({antPrefab != null}), CurrentSettings({currentNightSettings != null}), SpawnPoints({currentNightSettings?.spawnPoints?.Count ?? 0}) 중 하나 이상이 유효하지 않습니다.");
            // 스폰 실패 시 무한 루프 방지를 위해 다음 스폰 시간을 강제로 늦춤 (선택적)
            nextSpawnTime = Time.time + (currentNightSettings?.spawnInterval ?? defaultNightSettings?.spawnInterval ?? 5.0f);
            return;
        }

        // 현재 설정에 지정된 스폰 위치 리스트에서 랜덤하게 하나 선택
        int randomIndex = Random.Range(0, currentNightSettings.spawnPoints.Count);
        Transform selectedSpawnPoint = currentNightSettings.spawnPoints[randomIndex];

        // 선택된 스폰 위치가 유효한지 확인 (리스트 중간에 null이 들어간 경우 대비)
        if (selectedSpawnPoint != null)
        {
            GameObject newAnt = Instantiate(antPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
            antCounter++;
            newAnt.name = $"Ant_{antCounter}";
            // Debug.Log($"Generated: {newAnt.name} at {selectedSpawnPoint.name} ({selectedSpawnPoint.position})"); // 이전 로그 레벨 조정
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 현재 밤 설정의 SpawnPoints 리스트 내 {randomIndex} 인덱스가 null입니다. 스폰을 건너<0xEB>뜁니다.");
            // 선택적: 이 경우에도 nextSpawnTime을 늦춰서 무한 시도 방지
             nextSpawnTime = Time.time + currentNightSettings.spawnInterval;
        }
    }
}