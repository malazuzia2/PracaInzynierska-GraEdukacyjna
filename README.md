# Educational Game for Learning Procedural Programming

> Write code. Watch it grow in 3D.

This app is an interactive educational game built in Unity where players learn the fundamentals of procedural programming by instantly building three-dimensional voxel structures. Instead of reading theory, the player writes code in Lua — and immediately sees its effect in 3D space.

Developed as a Bachelor's thesis project at Lodz University of Technology.

---

## Preview

![Gameplay](https://github.com/malazuzia2/PracaInzynierska-GraEdukacyjna/blob/main/screenshots/GamePreview.png)

## How It Works

Gameplay is built around a four-step loop:

1. **Reference** — the player examines a target 3D object made of cubes, rotating it freely with an interactive camera
2. **Coding** — the player implements the `PlaceVoxel(x, y, z)` function in the Lua editor
3. **Execution** — clicking `Run Code` instantly generates a 3D structure based on the written code
4. **Evaluation** — the system compares the player's result to the reference and returns detailed feedback

The cycle repeats until 100% match is achieved or the player moves to the next level.

---

## Architecture

```
GameController
├── ScriptingEngine          ← C# ↔ Lua bridge (NLua)
│   ├── ExecuteScript()      ← lua.DoString() wrapped in try-catch
│   └── CallVoxelFunction()  ← iterative calls to PlaceVoxel(x, y, z)
│
├── CubeGridManager (×2)     ← player grid + reference grid
│   ├── SetCube()            ← Instantiate / Destroy / color update
│   ├── ClearGrid()          ← reset before each new execution
│   └── TryGetCubeColor()    ← O(1) lookup via Dictionary<Vector3Int, CubeInfo>
│
├── LevelManager             ← loads LevelData assets (Scriptable Objects)
└── CompareGrids()           ← scoring: correct / missing / extra / wrongColor
```

**Flyweight pattern** — a single cube prefab serves as a template for every block in the grid.  
**MaterialPropertyBlock** — per-block color changes without instantiating new materials.  
**Dictionary\<Vector3Int\>** — O(1) position lookups during grid comparison.

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── GameController.cs        ← game logic coordinator
│   ├── ScriptingEngine.cs       ← NLua integration, Lua sandbox
│   ├── CubeGridManager.cs       ← 3D grid management
│   ├── LevelManager.cs          ← level system
│   └── SyntaxHighlighter.cs     ← Lua syntax highlighting (Regex + Rich Text)
├── LevelData/                   ← LevelData asset files
├── Prefabs/
│   └── Cube_pref.prefab              ← single cube prefab (Flyweight)
└── Scenes/
    └── SampleScene.unity
```

---

## Getting Started

### Requirements

| Tool | Version |
|---|---|
| Unity | `6000.2.7f2` (Unity 6) |
| NLua | latest (via Package Manager) |
| TextMeshPro | built into Unity |
| OS | Windows / macOS / Linux |

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/malazuzia2/PracaInzynierska-GraEdukacyjna.git
   ```

2. Open the project in **Unity Hub** — select version `6000.2.7f2`

3. Unity will automatically import all dependencies (NLua, TextMeshPro)

4. Open the scene `Assets/Scenes/SampleScene.unity`

5. Press **Play** 

### Adding Custom Levels

New levels can be created without touching any source code:

```
Create → Game → Level Data
```

Fill in the fields in the Inspector:
- **Level Name** — display name shown in-game
- **Starting Code Hint** — task description + starter code shown in the editor
- **Solution Code** — reference Lua code that generates the correct shape
- Unity automatically pre-calculates `Reference Grid Data` from `Solution Code`

---

## Author

**Zuzanna Lewandowska** — Bachelor's thesis, Lodz University of Technology, Institute of Information Technology  

---

## License

This project was created for educational and research purposes as a diploma thesis.
