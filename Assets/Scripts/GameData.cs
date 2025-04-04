using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Variable/GameData", order = 0)]
public class GameData : ScriptableObject
{
    public GameMode gameMode;
    public Map map;
}
