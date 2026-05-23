using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// NenekNPC versi Rigidbody2D. Nenek mengikuti player sambil mengecek collider 2D.
/// </summary>
public class NenekNPC : MonoBehaviour
{
    public enum MissionState { Idle, Talking, Active, Completed }
    public MissionState State { get; private set; } = MissionState.Idle;

    [Header("Referensi")]
    public GameObject promptUI;
    public GameObject chatIcon;
    public Transform marketDestination;

    [Header("Follow Settings")]
    public float followSpeed = 2.5f;
    public float followDistance = 1.5f;
    public float interactRadius = 1.8f;
    public LayerMask blockingLayers;

    [Header("Anti Stuck")]
    public bool enableStuckEscape = true;
    public float stuckDurationBeforeEscape = 1.25f;
    public float escapeClearanceRadius = 0.25f;
    public float escapePlayerDistance = 1.4f;
    public float maxEscapeDistanceFromPlayer = 7f;

    [Header("Poin")]
    public int pointsOnComplete = 50;
    public int pointsPenaltyOnHit = 10;

    [Header("Dialog")]
    [TextArea(2, 4)]
    public string dialogAwal = "Aduh Nak, Nenek mau ke pasar tapi takut\nnyeberang jalan sendiri. Mau temani Nenek?";
    [TextArea(2, 4)]
    public string dialogSelesai = "Alhamdulillah, terima kasih banyak ya Nak!\nNenek sangat senang kamu mau membantu.";

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool playerInRange;
    private Vector2 startPosition;
    private float stuckTimer;
    private readonly RaycastHit2D[] castHits = new RaycastHit2D[4];
    private ContactFilter2D movementFilter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
        else
        {
            Debug.LogWarning("[Nenek] Rigidbody2D tidak ditemukan! Tambahkan komponen Rigidbody2D.");
        }

        movementFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = blockingLayers
        };

        if (promptUI != null)
            promptUI.SetActive(false);

        UpdateChatIcon();
    }

    private void Start()
    {
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("[Nenek] Player tidak ditemukan! Pastikan Player punya tag 'Player'.");

        StopMovement();
    }

    private void Update()
    {
        CheckPlayerProximity();
    }

    private void FixedUpdate()
    {
        if (State == MissionState.Active)
            FollowPlayer();
        else
            StopMovement();
    }

    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;
        if (State == MissionState.Completed)
        {
            HideInteractionUI();
            return;
        }
        if (State == MissionState.Active) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRadius;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (promptUI != null)
                promptUI.SetActive(inRange);
        }

        if (inRange && Keyboard.current.eKey.wasPressedThisFrame)
            OnPlayerInteract();
    }

    private void OnPlayerInteract()
    {
        if (State == MissionState.Idle)
        {
            if (MissionManager.Instance != null &&
                MissionManager.Instance.HasActiveMissionOtherThan(MissionType.EscortNenek))
            {
                ShowSystemHint("Selesaikan misi yang sedang berjalan terlebih dahulu.");
                return;
            }

            StartDialog();
        }
    }

    private void StartDialog()
    {
        State = MissionState.Talking;

        if (promptUI != null)
            promptUI.SetActive(false);

        UpdateChatIcon();

        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Nenek", dialogAwal, StartMission);
        else
        {
            Debug.Log("[Nenek] " + dialogAwal);
            StartMission();
        }
    }

    private void ShowCompletionDialog()
    {
        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Nenek", dialogSelesai);
        else
            Debug.Log("[Nenek] " + dialogSelesai);
    }

    private void ShowSystemHint(string message)
    {
        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Petunjuk Sistem", message);
        else
            Debug.Log("[Petunjuk Sistem] " + message);
    }

    private void StartMission()
    {
        State = MissionState.Active;
        MissionManager.Instance?.StartMission(MissionType.EscortNenek, this);
        UpdateChatIcon();
        CameraFocusCue.Instance?.ShowTarget(marketDestination);

        Debug.Log("[Nenek] Misi dimulai - mengantar Nenek ke pasar.");

        if (MissionHUD.Instance != null)
            MissionHUD.Instance.SetNenekMissionText("Antar ke pasar");
    }

    private void CompleteMission()
    {
        if (State == MissionState.Completed) return;

        State = MissionState.Completed;
        StopMovement();
        HideInteractionUI();

        if (EthicsManager.Instance != null)
            EthicsManager.Instance.AddPoints(pointsOnComplete, "Berhasil mengantar Nenek ke pasar");

        if (MissionHUD.Instance != null)
            MissionHUD.Instance.SetNenekMissionText("Selesai");

        MissionManager.Instance?.CompleteMission(MissionType.EscortNenek);

        Debug.Log("[Nenek] Misi SELESAI!");
        ShowCompletionDialog();
    }

    public void OnHitByVehicle()
    {
        if (State != MissionState.Active) return;

        State = MissionState.Idle;
        StopMovement();
        UpdateChatIcon();

        if (rb != null)
            rb.position = startPosition;
        else
            transform.position = startPosition;

        EthicsManager.Instance?.AddPoints(-pointsPenaltyOnHit, "Nenek tertabrak kendaraan");
        MissionManager.Instance?.ResetMission(MissionType.EscortNenek);
        MissionHUD.Instance?.SetNenekMissionText("Gagal - bicara lagi dengan Nenek");

        Debug.Log("[Nenek] Misi gagal: Nenek tertabrak kendaraan.");

        if (DialogBox.Instance != null)
            DialogBox.Instance.Show("Petunjuk Sistem", "Nenek tertabrak kendaraan. Bantu Nenek lagi dari awal.");
    }

    private void UpdateChatIcon()
    {
        if (chatIcon == null) return;

        bool canStartInteraction = State == MissionState.Idle || State == MissionState.Talking;
        chatIcon.SetActive(canStartInteraction);
    }

    private void HideInteractionUI()
    {
        playerInRange = false;

        if (promptUI != null)
            promptUI.SetActive(false);

        if (chatIcon != null)
            chatIcon.SetActive(false);
    }

    private void FollowPlayer()
    {
        if (playerTransform == null || rb == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= followDistance)
        {
            StopMovement();
            ResetStuckTimer();
            return;
        }

        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
        float moveDistance = followSpeed * Time.fixedDeltaTime;

        bool moved = MoveWithCollision(direction, moveDistance);
        UpdateStuckEscape(dist, moved);

        if (sr != null && direction.sqrMagnitude > 0.1f)
            sr.flipX = direction.x < 0f;
    }

    private bool MoveWithCollision(Vector2 direction, float moveDistance)
    {
        if (TryMove(direction, moveDistance)) return true;

        Vector2 slideX = new Vector2(direction.x, 0f).normalized;
        if (slideX != Vector2.zero && TryMove(slideX, moveDistance)) return true;

        Vector2 slideY = new Vector2(0f, direction.y).normalized;
        if (slideY != Vector2.zero)
            return TryMove(slideY, moveDistance);

        return false;
    }

    private bool TryMove(Vector2 direction, float moveDistance)
    {
        int hitCount = rb.Cast(direction, movementFilter, castHits, moveDistance);
        if (hitCount > 0) return false;

        rb.MovePosition(rb.position + direction * moveDistance);
        return true;
    }

    private void UpdateStuckEscape(float distanceToPlayer, bool moved)
    {
        if (!enableStuckEscape)
        {
            ResetStuckTimer();
            return;
        }

        if (moved)
        {
            ResetStuckTimer();
            return;
        }

        stuckTimer += Time.fixedDeltaTime;
        bool tooFarFromPlayer = distanceToPlayer >= maxEscapeDistanceFromPlayer;
        bool stuckLongEnough = stuckTimer >= stuckDurationBeforeEscape;

        if (!stuckLongEnough && !tooFarFromPlayer) return;

        if (TryEscapeNearPlayer())
            ResetStuckTimer();
    }

    private bool TryEscapeNearPlayer()
    {
        if (playerTransform == null || rb == null) return false;

        Vector2 playerPosition = playerTransform.position;
        Vector2 fromPlayerToNenek = (rb.position - playerPosition).normalized;
        if (fromPlayerToNenek == Vector2.zero)
            fromPlayerToNenek = Vector2.down;

        Vector2[] directions =
        {
            fromPlayerToNenek,
            Vector2.down,
            Vector2.up,
            Vector2.left,
            Vector2.right,
            new Vector2(-1f, -1f).normalized,
            new Vector2(1f, -1f).normalized,
            new Vector2(-1f, 1f).normalized,
            new Vector2(1f, 1f).normalized
        };

        foreach (Vector2 direction in directions)
        {
            Vector2 candidate = playerPosition + direction * escapePlayerDistance;
            if (IsEscapePositionClear(candidate))
            {
                rb.position = candidate;
                rb.linearVelocity = Vector2.zero;
                return true;
            }
        }

        return false;
    }

    private bool IsEscapePositionClear(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, escapeClearanceRadius, blockingLayers);
        return hit == null;
    }

    private void ResetStuckTimer()
    {
        stuckTimer = 0f;
    }

    private void StopMovement()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Market") && State == MissionState.Active)
            CompleteMission();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, escapeClearanceRadius);
    }
}
