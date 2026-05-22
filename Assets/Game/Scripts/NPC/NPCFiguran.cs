using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// NPC warga pasif yang memberi dialog pendek berdasarkan reputasi pemain.
/// Tidak memulai misi, hanya memperkuat rasa hidup di lingkungan.
/// </summary>
public class NPCFiguran : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string npcName = "Warga";

    [Header("Interaction")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private float interactRadius = 1.4f;

    [Header("Dialog Jelek")]
    [TextArea(2, 4)]
    [SerializeField] private string[] jelekDialogs =
    {
        "Hati-hati di jalan ya, jangan sembarangan menyeberang.",
        "Ayo bantu warga sekitar, Nak."
    };

    [Header("Dialog Lumayan")]
    [TextArea(2, 4)]
    [SerializeField] private string[] lumayanDialogs =
    {
        "Wah, kamu mulai dikenal suka membantu.",
        "Teruskan ya, lingkungan jadi lebih rapi."
    };

    [Header("Dialog Bagus")]
    [TextArea(2, 4)]
    [SerializeField] private string[] bagusDialogs =
    {
        "Kamu memang warga teladan!",
        "Banyak warga terbantu karena kamu."
    };

    private Transform player;
    private PlayerStats playerStats;
    private bool playerInRange;

    private void Start()
    {
        SetPromptVisible(false);
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
            FindPlayer();

        if (player == null) return;

        playerInRange = Vector2.Distance(transform.position, player.position) <= interactRadius;
        SetPromptVisible(playerInRange);

        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Talk();
    }

    private void Talk()
    {
        string message = GetDialogForCurrentTier();

        if (DialogBox.Instance != null)
            DialogBox.Instance.Show(npcName, message);
        else
            Debug.Log($"[{npcName}] {message}");
    }

    private string GetDialogForCurrentTier()
    {
        PlayerStats.ReputationTier tier = playerStats != null
            ? playerStats.Tier
            : PlayerStats.ReputationTier.Jelek;

        string[] source = tier switch
        {
            PlayerStats.ReputationTier.Bagus => bagusDialogs,
            PlayerStats.ReputationTier.Lumayan => lumayanDialogs,
            _ => jelekDialogs,
        };

        if (source == null || source.Length == 0)
            return "Semoga harimu menyenangkan.";

        return source[Random.Range(0, source.Length)];
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        player = playerObject.transform;
        playerStats = playerObject.GetComponent<PlayerStats>();
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptUI != null)
            promptUI.SetActive(visible);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
