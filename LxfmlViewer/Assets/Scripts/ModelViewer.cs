using UnityEngine;
using TMPro;

public class ModelViewer : MonoBehaviour
{
    public LxfmlApiClient apiClient;
    public TMP_InputField modelIdInput;

    public void OnLoadButtonClicked()
    {
        string modelId = modelIdInput.text;
        if (!string.IsNullOrEmpty(modelId))
        {
            apiClient.LoadModel(modelId);
        }
    }
}