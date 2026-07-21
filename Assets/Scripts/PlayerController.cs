using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("격자 관리자")]
    [SerializeField] private GridManager gridManager;

    [Header("시작 격자 좌표")]
    [SerializeField]
    private Vector2Int startPosition = new Vector2Int(1, 1);

    public AStarNode CurrentNode { get; private set; }

    // 플레이어가 한 칸 이동시 발생하는 이벤트
    public event Action<AStarNode> OnPlayerMoved;
    private bool canMove = true;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!canMove)
        {
            return;
        }

        Vector2Int direction = ReadDirection();

        if (direction == Vector2Int.zero)
        {
            return;
        }

        TryMove(direction);
    }

    // 설정된 시작 좌표에 해당하는 노드를 찾고, 플레이어 오브젝트를 해당 위치에 배치
    private void Initialize()
    {
        CurrentNode = gridManager.GetNode(startPosition.x,startPosition.y);

        if (CurrentNode == null)
        {
            Debug.LogError($"플레이어 시작 좌표가 격자 밖입니다. " + $"좌표: {startPosition}");
            return;
        }

        if (!CurrentNode.IsWalkable)
        {
            Debug.LogError($"플레이어 시작 좌표가 장애물입니다. " + $"좌표: {startPosition}");
            return;
        }

        UpdateWorldPosition();
    }

    // WSAD 또는 방향키를 확인해, 이동 방향을 Vector2Int 형태로 변환
    private Vector2Int ReadDirection()
    {
        if (Keyboard.current == null)
        {
            return Vector2Int.zero;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            return Vector2Int.up;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            return Vector2Int.down;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            return Vector2Int.left;
        }

        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            return Vector2Int.right;
        }

        return Vector2Int.zero;
    }

    // 현재 노드에서 전달받은 방향으로 한 칸 이동 시도
    // 격자 밖이거나 장애물인 경우 이동하지 않음
    private void TryMove(Vector2Int direction)
    {
        if (CurrentNode == null)
        {
            return;
        }

        int nextX = CurrentNode.X + direction.x;
        int nextY = CurrentNode.Y + direction.y;

        // 이동 가능 여부 체크
        if (!gridManager.CanMoveTo(nextX, nextY))
        {
            return;
        }

        AStarNode nextNode = gridManager.GetNode(nextX, nextY);
        CurrentNode = nextNode;

        UpdateWorldPosition();
        OnPlayerMoved?.Invoke(CurrentNode);
    }

    private void UpdateWorldPosition()
    {
        transform.position = gridManager.GetWorldPosition(CurrentNode);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void SetPosition(AStarNode node)
    {
        if (node == null || !node.IsWalkable)
        {
            return;
        }
        CurrentNode = node;
        UpdateWorldPosition();
    }
}