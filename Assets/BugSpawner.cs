using UnityEngine;
using System.Collections.Generic; // 배열이나 리스트 길이를 확인하기 위해 추가 (선택 사항)

public class BugSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("생성할 개미 프리팹")]
    public GameObject antPrefab;       // Inspector에서 연결할 개미 프리팹

    // --- 수정: 단일 Transform 대신 Transform 배열 사용 ---
    [Tooltip("개미가 생성될 수 있는 위치들 (Empty GameObject들의 Transform 연결)")]
    public Transform[] spawnPoints;     // 여러 스폰 위치를 담을 배열

    [Tooltip("개미 생성 간격 (초)")]
    public float spawnInterval = 5.0f; // 개미 생성 주기

    // --- Private Variables ---
    private float nextSpawnTime = 0f;  // 다음 개미가 생성될 시간
    private int antCounter = 0;        // 생성된 개미 수를 세는 카운터 (고유 이름 부여용)

    void Update()
    {
        // TimeManager가 존재하고 밤일 때만 스폰 로직 실행
        if (TimeManager.Instance == null || !TimeManager.Instance.isNight)
        {
            return; // TimeManager가 없거나 낮이면 아무것도 안 함
        }

        // 현재 시간이 다음 생성 시간이거나 지났는지 확인
        if (Time.time >= nextSpawnTime)
        {
            SpawnAnt(); // 개미 생성 함수 호출
            // 다음 생성 시간 계산 및 업데이트
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnAnt()
    {
        // --- 수정: 스폰 위치 배열 확인 및 랜덤 선택 로직 추가 ---

        // 개미 프리팹이 할당되었는지, 스폰 위치 배열이 null이 아니고 최소 1개 이상의 위치가 있는지 확인
        if (antPrefab != null && spawnPoints != null && spawnPoints.Length > 0)
        {
            // 0 부터 (배열 크기 - 1) 사이의 랜덤 정수 인덱스 생성
            int randomIndex = Random.Range(0, spawnPoints.Length);

            // 랜덤하게 선택된 스폰 위치 Transform 가져오기
            Transform selectedSpawnPoint = spawnPoints[randomIndex];

            // 선택된 스폰 위치가 null이 아닌지 한번 더 확인 (안전 장치)
            if (selectedSpawnPoint != null)
            {
                // 선택된 위치에서 개미 프리팹을 복제하여 씬에 생성
                GameObject newAnt = Instantiate(antPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

                // 카운터 증가 및 이름 부여
                antCounter++;
                newAnt.name = $"Ant_{antCounter}"; // 생성된 개미 오브젝트 이름 설정

                // 생성 로그 (어디서 생성되었는지 포함)
                Debug.Log($"Generated: {newAnt.name} at {selectedSpawnPoint.name} ({selectedSpawnPoint.position})");
            }
            else
            {
                Debug.LogError($"BugSpawner: SpawnPoints 배열의 {randomIndex} 인덱스가 null입니다!", gameObject);
            }
        }
        else
        {
            // 필수 설정이 누락된 경우 에러 로그 출력
            if(antPrefab == null)
                Debug.LogError("BugSpawner: Ant Prefab이 설정되지 않았습니다!", gameObject);
            if (spawnPoints == null || spawnPoints.Length == 0)
                Debug.LogError("BugSpawner: Spawn Points 배열이 비어있거나 설정되지 않았습니다!", gameObject);
        }
    }
}