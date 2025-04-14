using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class SeedInteraction : MonoBehaviour, IInteractable
{
    [Header("성장 설정")]
    [Tooltip("성장하여 생성될 새싹 프리팹")]
    public GameObject sproutPrefab;

    [Header("필요 자원")]
    [Tooltip("성장에 필요한 자원 이름 (PlayerInventory와 일치)")]
    public string requiredResource = "Water";
    [Tooltip("성장에 필요한 자원 개수")]
    public int resourceCost = 1;

    // --- 제거: interactionRange ---
    // [Header("상호작용")]
    // public float interactionRange = 1.5f;

    [Header("피드백")]
    [Tooltip("물 주기 성공 시 재생할 사운드 (선택 사항)")]
    public AudioClip wateringSound;
    [Range(0f, 1f)]
    public float wateringSoundVolume = 1.0f;

    private bool hasGrown = false;

    // 프롬프트 텍스트 (PlayerInteraction에서 사용)
    public string InteractionPrompt => "Press E to Water"; // 또는 "Water Seed (Needs " + resourceCost + ")" 등

    void Start()
    {
        // 플레이어 찾기 로직 제거됨
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
             Debug.LogError($"[{gameObject.name}] Collider2D 컴포넌트가 없습니다.", gameObject);
        }
    }

    // E키 + 클릭 상호작용으로 호출될 메서드
    public void Interact(GameObject interactor)
    {
        Debug.Log($"[{gameObject.name}] Interact() 호출됨. 상호작용 시도자: {interactor.name}");
        if (hasGrown) {
            Debug.Log($"[{gameObject.name}] 이미 성장함.");
            return;
        }

        PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
        if (playerInventory == null) {
             Debug.LogError($"[{gameObject.name}] 상호작용자({interactor.name})에게 PlayerInventory 컴포넌트가 없습니다!", interactor);
             return;
        }

        // --- 거리 체크 로직 제거됨 (PlayerInteraction에서 처리) ---

        if (playerInventory.HasEnoughResource(requiredResource, resourceCost)) {
             Debug.Log($"[{gameObject.name}] '{requiredResource}' 충분. 사용 시도...");
             if (playerInventory.UseResource(requiredResource, resourceCost)) {
                 Debug.Log($"[{gameObject.name}] '{requiredResource}' 사용 성공. 성장.");
                 GrowToSprout();
             }
        }
        else {
             Debug.Log($"[{gameObject.name}] '{requiredResource}' 부족. 필요: {resourceCost}");
             // 물 부족 피드백 (소리 등)
        }
    }

    // GrowToSprout 함수는 변경 없음
    void GrowToSprout()
    {
        if (hasGrown) return;
        if (sproutPrefab == null) {
             Debug.LogError($"[{gameObject.name}] Sprout Prefab이 연결되지 않았습니다!", gameObject);
             return;
        }
        hasGrown = true;
        if (wateringSound != null) {
             AudioSource.PlayClipAtPoint(wateringSound, transform.position, wateringSoundVolume);
        }
        Instantiate(sproutPrefab, transform.position, transform.rotation);
        Debug.Log($"[{gameObject.name}] 새싹({sproutPrefab.name}) 생성 완료.");
        Destroy(gameObject);
    }
}