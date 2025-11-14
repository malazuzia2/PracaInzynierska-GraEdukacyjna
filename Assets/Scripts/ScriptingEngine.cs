using UnityEngine;
using NLua; // We need this to use NLua's features

public class ScriptingEngine : MonoBehaviour
{
    private Lua lua; // This is our Lua "virtual machine"

    void Awake()
    {
        // Initialize the Lua state when the game starts
        lua = new Lua();
        // This line allows Lua to access C# public methods/types. It's powerful but be aware of security.
        lua.LoadCLRPackage();
    }

    /// <summary>
    /// Executes a string of Lua code. This is used to define the player's functions.
    /// </summary>
    /// <param name="code">The Lua code written by the player.</param>
    /// <returns>True if the code executed without errors, false otherwise.</returns>
    public bool ExecuteScript(string code)
    {
        try
        {
            lua.DoString(code);
            return true;
        }
        catch (NLua.Exceptions.LuaException e)
        {
            // If there's an error in the Lua code, log it to the Unity console for debugging.
            Debug.LogError("Lua Error: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Calls a specific Lua function to determine which block type to place at a given coordinate.
    /// </summary>
    /// <param name="functionName">The name of the Lua function to call.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <returns>An integer representing the block type (e.g., 0=empty, 1=red, etc.).</returns>
    public int CallVoxelFunction(string functionName, int x, int y, int z)
    {
        try
        {
            LuaFunction func = lua.GetFunction(functionName);
            if (func == null)
            {
                // The player hasn't defined the function, so we return 0 (empty).
                return 0;
            }

            // Call the function with our x, y, z arguments.
            object[] result = func.Call(x, y, z);

            // If the function returns a value, convert it to an integer.
            if (result != null && result.Length > 0 && result[0] != null)
            {
                // Lua numbers are doubles by default, so we cast to long first, then to int.
                return (int)(long)result[0];
            }
        }
        catch (NLua.Exceptions.LuaException e)
        {
            Debug.LogError($"Error calling Lua function '{functionName}': {e.Message}");
        }
        // If anything goes wrong or the function returns nothing, return 0 (empty).
        return 0;
    }
}