using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void StartNewInfiniteGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("Main");
    }

    public void StartNewCampaignGame()
    {
        // Load the progression game scene
        SceneManager.LoadScene("Guided");
    }

    public void BackToMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
