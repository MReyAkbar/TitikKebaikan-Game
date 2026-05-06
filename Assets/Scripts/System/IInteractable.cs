/// <summary>
/// Interface yang harus diimplementasikan oleh semua objek interaktif:
/// NPC, sampah, tempat sampah, zebra cross, dll.
/// Pemain memanggil Interact() saat menekan tombol E.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Dipanggil saat pemain berinteraksi dengan objek ini.
    /// </summary>
    void Interact(PlayerController player);

    /// <summary>
    /// Teks yang ditampilkan di atas kepala objek saat pemain dekat.
    /// Contoh: "Tekan E untuk Bicara"
    /// </summary>
    string InteractPrompt { get; }
}
