using UnityEngine;

/// <summary>
/// NPC Pak RT: misi kebersihan lingkungan.
/// - Pemain harus mengumpulkan sejumlah sampah
/// - Lalu membuangnya ke tempat sampah
/// - Progres dilacak: X/Y sampah terkumpul
/// </summary>
public class NPCPakRT : NPCBase
{
    [Header("Trash Mission")]
    [SerializeField] private int requiredTrashCount = 5;
    [SerializeField] private int pointsPerTrash     = 5;

    private int trashDelivered = 0;

    // ─── Awake ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        npcName       = "Pak RT";
        idleDialog    = "Lingkungan kita harus bersih ya, Nak!";
        missionDialog = $"Nak, tolong kumpulkan {requiredTrashCount} sampah yang berserakan " +
                        $"dan buang ke tempat sampah yang tersedia, ya!";
        completedDialog = "Wah, terima kasih ya Nak! Lingkungan kita jadi bersih sekarang!";
    }

    // ─── Mission ─────────────────────────────────────────────────────────────

    protected override void OfferMission(PlayerController player)
    {
        missionActive  = true;
        trashDelivered = 0;

        MissionManager.Instance?.StartMission(MissionType.CleanEnvironment, this);

        // Update HUD
        GameHUD.Instance?.UpdateMissionText($"Kumpulkan Sampah: {trashDelivered}/{requiredTrashCount}");

        Debug.Log("[Pak RT] Misi kebersihan dimulai!");
    }

    protected override int GetMissionPoints() => pointsPerTrash * requiredTrashCount + 10;

    // ─── Trash Delivery ──────────────────────────────────────────────────────

    /// <summary>
    /// Dipanggil oleh TrashBin ketika pemain membuang sampah ke dalamnya.
    /// </summary>
    public void OnTrashDelivered(PlayerController player, int count)
    {
        if (!missionActive) return;

        trashDelivered += count;
        trashDelivered  = Mathf.Min(trashDelivered, requiredTrashCount);

        // Poin per sampah
        PlayerStats stats = player.GetComponent<PlayerStats>();
        stats?.AddEthicsPoints(pointsPerTrash * count, "Membuang sampah ke tempatnya");

        // Update HUD
        GameHUD.Instance?.UpdateMissionText($"Kumpulkan Sampah: {trashDelivered}/{requiredTrashCount}");
        GameHUD.Instance?.ShowFloatingText($"+{pointsPerTrash * count}", player.transform.position);

        Debug.Log($"[Pak RT] Sampah terbuang: {trashDelivered}/{requiredTrashCount}");

        if (trashDelivered >= requiredTrashCount)
            CompleteMission(player);
    }

    public override void CompleteMission(PlayerController player)
    {
        MissionManager.Instance?.CompleteMission(MissionType.CleanEnvironment);
        base.CompleteMission(player);
    }
}
