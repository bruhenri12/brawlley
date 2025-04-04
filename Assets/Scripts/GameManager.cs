using System;
using System.Collections.Generic;
using Brawlley;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using System.Linq;
using UnityEngine.XR;
using Unity.Mathematics;

public enum GameMode { v1, v2, FFA }
public enum Map { Arena1, Arena2, Arena3, Volley }
public class GameManager : MonoBehaviour
{
    [Header("Game Resources")]
    [SerializeField] GameData gameData;
    [SerializeField] GameMode gameMode = GameMode.v1;
    [SerializeField] Map map = Map.Arena1;

    [Header("Player Resources")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] List<Player> players = new();
    int playerCount = 0;

    [Header("Control Resources")]
    [SerializeField] List<string> controlSchemes = new();

    [Header("Team Resources")]
    [SerializeField] List<Team> teams = new();
    [SerializeField] UIObjectContainer teamsUIContainer;
    int teamCount = 0;

    [Header("Map Resources")]
    [SerializeField] List<GameObject> maps = new();
    [SerializeField] List<GameObject> spawnPointsObjects = new();

    [Header("Spawn Resources")]
    [SerializeField] List<Transform> spawnPoints;

    [Header("Timer Resources")]
    [SerializeField] TMP_Text timerText;
    [SerializeField, Tooltip("Time in seconds")] float time = 60f;

    [Header("Game Over Resources")]
    [SerializeField] GameOverScreen gameOver;

    #region Methods
    void Awake()
    {
        GetGame();
        DefineMap();
        DefineTeams();
        DefineTeamsUI();
        StartCoroutine(TimerCoroutine());
    }
    public void GetGame()
    {
        gameMode = gameData.gameMode;
        map = gameData.map;
    }
    public void DefineTeams()
    {
        switch (gameMode)
        {
            case GameMode.v1:
                playerCount = 2;
                teamCount = 2;
                break;

            case GameMode.v2:
                playerCount = 4;
                teamCount = 2;
                break;

            case GameMode.FFA:
                playerCount = 4;
                teamCount = 4;
                break;

            default:
                Debug.LogError("Unsupported game mode!");
                break;
        }

        for (int i = 0; i < playerCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
            Transform spawnPoint = spawnPoints[randomIndex];
            playerPrefab.transform.position = spawnPoint.position;
            
            var player = PlayerInput.Instantiate(playerPrefab, controlScheme: controlSchemes[i], pairWithDevice: Keyboard.current, playerIndex: i);
            
            Player playerObject = player.gameObject.GetComponent<Player>();
            playerObject.playerName = $"Player {i}";
            playerObject.name = $"Player {i}";
            playerObject.spawnPoint = spawnPoint;
            spawnPoints.RemoveAt(randomIndex);
            players.Add(playerObject);
            Team team = teams[i % teamCount];
            playerObject.Team = team;
            team.players.Add(playerObject);
            team.playersAlive++;
        }
    }

    public void DefineTeamsUI()
    {
        for (int i = 0; i < teamCount; i++)
        {
            Team team = teams[i];
            GameObject teamUI = teamsUIContainer.AddObject();
            teamUI.name = team.name;
            UIObjectContainer playersUIContainer = teamUI.GetComponent<UIObjectContainer>();
            foreach (Player player in team.players)
            {
                GameObject playerUI = playersUIContainer.AddObject();
                playerUI.name = player.name;
                player.status = playerUI.GetComponent<PlayerStatus>();
                player.status.playerName.text = player.playerName;
                player.status.lives.text = player.GetComponent<PlayerHealth>().Lives.ToString();
                player.status.damageImage.color = Color.green;
            }
        }
    }

    public void DefineMap()
    {
        if (maps == null || maps.Count == 0)
        {
            Debug.LogError("No maps available to load!");
            return;
        }

        int mapIndex = (int)map % maps.Count;
        Instantiate(maps[mapIndex]);
        Instantiate(spawnPointsObjects[mapIndex]);
        spawnPoints = spawnPointsObjects[mapIndex].GetComponent<SpawnPoints>().spawnPoints;
    }

    
    IEnumerator TimerCoroutine()
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

    void HandleTimeout()
    {
        Debug.Log("Time's up!");

        List<Team> winningTeams = new();
        int maxPlayersAlive = -1;

        // Determine the teams with the most players alive
        for (int i = 0; i < teamCount; i++)
        {
            Team team = teams[i];
            int playersAlive = team.playersAlive;
            if (playersAlive > maxPlayersAlive)
            {
                maxPlayersAlive = playersAlive;
                winningTeams.Clear();
                winningTeams.Add(team);
            }
            else if (playersAlive == maxPlayersAlive)
            {
                winningTeams.Add(team);
            }
        }

        // If there's a tie, compare total lives
        if (winningTeams.Count > 1)
        {
            int maxLives = 0;
            List<Team> tiedTeams = new();

            foreach (Team team in winningTeams)
            {
                int totalLives = team.players.Sum(player => player.GetComponent<PlayerHealth>().Lives);
                if (totalLives > maxLives)
                {
                    maxLives = totalLives;
                    tiedTeams.Clear();
                    tiedTeams.Add(team);
                }
                else if (totalLives == maxLives)
                {
                    tiedTeams.Add(team);
                }
            }

            winningTeams = tiedTeams;

            // If there's still a tie, compare total damage
            if (winningTeams.Count > 1)
            {
                float minDamage = float.MaxValue;
                List<Team> finalWinners = new();

                foreach (Team team in winningTeams)
                {
                    float totalDamage = team.players.Sum(player => player.GetComponent<PlayerHealth>().Damage);
                    if (totalDamage < minDamage)
                    {
                        minDamage = totalDamage;
                        finalWinners.Clear();
                        finalWinners.Add(team);
                    }
                    else if (totalDamage == minDamage)
                    {
                        finalWinners.Add(team);
                    }
                }

                winningTeams = finalWinners;
            }
        }

        // Announce the results
        if (winningTeams.Count == 1)
        {
            HandleResults(winningTeams[0]);
        }
        else
        {
            HandleResults(winningTeams);
        }
    }

    void HandleResults(Team team)
    {
        Debug.Log("Game Over!");
        string message = $"The winner is <i> {team.name} </i> with <i> {team.playersAlive} </i> players alive!";
        if (gameMode == GameMode.FFA)
        {
            message = $"The winner is <i> {team.players[0].name} </i>!";
        }
        gameOver.Setup(message);
    }

    void HandleResults(List<Team> teams)
    {
        Debug.Log("Game Over!");
        string message;
        if (gameMode == GameMode.FFA)
        {
            message = "It's a tie between the following players:";
            foreach (Team team in teams)
            {
                message += $"\n<i> {team.players[0].name} </i>";
            }
            gameOver.Setup(message);
            return;
        }
        else
        {
            message = "It's a tie between the following teams: ";
            foreach (Team team in teams)
            {
                message += $"\n<i> {team.name} </i>";
            }
        }
        gameOver.Setup(message);
    }

    Team StandingTeam()
    {
        Team standingTeam = null;
        for (int i = 0; i < teamCount; i++)
        {
            if (teams[i].playersAlive > 0)
            {
                if (standingTeam == null)
                    standingTeam = teams[i];
                else
                    return null; // More than one team is standing
            }
        }
        return standingTeam;
    }

    public void HandlePlayerDamage(Player player, float damage)
    {
        float t = Mathf.Clamp01( damage / (4 * 10f) ); // Assuming max damage is 4 times the damage taken
        player.status.damageImage.color = Color.Lerp(Color.green, Color.red, t);
    }

    public void HandlePlayerDeath(Player player, int lives)
    {
        player.status.lives.text = lives.ToString();
        if (lives <= 0)
        {
            player.gameObject.SetActive(false);
            //player.status.gameObject.SetActive(false);
            player.Team.playersAlive--;
            if (player.Team.playersAlive <= 0)
            {
                player.Team.playersAlive = 0;
                // Handle team elimination
                Debug.Log($"{player.Team.name} has been eliminated!");

                Team standingTeam = StandingTeam();
                if (standingTeam != null)
                {
                    HandleResults(standingTeam);
                }
            }
        }
    }
    #endregion
}
