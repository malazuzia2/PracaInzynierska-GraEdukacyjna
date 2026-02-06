using UnityEngine;
using TMPro;
using System.Collections;  

public class LayoutSynchronizer : MonoBehaviour
{
    public RectTransform sourceTextRect;
    public RectTransform targetInputRect;
    public RectTransform contentRect;

    private TMP_Text sourceTextComponent;
    private bool isDirty = false;  

    void Awake()
    {
        if (sourceTextRect != null)
        {
            sourceTextComponent = sourceTextRect.GetComponent<TMP_Text>();
        }
    }
     
    void OnEnable()
    { 
        var inputField = targetInputRect.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(MarkAsDirty);
        }
    }
     
    void OnDisable()
    {
        var inputField = targetInputRect.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(MarkAsDirty);
        }
    }
     
    private void MarkAsDirty(string text)
    {
        isDirty = true;
    }

    void LateUpdate()
    { 
        if (isDirty)
        { 
            StartCoroutine(UpdateLayoutCoroutine());
            isDirty = false;  
        }
    }
     
    private IEnumerator UpdateLayoutCoroutine()
    { 
        yield return new WaitForEndOfFrame();

        if (sourceTextComponent == null || targetInputRect == null || contentRect == null)
        {
            yield break; 
        }

        float preferredHeight = sourceTextComponent.preferredHeight;

        sourceTextRect.sizeDelta = new Vector2(sourceTextRect.sizeDelta.x, preferredHeight);
        targetInputRect.sizeDelta = new Vector2(targetInputRect.sizeDelta.x, preferredHeight);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, preferredHeight);
    }
}