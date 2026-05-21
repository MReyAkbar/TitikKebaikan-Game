using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tempat sampah untuk membuang semua sampah yang sedang dibawa player.
/// Melaporkan progress ke Pak RT jika misi kebersihan aktif.
/// </summary>
public class TrashBin : MonoBehaviour
{
    [Header("Referensi")]
    public GameObject promptUI;
    public NPCPakRT pakRT;

    [Header("Interaction")]
    public float interactRadius = 1.4f;

    private Transform playerTransform;
    private PlayerController playerController;
    private bool playerInRange;

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
            playerController = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("[TrashBin] Player tidak ditemukan! Pastikan Player punya tag 'Player'.");
        }
    }

    private void Update()
    {
        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRadius;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (promptUI != null)
                promptUI.SetActive(inRange);
        }

        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            DumpTrash();
    }

    private void DumpTrash()
    {
        if (playerController == null) return;

        PlayerStats stats = playerController.GetComponent<PlayerStats>();
        if (stats == null) return;

        int count = stats.CurrentTrash;
        if (count == 0)
        {
            GameHUD.Instance?.ShowFloatingText("Kamu tidak membawa sampah.", transform.position);
            return;
        }

        playerController.DropAllTrash();
        Debug.Log($"[TrashBin] {count} sampah dibuang.");

        if (pakRT != null && pakRT.IsMissionActive)
            pakRT.OnTrashDelivered(playerController, count);
        else
            EthicsManager.Instance?.AddPoints(count * 2, "Membuang sampah ke tempat sampah");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
