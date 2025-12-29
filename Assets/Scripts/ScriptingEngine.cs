using UnityEngine;
using NLua;

public class ScriptingEngine : MonoBehaviour
{
    private Lua lua; 

    void Awake()
    { 
        lua = new Lua(); 
        lua.LoadCLRPackage();
    }
     
    public string ExecuteScript(string code)
    {
        try
        {
            lua.DoString(code);
            return null;
        }
        catch (NLua.Exceptions.LuaException e)
        {
            Debug.LogError("Lua Error: " + e.Message);
            return e.Message;
        }
    }

   public int CallVoxelFunction(string functionName, int x, int y, int z)
    {
        try
        {
            LuaFunction func = lua.GetFunction(functionName);
            if (func == null)
            { 
                return 0;
            }
             
            object[] result = func.Call(x, y, z);
             
            if (result != null && result.Length > 0 && result[0] != null)
            {
                 return (int)(long)result[0];
            }
        }
        catch (NLua.Exceptions.LuaException e)
        {
            Debug.LogError($"Error calling Lua function '{functionName}': {e.Message}");
        }
         return 0;
    }
}