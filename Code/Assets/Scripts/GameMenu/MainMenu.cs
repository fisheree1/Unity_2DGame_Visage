using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Logic to start the game
        Debug.Log("Game Started");
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        // Logic to quit the game
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
