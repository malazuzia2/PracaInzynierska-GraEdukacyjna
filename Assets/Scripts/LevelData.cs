using UnityEngine;
using System.Collections.Generic;
 
[System.Serializable]
public struct VoxelInfo
{
    public Vector3Int position;
    public int blockType;
}

[CreateAssetMenu(fileName = "New Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    [TextArea(10, 20)]  
    public string startingCodeHint;
    [TextArea(10, 20)]
    public string solutionCode;
    public List<VoxelInfo> referenceShape;
}