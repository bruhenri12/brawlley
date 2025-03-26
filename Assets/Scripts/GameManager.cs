using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Brawlley; // Para usar o Image

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> players;
    [SerializeField] private float time = 60;

    // Referências para a UI
    [SerializeField] private TMP_Text timerText; // Para mostrar o tempo restante
    [SerializeField] private List<TMP_Text> playerLivesTexts; // Para mostrar as vidas de cada jogador
    [SerializeField] private Canvas uiCanvas; // Referência ao Canvas
    [SerializeField] private List<GameObject> healthCircles; // Lista para acompanhar os círculos de saúde de cada jogador

    [SerializeField] private GameOverScreen gameOverScreen;

    private Dictionary<string, List<GameObject>> teams;

    private void Start()
    {
        teams = new Dictionary<string, List<GameObject>>
        {
            { "Vermelho", new List<GameObject>() },
            { "Azul", new List<GameObject>() },
            { "Verde", new List<GameObject>() },
            { "Amarelo", new List<GameObject>() }
        };
        foreach (GameObject player in players)
        {
            Player playerComponent = player.GetComponent<Player>();
            teams[playerComponent.Team].Add(player);
        }

        StartCoroutine(TimerCoroutine());
        UpdatePlayerLivesUI();
        UpdateHealthCircleUI();
    }

    private void HandlePlayerElimination(GameObject eliminatedPlayer)
    {
        eliminatedPlayer.SetActive(false);
        players.Remove(eliminatedPlayer);
        UpdatePlayerLivesUI();
        UpdateHealthCircleUI();
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        int activePlayers = players.Count(p => p.activeSelf);
        if (activePlayers == 1)
        {
            HandleGameOver("Vitória do time " + players[0].GetComponent<Player>().Team + "!");
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (time > 0)
        {
            timerText.text = "Time: " + Mathf.Round(time);
            yield return null;
            time -= Time.deltaTime;
        }

        timerText.text = "Time: 0";
        HandleTimeout();
    }

    private void HandleGameOver(string gameOverMessage)
    {
        gameOverScreen.Setup(gameOverMessage);
    }

    private void HandleTimeout()
    {
        int maxLives = 0;
        string winningTeam = "";
        Dictionary<string, int> teamLives = new Dictionary<string, int>();

        foreach (KeyValuePair<string, List<GameObject>> team in teams)
        {
            int totalLives = team.Value.Sum(p => p.GetComponent<PlayerHealth>().Lives);
            teamLives[team.Key] = totalLives;

            if (totalLives > maxLives)
            {
                maxLives = totalLives;
                winningTeam = team.Key;
            }
        }

        var topTeams = teamLives.Where(t => t.Value == maxLives).Select(t => t.Key).ToList();

        if (topTeams.Count > 1)
        {
            HandleGameOver("Empate!");
        }
        else
        {
            HandleGameOver("Vitória do time " + winningTeam + "!");
        }
    }

    public void HandlePlayerDamage(GameObject player, float damage)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        UpdateHealthCircleUI();
    }

    public void HandlePlayerDeath(GameObject player, int lives)
    {
        if (lives <= 0)
        {
            HandlePlayerElimination(player);
        }
        UpdatePlayerLivesUI();
        UpdateHealthCircleUI();
    }
    private void UpdatePlayerLivesUI()
    {
        for (int i = 0; i < playerLivesTexts.Count; i++)
        {
            if (i < players.Count && players[i].activeSelf)
            {
                PlayerHealth playerHealth = players[i].GetComponent<PlayerHealth>();
                if (playerLivesTexts[i] != null) // Check if the text object is not null
                {
                    playerLivesTexts[i].text = playerHealth.Lives.ToString();
                    playerLivesTexts[i].gameObject.SetActive(true);
                }
            }
            else
            {
                if (playerLivesTexts[i] != null) // Check if the text object is not null
                {
                    playerLivesTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateHealthCircleUI()
    {
        for (int i = 0; i < healthCircles.Count; i++)
        {
            if (i < players.Count && players[i].activeSelf)
            {
                PlayerHealth playerHealth = players[i].GetComponent<PlayerHealth>();
                if (healthCircles[i] != null)
                {
                    float t = playerHealth.Damage / (4 * 10f); // Assuming 10 is the damage threshold for full red
                    t = Mathf.Clamp01(t); // Ensure t is between 0 and 1
                    Color color = Color.Lerp(Color.green, Color.red, t);
                    healthCircles[i].GetComponent<Image>().color = color;
                    healthCircles[i].gameObject.SetActive(true);
                }
            }
            else
            {
                if (healthCircles[i] != null)
                {
                    healthCircles[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public Image GetHealthCircleImage(GameObject player)
    {
        int index = players.IndexOf(player);
        if (index >= 0 && index < healthCircles.Count)
        {
            return healthCircles[index].GetComponent<Image>();
        }
        return null;
    }


}
