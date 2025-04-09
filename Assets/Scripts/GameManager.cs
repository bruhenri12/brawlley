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
    public Map map = Map.Arena1;

    [Header("Volley Mode Resources")]
    [SerializeField] TMP_Text volleyScoreText;
    [SerializeField] int maxScore = 25;
    [SerializeField] int maxDifferencePoints = 2;
    [SerializeField] int maxSets = 3;
    [SerializeField] int currentSet = 0;

    [Header("Player Resources")]
    [SerializeField] List<GameObject> playerPrefabs = new();
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
    [SerializeField] List<SpawnPoints> spawnPointsComponents = new();
    [SerializeField] List<Transform> spawnPoints;

    [Header("Timer Resources")]
    [SerializeField] TMP_Text timerText;
    [SerializeField, Tooltip("Time in seconds")] float time = 60f;

    [Header("Preparation Resources")]
    public float preparationTime = 15;
    public bool isPreparing = true;

    [Header("Game Over Resources")]
    [SerializeField] GameOverScreen gameOver;

    #region Mono Behaviour Life Cycle Methods
    void Awake()
    {
        GetGame();
        DefineMap();
        DefineTeams();
        DefineTeamsUI();
        timerText.gameObject.SetActive(true);
        if (isPreparing)
        {
            StartCoroutine(PreparationCoroutine());
        }
        else
        {
            DefineMode();
        }
    }
    #endregion

    #region Initialization Methods
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

        List<Transform> spawnPointsHorizontallyOrdered = spawnPoints.OrderBy(spawnPoint => spawnPoint.position.x).ToList();

        List<List<Transform>> teamSpawnPoints = new();
        int pointsPerTeam = spawnPointsHorizontallyOrdered.Count / teamCount;

        for (int i = 0; i < teamCount; i++)
        {
            List<Transform> teamPoints = spawnPointsHorizontallyOrdered
                .Skip(i * pointsPerTeam)
                .Take(pointsPerTeam)
                .ToList();
            teamSpawnPoints.Add(teamPoints);
        }

        playerPrefabs = playerPrefabs.OrderBy(_ => UnityEngine.Random.value).ToList();

        for (int i = 0; i < playerCount; i++)
        {
            Transform spawnPoint = teamSpawnPoints[i % teamCount][i / teamCount];
            GameObject playerPrefab = playerPrefabs[i % playerPrefabs.Count];
            playerPrefab.transform.position = spawnPoint.position;
            
            var player = PlayerInput.Instantiate(playerPrefab, controlScheme: controlSchemes[i], pairWithDevice: Keyboard.current, playerIndex: i);
            
            Player playerObject = player.gameObject.GetComponent<Player>();
            playerObject.playerName = $"Player {i}";
            playerObject.name = $"Player {i}";
            playerObject.spawnPoint = spawnPoint;

            players.Add(playerObject);

            Team team = teams[i % teamCount];
            team.players.Add(playerObject);
            team.playersAlive++;
            playerObject.Team = team;
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
        maps[mapIndex].SetActive(true);
        spawnPointsComponents[mapIndex].gameObject.SetActive(true);
        spawnPoints = spawnPointsComponents[mapIndex].GetComponent<SpawnPoints>().spawnPoints;
    }

    public void DefineMode()
    {
        if (map != Map.Volley)
        {
            StartCoroutine(TimerCoroutine());
            return;
        }
        timerText.gameObject.SetActive(false);
        volleyScoreText.gameObject.SetActive(true);

        string scoreText = "Set " + (currentSet + 1) + " scores:";
        foreach (Team team in teams.Take(teamCount))
        {
            scoreText += $"\n<i> {team.name} (Sets {team.volleySets})</i>: <i> {team.volleyScore} </i>";
        }
        volleyScoreText.text = scoreText;
    }
    #endregion

    #region Game Management Methods
    IEnumerator PreparationCoroutine()
    {
        timerText.gameObject.SetActive(true);
        while (preparationTime > 0)
        {
            timerText.text = "Preparation: " + Mathf.Round(preparationTime);
            yield return null;
            preparationTime -= Time.deltaTime;
        }

        timerText.text = "Preparation: 0\nFinished!";
        isPreparing = false;

        foreach (Player player in players)
        {
            player.transform.position = player.spawnPoint.position;
        }

        DefineMode();
    }
    IEnumerator TimerCoroutine()
    {
        while (time > 0)
        {
            timerText.text = "Time: " + Mathf.Round(time);
            yield return null;
            time -= Time.deltaTime;
        }

        timerText.text = "Time: 0\nFinished!";
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

    public Transform GetRandomSpawnPoint()
    {
        int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex];
    }

    public void ScorePoint(string teamName)
    {
        Debug.Log($"Scoring point for team: {teamName}");
        Team team = teams.Take(teamCount).FirstOrDefault(t => t.name == teamName);
        team.volleyScore++;
        CheckSetMatch();
    }

    public void TeamFault(string teamName)
    {
        Debug.Log($"Fault point for team: {teamName}");
        List<Team> otherTeams = teams.Take(teamCount).Where(t => t.name != teamName).ToList();
        foreach (Team team in otherTeams)
        {
            team.volleyScore++;
        }
        CheckSetMatch();
    }

    public void CheckSetMatch()
    {
        
        string scoreText = "Set " + (currentSet + 1) + " scores:";
        foreach (Team team in teams.Take(teamCount))
        {
            scoreText += $"\n<i> {team.name} (Sets {team.volleySets})</i>: <i> {team.volleyScore} </i>";
        }
        volleyScoreText.text = scoreText;

        int setWinnerTeamIndex = -1;
        for (int i = 0; i < teamCount; i++)
        {
            if (teams[i].volleyScore >= maxScore && 
                teams[i].volleyScore - teams.Take(teamCount).Where(t => t != teams[i]).Max(t => t.volleyScore) >= maxDifferencePoints)
            {
                Debug.Log($"Team {teams[i].name} wins the set!");
                setWinnerTeamIndex = i;
                break;
            }
        }

        if (setWinnerTeamIndex == -1) return; // No winner yet

        teams[setWinnerTeamIndex].volleySets++;

        currentSet++;
        if (currentSet >= maxSets)
        {
            Team winner = teams.OrderByDescending(t => t.volleySets).FirstOrDefault();
            if (winner != null && teams.Count(t => t.volleySets == winner.volleySets) == 1)
            {
                Debug.Log($"Team {winner.name} wins the match!");
                HandleVolleyResults(winner);
            }
        }
        else
        {
            foreach (Team team in teams.Take(teamCount))
            {
                team.volleyScore = 0;
            }
            Debug.Log($"Set {currentSet + 1} completed. Next set starting...");

            scoreText = "Set " + (currentSet + 1) + " scores:";
            foreach (Team team in teams.Take(teamCount))
            {
                scoreText += $"\n<i> {team.name} (Sets {team.volleySets})</i>: <i> {team.volleyScore} </i>";
            }
            volleyScoreText.text = scoreText;
        }
    }
    #endregion

    #region Game Over Methods
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

            foreach (Team team in teams.Take(teamCount))
            {
                message += $"\n<i> {team.name} </i>";
            }
        }
        gameOver.Setup(message);
    }

    public void HandleVolleyResults(Team team)
    {
        Debug.Log("Game Over!");
        string message = $"The winner is <i> {team.name} </i>! They won <i> {team.volleySets} </i> sets!";
        message += "\nFinal Scores:";
        foreach (Team t in teams.Take(teamCount))
        {
            message += $"\n<i> {t.name} </i>: <i> {t.volleyScore} </i>";
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
    #endregion

    #region Player Management Methods
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
