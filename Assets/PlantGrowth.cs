using System.Collections; // 코루틴 사용을 위해 필요
using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    public float growthTime = 5.0f; // 성장에 걸리는 시간 (초 단위) - Inspector에서 수정 가능
    public GameObject maturePlantPrefab; // 다 자란 식물 프리팹을 연결할 변수 - Inspector에서 연결

    // 이 스크립트가 붙은 게임 오브젝트(씨앗)가 활성화될 때 자동으로 호출되는 함수
    void Start()
    {
        // 성장을 시작하는 코루틴 호출
        StartCoroutine(Grow());
    }

    // 성장 과정을 처리하는 코루틴 함수
    IEnumerator Grow()
    {
        // growthTime 만큼 기다림
        yield return new WaitForSeconds(growthTime);

        // 다 자란 식물 생성
        // Instantiate(무엇을, 어디에, 어떤 회전값으로);
        // maturePlantPrefab을 현재 씨앗의 위치(transform.position)와 회전값(transform.rotation)으로 생성
        Instantiate(maturePlantPrefab, transform.position, transform.rotation);

        // 현재 게임 오브젝트(씨앗)를 제거
        Destroy(gameObject);
    }
}   