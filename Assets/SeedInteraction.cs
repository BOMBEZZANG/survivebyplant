using UnityEngine;
using System.Collections;

// 필수 컴포넌트 명시
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))] // SpriteRenderer 추가
public class SeedInteraction : MonoBehaviour, IInteractable
{
    [Header("성장 설정")]
    public GameObject sproutPrefab;

    [Header("필요 자원")]
    public string requiredResource = "Water";
    public int resourceCost = 1;

    [Header("피드백")]
    public AudioClip wateringSound;
    [Range(0f, 1f)]
    public float wateringSoundVolume = 1.0f;

    // --- 하이라이트 관련 변수 ---
    [Header("Interaction Visuals")]
    [Tooltip("플레이어가 범위 내에 있을 때 하이라이트할 색상")]
    public Color highlightColor = Color.yellow;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    // ---------------------------

    private bool hasGrown = false;

    public string InteractionPrompt => $"Water Seed\n(Needs: {resourceCost} {requiredResource})";

void Start() // Awake 대신 Start 사용 가능
    {
        // --- 하이라이트 관련 초기화 ---
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] SpriteRenderer 컴포넌트가 없습니다!", gameObject);
        }

        // --- Collider 확인 및 Trigger 강제 변경 로직 *제거* ---
        // Collider2D col = GetComponent<Collider2D>(); // 여러 개일 수 있으므로 이 방식 비권장

        // 대신, 트리거용 콜라이더가 있는지 확인하고 없으면 경고 (선택적)
        bool hasTriggerCollider = false;
        Collider2D[] colliders = GetComponents<Collider2D>(); // 모든 콜라이더 가져오기
        if (colliders.Length == 0)
        {
             Debug.LogError($"[{gameObject.name}] Collider2D 컴포넌트가 없습니다! 하이라이트 및 상호작용 불가.", gameObject);
             enabled = false;
             return; // 콜라이더 없으면 Start 종료
        }

        foreach(Collider2D c in colliders)
        {
            if (c.isTrigger)
            {
                hasTriggerCollider = true;
                break; // 트리거 콜라이더 하나 찾으면 확인 종료
            }
        }

        if (!hasTriggerCollider)
        {
             // 경고: 사용자가 트리거 콜라이더 설정을 잊었을 수 있음을 알림
             Debug.LogWarning($"[{gameObject.name}] SeedInteraction: 하이라이트 기능을 위한 Trigger Collider (IsTrigger=true)가 없습니다. OnTriggerEnter/Exit가 작동하지 않을 수 있습니다.", gameObject);
             // 여기서 강제로 켜지 않음!
        }
        // -------------------------------------------------------
    }

    public void Interact(GameObject interactor)
    {
        if (hasGrown) return;
        PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
        if (playerInventory == null) return;

        if (playerInventory.HasEnoughResource(requiredResource, resourceCost)) {
             if (playerInventory.UseResource(requiredResource, resourceCost)) {
                 GrowToSprout();
             }
        } else {
             Debug.Log($"[{gameObject.name}] '{requiredResource}' 부족. 필요: {resourceCost}");
             // 부족 피드백 (소리 등)
        }
    }

    void GrowToSprout()
    {
        if (hasGrown) return;
        if (sproutPrefab == null) return;
        hasGrown = true;
        if (wateringSound != null) { AudioSource.PlayClipAtPoint(wateringSound, transform.position, wateringSoundVolume); }
        Instantiate(sproutPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    // --- 하이라이트 로직 ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 아직 성장하지 않았고, 렌더러가 있다면 하이라이트
            if (!hasGrown && spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 렌더러가 있다면 원래 색으로 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
    // ---------------------
}