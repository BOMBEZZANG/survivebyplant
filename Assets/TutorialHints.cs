using UnityEngine;
using System.Collections; // 코루틴 사용
using TMPro; // TextMeshPro 사용

public class TutorialHints : MonoBehaviour
{
    [Tooltip("심기 힌트를 표시할 UI 게임 오브젝트 (Text 또는 Panel 등)")]
    public GameObject plantingHintUI;

    [Tooltip("힌트가 표시될 시간 (초)")]
    public float hintDuration = 5.0f;

    // === Awake 대신 Start 또는 OnEnable 사용 권장 ===
    // PlayerInventory의 Awake보다 늦게 실행되도록 하기 위함
    void Start()
    {
        // 이벤트 구독
        PlayerInventory.OnFirstSeedCollected += ShowPlantingHint;

        // 시작 시 힌트 UI 비활성화 확실히 하기
        if (plantingHintUI != null)
        {
            plantingHintUI.SetActive(false);
        }
        else
        {
            Debug.LogError("TutorialHints: Planting Hint UI가 Inspector에 연결되지 않았습니다!", gameObject);
        }
    }

    // 오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
    void OnDestroy()
    {
        PlayerInventory.OnFirstSeedCollected -= ShowPlantingHint;
    }

    // 첫 씨앗 획득 이벤트가 발생하면 호출될 함수
    private void ShowPlantingHint()
    {
        if (plantingHintUI != null && !plantingHintUI.activeSelf) // UI가 있고 아직 활성화되지 않았다면
        {
            Debug.Log("Showing Planting Hint...");
            plantingHintUI.SetActive(true); // 힌트 UI 활성화
            StartCoroutine(HideHintAfterDelay(hintDuration)); // 일정 시간 후 숨기는 코루틴 시작

            // 이벤트 구독 해제 (한 번만 보여주기 위함)
            // 계속 보여주고 싶다면 이 줄을 주석 처리하거나 제거
            PlayerInventory.OnFirstSeedCollected -= ShowPlantingHint;
        }
    }

    // 지정된 시간 후에 힌트 UI를 비활성화하는 코루틴
    IEnumerator HideHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기

        if (plantingHintUI != null)
        {
            Debug.Log("Hiding Planting Hint after delay.");
            plantingHintUI.SetActive(false); // 힌트 UI 비활성화
        }
    }

    // (선택 사항) 플레이어가 P키를 누르면 힌트를 바로 숨기는 로직
    void Update()
    {
        if (plantingHintUI != null && plantingHintUI.activeSelf && Input.GetKeyDown(KeyCode.P))
        {
             Debug.Log("Hiding Planting Hint because P key was pressed.");
             StopCoroutine(HideHintAfterDelay(0f)); // 코루틴 중지 (이미 실행 중일 수 있으므로)
             plantingHintUI.SetActive(false); // 힌트 UI 비활성화
        }
    }
}