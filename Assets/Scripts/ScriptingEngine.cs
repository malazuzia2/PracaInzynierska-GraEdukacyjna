using UnityEngine;
using NLua;

public class ScriptingEngine : MonoBehaviour
{
    private Lua lua; // Nasza "wirtualna maszyna" Lua

    void Awake()
    {
        // Inicjalizujemy stan Lua na starcie
        lua = new Lua();
        // Ta linia pozwala Lua na dostêp do publicznych metod i typów C#.
        lua.LoadCLRPackage();
    }

    /// <summary>
    /// Wykonuje ci¹g znaków jako kod Lua, aby zdefiniowaæ funkcje gracza.
    /// </summary>
    /// <returns>True, jeœli kod wykona³ siê bez b³êdów, w przeciwnym razie false.</returns>
    public bool ExecuteScript(string code)
    {
        try
        {
            lua.DoString(code);
            return true;
        }
        catch (NLua.Exceptions.LuaException e)
        {
            // Jeœli jest b³¹d w kodzie Lua, logujemy go do konsoli Unity.
            // TODO: Wyœwietl ten b³¹d w UI, aby gracz go zobaczy³.
            Debug.LogError("Lua Error: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Wywo³uje konkretn¹ funkcjê Lua, aby okreœliæ typ bloku dla danych wspó³rzêdnych.
    /// </summary>
    /// <returns>Liczba ca³kowita reprezentuj¹ca typ bloku (np. 0=pusty, 1=czerwony, itd.).</returns>
    public int CallVoxelFunction(string functionName, int x, int y, int z)
    {
        try
        {
            LuaFunction func = lua.GetFunction(functionName);
            if (func == null)
            {
                // Gracz nie zdefiniowa³ funkcji, wiêc zwracamy 0 (pusty).
                return 0;
            }

            // Wywo³ujemy funkcjê z argumentami x, y, z.
            object[] result = func.Call(x, y, z);

            // Jeœli funkcja zwróci³a wartoœæ, konwertujemy j¹ na int.
            if (result != null && result.Length > 0 && result[0] != null)
            {
                // Liczby w Lua to domyœlnie double, wiêc rzutujemy najpierw na long, potem na int.
                return (int)(long)result[0];
            }
        }
        catch (NLua.Exceptions.LuaException e)
        {
            Debug.LogError($"Error calling Lua function '{functionName}': {e.Message}");
        }
        // Jeœli coœ pójdzie nie tak lub funkcja nic nie zwróci, zwracamy 0 (pusty).
        return 0;
    }
}