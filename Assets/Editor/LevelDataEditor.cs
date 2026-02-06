using UnityEngine;
using UnityEditor;
using NLua;
using System.Collections.Generic;


[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelData levelData = (LevelData)target;

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Shape", GUILayout.Height(40)))
        {
            GenerateShape(levelData);
        }
    }

    private void GenerateShape(LevelData levelData)
    {
        Lua lua = new Lua();
        try
        {
            lua.LoadCLRPackage();
            lua.DoString(levelData.solutionCode); 

            LuaFunction placeVoxelFunc = lua.GetFunction("PlaceVoxel");

            if (levelData.referenceShape == null)
                levelData.referenceShape = new List<VoxelInfo>();

            levelData.referenceShape.Clear();
             
            int range = 4;
            int voxelCount = 0;

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    for (int z = -range; z <= range; z++)
                    {
                        object[] result = placeVoxelFunc.Call(x, y, z);

                        if (result != null && result.Length > 0 && result[0] != null)
                        {
                            int blockType = (int)(long)result[0];

                            if (blockType != 0)
                            {
                                levelData.referenceShape.Add(new VoxelInfo
                                {
                                    position = new Vector3Int(x, y, z),
                                    blockType = blockType
                                });
                                voxelCount++;
                            }
                        }
                    }
                }
            }

            Debug.Log($"Voxels created: {voxelCount}");

            EditorUtility.SetDirty(levelData);
        }
        catch (NLua.Exceptions.LuaException e)
        {
            Debug.LogError("Lua Error during generation: " + e.Message);
        }
        finally
        {
            lua.Dispose();  
        }
    }
}