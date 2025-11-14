using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    [Header("Component References")]
    public TMP_InputField codeEditor;
    public CubeGridManager gridManager;
    public CubeGridManager referenceGridManager;
    public ScriptingEngine scriptingEngine;
    public TMP_Text scoreText;
    public int gridSize = 9;  
    [Header("Level Management")]
    public List<LevelData> levels;  
    private int currentLevelIndex = 0;

    // Ta metoda jest wywo³ywana na starcie gry
    void Start()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("Brak poziomów przypisanych do GameController!");
            return;
        }
        // Wczytujemy pierwszy poziom z listy
        LoadLevel(currentLevelIndex);
    }

    // This method is called when the "Run Code" button is clicked.
    public void OnRunCodeClicked()
    {
        // Clear all the old cubes from the scene.
        gridManager.ClearGrid();

        // Get the code from the editor.
        string playerCode = codeEditor.text;

        // Execute the player's code to define their functions (like PlaceVoxel).
        bool success = scriptingEngine.ExecuteScript(playerCode);

        // If the code had an error, stop here. The error is already logged to the console.
        if (!success)
        {
            return;
        }

        // Loop through every coordinate in our 10x10x10 grid.
        // We'll center the grid around (0,0,0) by looping from -5 to 4.
        int start = -Mathf.FloorToInt(gridSize / 2.0f);
        int end = Mathf.CeilToInt(gridSize / 2.0f);

        for (int x = start; x < end; x++)
        {
            for (int y = start; y < end; y++)
            {
                for (int z = start; z < end; z++)
                {
                    // Ask the Lua script what kind of block to place at this (x, y, z).
                    int blockType = scriptingEngine.CallVoxelFunction("PlaceVoxel", x, y, z);

                    // If the block type is not 0 (empty), then place a cube.
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

    // A helper function to convert the number from Lua into a Unity Color.
    private Color GetColorFromType(int type)
    {
        switch (type)
        {
            case 1: return Color.red;
            case 2: return Color.green;
            case 3: return Color.blue;
            case 4: return Color.yellow;
            default: return Color.white; // Default color for any other number
        }
    }

    public void LoadLevel(int levelIndex)
    {
        // Sprawdzamy, czy mamy taki poziom
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Próba wczytania nieistniej¹cego poziomu!");
            return;
        }

        currentLevelIndex = levelIndex;
        LevelData levelToLoad = levels[currentLevelIndex];

        // 1. Wyczyœæ siatkê referencyjn¹ i zbuduj nowy kszta³t na podstawie danych z pliku
        referenceGridManager.ClearGrid();
        foreach (VoxelInfo voxel in levelToLoad.referenceShape)
        {
            Color targetColor = GetColorFromType(voxel.blockType);
            referenceGridManager.SetCube(voxel.position, true, targetColor);
        }

        // 2. Ustaw kod startowy w edytorze
        codeEditor.text = levelToLoad.startingCodeHint;

        // 3. Wyczyœæ siatkê gracza i zresetuj wynik
        gridManager.ClearGrid();
        CompareGrids(); // Wywo³ujemy, aby zaktualizowaæ UI do stanu pocz¹tkowego
    }

    public void LoadNextLevel()
    {
        // Przechodzimy do nastêpnego indeksu, a operator % zapewnia zapêtlenie listy
        int nextIndex = (currentLevelIndex + 1) % levels.Count;
        LoadLevel(nextIndex);
    }

    // Publiczna metoda do prze³adowania obecnego poziomu
    public void ReloadCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    void CompareGrids()
    {
        int correctCubes = 0;      // Poprawne kostki (pozycja + kolor)
        int wrongColorCubes = 0;   // Kostki na dobrej pozycji, ale z³y kolor
        int missingCubes = 0;      // Brakuj¹ce kostki
        int extraCubes = 0;        // Dodatkowe, niepotrzebne kostki
        int totalReferenceCubes = 0;

        // --- NOWA MATEMATYKA DLA SIATKI ---
        // To pozwala na obs³ugê liczb nieparzystych (np. 9)
        // Dla 9: start = -4, end = 5. Pêtla idzie od -4 do 4 (9 kroków).
        int start = -Mathf.FloorToInt(gridSize / 2.0f);
        int end = Mathf.CeilToInt(gridSize / 2.0f);

        for (int x = start; x < end; x++)
        {
            for (int y = start; y < end; y++)
            {
                for (int z = start; z < end; z++)
                {
                    // Reszta kodu wewn¹trz pêtli pozostaje BEZ ZMIAN
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
        if (totalReferenceCubes > 0)
        {
            matchPercentage = ((float)correctCubes / totalReferenceCubes) * 100f;
        }
        else if (extraCubes == 0) // Jeœli celem jest pustka i gracz nic nie zbudowa³
        {
            matchPercentage = 100f;
        }

        // --- POPRAWIONA LOGIKA WYŒWIETLANIA ---
        // Sprawdzamy, czy wszystkie trzy warunki idealnego dopasowania s¹ spe³nione.
        if (matchPercentage >= 100f && extraCubes == 0 && wrongColorCubes == 0)
        {
            scoreText.color = Color.green;
            scoreText.text = "IDEALNIE!";
        }
        else
        {
            // W KA¯DYM INNYM przypadku, pokazujemy pe³ne statystyki.
            scoreText.color = Color.white;
            scoreText.text = $"Dopasowanie: {matchPercentage:F1}%\n" + // Nowa linia dla czytelnoœci
                             $"<color=red>B³êdny kolor: {wrongColorCubes}</color> | " +
                             $"<color=yellow>Brakuj¹ce: {missingCubes}</color> | " +
                             $"<color=grey>Dodatkowe: {extraCubes}</color>";
        }
    }

}