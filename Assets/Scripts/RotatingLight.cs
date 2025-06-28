using UnityEngine;

public class RotatingLight : MonoBehaviour
{
    public float rotationAngle = 90f; // Degrees to rotate each time
    public float interval = 10f; // Time in seconds between rotations
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            transform.Rotate(Vector3.up, rotationAngle);
            timer = 0f;
        }
    }
}