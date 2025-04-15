using UnityEngine;
using System.Collections; // 코루틴 사용 가능 (예: 쿨다운)

// 우물 오브젝트가 상호작용 가능하도록 IInteractable 구현
[RequireComponent(typeof(Collider2D))] // 상호작용 감지를 위해 Collider 필요
public class WellInteraction : MonoBehaviour, IInteractable
{
    [Header("Resource Settings")]
    [Tooltip("획득할 자원의 이름 (PlayerInventory와 일치)")]
    public string waterResourceName = "Water";
    [Tooltip("한 번 상호작용 시 얻는 물의 양")]
    public int waterAmountPerInteraction = 1;

    [Header("Feedback")]
    [Tooltip("물 획득 시 재생할 사운드 (선택 사항)")]
    public AudioClip collectSound;
    [Range(0f, 1f)]
    public float collectSoundVolume = 1.0f;
    // 필요하다면 파티클 이펙트 프리팹 변수 추가 가능
    // public GameObject collectParticlePrefab;

    [Header("Cooldown (Optional)")]
    [Tooltip("물 획득 후 다시 획득 가능할 때까지 걸리는 시간 (초). 0이면 쿨다운 없음.")]
    public float cooldownSeconds = 1.0f; // 예: 1초 쿨다운
    private float lastInteractionTime = -Mathf.Infinity; // 마지막 상호작용 시간 (쿨다운 계산용)

    // IInteractable 인터페이스 구현: 상호작용 프롬프트
    public string InteractionPrompt => $"Collect {waterResourceName}"; // 예: "Collect Water"

    // IInteractable 인터페이스 구현: 상호작용 로직
    public void Interact(GameObject interactor)
    {
        // 쿨다운 체크 (cooldownSeconds가 0보다 클 경우)
        if (cooldownSeconds > 0 && Time.time < lastInteractionTime + cooldownSeconds)
        {
            Debug.Log($"[{gameObject.name}] 우물이 아직 재충전 중입니다... (남은 시간: {(lastInteractionTime + cooldownSeconds - Time.time):F1}초)");
            // 여기에 '아직 물을 얻을 수 없음' 피드백 (소리 등) 추가 가능
            return; // 쿨다운 중이면 함수 종료
        }

        // 상호작용한 오브젝트(플레이어)에서 PlayerInventory 가져오기
        PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
            Debug.LogError($"[{gameObject.name}] 상호작용자({interactor.name})에게 PlayerInventory 컴포넌트가 없습니다!", interactor);
            return;
        }

        // 인벤토리에 물 추가 시도 (AddResource는 성공 여부 bool 반환)
        bool added = playerInventory.AddResource(waterResourceName, waterAmountPerInteraction);

        if (added)
        {
            Debug.Log($"[{gameObject.name}] 플레이어가 '{waterResourceName}' {waterAmountPerInteraction}개를 획득했습니다.");

            // 마지막 상호작용 시간 기록 (쿨다운용)
            lastInteractionTime = Time.time;

            // 획득 사운드 재생
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, collectSoundVolume);
            }

            // (선택 사항) 획득 파티클 효과 생성
            // if (collectParticlePrefab != null) {
            //     Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);
            // }
        }
        // AddResource가 실패한 경우 (예: 인벤토리 공간 부족 - PlayerInventory에서 처리 필요)
        // else {
        //     Debug.LogWarning($"[{gameObject.name}] '{waterResourceName}' 추가 실패. (PlayerInventory.AddResource 반환값 false)");
        //     // 여기에 '인벤토리가 가득 참' 등의 피드백 추가 가능
        // }
    }

    void Start()
    {
        // 초기 쿨다운 상태 설정 (게임 시작 시 바로 사용 가능하도록)
        lastInteractionTime = -cooldownSeconds;
         // Collider 확인
         if (GetComponent<Collider2D>() == null)
         {
              Debug.LogError($"[{gameObject.name}] Collider2D 컴포넌트가 없습니다. 상호작용이 불가능합니다.", gameObject);
         }
    }
}