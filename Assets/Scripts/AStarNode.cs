using System;
using UnityEngine;

[Serializable]
public class AStarNode
{
    // 격자 좌표
    public int X { get; private set; }
    public int Y { get; private set; }

    // 이동 가능한 칸인지 여부
    public bool IsWalkable { get; set; }

    // 시작점과 목적지 표시용
    public bool IsStart { get; set; }
    public bool IsGoal { get; set; }

    // A* 비용
    // G : 시작 노드부터 현재 노드 까지 비용
    // H : 목표 노드까지 예상 비용
    public int GCost { get; set; }
    public int HCost { get; set; }

    public int FCost
    {
        get
        {
            if (GCost == int.MaxValue)
            {
                return int.MaxValue;
            }

            return GCost + HCost;
        }
    }

    // 경로를 역추적하기 위한 이전 노드
    public AStarNode Parent { get; set; }

    // 해당 노드의 화면 오브젝트
    public NodeView View { get; private set; }

    public AStarNode(int x, int y, NodeView view)
    {
        X = x;
        Y = y;
        View = view;

        IsWalkable = true;
        IsStart = false;
        IsGoal = false;

        ResetSearchData();
    }

    public void ResetSearchData()
    {
        GCost = int.MaxValue;
        HCost = 0;
        Parent = null;
    }
}