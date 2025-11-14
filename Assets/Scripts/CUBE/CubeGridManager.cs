using UnityEngine;
using System.Collections.Generic;

public class CubeGridManager : MonoBehaviour
{
    public GameObject cubePrefab;
    public int targetLayer;
    private Dictionary<Vector3Int, GameObject> cubeGrid;

    void Awake()
    {
        cubeGrid = new Dictionary<Vector3Int, GameObject>();
    }

    // Usuniêto ca³¹ metodê Start(), poniewa¿ GameController
    // jest teraz odpowiedzialny za inicjalizacjê siatek.

    public void SetCube(Vector3Int position, bool active, Color color)
    {
        if (cubeGrid.TryGetValue(position, out GameObject existingCube))
        {
            if (!active)
            {
                Destroy(existingCube);
                cubeGrid.Remove(position);
            }
            else
            {
                existingCube.GetComponent<Renderer>().material.color = color;
            }
        }
        else
        {
            if (active)
            {
                GameObject newCube = Instantiate(cubePrefab, (Vector3)position, Quaternion.identity, this.transform);
                newCube.name = $"Cube ({position.x}, {position.y}, {position.z})";
                newCube.layer = targetLayer;

                newCube.GetComponent<Renderer>().material.color = color;
                cubeGrid[position] = newCube;
            }
        }
    }

    public void ClearGrid()
    {
        foreach (var cube in cubeGrid.Values)
        {
            Destroy(cube);
        }
        cubeGrid.Clear();
    }

    public bool TryGetCubeColor(Vector3Int position, out Color color)
    {
        if (cubeGrid.TryGetValue(position, out GameObject cube))
        {
            color = cube.GetComponent<Renderer>().material.color;
            return true;
        }
        color = Color.clear;
        return false;
    }
}