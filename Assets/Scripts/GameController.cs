using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Struct to define the relationship between an integer ID, a Color, and a Name.
[System.Serializable]
public struct ColorMapping
{
    public string colorName;
    public Color colorValue;
    public int blockType;
}

public class GameController : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private TMP_InputField codeEditor;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject colorPaletteItemPrefab;
    [SerializeField] private Transform colorPaletteContainer;

    [Header("System References")]
    [SerializeField] private CubeGridManager gridManager;
    [SerializeField] private CubeGridManager referenceGridManager;
    [SerializeField] private ScriptingEngine scriptingEngine;

    [Header("Game Settings")]
    [SerializeField] private List<ColorMapping> availableColors;
    [SerializeField] private int gridSize = 9;

    [Header("Level Management")]
    [SerializeField] private List<LevelData> levels;
    private int currentLevelIndex = 0;

    void Start()
    {
        // Validation check to ensure levels exist
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("No levels assigned to GameController!");
            return;
        }

        // Load the initial level
        LoadLevel(currentLevelIndex);

        // Generate the UI legend based on available colors
        PopulateColorPalette();
    }

    // ---------------------------------------------------------
    // Core Gameplay Logic
    // ---------------------------------------------------------

    /// <summary>
    /// Executed when the "Run Code" button is clicked.
    /// It interprets the Lua script and builds the voxel grid.
    /// </summary>
    public void OnRunCodeClicked()
    {
        // 1. Clear previous results
        gridManager.ClearGrid();

        // 2. Execute the Lua script
        string playerCode = codeEditor.text;
        bool success = scriptingEngine.ExecuteScript(playerCode);

        if (!success) return; // Stop if Lua syntax error occurred

        // 3. Iterate through the grid space
        // Calculate bounds to center the grid (e.g., from -4 to +4 for a size of 9)
        int start = -Mathf.FloorToInt(gridSize / 2.0f);
        int end = Mathf.CeilToInt(gridSize / 2.0f);

        for (int x = start; x < end; x++)
        {
            for (int y = start; y < end; y++)
            {
                for (int z = start; z < end; z++)
                {
                    // Call the specific function defined in Lua script
                    int blockType = scriptingEngine.CallVoxelFunction("PlaceVoxel", x, y, z);

                    // If the function returns a valid block type (>0), place a cube
                    if (blockType > 0)
                    {
                        Color blockColor = GetColorFromType(blockType);
                        gridManager.SetCube(new Vector3Int(x, y, z), true, blockColor);
                    }
                }
            }
        }

        // 4. Validate the result against the reference
        CompareGrids();
    }

    /// <summary>
    /// Compares the player's grid with the reference grid and calculates the score.
    /// </summary>
    private void CompareGrids()
    {
        int correctCubes = 0;
        int wrongColorCubes = 0;
        int missingCubes = 0;
        int extraCubes = 0;
        int totalReferenceCubes = 0;

        int start = -Mathf.FloorToInt(gridSize / 2.0f);
        int end = Mathf.CeilToInt(gridSize / 2.0f);

        for (int x = start; x < end; x++)
        {
            for (int y = start; y < end; y++)
            {
                for (int z = start; z < end; z++)
                {
                    Vector3Int currentPos = new Vector3Int(x, y, z);

                    bool playerHasCube = gridManager.TryGetCubeColor(currentPos, out Color playerColor);
                    bool referenceHasCube = referenceGridManager.TryGetCubeColor(currentPos, out Color referenceColor);

                    if (referenceHasCube)
                    {
                        totalReferenceCubes++;
                        if (playerHasCube)
                        {
                            // Both have cube: Check color match
                            if (playerColor == referenceColor) correctCubes++;
                            else wrongColorCubes++;
                        }
                        else
                        {
                            // Reference has cube, player does not
                            missingCubes++;
                        }
                    }
                    else if (playerHasCube)
                    {
                        // Player has cube where none should be
                        extraCubes++;
                    }
                }
            }
        }

        // Calculate score percentage
        float matchPercentage = 0;
        int totalErrors = wrongColorCubes + missingCubes + extraCubes;
        int totalChecked = correctCubes + totalErrors;

        if (totalChecked > 0)
        {
            matchPercentage = ((float)correctCubes / totalChecked) * 100f;
        }
        else
        {
            matchPercentage = 100f; // Empty grid matches empty grid
        }

        UpdateScoreUI(matchPercentage, wrongColorCubes, missingCubes, extraCubes);
    }

    // ---------------------------------------------------------
    // Level Management
    // ---------------------------------------------------------

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Attempted to load an invalid level index!");
            return;
        }

        currentLevelIndex = levelIndex;
        LevelData levelToLoad = levels[currentLevelIndex];

        // Setup Reference Grid
        referenceGridManager.ClearGrid();
        foreach (VoxelInfo voxel in levelToLoad.referenceShape)
        {
            Color targetColor = GetColorFromType(voxel.blockType);
            referenceGridManager.SetCube(voxel.position, true, targetColor);
        }

        // Reset Player Grid and UI
        codeEditor.text = levelToLoad.startingCodeHint;
        gridManager.ClearGrid();

        // Run comparison immediately to show initial state (usually 0%)
        CompareGrids();
    }

    public void LoadNextLevel()
    {
        int nextIndex = (currentLevelIndex + 1) % levels.Count;
        LoadLevel(nextIndex);
    }

    public void PreviousNextLevel()
    {
        int nextIndex = (currentLevelIndex - 1) % levels.Count;
        LoadLevel(nextIndex);
    }
    public void ReloadCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    // ---------------------------------------------------------
    // UI & Helper Methods
    // ---------------------------------------------------------

    private void UpdateScoreUI(float matchPercentage, int wrongColor, int missing, int extra)
    {
        if (matchPercentage >= 100f && extra == 0 && wrongColor == 0)
        {
            scoreText.color = Color.green;
            scoreText.text = "PERFECT!";
        }
        else
        {
            scoreText.color = Color.white;
            // Using Rich Text for coloring specific parts of the string
            scoreText.text = $"Match: {matchPercentage:F1}%\n" +
                             $"<color=red>Wrong Color: {wrongColor}</color> | " +
                             $"<color=yellow>Missing: {missing}</color> | " +
                             $"<color=white>Extra: {extra}</color>";
        }
    }

    private void PopulateColorPalette()
    {
        // Clear existing items
        foreach (Transform child in colorPaletteContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new items for each color in settings
        foreach (var mapping in availableColors)
        {
            GameObject newItem = Instantiate(colorPaletteItemPrefab, colorPaletteContainer);
            ColorPaletteItemUI itemUI = newItem.GetComponent<ColorPaletteItemUI>();

            if (itemUI != null)
            {
                itemUI.SetData(mapping.colorValue, mapping.blockType, mapping.colorName);
            }
            newItem.SetActive(true);
        }
    }

    /// <summary>
    /// Helper to find Color by integer type ID.
    /// </summary>
    private Color GetColorFromType(int type)
    {
        foreach (var mapping in availableColors)
        {
            if (mapping.blockType == type)
            {
                return mapping.colorValue;
            }
        }
        // Return magenta to indicate an error (missing color mapping)
        return Color.magenta;
    }
}