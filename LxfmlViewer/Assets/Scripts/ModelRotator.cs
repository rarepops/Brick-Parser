using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    [SerializeField]
    private readonly float rotationSpeed = 90f;

    private float currentRotation = 0f;
    private int currentAxisIndex = 0;
    private readonly Vector3[] rotationAxes = new Vector3[]
    {
        Vector3.up, // Y-axis
        Vector3.right, // X-axis
        new Vector3(1, 1, 0).normalized, // XY diagonal
        new Vector3(1, 0, 1).normalized, // XZ diagonal
        new Vector3(0, 1, 1).normalized, // YZ diagonal
        new Vector3(1, 1, 1).normalized, // XYZ diagonal
    };

    void Update()
    {
        Vector3 currentAxis = rotationAxes[currentAxisIndex];

        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        transform.Rotate(currentAxis, rotationThisFrame, Space.World);

        currentRotation += rotationThisFrame;

        // Check if we've completed a full rotation (360 degrees)
        if (currentRotation >= 360f)
        {
            currentRotation = 0f;
            currentAxisIndex = (currentAxisIndex + 1) % rotationAxes.Length;
            Debug.Log($"Switching to axis: {currentAxisIndex}");
        }
    }
}
