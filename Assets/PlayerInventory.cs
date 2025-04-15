using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Counts")]
    public int seedCount = 0;
    public int chitinCount = 0;
    public int waterCount = 0;
    // --- 새로운 자원 변수 추가 ---
    public int woodCount = 0;
    public int stoneCount = 0;
    public int fiberCount = 0;
    // --------------------------

    public event Action OnInventoryChanged;
    private bool hasPickedUpFirstSeed = false;
    public static event Action OnFirstSeedCollected;

    void Start()
    {
        // 초기값 설정은 여기서 하거나 Inspector에서 직접 설정
        // woodCount = 5; // 테스트용 초기값
        // stoneCount = 3;
        // fiberCount = 10;
        OnInventoryChanged?.Invoke(); // 초기 UI 업데이트 호출
    }

    public bool AddResource(string resourceType, int amount)
    {
        if (amount <= 0) return false;
        bool changed = false;
        bool collectedSeed = false;

        switch (resourceType)
        {
            case "Seed":
                if (!hasPickedUpFirstSeed && seedCount == 0) collectedSeed = true;
                seedCount += amount;
                Debug.Log($"Collected {amount} Seed. Total: {seedCount}");
                changed = true;
                break;
            case "ChitinScrap":
                chitinCount += amount;
                Debug.Log($"Collected {amount} Chitin Scrap. Total: {chitinCount}");
                changed = true;
                break;
            case "Water":
                waterCount += amount;
                Debug.Log($"Collected {amount} Water. Total: {waterCount}");
                changed = true;
                break;
            // --- 새로운 자원 추가 로직 ---
            case "Wood":
                woodCount += amount;
                Debug.Log($"Collected {amount} Wood. Total: {woodCount}");
                changed = true;
                break;
            case "Stone":
                stoneCount += amount;
                Debug.Log($"Collected {amount} Stone. Total: {stoneCount}");
                changed = true;
                break;
            case "Fiber":
                fiberCount += amount;
                Debug.Log($"Collected {amount} Fiber. Total: {fiberCount}");
                changed = true;
                break;
            // --------------------------
            default:
                Debug.LogWarning($"AddResource: Unknown resource type '{resourceType}'");
                return false;
        }

        if (changed) {
            OnInventoryChanged?.Invoke();
            if (collectedSeed) { /* ... */ }
        }
        return true;
    }

    public bool UseResource(string resourceType, int amount)
    {
         if (amount <= 0) return false;
         bool changed = false;

         switch (resourceType)
         {
             case "Seed":
                 if (seedCount >= amount) { seedCount -= amount; changed = true; } else { return false; }
                 break;
             case "ChitinScrap":
                 if (chitinCount >= amount) { chitinCount -= amount; changed = true; } else { return false; }
                 break;
            case "Water":
                 if (waterCount >= amount) { waterCount -= amount; changed = true; } else { Debug.Log("물이 부족합니다."); return false; }
                 break;
            // --- 새로운 자원 사용 로직 ---
            case "Wood":
                 if (woodCount >= amount) { woodCount -= amount; changed = true; Debug.Log($"Used {amount} Wood. Remaining: {woodCount}"); }
                 else { Debug.LogWarning($"Not enough Wood. Required: {amount}, Have: {woodCount}"); return false; }
                 break;
            case "Stone":
                 if (stoneCount >= amount) { stoneCount -= amount; changed = true; Debug.Log($"Used {amount} Stone. Remaining: {stoneCount}"); }
                 else { Debug.LogWarning($"Not enough Stone. Required: {amount}, Have: {stoneCount}"); return false; }
                 break;
            case "Fiber":
                 if (fiberCount >= amount) { fiberCount -= amount; changed = true; Debug.Log($"Used {amount} Fiber. Remaining: {fiberCount}"); }
                 else { Debug.LogWarning($"Not enough Fiber. Required: {amount}, Have: {fiberCount}"); return false; }
                 break;
            // --------------------------
             default:
                 Debug.LogWarning($"UseResource: Unknown resource type '{resourceType}'");
                 return false;
         }

         if (changed) { OnInventoryChanged?.Invoke(); }
         return true;
    }

    public bool HasEnoughResource(string resourceType, int amount)
    {
         if (amount <= 0) return true;
         switch (resourceType)
         {
             case "Seed": return seedCount >= amount;
             case "ChitinScrap": return chitinCount >= amount;
             case "Water": return waterCount >= amount;
             // --- 새로운 자원 보유량 확인 ---
             case "Wood": return woodCount >= amount;
             case "Stone": return stoneCount >= amount;
             case "Fiber": return fiberCount >= amount;
             // ---------------------------
             default: return false;
         }
    }
    public bool HasEnoughSeeds(int amount)
    {
        // 내부적으로 HasEnoughResource 함수 호출
        return HasEnoughResource("Seed", amount);
    }


    // --- AddSeeds, UseSeeds 는 PlayerInventory 외부에서 직접 호출하기보다
    // --- AddResource, UseResource 를 사용하는 것이 더 일관성 있습니다.
    // --- PlantingManager 등에서 이미 수정했다면 아래 함수들은 제거해도 무방합니다.
    // --- (호환성을 위해 남겨둠)

    /// <summary>
    /// [호환성용] 씨앗을 추가합니다. AddResource("Seed", amount) 사용 권장.
    /// </summary>
    public bool AddSeeds(int amount)
    {
        return AddResource("Seed", amount);
    }

    /// <summary>
    /// [호환성용] 씨앗을 사용합니다. UseResource("Seed", amount) 사용 권장.
    /// </summary>
    public bool UseSeeds(int amount)
    {
        return UseResource("Seed", amount);
    }

}