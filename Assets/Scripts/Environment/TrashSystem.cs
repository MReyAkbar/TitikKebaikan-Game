using UnityEngine;

// ════════════════════════════════════════════════════════════════
// TrashObject.cs - Sampah yang bisa dipungut pemain
// ════════════════════════════════════════════════════════════════

/// <summary>
/// Sampah yang berserakan di dunia game.
/// Pemain menekan E di dekatnya untuk memungut.
/// </summary>
public class TrashObject : MonoBehaviour, IInteractable
{
    [SerializeField] private string trashType = "Sampah Plastik";

    public string InteractPrompt => $"Tekan E untuk memungut {trashType}";
    public string TrashType      => trashType;

    public void Interact(PlayerController player)
    {
        bool picked = player.TryPickupTrash(this);

        if (picked)
        {
            Debug.Log($"[Trash] Dipungut: {trashType}");
            gameObject.SetActive(false); // Sembunyikan dari world, masuk inventory
        }
        else
        {
            GameHUD.Instance?.ShowFloatingText("Kapasitas penuh!", transform.position);
        }
    }
}


// ════════════════════════════════════════════════════════════════
// TrashBin.cs - Tempat sampah untuk membuang sampah yang dikumpulkan
// ════════════════════════════════════════════════════════════════

/// <summary>
/// Tempat sampah. Pemain berinteraksi untuk membuang semua sampah
/// yang dibawa, dan melaporkan ke Pak RT.
/// </summary>
public class TrashBin : MonoBehaviour, IInteractable
{
    [Header("Reference")]
    [SerializeField] private NPCPakRT pakRT; // Boleh null jika Pak RT belum misi aktif

    public string InteractPrompt => "Tekan E untuk membuang sampah";

    public void Interact(PlayerController player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        int count = stats.CurrentTrash;

        if (count == 0)
        {
            GameHUD.Instance?.ShowFloatingText("Kamu tidak membawa sampah.", transform.position);
            return;
        }

        // Buang semua sampah
        player.DropAllTrash();

        Debug.Log($"[TrashBin] {count} sampah dibuang.");

        // Laporkan ke Pak RT jika misi aktif
        if (pakRT != null && pakRT.IsMissionActive)
            pakRT.OnTrashDelivered(player, count);
        else
            // Poin kecil meski tidak ada misi aktif (kebiasaan baik)
            stats.AddEthicsPoints(count * 2, "Membuang sampah ke tempat sampah");

        GameHUD.Instance?.UpdateTrashUI(0, stats.MaxTrash);
    }
}
