using System.Collections;
using UnityEngine;

public class TimedDoor : MonoBehaviour
{
    [Header("격자 관리자")]
    [SerializeField] private GridManager gridManager;

    [Header("문이 위치한 격자 좌표")]
    [SerializeField] private Vector2Int doorPosition = new Vector2Int(4, 3);

    [Header("문 상태 변경 간격")]
    [SerializeField] private float toggleInterval = 2.0f;

    [Header("게임 시작 시 문 상태")]
    [SerializeField] private bool startClosed = true;

    [Header("캐릭터")]
    [SerializeField] private PlayerController player;
    [SerializeField] private MonsterController monster;

    // 문 위치 노드
    private AStarNode doorNode;
    private bool isClosed;

    private void Start()
    {
        Initialize();
    }

    // 문 초기 상태 설정
    private void Initialize()
    {
        if (gridManager == null)
        {
            Debug.LogError("TimedDoor에 GridManager가 연결되지 않았습니다.");
            return;
        }

        // 플레이어와 몬스터를 찾아둠
        player = FindFirstObjectByType<PlayerController>();
        monster = FindFirstObjectByType<MonsterController>();

        // 문이 설치될 격자 가져옴
        doorNode = gridManager.GetNode(doorPosition.x, doorPosition.y);

        if (doorNode == null)
        {
            Debug.LogError($"문 좌표가 격자 밖입니다. 좌표: {doorPosition}");
            return;
        }

        isClosed = startClosed;
        ApplyDoorState();

        // 일정 시간마다 문 상태 변경
        StartCoroutine(ToggleDoorRoutine());
    }

    // 문 여닫는 코루틴
    private IEnumerator ToggleDoorRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(toggleInterval);

            isClosed = !isClosed;
            ApplyDoorState();
        }
    }

    // 캐릭터가 있는 경우 문 못닫게 처리
    private bool CanCloseDoor()
    {
        if (doorNode == null)
        {
            return false;
        }

        if (player != null && player.CurrentNode == doorNode)
        {
            return false;
        }

        if (monster != null && monster.CurrentNode == doorNode)
        {
            return false;
        }

        return true;
    }

    // 문 상태 격자 반영
    // 닫힌 상태면 장애물, 열린 상태면 이동 가능하게 변경
    private void ApplyDoorState()
    {
        if (isClosed)
        {
            if (!CanCloseDoor())
            {
                isClosed = false;
                Debug.Log("캐릭터가 문 위치에 있어 문을 닫지 않습니다.");
                return;
            }

            gridManager.SetObstacle(doorPosition.x, doorPosition.y);
            Debug.Log($"문 닫힘: {doorPosition}");
        }
        else
        {
            gridManager.RemoveObstacle(doorPosition.x, doorPosition.y);
            Debug.Log($"문 열림: {doorPosition}");
        }
    }
}