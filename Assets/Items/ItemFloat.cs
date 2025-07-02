using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFloat : MonoBehaviour
{
    public float floatStrength = 0.3f; // Amplitude of the float
    public float floatSpeed = 2f;      // Frequency of the float
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatStrength;
        transform.position = new Vector3(startPos.x, startPos.y + newY, startPos.z);
    }
}
