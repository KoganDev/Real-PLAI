using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerWinningScreenControl : MonoBehaviour
{

    // ------------------------ Scene Names ---------------------------

    private const string mainMenuSceneName = "MainMenu";
    private const string levelSceneName = "BasicLevel";


    /// <summary>
    /// The function loads the level again. The function is called when the RestartLevelButton is pressed.
    /// </summary>
    public void ClickedTryLevelAgain()
    {
        SceneManager.LoadScene(levelSceneName);
    }

    /// <summary>
    /// The function loads the Main Menu scene. The function is called when the BackToMenuButton is pressed.
    /// </summary>
    public void ClickedBackToMenu()
    {
        // Load the main menu again
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
