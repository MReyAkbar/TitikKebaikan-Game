using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sampah yang bisa dipungut player.
/// Polanya dibuat mandiri seperti Nenek/Pak RT: proximity -> tekan E -> masuk inventory.
/// </summary>
public class TrashObject : MonoBehaviour
{
    [Header("Referensi")]
    public GameObject promptUI;
    public NPCPakRT pakRT;

    [Header("Interaction")]
    public float interactRadius = 1.2f;
    [SerializeField] private bool requireActiveMission = true;

    [Header("Trash Info")]
    [SerializeField] private string trashType = "Sampah Plastik";

    private Transform playerTransform;
    private PlayerController playerController;
    private bool playerInRange;
    private SimpleFloatEffect[] floatEffects;
    private bool floatEffectsActive;

    public string TrashType => trashType;

    private void Awake()
    {
        floatEffects = GetComponentsInChildren<SimpleFloatEffect>(true);
        SetFloatEffectsActive(!requireActiveMission, true);

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
            Debug.LogWarning("[Trash] Player tidak ditemukan! Pastikan Player punya tag 'Player'.");
        }
    }

    private void Update()
    {
        UpdateFloatEffects();
        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;
        if (!CanPickupTrash())
        {
            if (playerInRange)
            {
                playerInRange = false;
                if (promptUI != null)
                    promptUI.SetActive(false);
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRadius;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (promptUI != null)
                promptUI.SetActive(inRange);
        }

        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Pickup();
    }

    private void Pickup()
    {
        if (playerController == null) return;
        if (!CanPickupTrash()) return;

        bool picked = playerController.TryPickupTrash(this);
        if (!picked)
        {
            SFXManager.Instance?.PlayActionFailed();
            GameHUD.Instance?.ShowFloatingText("Kapasitas penuh!", transform.position);
            return;
        }

        SFXManager.Instance?.PlayTrashPickup();
        Debug.Log($"[Trash] Dipungut: {trashType}");
        gameObject.SetActive(false);
    }

    private bool CanPickupTrash()
    {
        if (requireActiveMission && pakRT == null)
            pakRT = FindFirstObjectByType<NPCPakRT>();

        return !requireActiveMission || (pakRT != null && pakRT.IsMissionActive);
    }

    private void UpdateFloatEffects()
    {
        SetFloatEffectsActive(CanPickupTrash());
    }

    private void SetFloatEffectsActive(bool active, bool force = false)
    {
        if (!force && floatEffectsActive == active) return;
        floatEffectsActive = active;

        if (floatEffects == null) return;

        foreach (SimpleFloatEffect floatEffect in floatEffects)
        {
            if (floatEffect != null)
                floatEffect.SetFloating(active);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
