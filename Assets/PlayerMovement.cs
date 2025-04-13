using UnityEngine;

// 이 스크립트는 Rigidbody2D 컴포넌트가 반드시 필요함을 명시
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("플레이어의 이동 속도")]
    public float moveSpeed = 5f; // Inspector에서 이 값이 0보다 큰지 확인하세요!

    [Header("Initialization")]
    [Tooltip("플레이어가 게임 시작 시 생성될 위치 (씬에 빈 게임오브젝트 배치 후 연결)")]
    public Transform startPosition; // 시작 위치 지정용 Transform

    // --- Private Variables ---
    private Rigidbody2D rb;       // Rigidbody2D 컴포넌트 참조 저장용
    private Vector2 movement; // 매 프레임 입력 값을 저장할 변수

    // 게임 오브젝트가 활성화될 때 또는 게임 시작 시 호출됨
    [System.Obsolete]
    void Start()
    {
        // Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        // 만약 Rigidbody2D가 없다면 에러 로그 출력 및 스크립트 비활성화
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody2D component!", gameObject);
            enabled = false; // 스크립트 작동 중지
            return;
        }

        // 2D 탑다운 또는 유사 환경에서는 중력 영향 안 받도록 설정
        rb.gravityScale = 0f;
        // 물리 효과로 인해 의도치 않게 회전하는 것을 방지 (필요 없다면 주석 처리)
        rb.freezeRotation = true;

        // 시작 위치 설정 (startPosition 변수가 Inspector에서 할당되었다면)
        if (startPosition != null)
        {
            transform.position = startPosition.position; // 지정된 위치로 플레이어 이동
            //Debug.Log($"플레이어 위치를 {startPosition.position} 로 설정했습니다.", gameObject);
        }
        else
        {
            // 시작 위치가 지정되지 않았다면 경고 로그 출력
            Debug.LogWarning("PlayerMovement: 시작 위치(startPosition)가 지정되지 않았습니다! 현재 위치에서 시작합니다.", gameObject);
        }

        // 중요: 게임 시작 시 이전 프레임의 물리 효과가 남아있을 수 있으므로 속도 초기화
        rb.velocity = Vector2.zero;        // 선형 속도 초기화 (linearVelocity와 동일하게 작동)
        rb.angularVelocity = 0f;           // 각속도(회전 속도) 초기화

        Debug.Log("PlayerMovement Start() 완료. 플레이어 위치: " + transform.position);
    }

    // 매 프레임 호출됨 (주로 입력 처리)
    void Update()
    {
        // 키보드 W, A, S, D 또는 방향키 입력 받기 (-1, 0, 1 값)
        movement.x = Input.GetAxisRaw("Horizontal"); // 좌/우 입력
        movement.y = Input.GetAxisRaw("Vertical");   // 상/하 입력

        // --- 디버깅 로그: 입력 값 확인 (필요시 주석 해제) ---
        // 입력 벡터의 제곱 크기가 0보다 약간 클 때 (즉, 입력이 있을 때) 로그 출력
        // if (movement.sqrMagnitude > 0.01f)
        // {
        //      Debug.Log($"Movement Input: ({movement.x}, {movement.y})");
        // }
        // --- 로그 끝 ---
    }

    // 고정된 시간 간격으로 호출됨 (주로 물리 관련 처리)
    [System.Obsolete]
    void FixedUpdate()
    {
        // Rigidbody가 없는 경우를 대비한 안전 체크
        if (rb == null) return;

        // 이동 방향 벡터 정규화 (대각선 이동 시 속도가 빨라지는 것 방지)
        // .normalized 는 벡터의 크기를 1로 만듭니다 (방향 정보만 남김).
        Vector2 targetVelocity = movement.normalized * moveSpeed;

        // Rigidbody의 속도(velocity)를 직접 설정하여 이동시킴
        rb.velocity = targetVelocity;

        // --- 디버깅 로그: 설정된 속도 및 실제 속도 확인 (활성화!) ---
        // 움직임이 있을 때만 (목표 속도가 0이 아닐 때) 로그를 출력하여 콘솔 오염 방지
        if (targetVelocity.sqrMagnitude > 0.01f)
        {
            // 소수점 두 자리까지만 표시하여 가독성 높임 (선택 사항)
            string targetVelStr = $"({targetVelocity.x:F2}, {targetVelocity.y:F2})";
            string currentVelStr = $"({rb.velocity.x:F2}, {rb.velocity.y:F2})";
            // 설정하려는 속도와, 설정 직후 Rigidbody의 실제 속도를 로그로 출력
           // Debug.Log($"Setting Velocity: {targetVelStr} | Current Velocity After Set: {currentVelStr}", gameObject);
        }
        // (선택 사항) 입력이 없을 때 즉시 멈추게 하려면 아래 주석 해제
        // else if (rb.velocity.sqrMagnitude > 0.01f) // 입력은 없는데 속도가 남아있다면
        // {
        //    rb.velocity = Vector2.zero; // 속도를 0으로 만들어 즉시 멈춤
        // }
        // --- 로그 끝 ---
    }
}