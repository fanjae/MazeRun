using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AStarPathFinder : MonoBehaviour
{
    [Header("격자 관리자")]
    [SerializeField] private GridManager gridManager;

    [Header("상태 텍스트")]
    [SerializeField] private TMP_Text statusText;

    // 시작 노드부터 목표 노드까지의 경로 탐색
    // 반환되는 리스트에는 시작 노드 제외, 다음 이동 노드부터 목표 노드 까지 저장
    public List<AStarNode> FindPath(AStarNode startNode,AStarNode goalNode)
    {
        if (startNode == null || goalNode == null)
        {
            SetStatus("Start 또는 Goal이 없습니다.");
            return null;
        }

        if (!startNode.IsWalkable || !goalNode.IsWalkable)
        {
            SetStatus("이동할 수 없는 Node입니다.");
            return null;
        }

        // 이전 탐색에 사용한 G, H, Parent 값 초기화
        gridManager.ResetSearchData();

        List<AStarNode> openList = new List<AStarNode>();

        HashSet<AStarNode> closedSet = new HashSet<AStarNode>();

        startNode.GCost = 0;
        startNode.HCost = CalculateDistance(startNode, goalNode);

        openList.Add(startNode);

        // 탐색 가능한 노드가 있을동안 반복
        while (openList.Count > 0)
        {
            // Open List에서 F 비용이 낮은 노드 선택
            AStarNode currentNode = GetLowestCostNode(openList);

            // 현재 노드가 목표 노드라면 최종 경로 생성
            if (currentNode == goalNode)
            {
                List<AStarNode> path = CreatePath(startNode, goalNode);
                SetStatus("경로 탐색 완료");

                return path;
            }

            // 현재 노드에서 Open List 제거, 탐색 완료 목록 Closed Set 추가
            openList.Remove(currentNode);
            closedSet.Add(currentNode);

            // 현재 노드의 상하좌우 이웃 노드를 가져옴.
            List<AStarNode> neighbors = gridManager.GetNeighbors(currentNode);

            for (int i = 0; i < neighbors.Count; i++)
            {
                AStarNode neighbor = neighbors[i];

                // 장애물 노드는 탐색하지 않음
                if (!neighbor.IsWalkable)
                {
                    continue;
                }

                // 이미 탐색한 노드는 다시 검색하지 않음
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newGCost = currentNode.GCost + 10;

                if (newGCost >= neighbor.GCost)
                {
                    continue;
                }

                // 더 짧은 경로를 찾은 경우, 비용과 부모 노드 갱신
                neighbor.GCost = newGCost;
                neighbor.HCost = CalculateDistance(neighbor,goalNode);

                neighbor.Parent = currentNode;

                // 아직 Open List에 없다면 탐색 목록 추가
                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                }
            }
        }

        SetStatus("경로를 찾을 수 없습니다.");

        return null;
    }

    // 두 노드 사이의 휴리스틱 비용 계산
    // 4방향 기준 맨허튼 거리를 적용하여 계산.
    private int CalculateDistance(AStarNode first,AStarNode second)
    {
        int xDistance = Mathf.Abs(first.X - second.X);
        int yDistance = Mathf.Abs(first.Y - second.Y);

        return (xDistance + yDistance) * 10;
    }

    // Open List에서 F 비용이 가장 낮은 노드 반환
    // F 비용이 같으면 H 비용이 더 낮은 노드를 선택
    private AStarNode GetLowestCostNode(List<AStarNode> openList)
    {
        AStarNode bestNode = openList[0];

        for (int i = 1; i < openList.Count; i++)
        {
            AStarNode candidate = openList[i];

            bool lowerFCost = candidate.FCost < bestNode.FCost;
            bool sameFCostLowerH = candidate.FCost == bestNode.FCost && candidate.HCost < bestNode.HCost;

            if (lowerFCost || sameFCostLowerH)
            {
                bestNode = candidate;
            }
        }

        return bestNode;
    }

    // 목표 노드부터 Parent를 따라 시작 노드까지 역추적, 리스트를 뒤집어 실제 이동 순서의 경로를 만듦.
    private List<AStarNode> CreatePath(AStarNode startNode,AStarNode goalNode)
    {
        List<AStarNode> path = new List<AStarNode>();

        AStarNode currentNode = goalNode;

        // 목표 노드에서 시작 노드까지 부모 노드를 따라감
        while (currentNode != startNode)
        {
            if (currentNode == null)
            {
                return null;
            }

            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        // 순서 뒤집기(리스트는 목표에서 시작 방향)
        path.Reverse();

        return path;
    }

    public void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}