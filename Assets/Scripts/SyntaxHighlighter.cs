using UnityEngine;
using TMPro;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq; // Potrzebne do string.Join

public class SyntaxHighlighter : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text displayField;

    [Header("Syntax Colors")]
    public Color keywordColor = new Color(0.34f, 0.61f, 0.84f);
    public Color commentColor = new Color(0.4f, 0.63f, 0.35f);
    public Color stringColor = new Color(0.8f, 0.5f, 0.33f);
    public Color numberColor = new Color(0.7f, 0.8f, 0.6f);
    public Color functionNameColor = new Color(0.86f, 0.86f, 0.6f);

    private readonly string[] keywords = { "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "if", "in", "local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while" };

    private Regex syntaxRegex;

    void Start()
    {
        string keywordPattern = $"\\b({string.Join("|", keywords)})\\b";
        string pattern =
            $"(?<comment>--.*)|" +         // Grupa "comment"
            $"(?<string>\"[^\"]*\")|" +   // Grupa "string"
            $"(?<function>{keywordPattern})|" + // Grupa "function" (dla s³owa kluczowego "function")
            $"(?<keyword>{keywordPattern})|" + // Grupa "keyword" (dla reszty s³ów kluczowych)
            $"(?<number>\\b\\d+\\.?\\d*\\b)|" + // Grupa "number" (z obs³ug¹ liczb zmiennoprzecinkowych)
            $"(?<funcname>(?<=function\\s+)[a-zA-Z_][a-zA-Z0-9_]*)"; // Grupa "funcname" (nazwa funkcji)

        syntaxRegex = new Regex(pattern, RegexOptions.ExplicitCapture);

        inputField.onValueChanged.AddListener(HighlightSyntax);

        if (!string.IsNullOrEmpty(inputField.text))
        {
            HighlightSyntax(inputField.text);
        }
    }

    void HighlightSyntax(string text)
    {
        string highlightedText = syntaxRegex.Replace(text, MatchEvaluator);

        displayField.text = highlightedText + "<color=#00000000>.</color>";
    }

    private string MatchEvaluator(Match match)
    {
        if (match.Groups["comment"].Success)
            return $"<color=#{ColorUtility.ToHtmlStringRGB(commentColor)}>{match.Value}</color>";
        if (match.Groups["string"].Success)
            return $"<color=#{ColorUtility.ToHtmlStringRGB(stringColor)}>{match.Value}</color>";
        if (match.Groups["function"].Value == "function")
            return $"<color=#{ColorUtility.ToHtmlStringRGB(keywordColor)}>{match.Value}</color>";
        if (match.Groups["keyword"].Success)
            return $"<color=#{ColorUtility.ToHtmlStringRGB(keywordColor)}>{match.Value}</color>";
        if (match.Groups["number"].Success)
            return $"<color=#{ColorUtility.ToHtmlStringRGB(numberColor)}>{match.Value}</color>";
        if (match.Groups["funcname"].Success)
            return $"<color=#{ColorUtility.ToHtmlStringRGB(functionNameColor)}>{match.Value}</color>";

        return match.Value;
    }
}