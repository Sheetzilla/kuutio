using TMPro;
using UnityEngine;

public class TimeInputFilter : MonoBehaviour
{
    private TMP_InputField inputField;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(FilterInput);
    }

    void FilterInput(string text)
    {
        string filtered = "";

        foreach (char c in text)
        {
            if (char.IsDigit(c) || c == ':')
                filtered += c;
        }

        if (text != filtered)
        {
            inputField.text = filtered;
            inputField.caretPosition = filtered.Length;
        }
    }
}