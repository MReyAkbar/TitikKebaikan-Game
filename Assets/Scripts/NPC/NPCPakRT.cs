using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// NPC Pak RT: memberi misi kebersihan lingkungan.
/// Alur dibuat mirip NenekNPC: proximity -> tekan E -> dialog -> misi aktif.
/// </summary>
public class NPCPakRT : MonoBehaviour
{
    public enum MissionState { Idle, Talking, Active, ReadyToReport, Completed }
    public MissionState State { get; private set; } = MissionState.Idle;

    [Header("Referensi")]
    public GameObject promptUI;

    [Header("Interaction")]
    public float interactRadius = 1.8f;

    [Header("Trash Mission")]
    [SerializeField] private int requiredTrashCount = 5;
    [SerializeField] private int pointsPerTrash = 5;
    [SerializeField] private int completionBonus = 10;

    [Header("Dialog")]
    [TextArea(2, 4)]
    public string dialogAwal = "Nak, lingkungan kita mulai kotor. Bisa bantu Pak RT mengumpulkan sampah?";
    [TextArea(2, 4)]
    public string dialogAktif = "Ayo lanjutkan, Nak. Kumpulkan sampah lalu buang ke tempat sampah.";
    [TextArea(2, 4)]
    public string dialogSiapLapor = "Bagus, semua sampah sudah dibuang. Ayo lapor ke Pak RT.";
    [TextArea(2, 4)]
    public string dialogSelesai = "Wah, terima kasih ya Nak! Lingkungan kita jadi bersih sekarang.";

    private Transform playerTransform;
    private bool playerInRange;
    private int trashDelivered;

    public bool IsMissionActive => State == MissionState.Active;
    public bool IsMissionCompleted => State == MissionState.Completed;

    private void Awake()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("[Pak RT] Player tidak ditemukan! Pastikan Player punya tag 'Player'.");
        }
    }

    private void Update()
    {
        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;
        if (State == MissionState.Completed) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRadius;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (promptUI != null)
                promptUI.SetActive(inRange);
        }

        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            OnPlayerInteract();
    }

    private void OnPlayerInteract()
    {
        if (State == MissionState.Idle)
            StartDialog();
        else if (State == MissionState.Active)
            ShowActiveDialog();
        else if (State == MissionState.ReadyToReport)
            CompleteMission();
    }

    private void StartDialog()
    {
        State = MissionState.Talking;

        if (promptUI != null)
            promptUI.SetActive(false);

        string message = $"{dialogAwal}\n\nTarget: {requiredTrashCount} sampah.";

        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Pak RT", message, StartMission);
        else
        {
            Debug.Log("[Pak RT] " + message);
            StartMission();
        }
    }

    private void ShowActiveDialog()
    {
        string message = $"{dialogAktif}\n\nProgress: {trashDelivered}/{requiredTrashCount}";

        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Pak RT", message);
        else
            Debug.Log("[Pak RT] " + message);
    }

    private void ShowCompletionDialog()
    {
        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Pak RT", dialogSelesai);
        else
            Debug.Log("[Pak RT] " + dialogSelesai);
    }

    private void StartMission()
    {
        State = MissionState.Active;
        trashDelivered = 0;

        MissionManager.Instance?.StartMission(MissionType.CleanEnvironment, this);
        MissionHUD.Instance?.SetPakRTMissionText($"Kumpulkan sampah {trashDelivered}/{requiredTrashCount}");

        Debug.Log("[Pak RT] Misi kebersihan dimulai!");
    }

    public void OnTrashDelivered(PlayerController player, int count)
    {
        if (State != MissionState.Active) return;
        if (player == null || count <= 0) return;

        int remaining = requiredTrashCount - trashDelivered;
        int countedTrash = Mathf.Min(count, remaining);

        trashDelivered += countedTrash;

        EthicsManager.Instance?.AddPoints(pointsPerTrash * countedTrash, "Membuang sampah ke tempatnya");

        MissionHUD.Instance?.SetPakRTMissionText($"Kumpulkan sampah {trashDelivered}/{requiredTrashCount}");
        GameHUD.Instance?.ShowFloatingText($"+{pointsPerTrash * countedTrash}", player.transform.position);

        Debug.Log($"[Pak RT] Sampah terbuang: {trashDelivered}/{requiredTrashCount}");

        if (trashDelivered >= requiredTrashCount)
            MarkReadyToReport();
    }

    private void MarkReadyToReport()
    {
        if (State != MissionState.Active) return;

        State = MissionState.ReadyToReport;
        MissionHUD.Instance?.SetPakRTMissionText("Lapor kembali");

        Debug.Log("[Pak RT] Semua sampah sudah dibuang. Menunggu laporan pemain.");
        ShowSystemHint(dialogSiapLapor);
    }

    private void ShowSystemHint(string message)
    {
        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Petunjuk Sistem", message);
        else
            Debug.Log("[Petunjuk Sistem] " + message);
    }

    private void CompleteMission()
    {
        if (State == MissionState.Completed) return;

        State = MissionState.Completed;

        if (promptUI != null)
            promptUI.SetActive(false);

        EthicsManager.Instance?.AddPoints(completionBonus, "Misi Pak RT selesai");

        MissionManager.Instance?.CompleteMission(MissionType.CleanEnvironment);
        MissionHUD.Instance?.SetPakRTMissionText("Selesai");

        Debug.Log("[Pak RT] Misi kebersihan selesai!");
        ShowCompletionDialog();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
