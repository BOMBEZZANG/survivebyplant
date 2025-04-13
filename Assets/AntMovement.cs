using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2D가 필수임을 명시 (코드 수정)
public class AntMovement : MonoBehaviour
{
    public float speed = 1.0f;
    public int damage = 10;
    private Transform targetHouse;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("AntMovement requires a Rigidbody2D component!", gameObject);
            enabled = false; // Rigidbody 없으면 스크립트 비활성화 (코드 수정)
            return;
        }

        // 중력 영향 안 받도록 설정 및 회전 고정 해제 (코드 수정)
        rb.gravityScale = 0;
        rb.freezeRotation = false; // 회전해야 하므로 false로 설정!

        GameObject houseObject = GameObject.FindGameObjectWithTag("House");
        if (houseObject != null)
        {
            targetHouse = houseObject.transform;
            // Debug.Log("개미: 목표 집 발견!"); // 필요시 주석 해제
        }
        else
        {
            Debug.LogError("개미: 'House' 태그를 가진 집을 찾을 수 없습니다!");
            Destroy(gameObject);
        }
    }

    [System.Obsolete]
    void FixedUpdate()
    {
        if (targetHouse != null && rb != null)
        {
            // 목표 방향 계산 (현재 위치 기준)
            Vector2 direction = ((Vector2)targetHouse.position - rb.position).normalized;

            // 이동 방향으로 개미 회전 (추가된 부분)
            // 이동 방향 벡터가 0이 아닐 때만 (즉, 움직일 때만) 회전 적용
            if (direction.sqrMagnitude > 0.01f) // magnitude 비교보다 sqrMagnitude 비교가 성능상 유리
            {
                // --- 아래 두 줄 중 사용하는 개미 스프라이트의 기본 방향에 맞는 것 하나만 주석 해제 ---

                // 1. 만약 개미 스프라이트가 기본적으로 오른쪽을 보고 있다면:
                transform.right = direction;

                // 2. 만약 개미 스프라이트가 기본적으로 위쪽을 보고 있다면:
                 //transform.up = direction; // (일반적인 TopDown 방식에서는 위쪽을 기준으로 할 때가 많음)

                // 3. 각도를 직접 계산하여 적용하는 더 정밀한 방법 (위 방법이 어색할 경우 사용):
                // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // transform.rotation = Quaternion.Euler(0, 0, angle - 90); // 스프라이트가 위쪽 기준일 때 -90도 필요할 수 있음
            }

            // Rigidbody의 속도를 설정하여 이동
            rb.velocity = direction * speed; // linearVelocity 대신 velocity 사용 (동일하게 작동)
        }
    }

    // OnCollisionEnter2D 및 ArrivedAtHouse 함수는 이전과 동일하게 유지
    [System.Obsolete]
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("House"))
        {
            // Debug.Log("Ant: OnCollisionEnter2D with House!", gameObject);
            ArrivedAtHouse();
        }
    }

    [System.Obsolete]
    void ArrivedAtHouse()
    {
        // Debug.Log("Ant: ArrivedAtHouse() CALLED!", gameObject);
        HouseHealth houseHealth = targetHouse.GetComponent<HouseHealth>();
        if (houseHealth != null)
        {
            houseHealth.TakeDamage(damage);
        }

        if(rb != null)
        {
            rb.velocity = Vector2.zero; // velocity 사용 (코드 수정)
        }

        Destroy(gameObject);
    }
}