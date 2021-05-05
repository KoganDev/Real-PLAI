using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DesignLevelControl : MonoBehaviour
{
    // ------------------------ Scene Names ---------------------------

    private const string mainMenuSceneName = "MainMenu";

    // ------------------------ Place Tiles Variables ---------------------------

    private bool isBlockClicked;
    private bool isSpikesClicked;
    private bool isEraserClicked;
    private bool isStartBlockClicked;
    private bool isEndBlockClicked;

    private float yCoorToolBarStarts = 4f;

    [SerializeField] private Tilemap foreground;

    [SerializeField] private Tile startBlock;
    [SerializeField] private Tile endBlock;
    [SerializeField] private Tile block;
    [SerializeField] private Tile spikes;

    // ------------------------ Expand Level Variables ---------------------------

    private int xAxisValue = 24;
    public static int backgroundWidth = 24;
    private int yStart = 0;
    private int yEnd = 15;

    [SerializeField] private Slider slider;
    [SerializeField] private Tile backgroundTile;
    [SerializeField] private BoxCollider2D backgroundCollider;
    [SerializeField] private Tilemap background;

    // ------------------------ Special Blocks Variables ---------------------------
    public static SpecialBlock startBlockData;
    public static SpecialBlock endBlockData;

    public struct SpecialBlock
    {
        public bool blockExists;
        public Vector3 blockPosition;
    };

    // ------------------------ Buttons Variables ---------------------------

    [SerializeField] private Button blockButton;
    [SerializeField] private Button spikesButton;
    [SerializeField] private Button eraseButton;
    [SerializeField] private Button startblockButton;
    [SerializeField] private Button endblockButton;

    // ------------------------ Button Colors Variables ---------------------------

    Color normal = new Color(0.1215686f, 0.4823529f, 0.5607843f, 1);
    Color selected = new Color(0.2039216f, 0.7647059f, 0.8862745f, 1);


    /// <summary>
    /// Initilaize level design variables when starting.
    /// </summary>
    private void Start()
    {
        // Change the maximum x coordinate that the slider can slide to by chnaging its max value
        slider.maxValue = 12;

        // All the buttons are not pressed at the start
        isBlockClicked = false;
        isSpikesClicked = false;
        isEraserClicked = false;
        isStartBlockClicked = false;
        isEndBlockClicked = false;

        // We have a start and end block in the start
        startBlockData = new SpecialBlock();
        startBlockData.blockExists = true;
        startBlockData.blockPosition = new Vector3(4, 5, 0);  // hardcoded coordinates

        endBlockData = new SpecialBlock();
        endBlockData.blockExists = true;
        endBlockData.blockPosition = new Vector3(19, 5, 0);  // hardcoded coordinates

    }

    /// <summary>
    /// the function changes the color of all buttons to their original normal color.
    /// </summary>
    private void UnmarkAllButtons()
    {
        // Unmark the block button
        var colors = blockButton.colors;
        colors.normalColor = normal;
        blockButton.colors = colors;

        // Unmark the spikes button
        colors = spikesButton.colors;
        colors.normalColor = normal;
        spikesButton.colors = colors;

        // Unmark the erase button
        colors = eraseButton.colors;
        colors.normalColor = normal;
        eraseButton.colors = colors;

        // Unmark the start block button
        colors = startblockButton.colors;
        colors.normalColor = normal;
        startblockButton.colors = colors;

        // Unmark the end block button
        colors = endblockButton.colors;
        colors.normalColor = normal;
        endblockButton.colors = colors;
    }

    /// <summary>
    /// The function changes the color of a button so it will look pressed when button is pressed.
    /// </summary>
    /// <param name="button">The button that may change a color.</param>
    /// <param name="mark">The value that decides whether to change the button's color.</param>
    private void MarkButtonIfNeeded(Button button, bool mark)
    {
        if(mark)
        {
            var colors = button.colors;
            colors.normalColor = selected;
            button.colors = colors;
        }
    }

    /// <summary>
    /// The function loads the Main Menu scene. The function is called when the BackToMenuButton is pressed.
    /// </summary>
    public void ClickedBack()
    {
        // Load the main menu again
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// The function changes the variables so that block tiles can be now drawn on the screen - on the foreground tilemap.
    /// The function is called when the block button is pressed.
    /// </summary>
    public void BlockClicked()
    {
        // Makes sure we can only draw blocks when the blockButton is clicked
        isBlockClicked = !isBlockClicked;
        isSpikesClicked = false;
        isEraserClicked = false;
        isStartBlockClicked = false;
        isEndBlockClicked = false;
        UnmarkAllButtons();
        MarkButtonIfNeeded(blockButton, isBlockClicked);
    }

    /// <summary>
    /// The function changes the variables so that spikes tiles can be now drawn on the screen - on the foreground tilemap.
    /// The function is called when the spikes button is pressed.
    /// </summary>
    public void SpikesClicked()
    {
        // Makes sure we can only draw spikes when the spikesButton is clicked
        isSpikesClicked = !isSpikesClicked;
        isBlockClicked = false;
        isEraserClicked = false;
        isStartBlockClicked = false;
        isEndBlockClicked = false;
        UnmarkAllButtons();
        MarkButtonIfNeeded(spikesButton, isSpikesClicked);
    }

    /// <summary>
    /// The function changes the variables so that we can now erase tiles from the screen - from the foreground tilemap.
    /// The function is called when the erase button is pressed.
    /// </summary>
    public void EraserClicked()
    {
        isEraserClicked = !isEraserClicked;
        isBlockClicked = false;
        isSpikesClicked = false;
        isStartBlockClicked = false;
        isEndBlockClicked = false;
        UnmarkAllButtons();
        MarkButtonIfNeeded(eraseButton, isEraserClicked);
    }

    /// <summary>
    /// The function changes the variables so that start blocks can be placed on the screen.
    /// The function is called when the start block button is pressed.
    /// </summary>
    public void StartBlockClicked()
    {
        isStartBlockClicked = !isStartBlockClicked;
        isEraserClicked = false;
        isBlockClicked = false;
        isSpikesClicked = false;
        isEndBlockClicked = false;
        UnmarkAllButtons();
        MarkButtonIfNeeded(startblockButton, isStartBlockClicked);
    }

    /// <summary>
    /// The function changes the variables so that end blocks can be placed on the screen.
    /// The function is called when the end block button is pressed.
    /// </summary>
    public void EndBlockClicked()
    {
        isEndBlockClicked = !isEndBlockClicked;
        isEraserClicked = false;
        isBlockClicked = false;
        isSpikesClicked = false;
        isStartBlockClicked = false;
        UnmarkAllButtons();
        MarkButtonIfNeeded(endblockButton, isEndBlockClicked);
    }

    /// <summary>
    /// The function changes the x value of the camera so that the user can move the screen with the slider UI.
    /// The function is called when the user tries to move the screen using the slider UI.
    /// </summary>
    /// <param name="xCoor"></param>
    public void MoveScreen(float xCoor)
    {
        // xCoor is not needed but the function needs to receive a value for the slider to work
        // Change the x coordinate of the camera so it can move with slider
        Camera.main.transform.position = new Vector3(slider.value, Camera.main.transform.position.y, Camera.main.transform.position.z);
    }

    /// <summary>
    /// The function expands the background. The function is called when the expand level button is pressed.
    /// </summary>
    public void IncreaseLevel()
    {
        for (int x = xAxisValue; x < xAxisValue + backgroundWidth; x++)
        {
            for (int y = yStart; y < yEnd; y++)
            {
                // The coordinates the tile will be printed at
                Vector3Int p = new Vector3Int(x, y, 0);
                // Draw the tile on the background
                background.SetTile(p, backgroundTile);
            }
        }

        // Increase the size of our backgroundCollider - No need to use backgroundCollider here
        //backgroundCollider.size = new Vector2(backgroundCollider.size.x + backgroundWidth, backgroundCollider.size.y);
        //backgroundCollider.offset = new Vector2(backgroundCollider.offset.x + backgroundWidth / 2, backgroundCollider.offset.y);
        // Increase the x coordinate so we can add a new background next to the one we currently added
        xAxisValue += backgroundWidth;

        // Change the maximum x coordinate that the slider can slide to by changing its max value
        slider.maxValue += backgroundWidth;
    }

    /// <summary>
    /// Entry: The function is called every game frame.
    /// Exit: The function enables the user to draw new tiles on the screen.
    /// </summary>
    private void Update()
    {
        // Input.GetMouseButtonDown(0) only once - need to click every tile we want to add
        if (Input.GetMouseButton(0))
        {
            // Makes sure we pressed a button before drawing tiles on the screen
            bool buttonClicked = false;
            Tile tile = null;
            if (isBlockClicked)
            {
                tile = block;
                buttonClicked = true;
            }
            else if (isSpikesClicked)
            {
                tile = spikes;
                buttonClicked = true;
            }
            else if (isEraserClicked)
            {
                // Tiles should be null because this is the way to erase existing tiles
                buttonClicked = true;
                tile = null;
            }
            else if (isStartBlockClicked)
            {
                tile = startBlock;
                buttonClicked = true;
            }
            else if (isEndBlockClicked)
            {
                tile = endBlock;
                buttonClicked = true;
            }

            if (buttonClicked)
            {
                // Get the mouse position in the game 
                Vector2 mouseCooridnates = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // Get the mouse position in the grid world
                Vector3Int currentCell = foreground.WorldToCell(mouseCooridnates);

                // The user can't place tiles on the toolbar
                if (mouseCooridnates.y <= yCoorToolBarStarts)
                {
                    return;
                }

                // If one of the special blocks already exists we can't create another one so exit the function
                if (tile == startBlock && startBlockData.blockExists)
                {
                    return;
                }
                if (tile == endBlock && endBlockData.blockExists)
                {
                    return;
                }

                // If we draw a new tile on a special tile update the struct values
                if (currentCell == startBlockData.blockPosition)
                {
                    startBlockData.blockExists = false;
                }
                else if (currentCell == endBlockData.blockPosition)
                {
                    endBlockData.blockExists = false;
                }

                // If we add a special block update the block's struct
                if (tile == startBlock)
                {
                    startBlockData.blockExists = true;
                    startBlockData.blockPosition = currentCell;
                }
                else if (tile == endBlock)
                {
                    endBlockData.blockExists = true;
                    endBlockData.blockPosition = currentCell;
                }

                // Draw tile in our tilemap
                foreground.SetTile(currentCell, tile);
            }

        }
    }

}