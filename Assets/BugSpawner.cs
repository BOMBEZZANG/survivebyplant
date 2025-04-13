using UnityEngine;

public class BugSpawner : MonoBehaviour
{
    [Tooltip("생성할 개미 프리팹")]
    public GameObject antPrefab;       // Inspector에서 연결할 개미 프리팹

    [Tooltip("개미가 생성될 위치 Transform")]
    public Transform spawnPoint;      // Inspector에서 연결할 스폰 위치

    [Tooltip("개미 생성 간격 (초)")]
    public float spawnInterval = 5.0f; // 개미 생성 주기

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
    } // <--- Update() 메서드 닫는 괄호

    void SpawnAnt()
    {
        // 개미 프리팹과 스폰 위치가 Inspector에서 제대로 할당되었는지 확인
        if (antPrefab != null && spawnPoint != null)
        { // <--- if 시작 괄호
            // 개미 프리팹을 복제하여 씬에 생성
            GameObject newAnt = Instantiate(antPrefab, spawnPoint.position, spawnPoint.rotation);

            // 카운터 증가 및 이름 부여
            antCounter++; // 카운터 값 1 증가
            newAnt.name = $"Ant_{antCounter}"; // 생성된 개미 오브젝트의 이름을 "Ant_1", "Ant_2" 등으로 설정

            // 생성된 개미의 이름과 함께 로그 출력
            Debug.Log($"Generated: {newAnt.name}");
        } // <--- if 끝 괄호
        else
        { // <--- else 시작 괄호
            // 필수 설정이 누락된 경우 에러 로그 출력
            Debug.LogError("BugSpawner: Ant Prefab 또는 Spawn Point가 설정되지 않았습니다!", gameObject);
        } // <--- else 끝 괄호

    } // <--- SpawnAnt() 메서드 닫는 괄호 (여기가 누락되었을 수 있습니다!)

} // <--- BugSpawner 클래스 닫는 괄호 (파일의 맨 마지막, 여기가 누락되었을 수 있습니다!)