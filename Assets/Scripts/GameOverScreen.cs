using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public TMP_Text teamText;
    public void Setup(string text)
    {
        gameObject.SetActive(true);
        teamText.text = text;
    }

    public void RestartButton()
    {
        SceneManager.LoadScene("GameOverScreen");
    }

    public void ExitButton()
    {
        SceneManager.LoadScene("Menu");
    }
}
