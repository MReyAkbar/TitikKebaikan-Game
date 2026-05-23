using UnityEngine;

/// <summary>
/// Zona Pasar — trigger area yang mendeteksi Nenek tiba.
/// 
/// Setup di Unity:
/// - Buat Empty GameObject bernama "PasarZone"
/// - Add BoxCollider2D → centang Is Trigger
/// - Add script ini
/// - Tag GameObject ini: "Market"
/// - Atur ukuran BoxCollider sesuai area pasar
/// </summary>
public class MarketZone : MonoBehaviour
{
    [Header("Visual (opsional)")]
    public SpriteRenderer zoneVisual; // sprite semi-transparan penanda area

    [Header("Mission Highlight")]
    public GameObject destinationHintRoot;
    public SpriteRenderer highlightRenderer;
    public bool showOnlyDuringNenekMission = true;
    public Color highlightColor = new Color(0.2f, 1f, 0.55f, 0.65f);
    public float pulseSpeed = 3f;
    public float pulseScale = 0.08f;
    public float minAlpha = 0.35f;
    public float maxAlpha = 0.85f;

    private NenekNPC nenek;
    private Vector3 hintBaseScale = Vector3.one;
    private bool highlightVisible;

    private void Start()
    {
        // Pastikan tag sudah benar
        if (!gameObject.CompareTag("Market"))
            Debug.LogWarning("[MarketZone] Tag belum diset ke 'Market'! Misi tidak akan trigger.");

        if (destinationHintRoot != null)
            hintBaseScale = destinationHintRoot.transform.localScale;

        if (highlightRenderer == null && zoneVisual != null)
            highlightRenderer = zoneVisual;

        SetHighlightVisible(!showOnlyDuringNenekMission);
    }

    private void Update()
    {
        bool shouldShow = ShouldShowHighlight();
        SetHighlightVisible(shouldShow);

        if (shouldShow)
            AnimateHighlight();
    }

    private bool ShouldShowHighlight()
    {
        if (!showOnlyDuringNenekMission)
            return true;

        if (nenek == null)
            nenek = FindFirstObjectByType<NenekNPC>();

        return nenek != null && nenek.State == NenekNPC.MissionState.Active;
    }

    private void SetHighlightVisible(bool visible)
    {
        if (highlightVisible == visible) return;
        highlightVisible = visible;

        if (destinationHintRoot != null)
            destinationHintRoot.SetActive(visible);

        if (highlightRenderer != null)
            highlightRenderer.enabled = visible;
    }

    private void AnimateHighlight()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        if (destinationHintRoot != null)
        {
            float scale = 1f + pulse * pulseScale;
            destinationHintRoot.transform.localScale = hintBaseScale * scale;
        }

        if (highlightRenderer != null)
        {
            Color color = highlightColor;
            color.a = Mathf.Lerp(minAlpha, maxAlpha, pulse);
            highlightRenderer.color = color;
        }
    }

    private void OnDrawGizmos()
    {
        // Tampilkan area pasar di Scene view (biru transparan)
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.25f);
        Gizmos.DrawCube(transform.position, transform.localScale);

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.8f);
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
