using TMPro;
using UnityEngine;

public class NodeView : MonoBehaviour
{
    [Header("노드 색상")]
    [SerializeField] private Color walkableColor = Color.white;
    [SerializeField] private Color obstacleColor = Color.black;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color goalColor = Color.red;

    [Header("컴포넌트")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TMP_Text costText;

    private AStarNode node;

    // NodeView가 표시할 AstarNode 연결
    // 현재 노드 상태를 화면에 반영
    public void Initialize(AStarNode newNode)
    {
        node = newNode;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateView();
    }

    // 노드의 현재 상태 기준으로 색상과 비용 텍스트를 모두 갱신
    public void UpdateView()
    {
        if (node == null)
        {
            return;
        }

        UpdateColor();
        UpdateCostText();
    }

    // 노드 상태에 따라 SpriteRender 색상 변경
    private void UpdateColor()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (!node.IsWalkable)
        {
            spriteRenderer.color = obstacleColor;
            return;
        }

        if (node.IsStart)
        {
            spriteRenderer.color = startColor;
            return;
        }

        if (node.IsGoal)
        {
            spriteRenderer.color = goalColor;
            return;
        }

        spriteRenderer.color = walkableColor;
    }

    // A*에서 탐색에서 계산된 F, G, H 비용 화면에 표시
    private void UpdateCostText()
    {
        if (costText == null)
        {
            return;
        }

        if (node.GCost == int.MaxValue)
        {
            costText.text = string.Empty;
            return;
        }

        costText.text = $"F:{node.FCost}\n" + $"G:{node.GCost} H:{node.HCost}";
    }

    // A* 비용 텍스트의 표시 여부 결정
    public void SetCostTextVisible(bool visible)
    {
        if (costText != null)
        {
            costText.gameObject.SetActive(visible);
        }
    }
}