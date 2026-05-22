using UnityEngine;

/// <summary>
/// Kelas dasar untuk semua NPC di game Titik Kebaikan.
/// Nenek dan Pak RT masing-masing merupakan turunan dari kelas ini.
/// </summary>
public abstract class NPCBase : MonoBehaviour, IInteractable
{
    [Header("NPC Info")]
    [SerializeField] protected string npcName = "NPC";
    [SerializeField] protected bool   hasMission = true;

    [Header("Dialog")]
    [SerializeField, TextArea(2, 5)]
    protected string idleDialog = "Halo, nak!";

    [SerializeField, TextArea(2, 5)]
    protected string missionDialog = "Bisakah kamu membantuku?";

    [SerializeField, TextArea(2, 5)]
    protected string completedDialog = "Terima kasih banyak!";

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject promptUI; // objek UI "Tekan E"

    // State
    protected bool missionActive    = false;
    protected bool missionCompleted = false;

    // ─── IInteractable ───────────────────────────────────────────────────────

    public virtual string InteractPrompt => $"Tekan E untuk bicara dengan {npcName}";

    public virtual void Interact(PlayerController player)
    {
        if (missionCompleted)
        {
            ShowDialog(completedDialog);
            return;
        }

        if (!missionActive && hasMission)
        {
            ShowDialog(missionDialog);
            OfferMission(player);
        }
        else
        {
            ShowDialog(idleDialog);
        }
    }

    // ─── Mission ─────────────────────────────────────────────────────────────

    /// <summary>Override di subclass untuk logika misi spesifik.</summary>
    protected abstract void OfferMission(PlayerController player);

    public virtual void CompleteMission(PlayerController player)
    {
        missionCompleted = true;
        missionActive    = false;
        ShowDialog(completedDialog);

        // Beri poin penyelesaian misi
        PlayerStats stats = player.GetComponent<PlayerStats>();
        stats?.AddEthicsPoints(GetMissionPoints(), $"Misi {npcName} selesai");

        Debug.Log($"[NPC] Misi {npcName} selesai!");
    }

    /// <summary>Poin etika yang diberikan saat misi selesai.</summary>
    protected virtual int GetMissionPoints() => 20;

    public bool IsMissionActive    => missionActive;
    public bool IsMissionCompleted => missionCompleted;
    public string NPCName          => npcName;

    // ─── Dialog ───────────────────────────────────────────────────────────────

    protected void ShowDialog(string text)
    {
        if (DialogBox.Instance != null)
            DialogBox.Instance.Show(npcName, text);
        else if (DialogManager.Instance != null)
            DialogManager.Instance.ShowDialog(npcName, text);
        else
            Debug.Log($"[{npcName}]: {text}");
    }

    // ─── Prompt UI ────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TRIGGER MASUK");
        if (other.CompareTag("Player"))
            promptUI?.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("TRIGGER KELUAR");
        if (other.CompareTag("Player"))
            promptUI?.SetActive(false);
    }
}
