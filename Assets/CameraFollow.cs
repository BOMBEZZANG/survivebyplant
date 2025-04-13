using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 플레이어 트랜스폼
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10); // z 값은 카메라가 앞에 보이도록 설정

    void LateUpdate()
    {
        if (target == null)
            return;
            
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}