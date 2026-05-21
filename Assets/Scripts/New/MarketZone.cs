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

    private void Start()
    {
        // Pastikan tag sudah benar
        if (!gameObject.CompareTag("Market"))
            Debug.LogWarning("[MarketZone] Tag belum diset ke 'Market'! Misi tidak akan trigger.");

        // Sembunyikan visual saat play (hanya untuk editor)
        if (zoneVisual != null)
            zoneVisual.enabled = false;
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
