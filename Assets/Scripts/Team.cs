using System.Collections.Generic;
using Brawlley;
using UnityEngine;

[System.Serializable]
public class Team
{
    public string name;
    public List<Player> players = new();
    public int playersAlive = 0;
    public int volleyScore = 0;
    public int volleySets = 0;
}
