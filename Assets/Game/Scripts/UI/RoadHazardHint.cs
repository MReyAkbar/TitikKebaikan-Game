using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menampilkan petunjuk rambu kecil saat player mendekati area jalan/zebra cross.
/// Bisa dipakai pada UI Image di World Space Canvas atau SpriteRenderer biasa.
/// </summary>
public class RoadHazardHint : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject hintRoot;
    [SerializeField] private bool followPlayer = true;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Traffic Light Optional")]
    [SerializeField] private TrafficLight linkedTrafficLight;
    [SerializeField] private Sprite greenLightWarningSprite;
    [SerializeField] private Sprite yellowLightWarningSprite;
    [SerializeField] private Sprite redLightCrossingSprite;
    [SerializeField] private Sprite defaultSprite;

    [Header("Visual Optional")]
    [SerializeField] private Image uiImage;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseAmount = 0.06f;

    private Transform player;
    private Vector3 initialScale;

    private void Awake()
    {
        if (hintRoot == null && transform.childCount > 0)
            hintRoot = transform.GetChild(0).gameObject;

        if (hintRoot == null)
        {
            Debug.LogWarning("[RoadHazardHint] Hint Root belum diisi. Buat child popup lalu drag ke field Hint Root.", this);
            return;
        }

        if (uiImage == null)
            uiImage = hintRoot.GetComponentInChildren<Image>(true);

        if (spriteRenderer == null)
            spriteRenderer = hintRoot.GetComponentInChildren<SpriteRenderer>(true);

        initialScale = hintRoot.transform.localScale;
        hintRoot.SetActive(false);
    }

    private void Update()
    {
        if (player == null || hintRoot == null || !hintRoot.activeSelf)
            return;

        UpdateSpriteByTrafficLight();

        if (followPlayer)
            hintRoot.transform.position = player.position + worldOffset;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        hintRoot.transform.localScale = initialScale * pulse;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        player = other.transform;
        ShowHint();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        player = null;
        HideHint();
    }

    private void ShowHint()
    {
        if (hintRoot == null)
            return;

        UpdateSpriteByTrafficLight();
        hintRoot.SetActive(true);
    }

    private void HideHint()
    {
        if (hintRoot == null)
            return;

        hintRoot.transform.localScale = initialScale;
        hintRoot.SetActive(false);
    }

    private void UpdateSpriteByTrafficLight()
    {
        Sprite selectedSprite = defaultSprite;

        if (linkedTrafficLight != null)
        {
            selectedSprite = linkedTrafficLight.State switch
            {
                TrafficLight.LightState.Green => greenLightWarningSprite,
                TrafficLight.LightState.Yellow => yellowLightWarningSprite,
                TrafficLight.LightState.Red => redLightCrossingSprite,
                _ => defaultSprite,
            };

            if (selectedSprite == null)
                selectedSprite = defaultSprite;
        }

        if (selectedSprite == null)
            return;

        if (uiImage != null)
            uiImage.sprite = selectedSprite;

        if (spriteRenderer != null)
            spriteRenderer.sprite = selectedSprite;
    }
}
