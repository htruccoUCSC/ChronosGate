using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void StartNewGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("Main");
    }

    public void BackToMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
