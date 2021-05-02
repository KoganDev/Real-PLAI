using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControl : MonoBehaviour
{

    // ------------------------ Scene Names ---------------------------

    private string createLevelSceneName = "CreateLevel";
    private string selectLevelSceneName = "SelectLevel";

    /// <summary>
    /// The function initalizes some general settings abount the game.
    /// </summary>
    private void Start()
    {
        // Have a fixed frame rate
        Application.targetFrameRate = 30;
    }

    /// <summary>
    /// The function loads the Create Level scene. The function is called when the CreateLevelButton is pressed.
    /// </summary>
    public void CreateLevelClicked()
    {
        SceneManager.LoadScene(createLevelSceneName);
    }

    /// <summary>
    /// The function loads the Select Level scene. The function is called when the PlayButton is pressed.
    /// </summary>
    public void PlayClicked()
    {
        SceneManager.LoadScene(selectLevelSceneName);
    }

    /// <summary>
    /// The function closes the game. The function is called when the ExitButton is pressed.
    /// </summary>
    public void ExitClicked()
    {
        Application.Quit();
    }
}
