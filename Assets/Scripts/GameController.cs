using UnityEngine;
using UnityEngine.UI; // Potrzebne do komponentu Image
using TMPro;
using System.Collections.Generic;

// Struktura do przechowywania informacji o kolorach
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
    public TMP_InputField codeEditor;
    public TMP_Text scoreText;
    public GameObject colorPaletteItemPrefab; // Prefab elementu palety UI
    public Transform colorPaletteContainer;   // Obiekt-rodzic, w którym tworzymy paletê

    [Header("Component References")]
    public CubeGridManager gridManager;
    public CubeGridManager referenceGridManager;
    public ScriptingEngine scriptingEngine;

    [Header("Game Settings")]
    public List<ColorMapping> availableColors; // Definiowalna paleta kolorów
    public int gridSize = 9;

    [Header("Level Management")]
    public List<LevelData> levels; // Lista wszystkich poziomów (plików LevelData)
    private int currentLevelIndex = 0;

    void Start()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("Brak poziomów przypisanych do GameController!");
            return;
        }

        // Wczytujemy pierwszy poziom z listy
        LoadLevel(currentLevelIndex);
        // Tworzymy legendê kolorów w UI
        PopulateColorPalette();
    }

    // Metoda wywo³ywana przez przycisk "Run Code"
    public void OnRunCodeClicked()
    {
        gridManager.ClearGrid();
        string playerCode = codeEditor.text;
        bool success = scriptingEngine.ExecuteScript(playerCode);

        if (!success) return;

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

    // Metoda do wczytywania poziomu o podanym indeksie
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Próba wczytania nieistniej¹cego poziomu!");
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

    // Metody do nawigacji (dla przycisków UI)
    public void LoadNextLevel()
    {
        int nextIndex = (currentLevelIndex + 1) % levels.Count;
        LoadLevel(nextIndex);
    }

    public void ReloadCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    // Metoda porównuj¹ca obie siatki
    void CompareGrids()
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
                        else missingCubes++;
                    }
                    else if (playerHasCube)
                    {
                        extraCubes++;
                    }
                }
            }
        }

        float matchPercentage = 0;
        int totalPossibleMistakes = totalReferenceCubes + extraCubes;
        if (totalPossibleMistakes > 0)
        {
            matchPercentage = ((float)correctCubes / (correctCubes + wrongColorCubes + missingCubes + extraCubes)) * 100f;
        }
        else
        {
            matchPercentage = 100f;
        }

        if (matchPercentage >= 100f && extraCubes == 0 && wrongColorCubes == 0)
        {
            scoreText.color = Color.green;
            scoreText.text = "IDEALNIE!";
        }
        else
        {
            scoreText.color = Color.white;
            scoreText.text = $"Dopasowanie: {matchPercentage:F1}%\n" +
                             $"<color=red>B³êdny kolor: {wrongColorCubes}</color> | " +
                             $"<color=yellow>Brakuj¹ce: {missingCubes}</color> | " +
                             $"<color=grey>Dodatkowe: {extraCubes}</color>";
        }
    }

    // Metoda tworz¹ca legendê kolorów w UI
    void PopulateColorPalette()
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

    // Prywatna metoda pomocnicza do zamiany typu bloku na kolor
    private Color GetColorFromType(int type)
    {
        foreach (var mapping in availableColors)
        {
            if (mapping.blockType == type)
            {
                return mapping.colorValue;
            }
        }
        return Color.magenta; // Magenta jako kolor b³êdu, jeœli typ nie zostanie znaleziony
    }
}