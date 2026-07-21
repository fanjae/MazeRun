using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("격자 관리자")]
    [SerializeField] private GridManager gridManager;

    [Header("A* 경로 탐색기")]
    [SerializeField] private AStarPathFinder pathFinder;

    [Header("플레이어")]
    [SerializeField] private PlayerController player;

    [Header("몬스터 시작 좌표")]
    [SerializeField] private Vector2Int startPosition = new Vector2Int(10, 6);

    public AStarNode CurrentNode { get; private set; }

    // 몬스터가 한 칸 이동 했을 때 발생하는 이벤트
    public event Action<AStarNode> OnMonsterMoved;

    // 현재 계산된 A* 경로
    private List<AStarNode> currentPath;

    private bool canMove = true;

    private void Start()
    {
        Initialize();

        // 플레이어가 새로운 칸으로 이동시 경로를 재계산
        if (player != null)
        {
            player.OnPlayerMoved += HandlePlayerMoved;
        }

        // 장애물 상태가 변경되면 현재 경로를 다시 계산
        if (gridManager != null)
        {
            gridManager.OnObstacleChanged += HandleObstacleChanged;
        }
    }

    // 오브젝트 제거 시 이벤트 해제
    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnPlayerMoved -= HandlePlayerMoved;
        }

        if (gridManager != null)
        {
            gridManager.OnObstacleChanged -= HandleObstacleChanged;
        }
    }

    private void Initialize()
    {
        if (gridManager == null)
        {
            Debug.LogError("MonsterController에 GridManager가 연결되지 않았습니다.");

            return;
        }

        CurrentNode = gridManager.GetNode(startPosition.x,startPosition.y);

        if (CurrentNode == null)
        {
            Debug.LogError($"몬스터 시작 좌표가 격자 밖입니다. 좌표: {startPosition}"
            );

            return;
        }

        if (!CurrentNode.IsWalkable)
        {
            Debug.LogError($"몬스터 시작 좌표가 장애물입니다. 좌표: {startPosition}"
            );

            return;
        }

        // 몬스터 오브젝트를 시작 노드 위치에 배치
        UpdateWorldPosition();

        if (player != null && player.CurrentNode != null)
        {
            RecalculatePath();
        }
    }

    private void HandlePlayerMoved(AStarNode playerNode)
    {
        if (!canMove)
        {
            return;
        }

        if (CurrentNode == null || playerNode == null)
        {
            return;
        }

        // 플레이어의 새로운 위치를 기준으로 경로 재계산 및, 계산된 경로의 첫번째 노드로 한 칸 이동
        RecalculatePath();
        MoveOneCell();
    }

    private void HandleObstacleChanged(AStarNode changedNode)
    {
        if (!canMove)
        {
            return;
        }

        if (CurrentNode == null || player == null || player.CurrentNode == null)
        {
            return;
        }

        RecalculatePath();
    }

    // 현재 몬스터 위치에서 플레이어 위치까지 A* 알고리즘으로 경로 재계산
    private void RecalculatePath()
    {
        if (pathFinder == null || player == null || CurrentNode == null || player.CurrentNode == null)
        {
            currentPath = null;
            return;
        }

        currentPath = pathFinder.FindPath(CurrentNode,player.CurrentNode);
    }

    // 계산된 최종 경로의 첫 번째 노드로 한 칸 이동
    // 경로가 유효하지 않으면 이동 전에 다시 계산
    private void MoveOneCell()
    {
        if (player == null || player.CurrentNode == null)
        {
            return;
        }

        // 현재 경로가 없거나 막히면 재계산
        if (!IsCurrentPathUsable())
        {
            RecalculatePath();
        }

        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        AStarNode nextNode = currentPath[0];

        // 다음 노드가 없거나 장애물인 경우 경로 재계산
        if (nextNode == null || !nextNode.IsWalkable)
        {
            RecalculatePath();

            if (currentPath == null || currentPath.Count == 0)
            {
                return;
            }

            nextNode = currentPath[0];

            if (nextNode == null || !nextNode.IsWalkable)
            {
                return;
            }
        }

        CurrentNode = nextNode;
        currentPath.RemoveAt(0);

        UpdateWorldPosition();

        // 몬스터 이동에 대해 GameManager에 알림
        OnMonsterMoved?.Invoke(CurrentNode);
    }

    // 현재 저장된 경로를 계속 사용 가능한지 체크
    private bool IsCurrentPathUsable()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < currentPath.Count; i++)
        {
            AStarNode pathNode = currentPath[i];

            if (pathNode == null || !pathNode.IsWalkable)
            {
                return false;
            }
        }

        AStarNode lastNode = currentPath[currentPath.Count - 1];

        if (lastNode != player.CurrentNode)
        {
            return false;
        }

        return true;
    }

    // 몬스터 오브젝트를 현재 노드의 월드 좌표로 이동
    private void UpdateWorldPosition()
    {
        if (gridManager == null || CurrentNode == null)
        {
            return;
        }

        transform.position = gridManager.GetWorldPosition(CurrentNode);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public bool IsOnSameNode(AStarNode node)
    {
        return CurrentNode == node;
    }
}