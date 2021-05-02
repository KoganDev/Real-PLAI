using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateLevelControl : MonoBehaviour
{

    // ------------------------ Level File Variables ---------------------------

    private string levelName;
    public static string levelPath;

    // ------------------------ Scene Names ---------------------------

    private string mainMenuSceneName = "MainMenu";
    private string designLevelSceneName = "DesignLevel";

    // ------------------------ UI Variables ---------------------------

    [SerializeField] private InputField inputField;
    [SerializeField] private Text messageBox;

    // ------------------------ Error Messages ---------------------------

    private const string invalidNameMessage = "This name is already taken.\nPlease choose another one.\n";

    /// <summary>
    /// The function makes sure there is a Levels directory to store levels in it and if not it creates a new directory.
    /// </summary>
    private void Start()
    {
        if(!Directory.Exists(Application.dataPath + "/Levels"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Levels");
        }
    }

    /// <summary>
    /// The function tries to create a new level with the name that the user chose - if possible loads the design 
    /// level scene with the new level. The function is called when the ReadyButton is pressed.
    /// </summary>
    public void ReadyClicked()
    {
        // LevelName will get its value from the input field UI element
        levelName = inputField.text;
        // Create the path of our new level
        levelPath = Application.dataPath + "/Levels/" + levelName + ".txt";

        // If a level with the same name already exists don't create a new one
        if (!File.Exists(levelPath))
        {
            // Create a new text file with no text inside - we'll later save Json information in there
            File.WriteAllText(levelPath, "");
            // Load the design level scene
            SceneManager.LoadScene(designLevelSceneName);
        }
        else
        {
            messageBox.text = invalidNameMessage;
        }
    }

    /// <summary>
    /// The function loads the Main Menu scene. The function is called when the BackButton is pressed.
    /// </summary>
    public void ClickedBack()
    {
        // Load the main menu again
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
