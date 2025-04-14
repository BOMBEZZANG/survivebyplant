using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Counts")]
    [Tooltip("플레이어가 현재 가지고 있는 씨앗의 개수")]
    public int seedCount = 0;

    [Tooltip("플레이어가 현재 가지고 있는 키틴 조각의 개수")]
    public int chitinCount = 0;

    // --- 물 자원 추가 ---
    [Tooltip("플레이어가 현재 가지고 있는 물의 양")]
    public int waterCount = 0; // 물 개수 추가 (초기값은 0 또는 테스트용 값 설정)

    public event Action OnInventoryChanged;
    private bool hasPickedUpFirstSeed = false;
    public static event Action OnFirstSeedCollected;

    void Start()
    {
        // 예시: 테스트를 위해 시작 시 물 5개 지급 (나중에 제거하거나 조절)
        //waterCount = 5;
        //Debug.Log($"초기 물 보유량: {waterCount}");
        // ------------------

        OnInventoryChanged?.Invoke();
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
            // --- 물 추가 로직 ---
            case "Water":
                waterCount += amount;
                Debug.Log($"Collected {amount} Water. Total: {waterCount}");
                changed = true;
                break;
            // -----------------
            default:
                Debug.LogWarning($"AddResource: Unknown resource type '{resourceType}'");
                return false;
        }

        if (changed) {
            OnInventoryChanged?.Invoke();
            if (collectedSeed)
            {
                hasPickedUpFirstSeed = true;
                OnFirstSeedCollected?.Invoke();
            }
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
                 if (seedCount >= amount) {
                     seedCount -= amount;
                     Debug.Log($"Used {amount} Seed. Remaining: {seedCount}");
                     changed = true;
                 } else { return false; }
                 break;
             case "ChitinScrap":
                 if (chitinCount >= amount) {
                     chitinCount -= amount;
                     Debug.Log($"Used {amount} Chitin Scrap. Remaining: {chitinCount}");
                     changed = true;
                 } else { return false; }
                 break;
            // --- 물 사용 로직 ---
            case "Water":
                 if (waterCount >= amount) {
                     waterCount -= amount;
                     Debug.Log($"Used {amount} Water. Remaining: {waterCount}");
                     changed = true;
                 } else {
                     Debug.Log("물이 부족합니다."); // 물 부족 시 메시지 추가
                     return false;
                 }
                 break;
            // -----------------
             default:
                 Debug.LogWarning($"UseResource: Unknown resource type '{resourceType}'");
                 return false;
         }

         if (changed) {
              OnInventoryChanged?.Invoke();
         }
         return true;
    }

    public bool HasEnoughResource(string resourceType, int amount)
    {
         if (amount <= 0) return true;
         switch (resourceType)
         {
             case "Seed":
                 return seedCount >= amount;
             case "ChitinScrap":
                 return chitinCount >= amount;
            // --- 물 보유량 확인 로직 ---
            case "Water":
                 return waterCount >= amount;
            // ---------------------
             default:
                 return false;
         }
    }
    /// <summary>
    /// [호환성 유지] 씨앗이 충분히 있는지 확인합니다. PlantingManager에서 현재 사용 중.
    /// </summary>
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