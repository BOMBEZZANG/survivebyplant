using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))] // AudioSource도 필수 컴포넌트로 추가
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("플레이어의 이동 속도")]
    public float moveSpeed = 5f;

    // --- 사운드 관련 변수 추가 ---
    [Header("Audio Settings")]
    [Tooltip("재생할 발걸음 소리 오디오 클립 배열")]
    public AudioClip[] footstepSounds; // 여러 발걸음 소리 저장 가능
    [Tooltip("발걸음 소리 재생 간격 (초)")]
    public float footstepInterval = 0.4f; // 이 시간마다 소리 재생 시도
    [Range(0f, 1f)]
    [Tooltip("발걸음 소리 볼륨")]
    public float footstepVolume = 0.8f;

    [Header("Initialization")]
    [Tooltip("플레이어가 게임 시작 시 생성될 위치 (씬에 빈 게임오브젝트 배치 후 연결)")]
    public Transform startPosition;

    // --- Private Variables ---
    private Rigidbody2D rb;
    private Vector2 movement;
    private AudioSource audioSource; // AudioSource 컴포넌트 참조
    private float nextFootstepTime = 0f; // 다음 발걸음 소리 재생 시간

    // Start 함수는 System.Obsolete 경고가 있으므로 Awake로 변경 권장
    [System.Obsolete]
    void Awake() // Start 대신 Awake 사용
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); // AudioSource 컴포넌트 가져오기

        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody2D component!", gameObject);
            enabled = false;
            return;
        }
        // --- AudioSource 확인 추가 ---
        if (audioSource == null)
        {
            Debug.LogError("PlayerMovement requires an AudioSource component!", gameObject);
            enabled = false;
            return;
        }
        // --- 사운드 배열 확인 ---
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
             Debug.LogWarning("PlayerMovement: Footstep Sounds 배열이 비어있거나 할당되지 않았습니다. 발걸음 소리가 재생되지 않습니다.", gameObject);
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (startPosition != null)
        {
            transform.position = startPosition.position;
        }
        else
        {
            Debug.LogWarning("PlayerMovement: 시작 위치(startPosition)가 지정되지 않았습니다! 현재 위치에서 시작합니다.", gameObject);
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Debug.Log("PlayerMovement Awake() 완료. 플레이어 위치: " + transform.position);
    }

    [System.Obsolete]
    void Update()
    {
        // 입력 처리
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // --- 발걸음 소리 재생 로직 ---
        HandleFootstepSounds();
    }

    // FixedUpdate는 물리 처리 유지
    [System.Obsolete]
    void FixedUpdate()
    {
        if (rb == null) return;

        Vector2 targetVelocity = movement.normalized * moveSpeed;
        rb.velocity = targetVelocity;

        // 디버깅 로그는 필요시 주석 해제
        // if (targetVelocity.sqrMagnitude > 0.01f) { ... }
    }

    // 발걸음 소리 처리 함수
    [System.Obsolete]
    void HandleFootstepSounds()
    {
        // 필수 요소 없으면 실행 중단
        if (audioSource == null || footstepSounds == null || footstepSounds.Length == 0) return;

        // 플레이어가 움직이고 있는지 확인 (속도 기준)
        // magnitude 비교보다 sqrMagnitude가 약간 더 효율적
        // 0.01f 같은 작은 값(threshold)보다 큰지 비교하여 거의 멈춘 상태는 제외
        bool isMoving = rb.velocity.sqrMagnitude > 0.1f * 0.1f; // 속도의 제곱이 0.01보다 큰가?

        if (isMoving)
        {
            // 현재 시간이 다음 소리 재생 시간이거나 지났는지 확인
            if (Time.time >= nextFootstepTime)
            {
                // 배열에서 랜덤하게 발걸음 소리 선택
                int randomIndex = Random.Range(0, footstepSounds.Length);
                AudioClip clipToPlay = footstepSounds[randomIndex];

                // 선택된 클립이 null이 아니면 재생
                if (clipToPlay != null)
                {
                    // PlayOneShot: 여러 소리가 겹쳐서 재생될 수 있음 (발걸음에 적합)
                    audioSource.PlayOneShot(clipToPlay, footstepVolume);
                }

                // 다음 재생 시간 업데이트
                nextFootstepTime = Time.time + footstepInterval;
            }
        }
        // (선택적) 움직이지 않을 때 다음 재생 시간을 현재 시간으로 리셋하여
        // 다시 움직이기 시작할 때 바로 소리가 나도록 할 수 있음
        // else
        // {
        //     nextFootstepTime = Time.time; // 멈추면 타이머 리셋
        // }
    }
}