using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DocumentationController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text contentText; // Jedno pole na ca³¹ treœæ
    public Button overviewButton;
    public Button geometryButton;
    public Button codeButton;

    [Header("Button Colors")]
    public Color activeTabColor = new Color(0.8f, 0.8f, 0.8f);
    public Color inactiveTabColor = Color.white;

    // Przechowujemy treœæ dokumentacji w zmiennych
    [TextArea(10, 20)]
    public string overviewInfo;
    [TextArea(10, 20)]
    public string geometryInfo;
    [TextArea(10, 20)]
    public string codeInfo;

    void Start()
    {
        // Pod³¹czamy funkcje do przycisków
        overviewButton.onClick.AddListener(ShowOverview);
        geometryButton.onClick.AddListener(ShowGeometry);
        codeButton.onClick.AddListener(ShowCode);

        // Poka¿ pierwsz¹ zak³adkê na starcie
        ShowOverview();
    }

    private void ResetButtonColors()
    {
        overviewButton.GetComponent<Image>().color = inactiveTabColor;
        geometryButton.GetComponent<Image>().color = inactiveTabColor;
        codeButton.GetComponent<Image>().color = inactiveTabColor;
    }

    public void ShowOverview()
    {
        contentText.text = overviewInfo;
        ResetButtonColors();
        overviewButton.GetComponent<Image>().color = activeTabColor;
    }

    public void ShowGeometry()
    {
        contentText.text = geometryInfo;
        ResetButtonColors();
        geometryButton.GetComponent<Image>().color = activeTabColor;
    }

    public void ShowCode()
    {
        contentText.text = codeInfo;
        ResetButtonColors();
        codeButton.GetComponent<Image>().color = activeTabColor;
    }
}