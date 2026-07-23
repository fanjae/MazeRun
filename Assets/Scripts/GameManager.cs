using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("게임 오브젝트")]
    [SerializeField] private PlayerController player;
    [SerializeField] private MonsterController monster;
    [SerializeField] private ExitPoint exitPoint;

    [Header("게임 결과 UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    private bool isGameEnded;

    private void Start()
    {
        isGameEnded = false;

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (player != null)
        {
            player.OnPlayerMoved += HandlePlayerMoved;
        }
        else
        {
            Debug.LogError("GameManager에 Player가 연결되지 않았습니다.");
        }

        if (monster != null)
        {
            monster.OnMonsterMoved += HandleMonsterMoved;
        }
        else
        {
            Debug.LogError("GameManager에 Monster가 연결되지 않았습니다.");
        }

        CheckGameState();
    }

    // GameManager 제거시 이벤트 해제
    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnPlayerMoved -= HandlePlayerMoved;
        }

        if (monster != null)
        {
            monster.OnMonsterMoved -= HandleMonsterMoved;
        }
    }

    // 플레이거 한 칸 이동 시 호출
    private void HandlePlayerMoved(AStarNode playerNode)
    {
        if (isGameEnded)
        {
            return;
        }

        CheckGameState();
    }

    // 몬스터 한 칸 이동 시 호출
    private void HandleMonsterMoved(AStarNode monsterNode)
    {
        if (isGameEnded)
        {
            return;
        }

        CheckGameState();
    }

    // 현재 플레이어와 몬스터 위치 확인.
    // 게임 오버 또는 게임 클리어 여부 판정
    private void CheckGameState()
    {
        // 이미 게임이 끝난 경우 중복 판정 막음
        if (isGameEnded)
        {
            return;
        }

        if (player == null || player.CurrentNode == null)
        {
            return;
        }

        // 몬스터와 플레이어가 같은 칸이면 게임 오버
        if (monster != null && monster.CurrentNode != null && monster.CurrentNode == player.CurrentNode)
        {
            GameOver();
            return;
        }

        // 플레이어가 출구에 도착하면 게임 클리어
        if (exitPoint != null && exitPoint.IsPlayerOnExit(player.CurrentNode))
        {
            GameClear();
        }
    }

    // 게임 클리어 상태로 전환
    private void GameClear()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        StopAllMovement();
        ShowResult("GAME CLEAR!");

        Debug.Log("게임 클리어");
    }

    // 게임 오버 상태로 전환
    private void GameOver()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        StopAllMovement();
        ShowResult("GAME OVER");

        Debug.Log("게임 오버");
    }

    // 플레이어 및 몬스터 이동 중지
    private void StopAllMovement()
    {
        if (player != null)
        {
            player.SetCanMove(false);
        }

        if (monster != null)
        {
            monster.SetCanMove(false);
        }
    }

    private void ShowResult(string message)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultText != null)
        {
            resultText.text = message;
        }
    }

    // 현재 씬을 다시 불러와 게임 상태 초기화
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}