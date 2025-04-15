using UnityEngine;

public class WorldUIFollow : MonoBehaviour
{
    [HideInInspector] // 부모 스크립트에서 설정하므로 Inspector 노출 안함
    public Transform targetTransform; // 따라다닐 대상 (씨앗, 새싹 등)
    [HideInInspector]
    public Vector3 offset; // 대상으로부터의 상대적 위치

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // 카메라 렌더링 후 위치 업데이트 (UI 떨림 방지)
    void LateUpdate()
    {
        if (targetTransform != null)
        {
            // 대상 위치에 오프셋을 더해 UI 위치 설정
            transform.position = targetTransform.position + offset;

            // UI가 항상 카메라를 바라보도록 회전 (World Space Canvas용)
            if (mainCamera != null)
            {
                transform.forward = mainCamera.transform.forward;
            }
        }
        else
        {
            // 따라갈 대상이 사라지면 UI도 함께 파괴
            Destroy(gameObject);
        }
    }
}