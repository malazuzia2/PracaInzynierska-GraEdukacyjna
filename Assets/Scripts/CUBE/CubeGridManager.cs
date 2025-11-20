using UnityEngine;
using System.Collections.Generic;

public class CubeGridManager : MonoBehaviour
{
    // Ma³a struktura do przechowywania informacji o pojedynczej kostce
    private struct CubeInfo
    {
        public GameObject gameObject;
        public Color color;
    }

    public GameObject cubePrefab;
    public int targetLayer;

    // Zmieniamy s³ownik, aby przechowywa³ nasz¹ now¹ strukturê CubeInfo
    private Dictionary<Vector3Int, CubeInfo> cubeGrid;

    private MaterialPropertyBlock propertyBlock;
    private static readonly int ColorID = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        // Inicjujemy nowy typ s³ownika
        cubeGrid = new Dictionary<Vector3Int, CubeInfo>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetCube(Vector3Int position, bool active, Color color)
    {
        if (cubeGrid.TryGetValue(position, out CubeInfo existingCubeInfo))
        {
            if (!active)
            {
                Destroy(existingCubeInfo.gameObject);
                cubeGrid.Remove(position);
            }
            else
            {
                // Aktualizujemy zarówno kolor w naszej strukturze, jak i na ekranie
                existingCubeInfo.color = color;
                Renderer cubeRenderer = existingCubeInfo.gameObject.GetComponent<Renderer>();
                propertyBlock.SetColor(ColorID, color);
                cubeRenderer.SetPropertyBlock(propertyBlock);
                // Zapisujemy zaktualizowan¹ strukturê z powrotem do s³ownika
                cubeGrid[position] = existingCubeInfo;
            }
        }
        else
        {
            if (active)
            {
                GameObject newCubeGO = Instantiate(cubePrefab, (Vector3)position, Quaternion.identity, this.transform);
                newCubeGO.name = $"Cube ({position.x}, {position.y}, {position.z})";
                newCubeGO.layer = targetLayer;

                Renderer cubeRenderer = newCubeGO.GetComponent<Renderer>();
                propertyBlock.SetColor(ColorID, color);
                cubeRenderer.SetPropertyBlock(propertyBlock);

                // Tworzymy now¹ strukturê CubeInfo i dodajemy j¹ do s³ownika
                CubeInfo newCubeInfo = new CubeInfo { gameObject = newCubeGO, color = color };
                cubeGrid[position] = newCubeInfo;
            }
        }
    }

    public void ClearGrid()
    {
        foreach (var cubeInfo in cubeGrid.Values)
        {
            Destroy(cubeInfo.gameObject);
        }
        cubeGrid.Clear();
    }

    // TA METODA JEST TERAZ POPRAWNA!
    // Odczytuje kolor z naszej zapisanej struktury, a nie z renderera.
    public bool TryGetCubeColor(Vector3Int position, out Color color)
    {
        if (cubeGrid.TryGetValue(position, out CubeInfo cubeInfo))
        {
            // Zwracamy kolor, który sami zapisaliœmy. To jest gwarantowanie poprawne.
            color = cubeInfo.color;
            return true;
        }

        color = Color.clear;
        return false;
    }
}