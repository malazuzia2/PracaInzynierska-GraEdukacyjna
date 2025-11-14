using UnityEngine;
using System.Collections.Generic;

// Definicja pojedynczej kostki w naszym poziomie
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
    [TextArea(10, 20)] // Daje wiêksze pole w Inspektorze
    public string startingCodeHint;
    public List<VoxelInfo> referenceShape;
}