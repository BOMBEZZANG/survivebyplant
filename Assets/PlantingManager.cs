using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantingManager : MonoBehaviour
{
    [Header("Required Components & Settings")]
    [Tooltip("메인 카메라 (없으면 자동으로 찾지만, 직접 연결 권장)")]
    public Camera mainCamera;
    [Tooltip("식물을 심을 땅 타일맵")]
    public Tilemap groundTilemap;
    [Tooltip("심을 수 있는 땅 타일 종류")]
    public TileBase landTile;
    [Tooltip("심을 씨앗 프리팹 (PlantGrowth.cs 포함)")]
    public GameObject seedPrefab;
    [Tooltip("식물이 이미 있는지 확인할 때 사용할 레이어 마스크")]
    public LayerMask plantLayerMask;

    [Header("Required References")]
    [Tooltip("플레이어 게임 오브젝트에 연결된 PlayerInventory 컴포넌트 - Inspector에서 연결 필수!")]
    public PlayerInventory playerInventory; // Public, Inspector에서 연결

    [Header("Mode State")]
    [Tooltip("현재 심기 모드 활성화 여부")]
    public bool isPlantingMode = false;

    void Start()
    {
        // --- 수정: Inspector에서 playerInventory 연결 확인 ---
        if (playerInventory == null)
        {
            // 이 에러가 발생하면 Inspector 연결이 안 된 것입니다!
            Debug.LogError($"[{gameObject.name}] PlantingManager Error: PlayerInventory가 Inspector에 연결되지 않았습니다! 심기 기능을 사용할 수 없습니다.", gameObject);
            enabled = false; // 스크립트 비활성화
            return;
        }

        // 메인 카메라 자동 할당 (연결 안 됐을 경우)
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
               Debug.LogError($"[{gameObject.name}] PlantingManager Error: 메인 카메라를 찾거나 연결할 수 없습니다! 카메라 설정을 확인하세요.", gameObject);
               enabled = false; // 스크립트 비활성화
            }
        }
         Debug.Log($"[{gameObject.name}] PlantingManager 초기화 완료. PlayerInventory 연결됨.", gameObject);
    }

    void Update()
    {
        // PlayerInventory 참조가 유효하지 않으면 아무것도 하지 않음 (Start에서 비활성화될 수도 있음)
        if (playerInventory == null) return;

        // --- 심기 모드 토글 로직 (예: P 키) ---
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"[{gameObject.name}] P Key Pressed! Current planting mode: {isPlantingMode}");

            if (!isPlantingMode) // 심기 모드 켜기 시도
            {
                // ===>>> 상세 디버그 로그 추가 <<<===
                int currentSeeds = playerInventory.seedCount;
                bool hasEnough = playerInventory.HasEnoughSeeds(1);
                Debug.Log($"[{gameObject.name}] Trying to activate Planting Mode. Seed Count: {currentSeeds}, HasEnoughSeeds(1): {hasEnough}");

                if (hasEnough) // HasEnoughSeeds 결과만 사용
                {
                    isPlantingMode = true;
                    Debug.Log("심기 모드 활성화 (클릭하여 씨앗 심기)");
                }
                else
                {
                    // 로그는 여기 한 곳에서만 출력
                    Debug.Log($"씨앗이 없어 심기 모드를 켤 수 없습니다. (현재 씨앗: {currentSeeds})");
                }
            }
            else // 심기 모드 끄기
            {
                isPlantingMode = false;
                Debug.Log("심기 모드 비활성화");
            }
        }
        // --- 심기 모드 토글 로직 끝 ---

        // --- 클릭하여 씨앗 심기 로직 ---
        if (isPlantingMode && Input.GetMouseButtonDown(0))
        {
             Debug.Log($"[{gameObject.name}] Planting attempt detected. Seed count before UseSeeds: {playerInventory.seedCount}");

             // 씨앗 사용 시도
             if (playerInventory.UseSeeds(1))
             {
                 Debug.Log($"[{gameObject.name}] Seed used successfully (Count now: {playerInventory.seedCount}). Proceeding to HandlePlantingInput.");
                 HandlePlantingInput(true); // 씨앗 사용 성공했으므로 true 전달
             }
             else
             {
                 // UseSeeds가 false를 반환한 경우 (씨앗 부족)
                 Debug.Log($"씨앗이 부족하여 심을 수 없습니다. (현재 씨앗: {playerInventory.seedCount})");
                 isPlantingMode = false;
                 Debug.Log("심기 모드 비활성화됨 (씨앗 부족)");
             }
        }
        // --- 클릭하여 씨앗 심기 로직 끝 ---
    }

    // 씨앗 심기 처리 (성공 시 true, 실패 시 false 반환하도록 수정 고려 가능)
    void HandlePlantingInput(bool seedAlreadyUsed)
    {
        if (mainCamera == null || groundTilemap == null || landTile == null) {
             Debug.LogError("PlantingManager: Planting failed because essential components (Camera, Tilemap, LandTile) are missing or not assigned.");
             if (seedAlreadyUsed) playerInventory.AddSeeds(1); // 씨앗 환불
             return;
        }
         if (seedPrefab == null) {
             Debug.LogError("PlantingManager: Seed Prefab이 Inspector에 연결되지 않아 심을 수 없습니다!");
             if (seedAlreadyUsed) playerInventory.AddSeeds(1); // 씨앗 환불
             return;
         }

        Vector3 screenClickPosition = Input.mousePosition;
        screenClickPosition.z = mainCamera.nearClipPlane + 1; // 카메라 바로 앞 약간의 거리
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenClickPosition);
        Vector3Int cellPosition = groundTilemap.WorldToCell(worldPosition);
        TileBase clickedTile = groundTilemap.GetTile(cellPosition);

        bool plantingSucceeded = false;

        if (clickedTile == landTile)
        {
            Vector3 plantCheckPosition = groundTilemap.GetCellCenterWorld(cellPosition);
            Collider2D existingPlant = Physics2D.OverlapCircle(plantCheckPosition, 0.1f, plantLayerMask);

            if (existingPlant == null)
            {
                Debug.Log($"[{gameObject.name}] Planting '{seedPrefab.name}' at {cellPosition}.");
                Instantiate(seedPrefab, plantCheckPosition, Quaternion.identity);
                // Debug.Log($"씨앗을 {cellPosition} 위치에 심었습니다."); // 이전 로그 중복 제거
                plantingSucceeded = true;
            }
            else
            {
                Debug.Log($"심기 실패: {cellPosition} 위치에 이미 식물이 있습니다.");
            }
        }
        else
        {
            Debug.Log($"심기 실패: {cellPosition} 위치는 심을 수 없는 타일입니다 ({clickedTile?.name ?? "NULL"}).");
        }

        // 씨앗을 사용했지만 심기에 실패한 경우 환불
        if (seedAlreadyUsed && !plantingSucceeded)
        {
             Debug.Log($"심기 실패하여 사용했던 씨앗 1개를 환불합니다. (이전 씨앗 개수: {playerInventory.seedCount})");
             playerInventory.AddSeeds(1); // 씨앗 다시 추가
             Debug.Log($"씨앗 환불 후 개수: {playerInventory.seedCount}");

             // 실패 시 심기 모드 자동 해제 (선택 사항)
             // isPlantingMode = false;
             // Debug.Log("심기 실패로 심기 모드 자동 비활성화됨");
        }

        // 심기 시도 후 모드 자동 해제 (선택 사항) - 성공/실패 무관하게 해제
        // if (isPlantingMode)
        // {
        //     isPlantingMode = false;
        //     Debug.Log("심기 시도 후 심기 모드 비활성화됨");
        // }
    }
}