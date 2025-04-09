using Brawlley;
using TMPro;
using UnityEngine;

public class PlayerUITag : MonoBehaviour
{
    public Player player;
    public Transform referenceTransform;
    public TMP_Text playerUIText;
    Camera cam;
    public void InitUI()
    {
        cam = Camera.main;
        playerUIText.text = player.playerName;
    }
    void Update()
    {
        playerUIText.rectTransform.position = cam.WorldToScreenPoint(referenceTransform.position);
    }
}
