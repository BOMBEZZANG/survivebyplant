using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantingManager : MonoBehaviour
{
    public Camera mainCamera;
    public Tilemap groundTilemap;
    public TileBase landTile;
    public GameObject seedPrefab; // 심는 용도의 씨앗 프리팹 (PlantGrowth.cs 포함)
    public LayerMask plantLayerMask;

    // >> 추가: 플레이어 인벤토리 참조 <<
    public PlayerInventory playerInventory;

    // >> 추가: 심기 모드 상태 변수 <<
    public bool isPlantingMode = false;

    void Start() // Start 함수 추가 또는 기존 것 사용
    {
        // PlayerInventory 찾기
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (playerInventory == null)
        {
            Debug.LogError("PlantingManager에서 PlayerInventory를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // --- 심기 모드 토글 로직 (예: P 키) ---
        if (Input.GetKeyDown(KeyCode.P)) // P 키를 누르면
        {
            if (!isPlantingMode) // 현재 심기 모드가 아닐 때
            {
                // 씨앗이 1개 이상 있을 때만 심기 모드 켜기 가능
                if (playerInventory != null && playerInventory.HasEnoughSeeds(1))
                {
                    isPlantingMode = true;
                    Debug.Log("심기 모드 활성화 (클릭하여 씨앗 심기)");
                }
                else
                {
                    Debug.Log("씨앗이 없어 심기 모드를 켤 수 없습니다.");
                }
            }
            else // 현재 심기 모드일 때
            {
                isPlantingMode = false; // 심기 모드 끄기
                Debug.Log("심기 모드 비활성화");
            }
        }
        // --- 심기 모드 토글 로직 끝 ---

        // --- 클릭하여 씨앗 심기 로직 ---
        // 심기 모드일 때만 + 마우스 왼쪽 버튼 클릭 감지
        if (isPlantingMode && Input.GetMouseButtonDown(0))
        {
            // 씨앗 사용 시도 (인벤토리 확인 및 차감)
            if (playerInventory != null && playerInventory.UseSeeds(1))
            {
                // 씨앗 사용에 성공하면 심기 시도
                HandlePlantingInput();
            }
            else
            {
                // 씨앗 사용에 실패하면 (씨앗 부족)
                Debug.Log("씨앗이 부족하여 심을 수 없습니다.");
                isPlantingMode = false; // 심기 모드 자동 비활성화
                Debug.Log("심기 모드 비활성화됨 (씨앗 부족)");
            }
        }
        // --- 클릭하여 씨앗 심기 로직 끝 ---
    }

    // HandlePlantingInput 함수는 씨앗을 '어디에' 심을지, '심을 수 있는지' 확인하고 '생성'만 담당
    void HandlePlantingInput()
    {
        Vector3 screenClickPosition = Input.mousePosition;
        screenClickPosition.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenClickPosition);
        Vector3Int cellPosition = groundTilemap.WorldToCell(worldPosition);
        TileBase clickedTile = groundTilemap.GetTile(cellPosition);

        if (clickedTile == landTile)
        {
            Vector3 plantCheckPosition = groundTilemap.GetCellCenterWorld(cellPosition);
            Collider2D existingPlant = Physics2D.OverlapCircle(plantCheckPosition, 0.1f, plantLayerMask);

            if (existingPlant == null)
            {
                // 생성 전에 로그 출력 (이제 이것이 의도된 생성인지 확인 가능)
                Debug.Log($"!!! [심기 모드] PlantingManager: '{seedPrefab?.name ?? "NULL"}' 생성 시도 from {this.gameObject.name}", this);
                Instantiate(seedPrefab, plantCheckPosition, Quaternion.identity);
                Debug.Log($"씨앗을 {cellPosition} 위치에 심었습니다.");

                // (선택) 하나 심고 나면 심기 모드 자동 해제?
                // isPlantingMode = false;
                // Debug.Log("씨앗을 심고 심기 모드 자동 비활성화됨");
            }
            else
            {
                Debug.Log("이미 식물이 심어져 있습니다.");
                // 중요: 씨앗 사용 실패 처리가 필요할 수 있음.
                // 이미 UseSeeds(1)로 차감했기 때문에, 여기에 도달하면 씨앗 1개를 낭비한 셈.
                // -> 개선: Instantiate 하기 전에 existingPlant 체크를 먼저 하도록 순서 변경 고려.
                // 또는 UseSeeds(1) 호출을 여기 if문 안으로 옮기기.
            }
        }
        else
        {
            Debug.Log("이곳에는 씨앗을 심을 수 없습니다.");
            // 중요: 씨앗 사용 실패 처리가 필요할 수 있음 (위와 동일).
        }
    }
}