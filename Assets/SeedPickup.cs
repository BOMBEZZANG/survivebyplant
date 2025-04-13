using UnityEngine;

public class SeedPickup : MonoBehaviour
{
    public int seedsToAdd = 1; // 이 아이템이 주는 씨앗의 개수

    // 다른 Collider2D가 이 오브젝트의 Trigger 안으로 들어왔을 때 호출됨
    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트가 "Player" 태그를 가지고 있는지 확인
        // (플레이어 오브젝트에 "Player" 태그가 설정되어 있어야 함)
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 씨앗 아이템 트리거에 닿았습니다!");

            // 플레이어 오브젝트에서 PlayerInventory 컴포넌트를 찾음
            PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();

            // PlayerInventory 컴포넌트가 있다면 씨앗 추가
            if (playerInventory != null)
            {
                playerInventory.AddSeeds(seedsToAdd);
                // 아이템 오브젝트 자신을 파괴하여 씬에서 제거
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("닿은 오브젝트('Player' 태그)에 PlayerInventory 컴포넌트가 없습니다.");
            }
        }
    }
}