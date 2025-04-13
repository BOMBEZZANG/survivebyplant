using UnityEngine;

// 플레이어의 인벤토리 (씨앗, 키틴 조각 등)를 관리하는 스크립트
public class PlayerInventory : MonoBehaviour
{
    // --- 보유 자원 변수 ---
    [Header("Inventory Counts")] // Inspector에서 섹션 구분
    [Tooltip("플레이어가 현재 가지고 있는 씨앗의 개수")]
    public int seedCount = 0; // 씨앗 개수

    [Tooltip("플레이어가 현재 가지고 있는 키틴 조각의 개수")]
    public int chitinCount = 0; // 키틴 조각 개수

    // (나중에 다른 자원을 추가한다면 여기에 변수 선언. 예: public int woodCount = 0;)


    // --- 일반적인 자원 관리 함수 (ResourcePickup.cs 등에서 사용 권장) ---

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

        // 자원 타입(문자열)에 따라 개수 증가 처리
        switch (resourceType)
        {
            case "Seed": // ResourcePickup 스크립트의 resourceType이 "Seed"일 경우
                seedCount += amount;
                Debug.Log($"Collected {amount} Seed. Total: {seedCount}");
                // TODO: 여기에 씨앗 개수 UI 업데이트 함수 호출 추가
                return true; // 추가 성공

            case "ChitinScrap": // ResourcePickup 스크립트의 resourceType이 "ChitinScrap"일 경우
                chitinCount += amount;
                Debug.Log($"Collected {amount} Chitin Scrap. Total: {chitinCount}");
                // TODO: 여기에 키틴 개수 UI 업데이트 함수 호출 추가
                return true; // 추가 성공

            // 나중에 다른 자원 타입 추가 시 여기에 case 추가
            // case "Wood":
            //     woodCount += amount;
            //     Debug.Log($"Collected {amount} Wood. Total: {woodCount}");
            //     return true;

            default: // 목록에 없는 자원 타입일 경우
                Debug.LogWarning($"AddResource: Unknown resource type '{resourceType}'");
                return false; // 알 수 없는 타입은 추가 실패
        }
    }

    /// <summary>
    /// 지정된 타입의 자원을 인벤토리에서 사용(차감)합니다. 성공 시 true 반환.
    /// (예: 제작, 건설 등에 사용)
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

         // 자원 타입에 따라 개수 확인 및 차감
         switch (resourceType)
         {
             case "Seed":
                 if (seedCount >= amount) // 보유량이 충분한지 확인
                 {
                     seedCount -= amount; // 개수 차감
                     Debug.Log($"Used {amount} Seed. Remaining: {seedCount}");
                     // TODO: 여기에 씨앗 개수 UI 업데이트 함수 호출 추가
                     return true; // 사용 성공
                 } else { // 보유량 부족
                     Debug.LogWarning($"Not enough Seed. Required: {amount}, Have: {seedCount}");
                     return false; // 사용 실패
                 }

             case "ChitinScrap":
                 if (chitinCount >= amount) // 보유량이 충분한지 확인
                 {
                     chitinCount -= amount; // 개수 차감
                     Debug.Log($"Used {amount} Chitin Scrap. Remaining: {chitinCount}");
                     // TODO: 여기에 키틴 개수 UI 업데이트 함수 호출 추가
                     return true; // 사용 성공
                 } else { // 보유량 부족
                     Debug.LogWarning($"Not enough Chitin Scrap. Required: {amount}, Have: {chitinCount}");
                     return false; // 사용 실패
                 }

             // 다른 자원 사용 로직 추가...
             default:
                 Debug.LogWarning($"UseResource: Unknown resource type '{resourceType}'");
                 return false; // 알 수 없는 타입은 사용 실패
         }
    }

    /// <summary>
    /// 지정된 타입의 자원이 필요한 양만큼 있는지 확인합니다.
    /// </summary>
    /// <param name="resourceType">확인할 자원의 종류</param>
    /// <param name="amount">필요한 양</param>
    /// <returns>충분하면 true, 아니면 false</returns>
    public bool HasEnoughResource(string resourceType, int amount)
    {
         if (amount <= 0) return true; // 0개 이하는 항상 충분하다고 간주

         // 자원 타입에 따라 보유량 비교
         switch (resourceType)
         {
             case "Seed":
                 return seedCount >= amount;
             case "ChitinScrap":
                 return chitinCount >= amount;
             // 다른 자원 확인 로직 추가...
             default:
                 // Debug.LogWarning($"HasEnoughResource: Unknown resource type '{resourceType}'"); // 로그 필요시 주석 해제
                 return false; // 알 수 없는 타입은 없다고 간주
         }
    }

    // --- 기존 씨앗 전용 함수들 (PlantingManager와의 호환성을 위해 유지) ---
    // 주석: 아래 함수들은 내부적으로 새로운 일반 자원 함수들을 호출합니다.
    // 나중에 PlantingManager도 UseResource("Seed", ...) 등을 사용하도록 수정하면 아래 함수들은 제거해도 됩니다.

    /// <summary>
    /// [호환성 유지] 씨앗을 추가합니다. AddResource("Seed", amount) 사용 권장.
    /// </summary>
    public bool AddSeeds(int amount)
    {
        // 내부적으로 AddResource 함수 호출
        bool result = AddResource("Seed", amount);
        // if (result) Debug.Log("(Called via AddSeeds)"); // 필요시 로그 추가
        return result;
    }

    /// <summary>
    /// [호환성 유지] 씨앗을 사용합니다. PlantingManager에서 현재 사용 중.
    /// </summary>
    public bool UseSeeds(int amount)
    {
        // 내부적으로 UseResource 함수 호출
        bool result = UseResource("Seed", amount);
        // if (result) Debug.Log("(Called via UseSeeds)"); // 필요시 로그 추가
        return result;
    }

    /// <summary>
    /// [호환성 유지] 씨앗이 충분히 있는지 확인합니다. PlantingManager에서 현재 사용 중.
    /// </summary>
    public bool HasEnoughSeeds(int amount)
    {
        // 내부적으로 HasEnoughResource 함수 호출
        return HasEnoughResource("Seed", amount);
    }
}