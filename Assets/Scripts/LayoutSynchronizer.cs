using UnityEngine;
using TMPro;
using System.Collections; // Potrzebne do Coroutines

public class LayoutSynchronizer : MonoBehaviour
{
    public RectTransform sourceTextRect;
    public RectTransform targetInputRect;
    public RectTransform contentRect;

    private TMP_Text sourceTextComponent;
    private bool isDirty = false; // Flaga, która mówi nam, czy trzeba zaktualizowaæ layout

    void Awake()
    {
        if (sourceTextRect != null)
        {
            sourceTextComponent = sourceTextRect.GetComponent<TMP_Text>();
        }
    }

    // Ta metoda jest wywo³ywana, gdy skrypt jest w³¹czany
    void OnEnable()
    {
        // Nas³uchujemy na zdarzenie zmiany tekstu bezpoœrednio z pola InputField
        // (Zak³adaj¹c, ¿e SyntaxHighlighter te¿ nas³uchuje na to samo)
        var inputField = targetInputRect.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(MarkAsDirty);
        }
    }

    // Ta metoda jest wywo³ywana, gdy skrypt jest wy³¹czany
    void OnDisable()
    {
        var inputField = targetInputRect.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(MarkAsDirty);
        }
    }

    // Gdy tekst siê zmienia, ustawiamy flagê "isDirty" na true
    private void MarkAsDirty(string text)
    {
        isDirty = true;
    }

    void LateUpdate()
    {
        // Jeœli layout jest "brudny" (wymaga aktualizacji)
        if (isDirty)
        {
            // Uruchamiamy Coroutine, która wykona siê na koniec tej klatki
            StartCoroutine(UpdateLayoutCoroutine());
            isDirty = false; // Resetujemy flagê
        }
    }

    // Coroutine do aktualizacji layoutu
    private IEnumerator UpdateLayoutCoroutine()
    {
        // Czekamy na sam koniec klatki, PO tym jak UI zakoñczy wszystkie swoje operacje
        yield return new WaitForEndOfFrame();

        if (sourceTextComponent == null || targetInputRect == null || contentRect == null)
        {
            yield break; // Zakoñcz, jeœli coœ nie jest przypisane
        }

        float preferredHeight = sourceTextComponent.preferredHeight;

        sourceTextRect.sizeDelta = new Vector2(sourceTextRect.sizeDelta.x, preferredHeight);
        targetInputRect.sizeDelta = new Vector2(targetInputRect.sizeDelta.x, preferredHeight);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, preferredHeight);
    }
}