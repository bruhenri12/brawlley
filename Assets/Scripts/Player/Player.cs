using UnityEngine;
using UnityEngine.InputSystem;

namespace Brawlley
{
    public class Player : MonoBehaviour
    {
        #region Player Resources
        [Header("Player Resources")]
        public string playerName = "Player";
        public PlayerController playerController;
        #endregion

        #region Player Data
        [Header("Player Data")]
        [SerializeField] Team team;
        public Team Team { get => team; set => team = value; }
        public Transform spawnPoint;
        public PlayerStatus status;

        //public GameInputs GameInputs => playerController.GameInputs;
        #endregion
    }
}
