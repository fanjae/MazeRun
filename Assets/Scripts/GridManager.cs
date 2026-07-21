using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("노드 프리팹")]
    [SerializeField] private NodeView nodePrefab;

    [Header("가로 노드 개수")]
    [SerializeField] private int width = 12;

    [Header("세로 노드 개수")]
    [SerializeField] private int height = 8;

    [Header("노드 사이 간격")]
    [SerializeField] private float cellSpacing = 1.0f;

    // X, Y 좌표를 이용해 노드에 빠르게 접근하기 위한 2차원 배열
    private AStarNode[,] nodes;

    // 모든 노드를 순차적으로 관리할 목적의 리스트
    private readonly List<AStarNode> allNodes = new List<AStarNode>();

    private bool isInitializing;

    // 장애물 상태 변경시 MonsterController에서 경로 재계산을 위한 이벤트
    public event Action<AStarNode> OnObstacleChanged;

    public int Width
    {
        get { return width; }
    }

    public int Height
    {
        get { return height; }
    }

    private void Awake()
    {
        isInitializing = true;

        CreateGrid();
        SetupDefaultMap();

        isInitializing = false;
    }

    // 설정된 가로·세로 크기에 맞춰 노드 프리팹 생성
    private void CreateGrid()
    {
        if (nodePrefab == null)
        {
            Debug.LogError("GridManager의 Node Prefab이 연결되지 않았습니다.");
            return;
        }

        nodes = new AStarNode[width, height];
        allNodes.Clear();

        float totalWidth = (width - 1) * cellSpacing;
        float totalHeight = (height - 1) * cellSpacing;

        float startX = -totalWidth * 0.5f;
        float startY = -totalHeight * 0.5f;

        // 모든 X, Y 좌표를 순회하며 노드 생성
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 nodePosition = transform.position + new Vector3(startX + x * cellSpacing,startY + y * cellSpacing,0.0f);

                // 노드 프리팹을 GridMAnager의 자식으로 생성
                NodeView nodeView = Instantiate(nodePrefab,nodePosition,Quaternion.identity,transform);

                nodeView.name = $"Node_{x}_{y}";

                // 현재 좌표와 NodeView를 가진, AStarNode 생성
                AStarNode node = new AStarNode(x,y,nodeView);

                nodes[x, y] = node;
                allNodes.Add(node);

                nodeView.Initialize(node);
            }
        }
    }

    private void SetupDefaultMap()
    {
        SetObstacle(2, 1);
        SetObstacle(2, 2);
        SetObstacle(2, 3);
        SetObstacle(2, 4);

        SetObstacle(5, 3);
        SetObstacle(6, 3);
        SetObstacle(7, 3);

        SetObstacle(8, 5);
        SetObstacle(9, 5);
    }

    public void SetObstacle(int x, int y)
    {
        SetWalkable(x, y, false);
    }

    public void RemoveObstacle(int x, int y)
    {
        SetWalkable(x, y, true);
    }

    // 지정한 좌표의 이동 가능 여부를 변경
    // 실제 상태가 변경시 화면을 갱신하고 이벤트를 발생
    public void SetWalkable(int x,int y,bool isWalkable)
    {
        AStarNode node = GetNode(x, y);

        if (node == null)
        {
            return;
        }

        // 이미 같은 상태라면 아무 작업도 하지 않음
        if (node.IsWalkable == isWalkable)
        {
            return;
        }

        node.IsWalkable = isWalkable;

        if (node.View != null)
        {
            node.View.UpdateView();
        }

        // 맵 최초 생성 중에는 이벤트를 발생시키지 않음
        if (!isInitializing)
        {
            OnObstacleChanged?.Invoke(node);
        }
    }

    // X, Y 격자 좌표에 해당하는 노드 반환
    public AStarNode GetNode(int x, int y)
    {
        if (nodes == null)
        {
            return null;
        }

        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return null;
        }

        return nodes[x, y];
    }

    // 지정한 좌표로 이동할 수 있는지 판단
    public bool CanMoveTo(int x, int y)
    {
        AStarNode node = GetNode(x, y);

        if (node == null)
        {
            return false;
        }

        return node.IsWalkable;
    }

    // 전달 받은 노드가 화면에 배치된 실제 월드 좌표 반환
    public Vector3 GetWorldPosition(AStarNode node)
    {
        if (node == null || node.View == null)
        {
            return Vector3.zero;
        }

        return node.View.transform.position;
    }

    // X, Y 격자 좌표에 해당하는 월드 좌표를 반환
    public Vector3 GetWorldPosition(int x,int y)
    {
        AStarNode node = GetNode(x, y);

        return GetWorldPosition(node);
    }

    // 현재 노드의 상하좌우에 위치한 이웃 노드 반환
    // 대각선 방향은 포함하지 않음
    public List<AStarNode> GetNeighbors(AStarNode node)
    {
        List<AStarNode> neighbors = new List<AStarNode>();

        if (node == null)
        {
            return neighbors;
        }

        AddNeighbor(neighbors,node.X + 1,node.Y); // 오른쪽
        AddNeighbor(neighbors,node.X - 1,node.Y); // 왼쪽
        AddNeighbor(neighbors,node.X,node.Y + 1); // 윗쪽
        AddNeighbor(neighbors,node.X,node.Y - 1); // 아래쪽

        return neighbors;
    }

    // 지정한 좌표의 노드가 존재하면, 이웃 노드 리스트에 추가
    private void AddNeighbor(List<AStarNode> neighbors,int x,int y)
    {
        AStarNode neighbor = GetNode(x, y);

        if (neighbor == null)
        {
            return;
        }

        neighbors.Add(neighbor);
    }

    public List<AStarNode> GetAllNodes()
    {
        return allNodes;
    }


    // 모든 노드의 A* 탐색 데이터 초기화
    public void ResetSearchData()
    {
        for (int i = 0;i < allNodes.Count;i++)
        {
            AStarNode node = allNodes[i];
            node.ResetSearchData();

            if (node.View != null)
            {
                node.View.UpdateView();
            }
        }
    }

    // 모든 노드 상태를 완전히 초기화
    public void ResetEntireGrid()
    {
        isInitializing = true;

        for (int i = 0; i < allNodes.Count; i++)
        {
            AStarNode node = allNodes[i];

            node.IsWalkable = true;
            node.IsStart = false;
            node.IsGoal = false;

            node.ResetSearchData();

            if (node.View != null)
            {
                node.View.UpdateView();
            }
        }

        SetupDefaultMap();

        isInitializing = false;

        // 맵 전체 상태가 변경됐음을 알립니다.
        OnObstacleChanged?.Invoke(null);
    }
}