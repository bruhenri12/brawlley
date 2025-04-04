using TMPro;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    public TMP_Text teamText;
    public void Setup(string text)
    {
        gameObject.SetActive(true);
        teamText.text = text;
    }
}
