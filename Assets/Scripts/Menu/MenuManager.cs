using System.Collections.Generic;
using UnityEngine;

namespace Brawlley
{
    public class MenuManager : MonoBehaviour
    {
        #region Data
        [Header("Menu Data")]
        Stack<MenuPanel> menuStack = new();

        [Header("Menu Panels")]
        [SerializeField] private GameObject gameModePanel;
        [SerializeField] private GameObject selectMapPanel;

        private int selectedMode = -1;
        private LoadScene loadScene;
        #endregion

        #region Unity Methods
        void Start()
        {
            gameModePanel.SetActive(true);
            selectMapPanel.SetActive(false);
            loadScene = FindObjectOfType<LoadScene>(); // Gets the reference to LoadScene
        }
        #endregion

        #region Menu Methods
        public void SubscribeMenuPanel(MenuPanel menuPanel) => menuStack.Push(menuPanel);
        public void UnsubscribeMenuPanel() => menuStack.Pop();
        public void CloseAllMenuPanels()
        {
            while (menuStack.Count > 0)
            {
                MenuPanel menuPanel = menuStack.Pop();
                menuPanel.Close();
            }
        }
        public void QuitGame() => Application.Quit();
        #endregion

        #region Game Mode Selection
        public void SelectMode(int mode)
        {
            selectedMode = mode;
            gameModePanel.SetActive(false);
            selectMapPanel.SetActive(true);
        }

        public void SelectMap(int sceneIndex)
        {
            if (selectedMode != -1 && loadScene != null)
            {
                loadScene.LoadSceneByIndex(sceneIndex);
            }
        }
        #endregion
    }
}
