using UnityEngine;
using UnityEngine.Tilemaps;

// 이 스크립트는 다양한 컴포넌트와 참조가 필요합니다.
// Inspector에서 필요한 연결을 확인해주세요.
public class PlantingManager : MonoBehaviour
{
    [Header("Required Components & Settings")]
    [Tooltip("메인 카메라 (없으면 자동으로 찾지만, 직접 연결 권장)")]
    public Camera mainCamera;
    [Tooltip("식물을 심을 땅 타일맵")]
    public Tilemap groundTilemap;
    [Tooltip("심을 수 있는 땅 타일 종류 (Tile Asset)")]
    public TileBase landTile;
    [Tooltip("심을 씨앗 프리팹 (PlantGrowth.cs 포함)")]
    public GameObject seedPrefab;
    [Tooltip("식물이 이미 있는지 확인할 때 사용할 레이어 마스크")]
    public LayerMask plantLayerMask;

    [Header("Required References")]
    [Tooltip("플레이어 게임 오브젝트에 연결된 PlayerInventory 컴포넌트 - Inspector에서 연결 필수!")]
    public PlayerInventory playerInventory; // Public, Inspector에서 연결

    [Header("Cursor Settings")]
    [Tooltip("심기 모드일 때 사용할 커서 텍스처")]
    public Texture2D plantingCursorTexture;
    [Tooltip("심기 커서의 핫스팟(클릭 지점). (0,0)은 좌측 상단.")]
    public Vector2 plantingCursorHotspot = Vector2.zero;

    // ===>>> 추가: 심기 사운드 설정 <<<===
    [Header("Audio Settings")]
    [Tooltip("씨앗을 성공적으로 심었을 때 재생할 오디오 클립")]
    public AudioClip plantingSound;
    [Range(0f, 1f)]
    [Tooltip("심는 소리의 볼륨 크기")]
    public float plantingSoundVolume = 1.0f;

    [Header("Mode State")]
    [Tooltip("현재 심기 모드 활성화 여부 (읽기 전용에 가까움)")]
    public bool isPlantingMode = false;

    void Start()
    {
        // --- 필수 참조 확인 ---
        if (playerInventory == null)
        {
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
               return; // Start 함수 종료
            }
        }
         // Debug.Log($"[{gameObject.name}] PlantingManager 초기화 완료. PlayerInventory 연결됨.", gameObject);

         // 시작 시 기본 커서로 확실히 설정
         Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void Update()
    {
        // PlayerInventory 참조가 유효하지 않으면 아무것도 하지 않음
        if (playerInventory == null) return;

        // --- 심기 모드 토글 로직 (P 키) ---
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePlantingMode();
        }
        // --- 심기 모드 토글 로직 끝 ---

        // --- 클릭하여 씨앗 심기 로직 ---
        if (isPlantingMode && Input.GetMouseButtonDown(0))
        {
             AttemptPlanting();
        }
        // --- 클릭하여 씨앗 심기 로직 끝 ---
    }

    /// <summary>
    /// 심기 모드를 켜거나 끕니다.
    /// </summary>
    void TogglePlantingMode()
    {
         if (!isPlantingMode) // 현재 모드가 꺼져있으면 -> 켜기 시도
         {
             // 씨앗이 있는지 먼저 확인
             if (playerInventory.HasEnoughSeeds(1))
             {
                 isPlantingMode = true;
                 Debug.Log("심기 모드 활성화 (클릭하여 씨앗 심기)");
                 // 커서 변경: 심기 모드 커서로 설정
                 if (plantingCursorTexture != null) {
                      Cursor.SetCursor(plantingCursorTexture, plantingCursorHotspot, CursorMode.Auto);
                 } else {
                      Debug.LogWarning("PlantingManager: Planting Cursor Texture가 할당되지 않았습니다.");
                 }
             }
             else
             {
                 // 씨앗 부족 로그
                 Debug.Log($"씨앗이 없어 심기 모드를 켤 수 없습니다. (현재 씨앗: {playerInventory.seedCount})");
             }
         }
         else // 현재 모드가 켜져있으면 -> 끄기
         {
             isPlantingMode = false;
             Debug.Log("심기 모드 비활성화");
             // 커서 변경: 기본 커서로 복원
             Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
         }
    }

    /// <summary>
    /// 마우스 클릭 시 씨앗 심기를 시도하는 로직
    /// </summary>
    void AttemptPlanting()
    {
        // Debug.Log($"[{gameObject.name}] Planting attempt detected. Seed count before UseSeeds: {playerInventory.seedCount}");

        // 씨앗 사용 시도 (인벤토리에서 1개 차감)
        if (playerInventory.UseSeeds(1))
        {
            // Debug.Log($"[{gameObject.name}] Seed used successfully (Count now: {playerInventory.seedCount}). Proceeding to HandlePlantingInput.");
            HandlePlantingInput(true); // 씨앗을 사용했으므로 true 전달
        }
        else
        {
            // UseSeeds가 false를 반환 (씨앗 부족)
            Debug.Log($"씨앗이 부족하여 심을 수 없습니다. (현재 씨앗: {playerInventory.seedCount})");
            isPlantingMode = false; // 씨앗 부족 시 모드 자동 해제
            Debug.Log("심기 모드 비활성화됨 (씨앗 부족)");
            // 커서 변경: 기본 커서로 복원
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }


    /// <summary>
    /// 실제 타일 확인, 식물 존재 여부 확인 후 씨앗 프리팹을 생성하는 함수
    /// </summary>
    /// <param name="seedAlreadyUsed">AttemptPlanting에서 씨앗이 이미 사용되었는지 여부</param>
    void HandlePlantingInput(bool seedAlreadyUsed)
    {
        // 필수 컴포넌트/프리팹 재확인 (안전 장치)
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

        // 마우스 위치 -> 월드 좌표 -> 타일맵 셀 좌표 변환
        Vector3 screenClickPosition = Input.mousePosition;
        screenClickPosition.z = mainCamera.nearClipPlane + 1; // 카메라와 약간 떨어진 거리 설정
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenClickPosition);
        Vector3Int cellPosition = groundTilemap.WorldToCell(worldPosition);
        TileBase clickedTile = groundTilemap.GetTile(cellPosition);

        bool plantingSucceeded = false; // 심기 성공 여부 플래그

        // 클릭한 타일이 심을 수 있는 땅(landTile)인지 확인
        if (clickedTile == landTile)
        {
            // 해당 셀의 중앙 월드 좌표 계산
            Vector3 plantCheckPosition = groundTilemap.GetCellCenterWorld(cellPosition);
            // 해당 위치에 이미 다른 식물(plantLayerMask에 속하는 콜라이더)이 있는지 확인
            Collider2D existingPlant = Physics2D.OverlapCircle(plantCheckPosition, 0.1f, plantLayerMask);

            // 기존 식물이 없다면 심기 진행
            if (existingPlant == null)
            {
                Debug.Log($"[{gameObject.name}] Planting '{seedPrefab.name}' at {cellPosition}.");
                // 씨앗 프리팹 생성
                Instantiate(seedPrefab, plantCheckPosition, Quaternion.identity);
                plantingSucceeded = true; // 성공 플래그 설정

                // ===>>> 추가: 심기 성공 시 사운드 재생 <<<===
                if (plantingSound != null)
                {
                    // 지정된 위치(plantCheckPosition)에서 사운드 재생 (볼륨 적용)
                    AudioSource.PlayClipAtPoint(plantingSound, plantCheckPosition, plantingSoundVolume);
                }
                // ===>>> 사운드 재생 끝 <<<===

                // Debug.Log($"씨앗을 {cellPosition} 위치에 심었습니다."); // 이전 로그와 중복될 수 있어 주석 처리
            }
            else // 기존 식물이 있다면
            {
                Debug.Log($"심기 실패: {cellPosition} 위치에 이미 식물이 있습니다.");
                // plantingSucceeded는 false 유지
            }
        }
        else // 클릭한 타일이 landTile이 아니라면
        {
            Debug.Log($"심기 실패: {cellPosition} 위치는 심을 수 없는 타일입니다 ({clickedTile?.name ?? "NULL"}).");
            // plantingSucceeded는 false 유지
        }

        // 씨앗을 사용했지만 심기에 실패한 경우 씨앗 환불
        if (seedAlreadyUsed && !plantingSucceeded)
        {
             Debug.Log($"심기 실패하여 사용했던 씨앗 1개를 환불합니다. (이전 씨앗 개수: {playerInventory.seedCount})");
             playerInventory.AddSeeds(1); // 씨앗 다시 추가 (AddResource 호출)
             // Debug.Log($"씨앗 환불 후 개수: {playerInventory.seedCount}"); // AddResource 내부 로그로 확인 가능

             // 실패 시 심기 모드 자동 해제 및 커서 복원 (선택 사항)
             // isPlantingMode = false;
             // Debug.Log("심기 실패로 심기 모드 자동 비활성화됨");
             // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        // 심기 시도 후 모드 자동 해제 (선택 사항) - 성공/실패 무관하게 해제하고 커서 복원
        // if (isPlantingMode)
        // {
        //     isPlantingMode = false;
        //     Debug.Log("심기 시도 후 심기 모드 비활성화됨");
        //     Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        // }
    }

    /// <summary>
    /// 스크립트가 비활성화되거나 게임 오브젝트가 파괴될 때 호출됩니다.
    /// </summary>
    void OnDisable()
    {
         // 만약 이 스크립트가 비활성화될 때 심기 모드였다면 커서를 되돌립니다.
         // (씬 전환 등으로 오브젝트가 사라질 때 커서가 모종삽으로 남는 현상 방지)
         if (isPlantingMode)
         {
             Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
             // Debug.Log("PlantingManager 비활성화되어 기본 커서로 복원."); // 필요시 주석 해제
         }
    }
}