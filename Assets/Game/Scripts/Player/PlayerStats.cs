using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menyimpan dan mengelola semua statistik pemain:
/// - Poin Etika (Ethics Points)
/// - Kategori Reputasi (Reputation Tier)
/// - Kapasitas Bawaan Sampah (Carry Capacity)
/// - Multiplier Kecepatan dari tier
/// </summary>
public class PlayerStats : MonoBehaviour
{
    // ─── Enums & Constants ──────────────────────────────────────────────────

    public enum ReputationTier { Jelek = 0, Lumayan = 1, Bagus = 2 }

    [Header("Ethics Points")]
    [SerializeField] private int startingPoints  = 0;
    [SerializeField] private int lumayenThreshold = 50;   // poin untuk naik ke Lumayan
    [SerializeField] private int bagusThreshold   = 120;  // poin untuk naik ke Bagus

    [Header("Speed Multipliers per Tier")]
    [SerializeField] private float speedJelek   = 1.0f;
    [SerializeField] private float speedLumayan = 1.15f;
    [SerializeField] private float speedBagus   = 1.30f;

    [Header("Carry Capacity")]
    [SerializeField] private int maxTrashCapacity = 5;

    // ─── Runtime State ───────────────────────────────────────────────────────

    private int ethicsPoints;
    private ReputationTier currentTier;
    private List<TrashObject> carriedTrash = new List<TrashObject>();
    private bool isEthicsManagerSubscribed;

    // ─── Events ──────────────────────────────────────────────────────────────

    public event System.Action<int>             OnPointsChanged;      // (newPoints)
    public event System.Action<ReputationTier>  OnTierChanged;        // (newTier)
    public event System.Action<int, int>        OnTrashChanged;       // (current, max)

    // ─── Properties ─────────────────────────────────────────────────────────

    public int   EthicsPoints   => ethicsPoints;
    public int   MaxTrash       => maxTrashCapacity;
    public int   CurrentTrash   => carriedTrash.Count;
    public ReputationTier Tier  => currentTier;

    public float SpeedMultiplier => currentTier switch
    {
        ReputationTier.Lumayan => speedLumayan,
        ReputationTier.Bagus   => speedBagus,
        _                      => speedJelek,
    };

    // ─── Unity Lifecycle ─────────────────────────────────────────────────────

    private void Awake()
    {
        ethicsPoints = startingPoints;
        currentTier  = ReputationTier.Jelek;
    }

    private void Start()
    {
        TrySubscribeToEthicsManager();
        OnPointsChanged?.Invoke(ethicsPoints);
        OnTierChanged?.Invoke(currentTier);
        OnTrashChanged?.Invoke(CurrentTrash, MaxTrash);
    }

    private void Update()
    {
        if (!isEthicsManagerSubscribed)
            TrySubscribeToEthicsManager();
    }

    private void OnDestroy()
    {
        if (isEthicsManagerSubscribed && EthicsManager.Instance != null)
            EthicsManager.Instance.OnPointsChanged -= SyncEthicsPoints;
    }

    // ─── Poin Etika ──────────────────────────────────────────────────────────

    /// <summary>Tambah poin etika. Nilai negatif untuk mengurangi.</summary>
    public void AddEthicsPoints(int amount, string reason = "")
    {
        if (EthicsManager.Instance != null)
        {
            EthicsManager.Instance.AddPoints(amount, reason);
            return;
        }

        int prev = ethicsPoints;
        ethicsPoints = Mathf.Max(0, ethicsPoints + amount);

        if (ethicsPoints != prev)
        {
            if (!string.IsNullOrEmpty(reason))
                Debug.Log($"[PlayerStats] Poin {(amount >= 0 ? "+" : "")}{amount} ({reason}). Total: {ethicsPoints}");

            OnPointsChanged?.Invoke(ethicsPoints);
            CheckTierUpgrade();
        }
    }

    private void TrySubscribeToEthicsManager()
    {
        if (isEthicsManagerSubscribed || EthicsManager.Instance == null) return;

        EthicsManager.Instance.OnPointsChanged += SyncEthicsPoints;
        isEthicsManagerSubscribed = true;
        SyncEthicsPoints(EthicsManager.Instance.GetPoints());
    }

    private void SyncEthicsPoints(int points)
    {
        int previousPoints = ethicsPoints;
        ethicsPoints = Mathf.Max(0, points);

        if (ethicsPoints != previousPoints)
            OnPointsChanged?.Invoke(ethicsPoints);

        CheckTierUpgrade();
    }

    private void CheckTierUpgrade()
    {
        ReputationTier newTier;

        if (ethicsPoints >= bagusThreshold)
            newTier = ReputationTier.Bagus;
        else if (ethicsPoints >= lumayenThreshold)
            newTier = ReputationTier.Lumayan;
        else
            newTier = ReputationTier.Jelek;

        if (newTier != currentTier)
        {
            currentTier = newTier;
            Debug.Log($"[PlayerStats] Tier naik ke: {currentTier}! Speed x{SpeedMultiplier}");
            OnTierChanged?.Invoke(currentTier);
        }
    }

    // ─── Sampah ───────────────────────────────────────────────────────────────

    public bool TryAddTrash(TrashObject trash)
    {
        if (carriedTrash.Count >= maxTrashCapacity)
        {
            Debug.Log("[PlayerStats] Kapasitas sampah penuh!");
            return false;
        }

        carriedTrash.Add(trash);
        OnTrashChanged?.Invoke(carriedTrash.Count, maxTrashCapacity);
        return true;
    }

    public void ClearTrash()
    {
        carriedTrash.Clear();
        OnTrashChanged?.Invoke(0, maxTrashCapacity);
    }

    public List<TrashObject> GetCarriedTrash() => new List<TrashObject>(carriedTrash);

    // ─── Debug / Save ─────────────────────────────────────────────────────────

    public void PrintStats()
    {
        Debug.Log($"=== PLAYER STATS ===\n" +
                  $"Poin Etika : {ethicsPoints}\n" +
                  $"Tier       : {currentTier}\n" +
                  $"Speed Mult : {SpeedMultiplier}\n" +
                  $"Sampah     : {carriedTrash.Count}/{maxTrashCapacity}");
    }
}
