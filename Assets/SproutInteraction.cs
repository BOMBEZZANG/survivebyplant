using UnityEngine;
using System.Collections.Generic;
using TMPro; // 요구사항 UI의 Text 컴포넌트 사용을 위해 추가

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
    public Color highlightColor = Color.yellow;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    // ---------------------------------

    // --- 요구사항 UI 관련 변수 추가 ---
    [Header("Requirement UI")]
    [Tooltip("필요 자원을 표시할 UI 프리팹 (World Space Canvas + TextMeshProUGUI 포함)")]
    public GameObject requirementUIPrefab;
    [Tooltip("새싹 위치 기준 UI 표시 오프셋")]
    public Vector3 uiOffset = new Vector3(0, 0.8f, 0);
    private GameObject requirementUIInstance;
    private TextMeshProUGUI requirementText;
    // ------------------------------------

    private bool hasGrown = false;

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

        // --- Collider 확인 (Trigger 강제 변경 로직 *제거됨*) ---
        bool hasTriggerCollider = false;
        Collider2D[] colliders = GetComponents<Collider2D>(); // 모든 콜라이더 가져오기
        if (colliders.Length == 0) {
             Debug.LogError($"[{gameObject.name}] Collider2D 컴포넌트가 없습니다! 하이라이트 및 상호작용 불가.", gameObject);
             enabled = false;
             return; // 콜라이더 없으면 Start 종료
        }
        // Trigger 콜라이더가 있는지 확인 (하이라이트에 필요)
        foreach(Collider2D c in colliders) {
            if (c.isTrigger) {
                hasTriggerCollider = true;
                break;
            }
        }
        // 경고 메시지 (없을 경우)
        if (!hasTriggerCollider) {
             Debug.LogWarning($"[{gameObject.name}] SproutInteraction: 하이라이트 기능을 위한 Trigger Collider (IsTrigger=true)가 없습니다. OnTriggerEnter/Exit가 작동하지 않을 수 있습니다. 해당 콜라이더의 Is Trigger를 켜주세요.", gameObject);
             // 자동으로 켜지 않음! 사용자가 직접 설정해야 함.
        }
        // -------------------------------------------------------

        // 성체 프리팹 연결 확인
        if (maturePlantPrefab == null) {
             Debug.LogError($"[{gameObject.name}] Mature Plant Prefab이 Inspector에 연결되지 않았습니다! 성장할 수 없습니다.", gameObject);
             enabled = false;
        }

        // --- 요구사항 UI 생성 및 설정 ---
        if (requirementUIPrefab != null)
        {
            requirementUIInstance = Instantiate(requirementUIPrefab, transform.position + uiOffset, Quaternion.identity);
            requirementUIInstance.name = $"{gameObject.name}_RequirementUI";

            WorldUIFollow follower = requirementUIInstance.GetComponent<WorldUIFollow>();
            if (follower != null) { follower.targetTransform = transform; follower.offset = uiOffset; }
            else { Debug.LogWarning($"[{gameObject.name}] Requirement UI Prefab에 WorldUIFollow 스크립트가 없습니다."); }

            requirementText = requirementUIInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (requirementText != null)
            {
                requirementText.text = GetRequirementString();
                requirementUIInstance.SetActive(false); // 시작 시 숨김
            }
            else { Debug.LogError($"[{gameObject.name}] Requirement UI Prefab 내부에 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다!", requirementUIInstance); Destroy(requirementUIInstance); }
        }
        else { Debug.LogWarning($"[{gameObject.name}] Requirement UI Prefab이 할당되지 않았습니다.", gameObject); }
        // --------------------------------
    }

    // 필요 자원 텍스트 생성 함수
    private string GetRequirementString()
    {
        List<string> requirements = new List<string>();
        if (waterCost > 0) requirements.Add($"Water: {waterCost}");
        if (chitinCost > 0) requirements.Add($"Chitin: {chitinCost}");

        if (requirements.Count > 0) return string.Join("\n", requirements);
        else return "";
    }

    // Interact 메서드는 변경 없음
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
            } else { /* 환불 */ }
        } else { /* 부족 로그 */ }
    }

    // GrowToMature 메서드는 변경 없음 (UI 파괴 로직 포함)
    void GrowToMature()
    {
        if (hasGrown) return;
        if (maturePlantPrefab == null) return;
        hasGrown = true;
        if (growSound != null) { AudioSource.PlayClipAtPoint(growSound, transform.position, growSoundVolume); }
        if (growthParticlePrefab != null) { /* 파티클 생성 */ }
        Instantiate(maturePlantPrefab, transform.position, transform.rotation);
        if (requirementUIInstance != null) Destroy(requirementUIInstance); // UI 파괴
        Destroy(gameObject); // 자신(새싹) 파괴
    }

    // --- 하이라이트 및 UI 표시 로직 추가 ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 아직 성장하지 않았을 때만 하이라이트 및 UI 표시
            if (!hasGrown)
            {
                 if (spriteRenderer != null) spriteRenderer.color = highlightColor;
                 if (requirementUIInstance != null) requirementUIInstance.SetActive(true); // UI 표시
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 하이라이트 및 UI 숨김
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            if (requirementUIInstance != null) requirementUIInstance.SetActive(false); // UI 숨김
        }
    }
    // ---------------------------------

    // --- 오브젝트 파괴 시 UI 정리 ---
    void OnDestroy()
    {
        if (requirementUIInstance != null)
        {
            Destroy(requirementUIInstance);
        }
    }
    // ---------------------------
}