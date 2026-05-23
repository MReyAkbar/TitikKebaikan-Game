using UnityEngine;
using System.Collections.Generic;

// ════════════════════════════════════════════════════════════════
// MissionType.cs - Enum tipe misi
// ════════════════════════════════════════════════════════════════

public enum MissionType
{
    None,
    EscortNenek,       // Antar nenek ke pasar
    CleanEnvironment,  // Kumpulkan dan buang sampah
}

// ════════════════════════════════════════════════════════════════
// MissionManager.cs - Singleton pengelola misi
// ════════════════════════════════════════════════════════════════

/// <summary>
/// Mengelola semua misi aktif, progress, dan status selesai/gagal.
/// Singleton yang bisa diakses dari mana saja.
/// </summary>
public class MissionManager : MonoBehaviour
{
    // ─── Singleton ──────────────────────────────────────────────────────────

    public static MissionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─── State ───────────────────────────────────────────────────────────────

    private Dictionary<MissionType, MissionStatus> missions = new();

    public enum MissionStatus { NotStarted, Active, Completed, Failed }

    // ─── Events ──────────────────────────────────────────────────────────────

    public event System.Action<MissionType>         OnMissionStarted;
    public event System.Action<MissionType>         OnMissionCompleted;
    public event System.Action<MissionType, string> OnMissionFailed;

    // ─── API ─────────────────────────────────────────────────────────────────

    public void StartMission(MissionType type, MonoBehaviour source)
    {
        missions[type] = MissionStatus.Active;
        OnMissionStarted?.Invoke(type);
        GameHUD.Instance?.UpdateMissionStatus(type, MissionStatus.Active);
        Debug.Log($"[MissionManager] Misi dimulai: {type}");
    }

    public void CompleteMission(MissionType type)
    {
        missions[type] = MissionStatus.Completed;
        OnMissionCompleted?.Invoke(type);
        GameHUD.Instance?.UpdateMissionStatus(type, MissionStatus.Completed);
        SFXManager.Instance?.PlayMissionComplete();
        PointPopupManager.GetActiveSceneInstance()?.ShowMissionCompleteFeedback(type);
        Debug.Log($"[MissionManager] Misi selesai: {type}");

        CheckAllMissionsComplete();
    }

    public void FailMission(MissionType type, string reason)
    {
        missions[type] = MissionStatus.Failed;
        OnMissionFailed?.Invoke(type, reason);
        GameHUD.Instance?.UpdateMissionStatus(type, MissionStatus.Failed);
        GameHUD.Instance?.ShowNotification($"Misi Gagal: {reason}");
        SFXManager.Instance?.PlayMissionFail();
        Debug.Log($"[MissionManager] Misi gagal: {type} - {reason}");
    }

    public void ResetMission(MissionType type)
    {
        missions[type] = MissionStatus.NotStarted;
        Debug.Log($"[MissionManager] Misi direset: {type}");
    }

    public MissionStatus GetStatus(MissionType type)
    {
        return missions.TryGetValue(type, out var s) ? s : MissionStatus.NotStarted;
    }

    public bool IsAllCompleted()
    {
        foreach (MissionType t in System.Enum.GetValues(typeof(MissionType)))
        {
            if (t == MissionType.None) continue;
            if (GetStatus(t) != MissionStatus.Completed) return false;
        }
        return true;
    }

    public bool HasActiveMissionOtherThan(MissionType type)
    {
        foreach (var mission in missions)
        {
            if (mission.Key == MissionType.None || mission.Key == type) continue;
            if (mission.Value == MissionStatus.Active) return true;
        }

        return false;
    }

    private void CheckAllMissionsComplete()
    {
        if (IsAllCompleted())
        {
            Debug.Log("[MissionManager] SEMUA MISI SELESAI! Game berakhir.");
            GameManager.Instance?.TriggerGameEnd();
        }
    }
}


// ════════════════════════════════════════════════════════════════
// CheckpointManager.cs - Sistem respawn pemain
// ════════════════════════════════════════════════════════════════

/// <summary>
/// Menyimpan posisi checkpoint terakhir yang dilewati pemain.
/// Respawn pemain ke sana saat misi gagal / tertabrak kendaraan.
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [SerializeField] private Transform defaultSpawn;

    private Vector2 lastCheckpoint;
    private bool    hasCheckpoint;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (defaultSpawn != null)
            lastCheckpoint = defaultSpawn.position;
    }

    public void RegisterCheckpoint(Vector2 position)
    {
        lastCheckpoint = position;
        hasCheckpoint  = true;
        Debug.Log($"[Checkpoint] Checkpoint disimpan di {position}");
    }

    public void RespawnPlayer(PlayerController player)
    {
        if (player == null) return;

        Vector2 spawnPos = hasCheckpoint ? lastCheckpoint :
                           (defaultSpawn != null ? (Vector2)defaultSpawn.position : Vector2.zero);

        player.transform.position = spawnPos;
        Debug.Log($"[Checkpoint] Pemain di-respawn ke {spawnPos}");
    }
}


// ════════════════════════════════════════════════════════════════
// Checkpoint.cs - Trigger checkpoint di scene
// ════════════════════════════════════════════════════════════════

/// <summary>
/// Letakkan di scene. Saat pemain melalui area ini,
/// posisi checkpoint diperbarui.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private bool showGizmo = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            CheckpointManager.Instance?.RegisterCheckpoint(transform.position);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}
