using UnityEngine;

namespace Brawlley
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] GameData gameData;

        #region Menu Methods
        public void SetGameMode(int mode) => gameData.gameMode = (GameMode)mode;
        public void SetMap(int map) => gameData.map = (Map)map;
        public void QuitGame() => Application.Quit();
        #endregion
    }
}
