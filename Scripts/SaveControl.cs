using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SaveControl : MonoBehaviour
{

    // ------------------------ Graphic Variables ---------------------------

    [SerializeField] private Tilemap background;
    [SerializeField] private Tilemap foreground;

    // ------------------------ UI Variables ---------------------------

    [SerializeField] private Text messages;

    // ------------------------ Error Messages ---------------------------

    private const string error1 = "Missing start block/end block\n";
    private const string error2 = "Remove block on start block\n";
    private const string error3 = "The start block needs to be from the left of the end block\n";

    /// <summary>
    /// The function returns a string that contains the errors the level has. It can send up to one error at a time.
    /// </summary>
    /// <returns>A message that conatins level errors.</returns>
    private string CanSave()
    {
        string errorMessages = "";
        DesignLevelControl.SpecialBlock startBlock = DesignLevelControl.startBlockData;
        DesignLevelControl.SpecialBlock endBlock = DesignLevelControl.endBlockData;
        Vector3Int blockAbovePosition = new Vector3Int((int)startBlock.blockPosition.x, (int)startBlock.blockPosition.y + 1, 0);
        if (!startBlock.blockExists || !endBlock.blockExists)
        {
            // There nust be a start block and an end block
            errorMessages = error1;
        }
        else if(foreground.GetTile(blockAbovePosition) != null)
        {
            // There can't be a block of the start block
            errorMessages = error2;
        }
        else if(startBlock.blockPosition.x > endBlock.blockPosition.x)
        {
            // The start block must be on the left to the end block
            errorMessages = error3;
            Debug.Log("Start x: " + startBlock.blockPosition.x + " End x" + endBlock.blockPosition.x);
        }
        return errorMessages;
    }

    /// <summary>
    /// The function saves info about every tile in the Tilemap to the given AllDataJson object.
    /// </summary>
    /// <param name="tilemap">A tilemap object that we svae the data in it.</param>
    /// <param name="data">AllDataJson object that we'll save data to.</param>
    private void SaveTilemap(Tilemap tilemap, AllDataJson data)
    {
        // Get the cell x and y borders of the given Tilemap
        BoundsInt bounds = tilemap.cellBounds;
        // Get an array of all the tiles in the foreground between the borders we got
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        // Move on all the tiles from the x axis
        for (int x = 0; x < bounds.size.x; x++)
        {
            // Move on all the tiles from the x axis
            for (int y = 0; y < bounds.size.y; y++)
            {
                // Get the tile in the 2d array by calculating the position the tile is on the screen
                TileBase tile = allTiles[x + y * bounds.size.x];
                // If we found a tile - the tile exists
                if (tile != null)
                {
                    // Create a new object to represnt a tile and save the info of the tile there
                    TileDataJson obj = new TileDataJson
                    {
                        tileName = tile.name,
                        position = new Vector3(x, y, 0),
                        tilemapName = tilemap.name
                    };
                    // Add the tile to the list of tile info in the given object data
                    data.tilesDataInJson.Add(obj);
                }
            }
        }
    }

    /// <summary>
    /// The function saves all the info about the level we are working on to a text file using Json format.
    /// The function is called when the save buttion is clicked.
    /// </summary>
    public void SaveClicked()
    {
        // Take the path with the name of the level from previous screen - the level the user just created
        string path = CreateLevelControl.levelPath;

        // Check if it is possible to save and if not show error messages
        string errorMessages = CanSave();
        if (errorMessages != "")
        {
            messages.text = errorMessages;
            return;
        }

        // Show saved on the screen if the level can be saved
        messages.text = "Saved";

        // Create a new object that will conatin the info about the level
        AllDataJson data = new AllDataJson();
        // Create the list of TileDataJsons that will conatin the info about the tiles of the level
        data.tilesDataInJson = new List<TileDataJson>();

        // Clear all the old data from the text file of the level - just to make sure
        File.WriteAllText(path, "");
        // Makes the cell bounds of the background tilemap match the tiles inside
        background.CompressBounds();
        // Save the tiles from the background foreground
        SaveTilemap(background, data);

        // Change the size of the foreground to the size of the background so the positions of the tiles will match
        foreground.size = background.size;
        // Make sure they start from the same coordinates as well
        foreground.origin = background.origin;
        // Resize the foreground foreground with the new size
        foreground.ResizeBounds();
        // Save the tiles from the foreground foreground
        SaveTilemap(foreground, data);

        // Now after our object contains all the tiles convert it to JSON
        string jsonData = JsonUtility.ToJson(data);
        // Enter Json info to the text file of the current level
        File.AppendAllText(path, jsonData);
    }

}

// TileDataJson is an object that can store the information of a tile
[Serializable]
public class TileDataJson
{
    public string tileName;
    public Vector3 position;
    public string tilemapName;
}

// AllDataJson is an object that saves all the tiles of a level from all tilemaps
[Serializable]
public class AllDataJson
{
    public List<TileDataJson> tilesDataInJson;
}
