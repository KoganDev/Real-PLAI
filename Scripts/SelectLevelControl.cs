using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectLevelControl : MonoBehaviour
{

    // ------------------------ Scene Names ---------------------------

    private const string basicLevelSceneName = "BasicLevel";
    private const string basicAILevelSceneName = "AIBasicLevel";
    private const string mainMenuSceneName = "MainMenu";

    // ------------------------ UI Variables ---------------------------

    [SerializeField] Dropdown levelOptions;
    [SerializeField] Dropdown whoPlays;

    // ------------------------ File Paths ---------------------------

    private string folderPath;
    public static string chosenLevelPath;

    // ------------------------ General Variables ---------------------------

    private List<string> levels;


    /// <summary>
    /// The function puts all the possible level names in the dropdown UI that contains all the playable levels.
    /// The Start function is called before the first frame update.
    /// </summary>
    void Start()
    {
        // If the levels directory doesn't exists create one
        if (!Directory.Exists(Application.dataPath + "/Levels"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Levels");
        }

        // This is the path to the folder with all the levels
        folderPath = Application.dataPath + "/Levels";
        // In this list we'll save all the playable level names
        levels = new List<string>();
        // Get to the folder that conatains all the levels
        DirectoryInfo dir = new DirectoryInfo(folderPath);
        // Get all the text files from the directory
        FileInfo[] info = dir.GetFiles("*.txt");
        // The first value the user will see in the dropdown is "Please select a level"
        levels.Add("Please Select A Level");
        foreach (FileInfo file in info)
        {
            // Get the names of all the text files from the Levels directory (folder) without their .txt extension
            levels.Add(file.Name.Replace(".txt", ""));
        }

        // Delete all the current options - to make sure
        levelOptions.ClearOptions();
        // Add the playable levels to the dropdown UI
        levelOptions.AddOptions(levels);
    }

    /// <summary>
    /// The function loads the Main Menu scene. The function is called when the BackButton is pressed.
    /// </summary>
    public void ClickedBack()
    {
        // Load the main menu again
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// The function loads the Basic Level scene after saving the name of the chosen level.
    /// The function is called when the player clicks on the Ready Button.
    /// </summary>
    public void UserReady()
    {
        // Get the name of the chosen level
        string levelName = levelOptions.options[levelOptions.value].text;

        // Make Sure the user chose a level and not the basic display text
        if(levelName != "Please Select A Level")
        {
            // The path of the chosen text file that conatins the level
            chosenLevelPath = folderPath + "/" + levelName + ".txt";

            if(whoPlays.options[whoPlays.value].text == "Player Plays")
            {
                // Load the basic player level
                SceneManager.LoadScene(basicLevelSceneName);
            }
            else
            {
                // Load the basic AI level
                SceneManager.LoadScene(basicAILevelSceneName);
            }
        }
    }
}
