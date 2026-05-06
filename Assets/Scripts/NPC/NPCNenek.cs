using UnityEngine;
using System.Collections;

/// <summary>
/// NPC Nenek: misi utama mengantar ke pasar.
/// - Nenek mengikuti pemain saat misi aktif
/// - Bonus poin saat melewati zebra cross
/// - Misi gagal jika nenek tertabrak kendaraan
/// - Timer countdown untuk misi berbatas waktu
/// </summary>
public class NPCNenek : NPCBase
{
    [Header("Escort Settings")]
    [SerializeField] private Transform  marketDestination;
    [SerializeField] private float      followSpeed    = 2.5f;
    [SerializeField] private float      followDistance = 1.2f;
    [SerializeField] private float      arrivalRadius  = 0.8f;
    [SerializeField] private float      missionTimeLimit = 120f; // detik, 0 = tidak ada limit

    [Header("Points")]
    [SerializeField] private int zebraBonus        = 15;
    [SerializeField] private int missionBasePoints = 30;
    [SerializeField] private int safeArrivalBonus  = 20;
    [SerializeField] private int timeBonus         = 10; // jika selesai cepat

    // State
    private PlayerController escortPlayer;
    private float missionTimer;
    private bool  timerRunning;
    private Rigidbody2D npcRb;

    // ─── Unity Lifecycle ────────────────────────────────────────────────────

    protected void Awake()
    {
        npcName      = "Nenek";
        idleDialog   = "Nak, Nenek mau ke pasar tapi ramai sekali jalanannya...";
        missionDialog = "Maukah kamu menemani Nenek pergi ke pasar? Nenek takut menyeberang sendiri.";
        completedDialog = "Terima kasih ya Nak, sudah baik sekali kamu!";

        npcRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!missionActive) return;

        FollowPlayer();
        CheckArrival();
        UpdateTimer();
    }

    // ─── Mission ────────────────────────────────────────────────────────────

    protected override void OfferMission(PlayerController player)
    {
        missionActive = true;
        escortPlayer  = player;

        player.SetCarryingNPC(true);

        if (missionTimeLimit > 0)
        {
            missionTimer  = missionTimeLimit;
            timerRunning  = true;
        }

        // Beritahu MissionManager
        MissionManager.Instance?.StartMission(MissionType.EscortNenek, this);

        Debug.Log("[Nenek] Misi escort dimulai!");
    }

    protected override int GetMissionPoints() => missionBasePoints + safeArrivalBonus;

    // ─── Follow Logic ───────────────────────────────────────────────────────

    private void FollowPlayer()
    {
        if (escortPlayer == null) return;

        float dist = Vector2.Distance(transform.position, escortPlayer.transform.position);

        if (dist > followDistance)
        {
            Vector2 dir = ((Vector2)escortPlayer.transform.position - (Vector2)transform.position).normalized;
            if (npcRb != null)
                npcRb.linearVelocity = dir * followSpeed;
        }
        else
        {
            if (npcRb != null) npcRb.linearVelocity = Vector2.zero;
        }
    }

    // ─── Arrival ────────────────────────────────────────────────────────────

    private void CheckArrival()
    {
        if (marketDestination == null) return;

        float dist = Vector2.Distance(transform.position, marketDestination.position);
        if (dist <= arrivalRadius)
        {
            OnArrivedAtMarket();
        }
    }

    private void OnArrivedAtMarket()
    {
        timerRunning = false;
        escortPlayer?.SetCarryingNPC(false);

        // Bonus waktu
        PlayerStats stats = escortPlayer?.GetComponent<PlayerStats>();
        if (stats != null && timerRunning == false && missionTimer > missionTimeLimit * 0.5f)
            stats.AddEthicsPoints(timeBonus, "Mengantarkan Nenek dengan cepat");

        CompleteMission(escortPlayer);
        MissionManager.Instance?.CompleteMission(MissionType.EscortNenek);

        if (npcRb != null) npcRb.linearVelocity = Vector2.zero;
    }

    // ─── Timer ──────────────────────────────────────────────────────────────

    private void UpdateTimer()
    {
        if (!timerRunning || missionTimeLimit <= 0) return;

        missionTimer -= Time.deltaTime;

        // Update HUD timer
        GameHUD.Instance?.UpdateTimer(missionTimer);

        if (missionTimer <= 0)
            OnMissionFailed("Waktu habis!");
    }

    // ─── Hit by Vehicle ─────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!missionActive) return;

        if (other.CompareTag("Vehicle"))
            OnMissionFailed("Nenek tertabrak kendaraan!");
    }

    public void OnMissionFailed(string reason)
    {
        Debug.Log($"[Nenek] Misi gagal: {reason}");
        missionActive = false;
        timerRunning  = false;

        escortPlayer?.SetCarryingNPC(false);
        MissionManager.Instance?.FailMission(MissionType.EscortNenek, reason);

        // Respawn di checkpoint terakhir
        CheckpointManager.Instance?.RespawnPlayer(escortPlayer);
    }

    // ─── Zebra Cross Bonus ───────────────────────────────────────────────────

    /// <summary>Dipanggil oleh ZebraCross saat nenek melewatinya.</summary>
    public void OnCrossedZebra()
    {
        if (!missionActive) return;

        PlayerStats stats = escortPlayer?.GetComponent<PlayerStats>();
        stats?.AddEthicsPoints(zebraBonus, "Menyeberang di zebra cross");

        GameHUD.Instance?.ShowFloatingText($"+{zebraBonus} Patuh Lalu Lintas!", transform.position);
        Debug.Log("[Nenek] Zebra Cross bonus!");
    }
}
