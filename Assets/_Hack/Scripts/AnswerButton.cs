using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnswerButton : MonoBehaviour
{
    public string Prompt { get; private set; }
    public string State { get; private set; }

    // Reference to the TextMeshPro component within this button
    [SerializeField]
    private TMP_Text _textComponent;

    // Initialize the button with data
    public void Initialize(string prompt, string state)
    {
        Prompt = prompt;
        State = state;

        if (_textComponent == null)
            _textComponent = GetComponentInChildren<TMP_Text>();

        _textComponent.text = prompt;

        // You might want to add the button click listener here or ensure it's added elsewhere
    }
}