using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



[System.Serializable]
public class AnthropicResponse
{
    public string prompt;
    public ResponseState state;
}

[System.Serializable]
public class ResponseState
{
    public string prompt;
    public string state; // "future" or "past"
}

public class AnthropicBridge : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _userPrompt;

    [SerializeField]
    private GameObject _prefabAnswer,
        _prefabStory,
        _prefabGoals;

    [SerializeField]
    private Transform _contentAnswer,
        _contentStory,
        _contentGoals;

    // Call this method to start the process (for demonstration, it could be called on some event)
    // Method to start the web request coroutine with the user's prompt
    public void FetchAndPopulate()
    {
        StartCoroutine(FetchResponsesFromAPI());
    }

    IEnumerator FetchResponsesFromAPI()
    {
        // Encode the userPrompt to ensure it's safely included in the URL
        string encodedPrompt = UnityWebRequest.EscapeURL(_userPrompt.text);
        // Update the apiUrl to point to your local server
        string apiUrl = $"http://localhost:8000?prompt={encodedPrompt}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            string password = "";
            string encodedPassword = System.Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(password)
            );
            webRequest.SetRequestHeader("Authorization", "Basic " + encodedPassword);

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                // Log the response for debugging
                Debug.Log(webRequest.downloadHandler.text);

                // Assuming the response is directly in the desired format
                AnthropicResponse[] responsesArray = JsonHelper.FromJson<AnthropicResponse>(
                    webRequest.downloadHandler.text
                );

                if (responsesArray == null)
                {
                    Debug.LogError("Failed to parse the JSON response or the response was empty.");
                    yield break;
                }

                List<AnthropicResponse> responses = new List<AnthropicResponse>(responsesArray);
                PopulateAnswers(responses);
            }
        }
    }

    // Dummy parse method, replace with actual JSON parsing
    List<AnthropicResponse> ParseResponse(string jsonResponse)
    {
        // Implement JSON parsing here to convert jsonResponse to List<AnthropicResponse>
        // This is a placeholder implementation
        return new List<AnthropicResponse>();
    }

    private void ClearContent(Transform content)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    // public void PopulateAnswers(List<AnthropicResponse> responses)
    // {
    //     ClearContent(_contentAnswer);

    //     foreach (var response in responses)
    //     {
    //         GameObject answerInstance = Instantiate(_prefabAnswer, _contentAnswer);
    //         TMP_Text textComponent = answerInstance.GetComponentInChildren<TMP_Text>();
    //         if (textComponent != null)
    //         {
    //             textComponent.text = response.prompt;
    //         }

    //         // Adjust this section to set the tag or some identifier for the category
    //         answerInstance.tag = response.state.state; // Example: Setting the tag

    //         Button button = answerInstance.GetComponent<Button>();
    //         if (button != null)
    //         {
    //             // Now passing the GameObject (answerInstance) itself to the SelectAnswer method
    //             button.onClick.AddListener(() => SelectAnswer(answerInstance));
    //         }
    //     }
    // }
    public void PopulateAnswers(List<AnthropicResponse> responses)
    {
        ClearContent(_contentAnswer);

        foreach (var response in responses)
        {
            GameObject answerInstance = Instantiate(_prefabAnswer, _contentAnswer);
            AnswerButton answerButton = answerInstance.GetComponent<AnswerButton>();

            if (answerButton != null)
            {
                answerButton.Initialize(response.prompt, response.state.state);

                Button btn = answerInstance.GetComponent<Button>();
                if (btn != null)
                {
                    // Remove all previous listeners to ensure we don't add multiple to the same button
                    btn.onClick.RemoveAllListeners();
                    // Use a lambda that captures the answerButton component for this iteration
                    btn.onClick.AddListener(() => SelectAnswer(answerButton));
                }
            }
        }
    }

    // // Change the parameter to GameObject to directly work with the pressed button
    // public void SelectAnswer(GameObject answerButton)
    // {
    //     // Find the TMP_Text component within the button's prefab
    //     TMP_Text textComponent = answerButton.GetComponentInChildren<TMP_Text>();
    //     if (textComponent == null)
    //         return; // Safety check

    //     // Determine the category of the response based on some stored value or direct comparison
    //     // This assumes you have a way to determine if the button's message is "future" or "past"
    //     // You might need to adjust this logic to suit how you're categorizing messages
    //     string category = answerButton.tag; // Example: Using the GameObject's tag to store the category

    //     // Populate the appropriate content based on the category
    //     if (category == "future")
    //     {
    //         AddToContent(_prefabGoals, _contentGoals, textComponent.text);
    //     }
    //     else if (category == "past")
    //     {
    //         AddToContent(_prefabStory, _contentStory, textComponent.text);
    //     }
    // }
    public void SelectAnswer(AnswerButton answerButton)
    {
        // Directly access the state stored in the AnswerButton
        if (answerButton.State == "future")
        {
            AddToContent(_prefabGoals, _contentGoals, answerButton.Prompt);
        }
        else if (answerButton.State == "past")
        {
            AddToContent(_prefabStory, _contentStory, answerButton.Prompt);
        }
    }

    private void AddToContent(GameObject prefab, Transform content, string text)
    {
        GameObject instance = Instantiate(prefab, content);
        TMP_Text textComponent = instance.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }
}
