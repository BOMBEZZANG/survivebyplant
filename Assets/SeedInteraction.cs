using UnityEngine;
using System.Collections;
using TMPro; // TextMeshPro 사용

// 필수 컴포넌트 명시
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
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

    [Header("피드백")]
    [Tooltip("물 주기 성공 시 재생할 사운드 (선택 사항)")]
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

    // --- 요구사항 UI 관련 변수 ---
    [Header("Requirement UI")]
    [Tooltip("필요 자원을 표시할 UI 프리팹 (World Space Canvas + TextMeshProUGUI 포함)")]
    public GameObject requirementUIPrefab;
    [Tooltip("씨앗 위치 기준 UI 표시 오프셋")]
    public Vector3 uiOffset = new Vector3(0, 0.6f, 0);
    private GameObject requirementUIInstance;
    private TextMeshProUGUI requirementText;
    // --------------------------

    private bool hasGrown = false;

    // 플레이어에게 표시될 상호작용 안내 텍스트
    public string InteractionPrompt => $"Water Seed"; // 필요 자원은 UI로 표시하므로 단순화

    void Start()
    {
        // 스프라이트 렌더러 초기화 및 원래 색상 저장
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) { originalColor = spriteRenderer.color; }
        else { Debug.LogError($"[{gameObject.name}] SpriteRenderer 컴포넌트가 없습니다!", gameObject); }

        // 콜라이더 확인 및 트리거 설정 확인
        bool hasTriggerCollider = false;
        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length == 0) {
             Debug.LogError($"[{gameObject.name}] Collider2D 컴포넌트가 없습니다! 하이라이트 및 상호작용 불가.", gameObject);
             enabled = false;
             return;
        }
        foreach(Collider2D c in colliders) {
            if (c.isTrigger) { hasTriggerCollider = true; break; }
        }
        if (!hasTriggerCollider) {
             // 하이라이트를 위해서는 Trigger=true인 콜라이더가 *반드시* 필요합니다.
             Debug.LogError($"[{gameObject.name}] SeedInteraction: 하이라이트 기능을 위한 Trigger Collider (IsTrigger=true)가 없습니다! Inspector에서 설정해주세요.", gameObject);
             // 경고 대신 에러로 변경하여 문제 인지 강화
        }

        // 요구사항 UI 생성 및 설정
        if (requirementUIPrefab != null)
        {
            requirementUIInstance = Instantiate(requirementUIPrefab, transform.position + uiOffset, Quaternion.identity);
            requirementUIInstance.name = $"{gameObject.name}_RequirementUI";

            WorldUIFollow follower = requirementUIInstance.GetComponent<WorldUIFollow>();
            if (follower != null) { follower.targetTransform = transform; follower.offset = uiOffset; }
            else { Debug.LogWarning($"[{gameObject.name}] Requirement UI Prefab에 WorldUIFollow 스크립트가 없습니다."); }

            requirementText = requirementUIInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (requirementText != null) {
                requirementText.text = GetRequirementString();
                requirementUIInstance.SetActive(false); // 시작 시 숨김
            }
            else { Debug.LogError($"[{gameObject.name}] Requirement UI Prefab 내부에 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다!", requirementUIInstance); Destroy(requirementUIInstance); }
        }
        else { Debug.LogWarning($"[{gameObject.name}] Requirement UI Prefab이 할당되지 않았습니다.", gameObject); }
    }

    // 필요 자원 텍스트 생성 함수
    private string GetRequirementString()
    {
        if (resourceCost > 0) {
            return $"{requiredResource}: {resourceCost}";
        } else return "";
    }

    // E 키 상호작용 시 호출될 함수
    public void Interact(GameObject interactor)
    {
        if (hasGrown) return;
        PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
        if (playerInventory == null) return;

        Debug.Log($"[{gameObject.name}] Interact() 시도. 필요: {requiredResource} {resourceCost}개");
        if (playerInventory.HasEnoughResource(requiredResource, resourceCost)) {
             if (playerInventory.UseResource(requiredResource, resourceCost)) {
                 Debug.Log($"[{gameObject.name}] 자원 사용 성공. 성장 시작.");
                 GrowToSprout();
             }
        } else {
             Debug.Log($"[{gameObject.name}] '{requiredResource}' 부족.");
             // 부족 피드백 (소리 등)
        }
    }

    // 새싹으로 성장하는 함수
    void GrowToSprout()
    {
        if (hasGrown) return;
        if (sproutPrefab == null) { Debug.LogError($"[{gameObject.name}] Sprout Prefab이 없습니다!", gameObject); return; }

        hasGrown = true;
        Debug.Log($"[{gameObject.name}] 새싹으로 성장 중...");

        if (wateringSound != null) { AudioSource.PlayClipAtPoint(wateringSound, transform.position, wateringSoundVolume); }

        Instantiate(sproutPrefab, transform.position, transform.rotation);

        // 요구사항 UI 오브젝트 파괴
        if (requirementUIInstance != null) Destroy(requirementUIInstance);

        // 씨앗 오브젝트 파괴
        Destroy(gameObject);
    }

    // 플레이어가 범위에 들어왔을 때 (하이라이트 및 UI 표시)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!hasGrown) // 아직 성장 안했을때만
            {
                 if (spriteRenderer != null) spriteRenderer.color = highlightColor;
                 if (requirementUIInstance != null) requirementUIInstance.SetActive(true);
            }
        }
    }

    // 플레이어가 범위를 벗어났을 때 (하이라이트 및 UI 숨김)
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            if (requirementUIInstance != null) requirementUIInstance.SetActive(false);
        }
    }

    // 오브젝트 파괴 시 UI 정리
    void OnDestroy()
    {
        if (requirementUIInstance != null)
        {
            Destroy(requirementUIInstance);
        }
    }
}