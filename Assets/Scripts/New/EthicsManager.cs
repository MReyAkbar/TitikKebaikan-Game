using UnityEngine;

/// <summary>
/// Menyimpan dan mengelola Poin Etika pemain.
/// Singleton sederhana, tidak bergantung script lain.
/// </summary>
public class EthicsManager : MonoBehaviour
{
    public static EthicsManager Instance { get; private set; }

    [Header("Poin Awal")]
    public int ethicsPoints = 0;

    // Event dipanggil setiap poin berubah - GameHUD subscribe ke ini
    public event System.Action<int> OnPointsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Tambah atau kurangi poin. Gunakan nilai negatif untuk mengurangi.</summary>
    public void AddPoints(int amount, string reason = "")
    {
        ethicsPoints += amount;
        ethicsPoints = Mathf.Max(0, ethicsPoints); // tidak boleh minus

        if (reason != "")
            Debug.Log($"[Poin] {(amount >= 0 ? "+" : "")}{amount} — {reason}. Total: {ethicsPoints}");

        OnPointsChanged?.Invoke(ethicsPoints);
    }

    public int GetPoints() => ethicsPoints;
}
