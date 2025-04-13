using UnityEngine;
using System; // Action 사용을 위해 추가

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Counts")]
    public int seedCount = 0;
    public int chitinCount = 0;

    // ===>>> 추가: 인벤토리 변경 이벤트 <<<===
    public event Action OnInventoryChanged;

    // ===>>> 추가: 게임 시작 시 초기 상태 알림 <<<===
    void Start() // 또는 Awake()
    {
        // 초기값 로드 로직이 있다면 여기서 수행 후 이벤트 호출
        // ...
        OnInventoryChanged?.Invoke(); // UI가 초기값을 표시하도록 이벤트 호출
    }

    public bool AddResource(string resourceType, int amount)
    {
        if (amount <= 0) return false;

        bool changed = false; // 인벤토리 변경 여부 플래그

        switch (resourceType)
        {
            case "Seed":
                seedCount += amount;
                Debug.Log($"Collected {amount} Seed. Total: {seedCount}");
                changed = true; // 변경됨
                break;

            case "ChitinScrap":
                chitinCount += amount;
                Debug.Log($"Collected {amount} Chitin Scrap. Total: {chitinCount}");
                changed = true; // 변경됨
                break;

            default:
                Debug.LogWarning($"AddResource: Unknown resource type '{resourceType}'");
                return false;
        }

        // ===>>> 추가: 변경되었을 경우 이벤트 호출 <<<===
        if (changed) {
            OnInventoryChanged?.Invoke();
        }
        return true;
    }

    public bool UseResource(string resourceType, int amount)
    {
         if (amount <= 0) return false;

         bool changed = false; // 인벤토리 변경 여부 플래그

         switch (resourceType)
         {
             case "Seed":
                 if (seedCount >= amount)
                 {
                     seedCount -= amount;
                     Debug.Log($"Used {amount} Seed. Remaining: {seedCount}");
                     changed = true; // 변경됨
                 } else {
                     Debug.LogWarning($"Not enough Seed. Required: {amount}, Have: {seedCount}");
                     return false;
                 }
                 break;

             case "ChitinScrap":
                 if (chitinCount >= amount)
                 {
                     chitinCount -= amount;
                     Debug.Log($"Used {amount} Chitin Scrap. Remaining: {chitinCount}");
                     changed = true; // 변경됨
                 } else {
                     Debug.LogWarning($"Not enough Chitin Scrap. Required: {amount}, Have: {chitinCount}");
                     return false;
                 }
                 break;
             default:
                 Debug.LogWarning($"UseResource: Unknown resource type '{resourceType}'");
                 return false;
         }

        // ===>>> 추가: 변경되었을 경우 이벤트 호출 <<<===
        if (changed) {
             OnInventoryChanged?.Invoke();
         }
        return true;
    }

    // HasEnoughResource, AddSeeds, UseSeeds, HasEnoughSeeds 함수는 그대로 둡니다.
    public bool HasEnoughResource(string resourceType, int amount)
    {
         if (amount <= 0) return true;
         switch (resourceType) {
             case "Seed": return seedCount >= amount;
             case "ChitinScrap": return chitinCount >= amount;
             default: return false;
         }
    }
    public bool AddSeeds(int amount) { return AddResource("Seed", amount); }
    public bool UseSeeds(int amount) { return UseResource("Seed", amount); }
    public bool HasEnoughSeeds(int amount) { return HasEnoughResource("Seed", amount); }
}