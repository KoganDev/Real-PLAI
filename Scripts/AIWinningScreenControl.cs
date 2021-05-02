﻿using System.Collections;
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

    [SerializeField] private Text numberOfTriesTetx;
    private string numberOfTriesTextContent;


    // -------------------------- Temp ----------------------------------

    static int countWinnings = 0;

    private void Start()
    {
        numberOfTriesTextContent = "Number Of Tries: " + AIController.numberOfTries;
        numberOfTriesTetx.text = numberOfTriesTextContent;
        AIController.numberOfTries = 0;  // Start counting again from zero after winning

        /*
        // Temp code to train again automatically
        countWinnings++;
        Debug.Log("Won again - " + countWinnings.ToString() + " times");
        SceneManager.LoadScene(levelSceneName);  // Load The Level Scene Again to Train More Automatically
        */
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
