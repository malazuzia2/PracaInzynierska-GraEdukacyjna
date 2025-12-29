using UnityEngine;
using System.Collections.Generic;

public class CubeGridManager : MonoBehaviour
{
    private struct CubeInfo
    {
        public GameObject gameObject;
        public Color color;
    }

    public GameObject cubePrefab;
    public int targetLayer;

    private Dictionary<Vector3Int, CubeInfo> cubeGrid;

    private MaterialPropertyBlock propertyBlock;
    private static readonly int ColorID = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
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
                existingCubeInfo.color = color;
                Renderer cubeRenderer = existingCubeInfo.gameObject.GetComponent<Renderer>();
                propertyBlock.SetColor(ColorID, color);
                cubeRenderer.SetPropertyBlock(propertyBlock);
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

 
    public bool TryGetCubeColor(Vector3Int position, out Color color)
    {
        if (cubeGrid.TryGetValue(position, out CubeInfo cubeInfo))
        {
            color = cubeInfo.color;
            return true;
        }

        color = Color.clear;
        return false;
    }
}