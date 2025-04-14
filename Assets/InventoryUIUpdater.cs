using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 사용

public class InventoryUIUpdater : MonoBehaviour
{
    [Header("UI Element References")]
    [Tooltip("씨앗 개수를 표시할 TextMeshProUGUI 컴포넌트")]
    public TextMeshProUGUI seedCountText;

    [Tooltip("키틴 조각 개수를 표시할 TextMeshProUGUI 컴포넌트")]
    public TextMeshProUGUI chitinCountText;

    // --- 물 개수 UI 요소 추가 ---
    [Tooltip("물 개수를 표시할 TextMeshProUGUI 컴포넌트")]
    public TextMeshProUGUI waterCountText; // Inspector에서 연결할 물 개수 Text UI

    // --- Private Variables ---
    private PlayerInventory playerInventory; // 플레이어 인벤토리 참조

    void Start()
    {
        // 씬에서 PlayerInventory 컴포넌트를 찾습니다.
        playerInventory = FindFirstObjectByType<PlayerInventory>();

        if (playerInventory == null)
        {
            Debug.LogError("InventoryUIUpdater: PlayerInventory를 씬에서 찾을 수 없습니다!", gameObject);
            enabled = false;
            return;
        }

        // --- 필수 UI 요소 확인 (물 포함) ---
        if (seedCountText == null || chitinCountText == null || waterCountText == null) // waterCountText 확인 추가
        {
             Debug.LogError("InventoryUIUpdater: 필요한 UI Text 요소(씨앗, 키틴, 물) 중 하나 이상이 Inspector에 연결되지 않았습니다!", gameObject);
             enabled = false;
             return;
        }

        // 이벤트 구독 및 초기 업데이트
        playerInventory.OnInventoryChanged += UpdateInventoryDisplay;
        UpdateInventoryDisplay(); // 초기값 표시
    }

    void OnDestroy() // 또는 OnDisable()
    {
        // 스크립트가 파괴되거나 비활성화될 때 이벤트 구독 해제
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateInventoryDisplay;
        }
    }

    // 인벤토리 UI를 업데이트하는 함수 (이벤트 핸들러)
    private void UpdateInventoryDisplay()
    {
        if (playerInventory == null) return; // 안전 장치

        if (seedCountText != null)
        {
            seedCountText.text = $"x {playerInventory.seedCount}";
        }

        if (chitinCountText != null)
        {
            chitinCountText.text = $"x {playerInventory.chitinCount}";
        }

        // --- 물 개수 업데이트 로직 추가 ---
        if (waterCountText != null)
        {
            waterCountText.text = $"x {playerInventory.waterCount}"; // "x [개수]" 형식으로 물 개수 표시
        }
        // ----------------------------
    }
}