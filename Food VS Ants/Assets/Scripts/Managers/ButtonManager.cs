using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void MainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void GameOverScene()
    {
        SceneManager.LoadScene("GameOverScene");
    }

    public void WinScene()
    {
        SceneManager.LoadScene("WinScene");
    }

    public void MenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
