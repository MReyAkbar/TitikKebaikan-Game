using UnityEngine;

/// <summary>
/// Efek visual idle sederhana untuk object collectible: naik-turun pelan dan rotasi opsional.
/// Pasang di child visual agar collider/interaksi parent tetap diam.
/// </summary>
public class SimpleFloatEffect : MonoBehaviour
{
    [Header("Float")]
    [SerializeField] private float amplitude = 0.08f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool randomizeStart = true;

    [Header("Optional Rotation")]
    [SerializeField] private float rotationAmplitude = 0f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private float phaseOffset;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
        phaseOffset = randomizeStart ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    private void Update()
    {
        float wave = Mathf.Sin(Time.time * speed + phaseOffset);
        transform.localPosition = startLocalPosition + Vector3.up * (wave * amplitude);

        if (rotationAmplitude > 0f)
            transform.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
    }
}
