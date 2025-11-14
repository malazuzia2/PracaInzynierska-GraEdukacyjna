using UnityEngine;
using System.Collections.Generic;

// This static class provides methods to generate mesh data for voxels.
public static class VoxelMeshGenerator
{
    // Enum to represent the 6 faces of a cube
    public enum FaceDirection { Top, Bottom, Front, Back, Left, Right }

    // Vertices for a single QUAD (face), assuming it lies on the XY plane, centered at (0,0,0)
    // and facing +Z. We'll rotate and offset these for each face direction.
    // Order: Bottom-Left (0), Bottom-Right (1), Top-Right (2), Top-Left (3) when looking at the quad's +Z side
    private static readonly Vector3[] quadVertices = new Vector3[]
    {
        new Vector3(-0.5f, -0.5f, 0f), // 0: Bottom-Left
        new Vector3( 0.5f, -0.5f, 0f), // 1: Bottom-Right
        new Vector3( 0.5f,  0.5f, 0f), // 2: Top-Right
        new Vector3(-0.5f,  0.5f, 0f)  // 3: Top-Left
    };

    // Define triangle indices for each face direction, ensuring correct winding order (counter-clockwise)
    // when viewed from the *outside* of the cube.
    // The quadVertices are defined such that {0,1,2,0,2,3} is CCW when looking at its +Z side.
    // We apply rotations to this base quad, then select the appropriate triangle winding.
    private static readonly int[][] faceTriangles = new int[][]
    {
        // For FaceDirection.Top (rotated so original +Z becomes +Y):
        // Vertices 0,1,2,3 become Top-Left-Front, Top-Right-Front, Top-Right-Back, Top-Left-Back
        // {0, 3, 2, 0, 2, 1} is CCW when looking down from +Y.
        new int[] { 0, 3, 2, 0, 2, 1 }, // Top

        // For FaceDirection.Bottom (rotated so original +Z becomes -Y):
        // {0, 1, 2, 0, 2, 3} is CCW when looking up from -Y.
        new int[] { 0, 1, 2, 0, 2, 3 }, // Bottom

        // For FaceDirection.Front (original +Z, no extra rotation):
        // {0, 1, 2, 0, 2, 3} is CCW when looking at +Z.
        new int[] { 0, 1, 2, 0, 2, 3 }, // Front

        // For FaceDirection.Back (rotated 180 around Y, original +Z becomes -Z):
        // {0, 3, 2, 0, 2, 1} is CCW when looking at -Z.
        new int[] { 0, 3, 2, 0, 2, 1 }, // Back

        // For FaceDirection.Left (rotated -90 around Y, original +Z becomes -X):
        // {0, 3, 2, 0, 2, 1} is CCW when looking at -X.
        new int[] { 0, 3, 2, 0, 2, 1 }, // Left

        // For FaceDirection.Right (rotated 90 around Y, original +Z becomes +X):
        // {0, 1, 2, 0, 2, 3} is CCW when looking at +X.
        new int[] { 0, 1, 2, 0, 2, 3 }  // Right
    };


    // Normals for each face direction. Used to offset the quad from the center of the voxel.
    private static readonly Vector3[] faceNormals = new Vector3[]
    {
        Vector3.up,     // Top (+Y)
        Vector3.down,   // Bottom (-Y)
        Vector3.forward,// Front (+Z)
        Vector3.back,   // Back (-Z)
        Vector3.left,   // Left (-X)
        Vector3.right   // Right (+X)
    };

    // This method adds the vertices, triangles, and colors for a single face
    // to the provided mesh data lists.
    public static void AddFace(FaceDirection direction, Vector3 voxelOffset, Color faceColor,
                               List<Vector3> allVertices, List<int> allTriangles, List<Color> allColors)
    {
        // Store the current vertex count. This will be the starting index
        // for the 4 new vertices we're about to add for this face.
        int currentVertexCount = allVertices.Count;

        // Determine the transformation (rotation) needed for the quad to align with the face direction
        Quaternion rotation = Quaternion.identity; // Default for front (+Z) face (no rotation needed if quad is on XY and facing Z)

        switch (direction)
        {
            case FaceDirection.Top:
                rotation = Quaternion.Euler(90, 0, 0); // Rotate quad 90 degrees around X to face +Y
                break;
            case FaceDirection.Bottom:
                rotation = Quaternion.Euler(-90, 0, 0); // Rotate quad -90 degrees around X to face -Y
                break;
            case FaceDirection.Front: // Already aligned with +Z, no extra rotation needed
                rotation = Quaternion.identity;
                break;
            case FaceDirection.Back:
                rotation = Quaternion.Euler(0, 180, 0); // Rotate quad 180 degrees around Y to face -Z
                break;
            case FaceDirection.Left:
                rotation = Quaternion.Euler(0, -90, 0); // Rotate quad -90 degrees around Y to face -X
                break;
            case FaceDirection.Right:
                rotation = Quaternion.Euler(0, 90, 0); // Rotate quad 90 degrees around Y to face +X
                break;
        }

        // Add the 4 vertices for this face
        for (int i = 0; i < quadVertices.Length; i++) // quadVertices.Length is 4
        {
            // 1. Rotate the base quad vertex to the correct face orientation
            Vector3 rotatedVertex = rotation * quadVertices[i];

            // 2. Offset the rotated vertex by half a unit along the face's normal direction.
            //    This moves the quad from the center of the voxel (where its Z=0 plane is)
            //    to the actual surface of the voxel.
            Vector3 finalVertex = rotatedVertex + faceNormals[(int)direction] * 0.5f;

            // 3. Apply the overall voxel's grid position offset
            finalVertex += voxelOffset;

            allVertices.Add(finalVertex);
            allColors.Add(faceColor); // Each vertex of the face gets the same color
        }

        // Add the triangle indices for this face.
        // We now use the specific triangle array for the current direction.
        int[] currentFaceTriangles = faceTriangles[(int)direction]; // <--- USE DIRECTION-SPECIFIC TRIANGLES

        for (int i = 0; i < currentFaceTriangles.Length; i++) // currentFaceTriangles.Length is 6 (for 2 triangles)
        {
            allTriangles.Add(currentVertexCount + currentFaceTriangles[i]);
        }
    }
}