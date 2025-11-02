using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LxfmlApiClient : MonoBehaviour
{
    private const string API_BASE = "http://localhost:5130/api/v1";
    private GameObject currentModel;

    /// <summary>
    /// Fetch a model from the API by ID and instantiate it in the scene.
    /// </summary>
    public void LoadModel(string modelId)
    {
        StartCoroutine(FetchModelCoroutine(modelId));
    }

    private IEnumerator FetchModelCoroutine(string modelId)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{API_BASE}/models/{modelId}"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                ModelDto model = JsonUtility.FromJson<ModelDto>(json);
                
                // Delete previous model if exists
                if (currentModel != null)
                {
                    Destroy(currentModel);
                }
                
                RenderModel(model);
                Debug.Log($"Loaded model: {model.name} ({model.parts.Length} parts)");
            }
            else
            {
                Debug.LogError($"Failed to load model: {request.error}");
            }
        }
    }

    private void RenderModel(ModelDto model)
    {
        var container = new GameObject(model.name);
        currentModel = container;
        
        // Add rotation component
        container.AddComponent<ModelRotator>();

        foreach (var part in model.parts)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Part_{part.designId}";
            cube.transform.parent = container.transform;

            // Apply 4x4 transformation matrix
            var matrix = new Matrix4x4();
            for (int i = 0; i < 16; i++)
            {
                matrix[i / 4, i % 4] = part.transformMatrix[i];
            }

            cube.transform.position = matrix.GetColumn(3);
            cube.transform.rotation = Quaternion.LookRotation(
                matrix.GetColumn(2),
                matrix.GetColumn(1)
            );
            cube.transform.localScale = new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude
            );

            // Apply random color
            var renderer = cube.GetComponent<Renderer>();
            renderer.material.color = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            );

            Object.Destroy(cube.GetComponent<Collider>());
        }
    }
}

[System.Serializable]
public class ModelDto
{
    public string name;
    public int partCount;
    public PartDto[] parts;
}

[System.Serializable]
public class PartDto
{
    public string uuid;
    public int designId;
    public float[] transformMatrix;
}