using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class SproutInteraction : MonoBehaviour, IInteractable
{
    [Header("성장 설정")]
    public GameObject maturePlantPrefab;

    [Header("필요 자원")]
    public int waterCost = 1;
    public int chitinCost = 1;

    [Header("피드백")]
    public AudioClip growSound;
    [Range(0f, 1f)]
    public float growSoundVolume = 1.0f;
    // --- 추가: 성장 파티클 효과 ---
    [Tooltip("성장 시 재생할 파티클 이펙트 프리팹 (선택 사항)")]
    public GameObject growthParticlePrefab;

    private bool hasGrown = false;

    // --- 수정: 여러 자원 요구사항을 표시하도록 InteractionPrompt 개선 ---
    public string InteractionPrompt
    {
        get
        {
            // 필요한 자원 목록 문자열 생성
            List<string> requiredItems = new List<string>();
            if (waterCost > 0) requiredItems.Add($"{waterCost} Water");
            if (chitinCost > 0) requiredItems.Add($"{chitinCost} Chitin");
            // 다른 자원 추가 시 여기에 추가

            // 목록이 비어있지 않으면 문자열 조합, 비어있으면 기본 메시지
            if (requiredItems.Count > 0)
            {
                return $"({string.Join(", ", requiredItems)})"; // 예: "Grow Plant (Needs: 1 Water, 1 Chitin)"
            }
            else
            {
                return "Grow Plant"; // 필요한 자원이 없을 경우 (거의 없겠지만)
            }
        }
    }

    void Start()
    {
        // Collider 설정 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
             Debug.LogError($"[{gameObject.name}] Collider2D 컴포넌트가 없습니다. 상호작용이 불가능합니다.", gameObject);
        }
         // 성체 프리팹 연결 확인
         if (maturePlantPrefab == null)
         {
             Debug.LogError($"[{gameObject.name}] Mature Plant Prefab이 Inspector에 연결되지 않았습니다! 성장할 수 없습니다.", gameObject);
             enabled = false; // 스크립트 비활성화
         }
    }

    // 플레이어가 'E' 키 상호작용 시 PlayerInteraction 스크립트에 의해 호출될 메서드
    public void Interact(GameObject interactor)
    {
        Debug.Log($"[{gameObject.name}] Interact() 호출됨. 상호작용 시도자: {interactor.name}");

        // 이미 성장했으면 아무것도 안 함
        if (hasGrown)
        {
            Debug.Log($"[{gameObject.name}] 이미 성장하여 상호작용 불가.");
            return;
        }

        // 플레이어 인벤토리 가져오기
        PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
             Debug.LogError($"[{gameObject.name}] 상호작용자({interactor.name})에게 PlayerInventory 컴포넌트가 없습니다!", interactor);
             return;
        }

        // --- 수정: 모든 필요한 자원 확인 ---
        bool hasWater = playerInventory.HasEnoughResource("Water", waterCost);
        bool hasChitin = playerInventory.HasEnoughResource("ChitinScrap", chitinCost);
        // 다른 자원 필요 시 여기에 추가

        // 모든 자원이 충분한지 확인
        if (hasWater && hasChitin /* && 다른 자원 확인 */)
        {
            Debug.Log($"[{gameObject.name}] 모든 필요 자원(물: {waterCost}, 키틴: {chitinCost}) 보유 확인. 자원 사용 시도...");

            // --- 수정: 모든 필요한 자원 사용 시도 ---
            bool waterUsed = playerInventory.UseResource("Water", waterCost);
            bool chitinUsed = playerInventory.UseResource("ChitinScrap", chitinCost);
            // 다른 자원 사용 시도 추가

            // 모든 자원 사용에 성공했는지 확인
            if (waterUsed && chitinUsed /* && 다른 자원 사용 성공 */)
            {
                 Debug.Log($"[{gameObject.name}] 모든 자원 사용 성공. 성체로 성장합니다.");
                 // 성장 처리 함수 호출
                 GrowToMature();
            }
            else // 하나라도 자원 사용에 실패한 경우 (이론상 거의 발생 안함, 동시성 문제 등 예외처리)
            {
                 Debug.LogError($"[{gameObject.name}] 자원 사용 중 오류 발생! 사용된 자원 환불 시도.");
                 // 사용 성공했던 자원들을 다시 환불 처리 (안전 장치)
                 if (waterUsed) playerInventory.AddResource("Water", waterCost);
                 if (chitinUsed) playerInventory.AddResource("ChitinScrap", chitinCost);
                 // 다른 자원 환불 처리 추가
            }
        }
        else // 하나라도 자원이 부족한 경우
        {
            // 부족한 자원 메시지 생성 (더 친절하게)
            List<string> missingItems = new List<string>();
            if (!hasWater && waterCost > 0) missingItems.Add($"{waterCost} Water");
            if (!hasChitin && chitinCost > 0) missingItems.Add($"{chitinCost} Chitin");
            // 다른 부족한 자원 추가

            Debug.Log($"[{gameObject.name}] 성장에 필요한 자원이 부족합니다. 부족: {string.Join(", ", missingItems)}");
            // 여기에 사용자에게 부족 알림 UI나 사운드 효과 추가 가능
        }
    }

    // 성체 식물로 성장하는 함수
   void GrowToMature()
    {
        if (hasGrown) return;
        if (maturePlantPrefab == null) return;

        hasGrown = true;

        // 성장 사운드 재생
        if (growSound != null)
        {
            AudioSource.PlayClipAtPoint(growSound, transform.position, growSoundVolume);
        }

        // --- 추가: 파티클 효과 생성 ---
        if (growthParticlePrefab != null)
        {
            // 파티클 생성 (월드 좌표에)
            GameObject particleEffect = Instantiate(growthParticlePrefab, transform.position, Quaternion.identity);
            // 파티클이 자동으로 사라지도록 설정 (파티클 시스템 자체 설정 또는 Destroy 사용)
            Destroy(particleEffect, 3.0f); // 예: 3초 후 파티클 오브젝트 제거
        }
        // --------------------------

        // 성체 식물 생성
        Instantiate(maturePlantPrefab, transform.position, transform.rotation);
        Debug.Log($"[{gameObject.name}] 성체 식물({maturePlantPrefab.name}) 생성 완료.");

        // 새싹 오브젝트 제거
        Destroy(gameObject);
    }
}