using UnityEngine;

public class AntMovement : MonoBehaviour
{
    public float speed = 1.0f;
    public int damage = 10;
    private Transform targetHouse;
    private Rigidbody2D rb; // Rigidbody2D 참조 변수 추가

    void Start()
    {
        // Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("AntMovement requires a Rigidbody2D component!", gameObject);
        }
        // Body Type이 Dynamic인지 확인 (권장)
        if (rb != null && rb.bodyType != RigidbodyType2D.Dynamic)
        {
             Debug.LogWarning("Ant's Rigidbody2D Body Type is not Dynamic. Collision events might be less reliable. Setting Gravity Scale to 0.", gameObject);
             rb.gravityScale = 0; // Kinematic이라도 중력은 0으로
        }
        else if (rb != null) {
            rb.gravityScale = 0; // Dynamic일 때도 중력 0으로 확실히 설정
        }


        GameObject houseObject = GameObject.FindGameObjectWithTag("House");
        if (houseObject != null)
        {
            targetHouse = houseObject.transform;
            Debug.Log("개미: 목표 집 발견!");
        }
        else
        {
            Debug.LogError("개미: 'House' 태그를 가진 집을 찾을 수 없습니다!");
            Destroy(gameObject);
        }
    }

    // 물리 관련 로직은 FixedUpdate에서 처리하는 것이 좋음
    void FixedUpdate()
    {
        // 목표(집)가 설정되어 있고 Rigidbody가 있다면 그쪽으로 이동
        if (targetHouse != null && rb != null)
        {
            // 목표 방향 계산
            Vector2 direction = ((Vector2)targetHouse.position - rb.position).normalized;

            // Rigidbody의 속도를 설정하여 이동 (물리 시스템에 더 친화적)
            rb.linearVelocity = direction * speed;

            // (선택 사항) 이동 방향으로 개미 스프라이트 회전
            // transform.right = direction; // 개미 스프라이트가 오른쪽을 보도록 설정되어 있다면
        }
    }

    void Update()
    {
        // Update에서는 더 이상 이동이나 거리 체크를 하지 않음
    }

    // ** 콜라이더(IsTrigger=OFF)가 다른 콜라이더와 처음 충돌했을 때 호출되는 함수 **
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 상대방 게임 오브젝트의 태그가 "House"인지 확인
        if (collision.gameObject.CompareTag("House"))
        {
            Debug.Log("Ant: OnCollisionEnter2D with House!", gameObject); // 충돌 로그 확인
            ArrivedAtHouse(); // 집에 도착했으므로 처리 함수 호출
        }
        // (선택 사항) 식물과 충돌했을 때 멈추게 할 수도 있음
        // else if (collision.gameObject.CompareTag("Plant")) // 식물 태그가 "Plant"라고 가정
        // {
        //     if (rb != null) rb.velocity = Vector2.zero; // 속도를 0으로 만들어 멈춤
        // }
    }

    // ArrivedAtHouse 함수는 거의 그대로 유지 (Rigidbody 속도 0으로 추가)
    void ArrivedAtHouse()
    {
        Debug.Log("Ant: ArrivedAtHouse() CALLED!", gameObject); // 호출 확인 로그
        HouseHealth houseHealth = targetHouse.GetComponent<HouseHealth>();
        if (houseHealth != null)
        {
            houseHealth.TakeDamage(damage);
        }

        // 도착했으므로 더 이상 움직이지 않도록 속도를 0으로 설정
        if(rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Destroy(gameObject); // 개미 오브젝트 제거
    }
}