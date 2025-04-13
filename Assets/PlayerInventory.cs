using UnityEngine;
using System; // Action 사용을 위해 필요

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Counts")]
    [Tooltip("플레이어가 현재 가지고 있는 씨앗의 개수")]
    public int seedCount = 0; // 씨앗 개수

    [Tooltip("플레이어가 현재 가지고 있는 키틴 조각의 개수")]
    public int chitinCount = 0; // 키틴 조각 개수

    // === 인벤토리 변경 이벤트 ===
    public event Action OnInventoryChanged;

    // === 첫 씨앗 획득 추적용 변수 및 이벤트 ===
    private bool hasPickedUpFirstSeed = false;
    public static event Action OnFirstSeedCollected; // 다른 스크립트에서 감지할 static 이벤트

    void Start()
    {
        // 초기값 설정 후 인벤토리 UI 업데이트 요청
        // (만약 저장된 데이터 로드 등이 있다면 여기서 처리)
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 지정된 타입의 자원을 인벤토리에 추가합니다. 성공 시 true 반환.
    /// </summary>
    /// <param name="resourceType">추가할 자원의 종류 (문자열, 예: "Seed", "ChitinScrap")</param>
    /// <param name="amount">추가할 양</param>
    /// <returns>자원 추가 성공 여부</returns>
    public bool AddResource(string resourceType, int amount)
    {
        // 추가할 양이 0 이하면 함수 종료 (오류 방지)
        if (amount <= 0)
        {
            Debug.LogWarning($"AddResource: Invalid amount ({amount}) for type '{resourceType}'");
            return false;
        }

        bool changed = false; // 인벤토리 변경 여부 플래그
        bool collectedSeed = false; // 이번 호출로 씨앗을 얻었는지 확인

        // 자원 타입(문자열)에 따라 개수 증가 처리
        switch (resourceType)
        {
            case "Seed": // ResourcePickup 스크립트의 resourceType이 "Seed"일 경우
                // 첫 씨앗인지 확인 (튜토리얼 힌트용)
                if (!hasPickedUpFirstSeed && seedCount == 0)
                {
                    collectedSeed = true;
                }
                seedCount += amount;
                Debug.Log($"Collected {amount} Seed. Total: {seedCount}");
                changed = true; // 변경됨
                break;

            case "ChitinScrap": // ResourcePickup 스크립트의 resourceType이 "ChitinScrap"일 경우
                chitinCount += amount;
                Debug.Log($"Collected {amount} Chitin Scrap. Total: {chitinCount}");
                changed = true; // 변경됨
                break;

            // 나중에 다른 자원 타입 추가 시 여기에 case 추가...
            default: // 목록에 없는 자원 타입일 경우
                Debug.LogWarning($"AddResource: Unknown resource type '{resourceType}'");
                return false; // 알 수 없는 타입은 추가 실패
        }

        // 변경되었을 경우 이벤트 호출
        if (changed) {
            OnInventoryChanged?.Invoke(); // 인벤토리 UI 업데이트용

            // 첫 씨앗 획득 이벤트 호출 (해당될 경우)
            if (collectedSeed)
            {
                hasPickedUpFirstSeed = true; // 플래그 설정 (다시는 호출 안 되도록)
                OnFirstSeedCollected?.Invoke(); // 튜토리얼 힌트용 이벤트 발생
                Debug.Log("First seed collected! Firing OnFirstSeedCollected event.");
            }
        }
        return true; // 자원 추가 자체는 성공했으므로 true 반환
    }

    /// <summary>
    /// 지정된 타입의 자원을 인벤토리에서 사용(차감)합니다. 성공 시 true 반환.
    /// </summary>
    /// <param name="resourceType">사용할 자원의 종류</param>
    /// <param name="amount">사용할 양</param>
    /// <returns>자원 사용 성공 여부 (개수가 충분하면 true)</returns>
    public bool UseResource(string resourceType, int amount)
    {
         // 사용할 양이 0 이하면 함수 종료
         if (amount <= 0)
         {
             Debug.LogWarning($"UseResource: Invalid amount ({amount}) for type '{resourceType}'");
             return false;
         }

         bool changed = false; // 인벤토리 변경 여부 플래그

         // 자원 타입에 따라 개수 확인 및 차감
         switch (resourceType)
         {
             case "Seed":
                 if (seedCount >= amount) // 보유량이 충분한지 확인
                 {
                     seedCount -= amount; // 개수 차감
                     Debug.Log($"Used {amount} Seed. Remaining: {seedCount}");
                     changed = true; // 변경됨
                 } else { // 보유량 부족
                     // Debug.LogWarning($"Not enough Seed. Required: {amount}, Have: {seedCount}"); // PlantingManager에서 로그 출력하므로 여기선 생략 가능
                     return false; // 사용 실패
                 }
                 break;

             case "ChitinScrap":
                 if (chitinCount >= amount) // 보유량이 충분한지 확인
                 {
                     chitinCount -= amount; // 개수 차감
                     Debug.Log($"Used {amount} Chitin Scrap. Remaining: {chitinCount}");
                     changed = true; // 변경됨
                 } else { // 보유량 부족
                     Debug.LogWarning($"Not enough Chitin Scrap. Required: {amount}, Have: {chitinCount}");
                     return false; // 사용 실패
                 }
                 break;

             // 다른 자원 사용 로직 추가...
             default:
                 Debug.LogWarning($"UseResource: Unknown resource type '{resourceType}'");
                 return false; // 알 수 없는 타입은 사용 실패
         }

         // 변경되었을 경우 이벤트 호출
         if (changed) {
              OnInventoryChanged?.Invoke(); // UI 업데이트용 이벤트
         }
         return true; // 사용 성공
    }

    /// <summary>
    /// 지정된 타입의 자원이 필요한 양만큼 있는지 확인합니다.
    /// </summary>
    /// <param name="resourceType">확인할 자원의 종류</param>
    /// <param name="amount">필요한 양</param>
    /// <returns>충분하면 true, 아니면 false</returns>
    public bool HasEnoughResource(string resourceType, int amount)
    {
         if (amount <= 0) return true; // 0개 이하는 항상 충분

         switch (resourceType)
         {
             case "Seed":
                 // ===>>> 이전에 추가했던 디버그 로그 포함! <<<===
                 Debug.Log($"[PlayerInventory] Checking HasEnoughResource('Seed', {amount}). Current seedCount: {seedCount}. Comparison '{seedCount} >= {amount}' is {(seedCount >= amount)}");
                 return seedCount >= amount;

             case "ChitinScrap":
                 Debug.Log($"[PlayerInventory] Checking HasEnoughResource('ChitinScrap', {amount}). Current chitinCount: {chitinCount}. Comparison '{chitinCount} >= {amount}' is {(chitinCount >= amount)}");
                 return chitinCount >= amount;

             // 다른 자원 확인 로직 추가...
             default:
                 Debug.LogWarning($"[PlayerInventory] HasEnoughResource called with unknown type: {resourceType}");
                 return false; // 알 수 없는 타입은 없다고 간주
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