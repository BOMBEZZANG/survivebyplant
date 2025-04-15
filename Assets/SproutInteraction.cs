using UnityEngine;
using System.Collections.Generic;

// 필수 컴포넌트 명시
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))] // SpriteRenderer 추가
public class SproutInteraction : MonoBehaviour, IInteractable
{
    [Header("성장 설정")]
    [Tooltip("성장하여 최종적으로 생성될 성체 식물 프리팹")]
    public GameObject maturePlantPrefab;

    [Header("필요 자원")]
    [Tooltip("성장에 필요한 물의 양")]
    public int waterCost = 1;
    [Tooltip("성장에 필요한 키틴 조각의 양")]
    public int chitinCost = 1;

    [Header("피드백")]
    [Tooltip("성장 성공 시 재생할 사운드 (선택 사항)")]
    public AudioClip growSound;
    [Range(0f, 1f)]
    public float growSoundVolume = 1.0f;
    [Tooltip("성장 시 재생할 파티클 이펙트 프리팹 (선택 사항)")]
    public GameObject growthParticlePrefab;

    // --- 하이라이트 관련 변수 추가 ---
    [Header("Interaction Visuals")]
    [Tooltip("플레이어가 범위 내에 있을 때 하이라이트할 색상")]
    public Color highlightColor = Color.yellow; // Inspector에서 색상 변경 가능
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    // ---------------------------------

    private bool hasGrown = false;

    // InteractionPrompt는 이전과 동일
    public string InteractionPrompt
    {
        get
        {
            List<string> requiredItems = new List<string>();
            if (waterCost > 0) requiredItems.Add($"{waterCost} Water");
            if (chitinCost > 0) requiredItems.Add($"{chitinCost} Chitin");
            if (requiredItems.Count > 0) return $"Grow Plant\n(Needs: {string.Join(", ", requiredItems)})";
            else return "Grow Plant";
        }
    }

void Start()
    {
        // --- 하이라이트 관련 초기화 ---
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            originalColor = spriteRenderer.color;
        } else {
            Debug.LogError($"[{gameObject.name}] SpriteRenderer 컴포넌트가 없습니다!", gameObject);
        }

        // --- Collider 확인 및 Trigger 강제 변경 로직 *제거* ---
        bool hasTriggerCollider = false;
        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length == 0) {
             Debug.LogError($"[{gameObject.name}] Collider2D 컴포넌트가 없습니다!", gameObject);
             enabled = false;
             return;
        }
        foreach(Collider2D c in colliders) {
            if (c.isTrigger) { hasTriggerCollider = true; break; }
        }
        if (!hasTriggerCollider) {
             Debug.LogWarning($"[{gameObject.name}] SproutInteraction: 하이라이트 기능을 위한 Trigger Collider (IsTrigger=true)가 없습니다.", gameObject);
        }
        // -------------------------------------------------------

        // 성체 프리팹 연결 확인
        if (maturePlantPrefab == null) { /* ... */ }
    }
    // Interact 메서드는 이전과 동일
    public void Interact(GameObject interactor)
    {
        // ... (자원 확인, 소모, GrowToMature 호출 로직) ...
        if (hasGrown) return;
        PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
        if (playerInventory == null) return;

        bool hasWater = playerInventory.HasEnoughResource("Water", waterCost);
        bool hasChitin = playerInventory.HasEnoughResource("ChitinScrap", chitinCost);

        if (hasWater && hasChitin) {
            bool waterUsed = playerInventory.UseResource("Water", waterCost);
            bool chitinUsed = playerInventory.UseResource("ChitinScrap", chitinCost);
            if (waterUsed && chitinUsed) {
                GrowToMature();
            } else {
                // 자원 환불
                if (waterUsed) playerInventory.AddResource("Water", waterCost);
                if (chitinUsed) playerInventory.AddResource("ChitinScrap", chitinCost);
            }
        } else {
            // 부족 메시지 로그 (또는 UI 피드백)
            List<string> missingItems = new List<string>();
            if (!hasWater && waterCost > 0) missingItems.Add($"{waterCost} Water");
            if (!hasChitin && chitinCost > 0) missingItems.Add($"{chitinCost} Chitin");
            Debug.Log($"[{gameObject.name}] 성장에 필요한 자원이 부족합니다. 부족: {string.Join(", ", missingItems)}");
        }
    }

    // GrowToMature 메서드는 이전과 동일
    void GrowToMature()
    {
        // ... (사운드, 파티클, 성체 생성, 자신 파괴 로직) ...
        if (hasGrown) return;
        if (maturePlantPrefab == null) return;
        hasGrown = true;
        if (growSound != null) { AudioSource.PlayClipAtPoint(growSound, transform.position, growSoundVolume); }
        if (growthParticlePrefab != null) {
            GameObject particleEffect = Instantiate(growthParticlePrefab, transform.position, Quaternion.identity);
            Destroy(particleEffect, 3.0f);
        }
        Instantiate(maturePlantPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    // --- 하이라이트 로직 추가 ---
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
    // ---------------------------
}