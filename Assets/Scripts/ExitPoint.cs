using UnityEngine;

public class ExitPoint : MonoBehaviour
{
    [Header("격자 관리자")]
    [SerializeField] private GridManager gridManager;

    [Header("출구 좌표")]
    [SerializeField]
    private Vector2Int exitPosition = new Vector2Int(10, 1);

    // 출구가 위치한 실제 격자 노드
    public AStarNode CurrentNode { get; private set; }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // GridManager에서 출구 좌표에 해당하는 노드 가져옴
        CurrentNode = gridManager.GetNode(exitPosition.x,exitPosition.y);

        if (CurrentNode == null)
        {
            Debug.LogError($"출구 좌표가 격자 밖입니다. 좌표: {exitPosition}");
            return;
        }

        if (!CurrentNode.IsWalkable)
        {
            Debug.LogError($"출구 좌표가 장애물입니다. 좌표: {exitPosition}");
            return;
        }

        transform.position = gridManager.GetWorldPosition(CurrentNode);
    }

    public bool IsPlayerOnExit(AStarNode playerNode)
    {
        return CurrentNode == playerNode;
    }
}