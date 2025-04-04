using System.Collections.Generic;
using Brawlley;
using UnityEngine;

[System.Serializable]
public class Team
{
    public string name;
    public List<Player> players = new();
    public int playersAlive = 0;
}
