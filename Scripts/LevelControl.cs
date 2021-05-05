using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class LevelControl : MonoBehaviour
{

    // ------------------------ Scene Names ---------------------------

    private const string mainMenuSceneName = "MainMenu";
    private const string AISceneName = "AIBasicLevel";

    // ------------------------ Level File Variables ---------------------------

    private string levelPath;

    [SerializeField] private Rigidbody2D characterRigidBody;

    // ------------------------ Level Graphic Variables ---------------------------

    [SerializeField] private Tilemap background;
    [SerializeField] private Tilemap foreground;
    [SerializeField] private Tilemap dangerLayer;

    [SerializeField] private Tile startBlock;
    [SerializeField] private Tile endBlock;
    [SerializeField] private Tile block;
    [SerializeField] private Tile spikes;
    [SerializeField] private Tile sky;

    // ------------------------ Level Borders Variables ---------------------------

    public static int rectangles;

    public static FurthestCoordinates colliderCoords;
    public struct FurthestCoordinates
    {
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;
    }

    [SerializeField] private BoxCollider2D leftBorder;
    [SerializeField] private BoxCollider2D rightBorder;
    [SerializeField] private BoxCollider2D topBorder;
    [SerializeField] private BoxCollider2D buttomBorder;

    [SerializeField] private PolygonCollider2D backgroungCollider;

    [SerializeField] private CinemachineConfiner bordersConfiner;

    // ------------------------ General Level Variables ---------------------------

    [SerializeField] private Rigidbody2D destination;

    private Vector3 playerPosition;

    /// <summary>
    /// The function initializes the chosen level. The Start function is called before the first frame update.
    /// </summary>
    void Start()
    {
        // We save the coordiantes of the background collider
        colliderCoords = new FurthestCoordinates();
        InitCoords();
        // Get the path of the chosen level
        levelPath = SelectLevelControl.chosenLevelPath;
        // Load the level's tiles
        LoadLevel();

        // Calculate of how many rectangles the current level is made of
        rectangles = ((int)Mathf.Ceil((colliderCoords.xMax + 1) - colliderCoords.xMin) / DesignLevelControl.backgroundWidth);

        // This is the correct way to calculate the width of the level
        colliderCoords.xMax = rectangles * 23.75f;

        // Create the camera's borders
        UpdateCameraBodrers();

        // Create the level's borders
        CreateBorders();

        // Change the player's position
        characterRigidBody.transform.position = playerPosition;
    }

    /// <summary>
    /// The function updates the camera's borders so it will stay in the level's screen.
    /// </summary>
    private void UpdateCameraBodrers()
    {
        // Update the borders of the screen so the camera can stay inside the game screen
        Vector2[] points = backgroungCollider.points;
        points[0] = new Vector2(colliderCoords.xMin, colliderCoords.yMax);
        points[1] = new Vector2(colliderCoords.xMin, colliderCoords.yMin);
        points[2] = new Vector2(colliderCoords.xMax, colliderCoords.yMin);
        points[3] = new Vector2(colliderCoords.xMax, colliderCoords.yMax);
        backgroungCollider.points = points;

        // Update the borders of the cinemachine confiner - makes the camera stay inside the level
        bordersConfiner.InvalidatePathCache();
        bordersConfiner.m_BoundingShape2D = backgroungCollider;
    }

    /// <summary>
    /// The function loads the Main Menu scene. The function is called when the BackToMenuButton is pressed.
    /// </summary>
    public void ClickedBack()
    {
        // Get the current scene
        Scene scene = SceneManager.GetActiveScene();
        // If the AI plays we need to save the neural networks and the memories
        if(scene.name == AISceneName)
        {
            AIController.SaveAll();
            AIController.overallTime.Reset();
            AIController.startedTime = false;
            AIController.numberOfTries = 0;
        }
        // Load the main menu again
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// The function changes the size and the position of the level's borders.
    /// </summary>
    private void CreateBorders()
    {
        // I choose a meaningless number as the size of the border
        float sizeOfBorder = 5;
        float xSizeOfLevel = colliderCoords.xMax - colliderCoords.xMin;
        float ySizeOfLevel = colliderCoords.yMax - colliderCoords.yMin;
        float zCoor = leftBorder.transform.position.z;  // The z coordinate doesn't really matter

        // The size of the left border will be: width - my meaningless number. height: the max y coordinate minus the min y coordinate
        leftBorder.size = new Vector2(sizeOfBorder, ySizeOfLevel);
        leftBorder.transform.position = new Vector3(colliderCoords.xMin - sizeOfBorder / 2, colliderCoords.yMax / 2, zCoor);

        // The size of the right border will be: width - my meaningless number. height: the max y coordinate minus the min y coordinate
        rightBorder.size = new Vector2(sizeOfBorder, ySizeOfLevel);
        rightBorder.transform.position = new Vector3(colliderCoords.xMax + sizeOfBorder / 2, colliderCoords.yMax / 2, zCoor);

        // The size of the buttom border will be: width - the max x coordinate minus the min x coordinate. height: my meaningless number
        buttomBorder.size = new Vector2(xSizeOfLevel, sizeOfBorder);
        buttomBorder.transform.position = new Vector3(colliderCoords.xMin + xSizeOfLevel / 2, colliderCoords.yMin - sizeOfBorder / 2, zCoor);

        // The size of the top border will be: width - the max x coordinate minus the min x coordinate. height: my meaningless number
        topBorder.size = new Vector2(xSizeOfLevel, sizeOfBorder);
        topBorder.transform.position = new Vector3(colliderCoords.xMin + xSizeOfLevel / 2, colliderCoords.yMax + sizeOfBorder / 2, zCoor);
    }

    /// <summary>
    /// The function initializes the background collider's borders. The function Start calls this function.
    /// </summary>
    private void InitCoords()
    {
        colliderCoords.xMin = 0;
        colliderCoords.xMax = 0;
        colliderCoords.yMin = 0;
        colliderCoords.yMax = 0;
    }

    /// <summary>
    /// The function loads the level from the matching text file of the level. The function is called by the Start function.
    /// </summary>
    private void LoadLevel()
    {
        string data = File.ReadAllText(levelPath);
        // Read all the data tile from the level text file and save it in a variable
        AllDataJson jsonData = JsonUtility.FromJson<AllDataJson>(data);
        foreach (TileDataJson tileData in jsonData.tilesDataInJson)
        {
            // If the current tile was set in the background place this tile in the background tilemap 
            if (tileData.tilemapName == "Background")
            {
                // Set the tile in the tilemap
                background.SetTile(Vector3Int.FloorToInt(tileData.position), sky);
                // Update the furthest coordinates of the background tilemap
                UpdateColliderCoords(tileData.position);
            }
            // If the current tile was set in the foreground place this tile in the foreground tilemap 
            else if (tileData.tilemapName == "Foreground")
            {
                if (tileData.tileName == "Block")
                {
                    // Set the block tile in the tilemap
                    foreground.SetTile(Vector3Int.FloorToInt(tileData.position), block);
                }
                else if (tileData.tileName == "StartBlock")
                {
                    // Set the start block tile in the tilemap
                    foreground.SetTile(Vector3Int.FloorToInt(tileData.position), startBlock);
                    // The player's position should be one block up compare to the start block
                    playerPosition = new Vector3(tileData.position.x + 0.5f, tileData.position.y + 2.1f, tileData.position.z);
                }
                else if (tileData.tileName == "EndBlock")
                {
                    // Set the end block tile in the tilemap
                    foreground.SetTile(Vector3Int.FloorToInt(tileData.position), endBlock);
                    // Set the destination position and set the destination game object to that position
                    Vector3 endPosition = new Vector2(tileData.position.x + 0.5f, tileData.position.y + 1f);
                    destination.position = endPosition;
                }
                else if (tileData.tileName == "Spikes")
                {
                    // Spikes is special because we put it in a different layer because it behaves differently
                    // Set the spikes tile in the tilemap
                    dangerLayer.SetTile(Vector3Int.FloorToInt(tileData.position), spikes);
                }
            }
        }

        // Need to add a bit to the coordinates so it will match correctly
        colliderCoords.yMax += 0.85f;
    }

    /// <summary>
    /// The function updates the furthest coordinates of the backgeround by comapring the current furthest coordinates
    /// to a new tile in the background. The funciton is used by the LoadLevel function.
    /// </summary>
    /// <param name="tilePosition">A new tile in the background.</param>
    private void UpdateColliderCoords(Vector3 tilePosition)
    {
        if (colliderCoords.xMin > tilePosition.x)
        {
            colliderCoords.xMin = tilePosition.x;
        }
        else if (colliderCoords.xMax < tilePosition.x)
        {
            colliderCoords.xMax = tilePosition.x;
        }

        if (colliderCoords.yMin > tilePosition.y)
        {
            colliderCoords.yMin = tilePosition.y;
        }
        else if (colliderCoords.yMax < tilePosition.y)
        {
            colliderCoords.yMax = tilePosition.y;
        }
    }
}
