using UnityEngine;

/// <summary>
/// Efek idle sederhana untuk NPC tanpa sprite sheet: scale naik-turun halus seperti bernapas.
/// Pasang di child visual/sprite agar collider dan radius interaksi parent tidak berubah.
/// </summary>
public class NPCIdleBreathEffect : MonoBehaviour
{
    [Header("Breathing")]
    [SerializeField] private float scaleAmount = 0.025f;
    [SerializeField] private float speed = 1.4f;
    [SerializeField] private bool affectX = true;
    [SerializeField] private bool affectY = true;
    [SerializeField] private bool randomizeStart = true;

    private Vector3 startScale;
    private float phaseOffset;

    private void Awake()
    {
        startScale = transform.localScale;
        phaseOffset = randomizeStart ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    private void Update()
    {
        float wave = (Mathf.Sin(Time.time * speed + phaseOffset) + 1f) * 0.5f;
        float scaleOffset = wave * scaleAmount;

        transform.localScale = new Vector3(
            startScale.x * (affectX ? 1f + scaleOffset : 1f),
            startScale.y * (affectY ? 1f + scaleOffset : 1f),
            startScale.z
        );
    }
}
