using UnityEngine;

// 이 스크립트는 SeedPickup, ChitinScrapPickup 등 줍는 아이템 프리팹에 붙입니다.
// 프리팹에는 반드시 Is Trigger가 켜진 Collider2D 컴포넌트가 있어야 합니다.
public class ResourcePickup : MonoBehaviour
{
    [Header("Resource Details")]
    [Tooltip("이 아이템이 나타내는 자원의 종류 (PlayerInventory의 AddResource와 일치해야 함)")]
    public string resourceType = "ChitinScrap"; // 프리팹 종류에 맞게 Inspector에서 수정 (예: 씨앗이면 "Seed")

    [Tooltip("이 아이템 하나가 주는 자원의 양")]
    public int amount = 1;

    [Header("Feedback")]
    [Tooltip("아이템을 주웠을 때 재생할 오디오 클립 (선택 사항)")]
    public AudioClip pickupSoundClip; // Inspector에서 사운드 파일 연결

    // 이 오브젝트의 Trigger Collider 안으로 다른 Collider가 들어왔을 때 호출됨
    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트의 태그가 "Player"인지 확인
        // (Player 게임 오브젝트에 "Player" 태그가 설정되어 있어야 함)
        if (other.CompareTag("Player"))
        {
            // 상세 로그 (필요시 주석 해제)
            // Debug.Log($"ResourcePickup: Player entered trigger for {resourceType}");

            // 플레이어 오브젝트에서 PlayerInventory 컴포넌트를 찾음
            PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();

            // PlayerInventory 컴포넌트를 찾았는지 확인
            if (playerInventory == null)
            {
                Debug.LogWarning($"ResourcePickup: Player object ('{other.name}') does not have PlayerInventory component!", gameObject);
                return; // PlayerInventory 없으면 함수 종료
            }

            // PlayerInventory에 자원 추가 시도 및 성공 여부 확인
            if (playerInventory.AddResource(resourceType, amount))
            {
                // === 사운드 재생 ===
                if (pickupSoundClip != null)
                {
                    // 아이템이 사라지는 위치(현재 위치)에서 사운드 재생
                    // PlayClipAtPoint는 씬에 임시 AudioSource를 생성하고 재생 후 자동 제거함
                    AudioSource.PlayClipAtPoint(pickupSoundClip, transform.position);
                }
                // === 사운드 재생 끝 ===

                // 자원 추가에 성공했으므로, 이 아이템 오브젝트를 파괴하여 제거
                Destroy(gameObject);
            }
            // AddResource가 false를 반환하는 경우 (예: 인벤토리 꽉 참 등 PlayerInventory에서 처리)
            // else
            // {
            //    Debug.Log($"ResourcePickup: Could not add {resourceType} (returned false from PlayerInventory)."); // 필요시 주석 해제
            // }
        }
    } // OnTriggerEnter2D 끝

} // ResourcePickup 클래스 끝 (여기까지만 있어야 함!)