using UnityEngine;
using System.Collections.Generic;

// Struct to hold individual voxel data
// (Ensure this struct is defined, often in its own file or at the top of VoxelGridManager)



// This script manages a 3D grid of voxels and generates a single mesh to represent them.
public class VoxelGridManager : MonoBehaviour
{
    // Public references to Unity components
    public MeshFilter meshFilter;       // Where the generated mesh will be stored
    public MeshRenderer meshRenderer;   // How the mesh will be rendered (needs a material)
    public Material voxelMaterial;      // The material to use for rendering voxels

    // Private internal data
    private Dictionary<Vector3Int, VoxelData> voxelGrid; // Stores our active voxels by position
    private Mesh mesh; // The actual mesh object we'll manipulate

    // --- Unity Lifecycle Methods ---

    void Awake()
    {
        Debug.Log("VoxelGridManager: Awake called on " + gameObject.name);

        // Ensure we have a MeshFilter and MeshRenderer
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                Debug.Log("VoxelGridManager: Added MeshFilter to " + gameObject.name);
            }
        }
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                Debug.Log("VoxelGridManager: Added MeshRenderer to " + gameObject.name);
            }
        }

        // Initialize our voxel grid data structure
        voxelGrid = new Dictionary<Vector3Int, VoxelData>();

        // Create a new Mesh object to store our voxel geometry
        mesh = new Mesh();
        meshFilter.mesh = mesh; // <--- THIS IS KEY: Assigns OUR new mesh
        Debug.Log("VoxelGridManager: New Mesh created and assigned to MeshFilter. Current meshFilter.mesh: " + meshFilter.mesh.name);


        // Assign the material if it's set in the inspector
        if (voxelMaterial != null)
        {
            meshRenderer.material = voxelMaterial;
            Debug.Log("VoxelGridManager: Material assigned: " + voxelMaterial.name);
        }
        else
        {
            Debug.LogWarning("VoxelGridManager: voxelMaterial is not assigned in the Inspector!");
        }
    }


    void Start()
    {
        Debug.Log("VoxelGridManager: Start called on " + gameObject.name);

        // --- FOR TESTING ONLY: Place a single voxel ---
        // Let's place a red voxel at grid position (0, 0, 0)
        SetVoxel(new Vector3Int(0, 0, 0), true, Color.red);
        // And a green one at (1, 0, 0)
        SetVoxel(new Vector3Int(1, 0, 0), true, Color.green);
        // And a blue one at (0, 1, 0)
        SetVoxel(new Vector3Int(0, 1, 0), true, Color.blue);
        // --- END TEST ---
        Debug.Log("VoxelGridManager: About to call GenerateMesh(). Voxel count: " + voxelGrid.Count);
        GenerateMesh();
        Debug.Log("VoxelGridManager: GenerateMesh() finished. Current meshFilter.mesh vertex count: " + (meshFilter.mesh != null ? meshFilter.mesh.vertexCount.ToString() : "null"));

    }

    // --- Public Methods for Grid Management ---

    // Adds or updates a voxel in the grid.
    public void SetVoxel(Vector3Int position, bool active, Color color)
    {
        if (active)
        {
            // If voxel should be active, add/update it
            voxelGrid[position] = new VoxelData(position, true, color);
        }
        else
        {
            // If voxel should be inactive, remove it from the grid if it exists
            if (voxelGrid.ContainsKey(position))
            {
                voxelGrid.Remove(position);
            }
        }
    }

    // Gets the VoxelData at a specific position. Returns an inactive VoxelData if not found.
    public VoxelData GetVoxel(Vector3Int position)
    {
        if (voxelGrid.TryGetValue(position, out VoxelData data))
        {
            return data;
        }
        return new VoxelData(position, false, Color.clear); // Return an inactive voxel
    }

    // Clears all voxels from the grid.
    public void ClearGrid()
    {
        voxelGrid.Clear();
        GenerateMesh(); // Update the mesh to be empty
    }

    // Rebuilds the entire mesh based on the current voxelGrid data.
    public void GenerateMesh()
    {
        // Clear previous mesh data
        Debug.Log("VoxelGridManager: GenerateMesh() started.");
        // Clear previous mesh data
        if (mesh == null)
        {
            Debug.LogError("GenerateMesh called but 'mesh' object is null!");
            mesh = new Mesh(); // Re-create if somehow null
            meshFilter.mesh = mesh;
        }
        mesh.Clear(); // <--- THIS CLEARS ANY PREVIOUS MESH DATA (like "Cube")

        Debug.Log("VoxelGridManager: Mesh cleared.");
        // Lists to accumulate all vertices, triangles, and colors for the entire mesh
        List<Vector3> allVertices = new List<Vector3>();
        List<int> allTriangles = new List<int>();
        List<Color> allColors = new List<Color>();

        // Iterate through each active voxel in our grid
        foreach (var kvp in voxelGrid)
        {
            Vector3Int voxelPos = kvp.Key;
            VoxelData voxel = kvp.Value;

            // Calculate the world position offset for this voxel
            // Each voxel is a unit cube, centered at its integer coordinates.
            Vector3 worldOffset = new Vector3(voxelPos.x, voxelPos.y, voxelPos.z);

            Debug.Log($"Processing voxel at {voxelPos} with base color {voxel.color}");

            // --- Determine which faces to draw ---
            // We only draw a face if there isn't another active voxel next to it.
            // This is a basic form of 'culling' for efficiency.

            // Check Top face (+Y) - UNCOMMENTED!
            if (!GetVoxel(voxelPos + Vector3Int.up).isActive)
            {
                VoxelMeshGenerator.AddFace(VoxelMeshGenerator.FaceDirection.Top, worldOffset, voxel.color,
                                            allVertices, allTriangles, allColors);
                Debug.Log($"  Drawing TOP face for {voxelPos} with color {voxel.color}");
            }
            // Check Bottom face (-Y)
            if (!GetVoxel(voxelPos + Vector3Int.down).isActive)
            {
                VoxelMeshGenerator.AddFace(VoxelMeshGenerator.FaceDirection.Bottom, worldOffset, voxel.color,
                                            allVertices, allTriangles, allColors);
            }
            // Check Front face (+Z)
            if (!GetVoxel(voxelPos + Vector3Int.forward).isActive)
            {
                VoxelMeshGenerator.AddFace(VoxelMeshGenerator.FaceDirection.Front, worldOffset, voxel.color,
                                                            allVertices, allTriangles, allColors);
            }
            // Check Back face (-Z)
            if (!GetVoxel(voxelPos + Vector3Int.back).isActive)
            {
                VoxelMeshGenerator.AddFace(VoxelMeshGenerator.FaceDirection.Back, worldOffset, voxel.color,
                                            allVertices, allTriangles, allColors);
            }
            // Check Left face (-X)
            if (!GetVoxel(voxelPos + Vector3Int.left).isActive)
            {
                VoxelMeshGenerator.AddFace(VoxelMeshGenerator.FaceDirection.Left, worldOffset, voxel.color,
                                            allVertices, allTriangles, allColors);
            }
            // Check Right face (+X)
            if (!GetVoxel(voxelPos + Vector3Int.right).isActive)
            {
                VoxelMeshGenerator.AddFace(VoxelMeshGenerator.FaceDirection.Right, worldOffset, voxel.color,
                                            allVertices, allTriangles, allColors);
            }
        }

        // Assign all accumulated data to the mesh
        if (allVertices.Count > 0)
        {
            mesh.SetVertices(allVertices);
            mesh.SetTriangles(allTriangles, 0); // Subset 0
            mesh.SetColors(allColors);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            Debug.Log("VoxelGridManager: Mesh data assigned. Vertices: " + allVertices.Count + ", Triangles: " + allTriangles.Count / 3);
        }
        else
        {
            Debug.Log("VoxelGridManager: No vertices generated for mesh. Clearing mesh.");
            mesh.Clear(); // Ensure mesh is truly empty if no voxels
        }
        Debug.Log("VoxelGridManager: GenerateMesh() finished. Final mesh vertex count: " + mesh.vertexCount);
    }
}