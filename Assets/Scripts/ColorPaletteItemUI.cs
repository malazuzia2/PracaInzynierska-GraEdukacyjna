using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPaletteItemUI : MonoBehaviour
{
    // Publiczne referencje, które przypiszemy w Inspektorze
    public Image colorSwatchImage;
    public TMP_Text colorInfoText;

    // Metoda do ustawiania danych z zewn¹trz
    public void SetData(Color color, int blockType, string colorName)
    {
        colorSwatchImage.color = color;
        colorInfoText.text = $"{blockType}";
    }
}