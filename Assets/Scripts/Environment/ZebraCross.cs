using UnityEngine;

/// <summary>
/// Area zebra cross. Memberi poin jika player menyeberang saat lampu kendaraan merah,
/// dan penalti jika menyeberang saat kendaraan boleh jalan.
/// </summary>
public class ZebraCross : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TrafficLight linkedTrafficLight;

    [Header("Points")]
    [SerializeField] private int safeCrossingBonus = 5;
    [SerializeField] private int escortBonus = 10;
    [SerializeField] private int unsafeCrossingPenalty = 10;
    [SerializeField] private float triggerCooldown = 2f;

    [Header("Messages")]
    [SerializeField] private bool showDialogHints = false;
    [SerializeField] private string safeMessage = "Bagus, menyeberang saat kendaraan berhenti.";
    [SerializeField] private string unsafeMessage = "Hati-hati, jangan menyeberang saat kendaraan boleh jalan.";
    [SerializeField] private string yellowMessage = "Hati-hati, tunggu kendaraan benar-benar berhenti.";

    private float lastTriggerTime = -999f;
    private bool playerInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerInside) return;
        if (Time.time < lastTriggerTime + triggerCooldown) return;

        playerInside = true;
        lastTriggerTime = Time.time;

        if (linkedTrafficLight == null)
        {
            Reward(safeCrossingBonus, safeMessage, other.transform.position);
            return;
        }

        if (linkedTrafficLight.IsRedLight)
        {
            int points = IsEscortingNenek() ? safeCrossingBonus + escortBonus : safeCrossingBonus;
            Reward(points, safeMessage, other.transform.position);
            return;
        }

        if (linkedTrafficLight.IsYellowLight)
        {
            ShowSystemHint(yellowMessage);
            return;
        }

        Penalize(unsafeCrossingPenalty, unsafeMessage, other.transform.position);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }

    private bool IsEscortingNenek()
    {
        NenekNPC nenek = FindFirstObjectByType<NenekNPC>();
        return nenek != null && nenek.State == NenekNPC.MissionState.Active;
    }

    private void Reward(int points, string message, Vector3 worldPosition)
    {
        EthicsManager.Instance?.AddPoints(points, message);
        ShowSystemHint(message);
    }

    private void Penalize(int points, string message, Vector3 worldPosition)
    {
        EthicsManager.Instance?.AddPoints(-points, message);
        ShowSystemHint(message);
    }

    private void ShowSystemHint(string message)
    {
        if (!showDialogHints)
        {
            Debug.Log("[Petunjuk Sistem] " + message);
            return;
        }

        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Petunjuk Sistem", message);
        else
            Debug.Log("[Petunjuk Sistem] " + message);
    }
}
