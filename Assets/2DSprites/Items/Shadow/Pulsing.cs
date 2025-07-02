using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowPulse : MonoBehaviour
{
    public float pulseAmount = 0.2f;        // How much the shadow scales
    public float pulseSpeed = 2f;           // Speed of the pulsing
    public Transform shadowTransform;       // Assign your shadow object here

    private Vector3 shadowStartScale;

    void Start()
    {
        if (shadowTransform != null)
            shadowStartScale = shadowTransform.localScale;
    }

    void Update()
    {
        if (shadowTransform != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f; // Range 0 to 1
            float scale = 1 + (pulse * pulseAmount);
            shadowTransform.localScale = shadowStartScale * scale;
        }
    }
}