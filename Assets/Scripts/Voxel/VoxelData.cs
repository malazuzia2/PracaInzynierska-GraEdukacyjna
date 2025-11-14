using UnityEngine; 
 
public struct VoxelData
{
    public Vector3Int position; // Grid coordinates (x, y, z)
    public bool isActive;       // True if the voxel exists, false if empty
    public Color color;         // The color of this voxel

    // Constructor to easily create VoxelData objects
    public VoxelData(Vector3Int pos, bool active, Color col)
    {
        position = pos;
        isActive = active;
        color = col;
    }
}