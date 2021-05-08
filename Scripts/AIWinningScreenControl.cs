using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AIWinningScreenControl : MonoBehaviour
{

    // ------------------------ Scene Names ---------------------------

    private const string mainMenuSceneName = "MainMenu";
    private const string levelSceneName = "AIBasicLevel";

    // ------------------------ UI Variables ---------------------------

    [SerializeField] private Text numberOfTriesText;
    private string numberOfTriesTextContent = "Number Of Tries: ";

    [SerializeField] private Text winningTimeText;
    private string winningTimeTextContent = "Try Time: ";

    [SerializeField] private Text overallTimeText;
    private string overallTimeTextContent = "Overall Time: ";


    // -------------------------- Temp ----------------------------------

    static int countWinnings = 0;

    private void Start()
    {
        string overallTimeTemp; // 
        string tryTimeTemp; // 
        int numberOfTriesTemp; // 


        // Get number of tries
        numberOfTriesText.text = numberOfTriesTextContent + AIController.numberOfTries;

        numberOfTriesTemp = AIController.numberOfTries; // 

        AIController.numberOfTries = 0;  // Start counting again from zero after winning

        // Get last try time
        System.TimeSpan time = AIController.tryTime.Elapsed;
        string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);

        tryTimeTemp = elapsedTime; // 

        winningTimeText.text = winningTimeTextContent + elapsedTime;

        // Get overall time
        time = AIController.overallTime.Elapsed;
        elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
        overallTimeText.text = overallTimeTextContent + elapsedTime;

        overallTimeTemp = elapsedTime; // 

        AIController.overallTime.Reset();  // Reset overall time
        AIController.startedTime = false;

        // Temp code to train again automatically
        countWinnings++; // 
        Debug.Log("Won " + countWinnings.ToString() + " times"); // 
        Debug.Log("Time: " + overallTimeTemp + " Try Time: " + tryTimeTemp + " tries: " + numberOfTriesTemp); // 

        SceneManager.LoadScene(levelSceneName);  // Load The Level Scene Again to Train More Automatically // 
    }


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
