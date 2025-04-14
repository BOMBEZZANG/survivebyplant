using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

// 이 스크립트는 Collider2D가 반드시 필요함을 명시 (없으면 에디터에서 경고)
[RequireComponent(typeof(Collider2D))]
public class ResourcePickup : MonoBehaviour
{
    [Header("Resource Details")]
    [Tooltip("이 아이템이 나타내는 자원의 종류 (PlayerInventory의 AddResource와 일치해야 함)")]
    public string resourceType = "ChitinScrap"; // 프리팹 종류에 맞게 Inspector에서 수정 (예: 씨앗이면 "Seed")

    [Tooltip("이 아이템 하나가 주는 자원의 양")]
    public int amount = 1;

    // --- 추가: 픽업 지연 시간 설정 ---
    [Header("Pickup Delay")]
    [Tooltip("아이템이 생성된 후 획득 가능해질 때까지의 시간 (초)")]
    public float pickupDelay = 0.5f; // 0.5초 후 획득 가능

    [Header("Feedback")]
    [Tooltip("아이템을 주웠을 때 재생할 오디오 클립 (선택 사항)")]
    public AudioClip pickupSoundClip;

    // --- 내부 변수 ---
    private Collider2D pickupCollider; // 이 오브젝트의 콜라이더 참조
    private bool canBePickedUp = false; // 픽업 가능 상태 플래그 (대체 방법용, 현재는 콜라이더 활성화/비활성화 사용)

    void Awake() // Start 대신 Awake 사용 권장 (비활성화 전에 실행)
    {
        // 자신의 Collider2D 컴포넌트를 찾음
        pickupCollider = GetComponent<Collider2D>();

        if (pickupCollider == null)
        {
            Debug.LogError($"ResourcePickup ({gameObject.name}): Collider2D 컴포넌트를 찾을 수 없습니다! 픽업이 작동하지 않습니다.", gameObject);
            enabled = false; // 스크립트 비활성화
            return;
        }

        // Collider가 Trigger 모드인지 확인 (필수)
        if (!pickupCollider.isTrigger)
        {
             Debug.LogWarning($"ResourcePickup ({gameObject.name}): Collider2D가 Is Trigger로 설정되어 있지 않습니다. 자동으로 설정합니다.", gameObject);
             pickupCollider.isTrigger = true;
        }

        // --- 시작 시 콜라이더 비활성화 ---
        pickupCollider.enabled = false;
        // Debug.Log($"ResourcePickup ({gameObject.name}): Collider 비활성화됨. {pickupDelay}초 후 활성화됩니다."); // 필요시 주석 해제

        // --- 지연 후 콜라이더 활성화 코루틴 시작 ---
        StartCoroutine(EnablePickupAfterDelay());
    }

    // 지정된 시간 후에 콜라이더를 활성화하는 코루틴
    IEnumerator EnablePickupAfterDelay()
    {
        // pickupDelay 만큼 대기
        yield return new WaitForSeconds(pickupDelay);

        // 대기 후 콜라이더가 여전히 유효하다면 (오브젝트가 파괴되지 않았다면)
        if (pickupCollider != null)
        {
            pickupCollider.enabled = true; // 콜라이더 활성화
            canBePickedUp = true; // 플래그 업데이트 (대체 방법용)
            // Debug.Log($"ResourcePickup ({gameObject.name}): Collider 활성화됨. 이제 픽업 가능."); // 필요시 주석 해제
        }
    }

    // 이 오브젝트의 Trigger Collider 안으로 다른 Collider가 들어왔을 때 호출됨
    void OnTriggerEnter2D(Collider2D other)
    {
        // --- 콜라이더가 활성화된 상태에서만 아래 로직 실행 ---
        // (EnablePickupAfterDelay 코루틴이 실행된 후)

        if (other.CompareTag("Player"))
        {
            PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();

            if (playerInventory == null)
            {
                Debug.LogWarning($"ResourcePickup: Player object ('{other.name}') does not have PlayerInventory component!", gameObject);
                return;
            }

            if (playerInventory.AddResource(resourceType, amount))
            {
                if (pickupSoundClip != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSoundClip, transform.position);
                }
                Destroy(gameObject);
            }
        }
    }
}