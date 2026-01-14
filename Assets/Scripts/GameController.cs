using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
 
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
    public TMP_Text errorText;

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
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("No levels assigned to GameController!");
            return;
        }
         
        LoadLevel(currentLevelIndex); 
        PopulateColorPalette();
    }
     
    public void OnRunCodeClicked()
    {
        errorText.text = "";
        errorText.gameObject.SetActive(false);

        gridManager.ClearGrid();
        
        string playerCode = codeEditor.text;
        string errorMessage = scriptingEngine.ExecuteScript(playerCode);

        if (!string.IsNullOrEmpty(errorMessage))
        { 
            errorText.gameObject.SetActive(true);
            errorText.text = errorMessage;
            return;  
        }


        int start = -Mathf.FloorToInt(gridSize / 2.0f);
        int end = Mathf.CeilToInt(gridSize / 2.0f);

        for (int x = start; x < end; x++)
        {
            for (int y = start; y < end; y++)
            {
                for (int z = start; z < end; z++)
                { 
                    int blockType = scriptingEngine.CallVoxelFunction("PlaceVoxel", x, y, z);
                     
                    if (blockType > 0)
                    {
                        Color blockColor = GetColorFromType(blockType);
                        gridManager.SetCube(new Vector3Int(x, y, z), true, blockColor);
                    }
                }
            }
        } 
        CompareGrids();
    }
     
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
                            if (playerColor == referenceColor) correctCubes++;
                            else wrongColorCubes++;
                        }
                        else
                        { 
                            missingCubes++;
                        }
                    }
                    else if (playerHasCube)
                    { 
                        extraCubes++;
                    }
                }
            }
        }
         
        float matchPercentage = 0;
        int totalErrors = wrongColorCubes + missingCubes + extraCubes;
        int totalChecked = correctCubes + totalErrors;

        if (totalChecked > 0)
        {
            matchPercentage = ((float)correctCubes / totalChecked) * 100f;
        }
        else
        {
            matchPercentage = 100f;  
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
 
        referenceGridManager.ClearGrid();
        foreach (VoxelInfo voxel in levelToLoad.referenceShape)
        {
            Color targetColor = GetColorFromType(voxel.blockType);
            referenceGridManager.SetCube(voxel.position, true, targetColor);
        }
         
        codeEditor.text = levelToLoad.startingCodeHint;
        gridManager.ClearGrid();
         
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
            scoreText.text = $"Match: {matchPercentage:F1}%\n" +
                             $"<color=red>Wrong Color: {wrongColor}</color> | " +
                             $"<color=yellow>Missing: {missing}</color> | " +
                             $"<color=white>Extra: {extra}</color>";
        }
    }

    private void PopulateColorPalette()
    { 
        foreach (Transform child in colorPaletteContainer)
        {
            Destroy(child.gameObject);
        }
         
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
    private Color GetColorFromType(int type)
    {
        foreach (var mapping in availableColors)
        {
            if (mapping.blockType == type)
            {
                return mapping.colorValue;
            }
        } 
        return Color.magenta;
    }
}