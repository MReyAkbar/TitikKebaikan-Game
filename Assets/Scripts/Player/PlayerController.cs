using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mengatur pergerakan pemain, animasi, dan interaksi dasar.
/// Mendukung WASD / Arrow Keys sesuai GDD Titik Kebaikan.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 4f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactableLayer;

    // Komponen
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerStats stats;

    // State
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down; // default facing down
    private bool isSprinting;
    private bool isCarryingNPC;

    // Animator parameter hashes (lebih efisien dari string)
    private static readonly int AnimMoveX    = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveY    = Animator.StringToHash("MoveY");
    private static readonly int AnimSpeed    = Animator.StringToHash("Speed");
    private static readonly int AnimCarrying = Animator.StringToHash("IsCarrying");

    // ─── Unity Lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        rb    = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();

        if (stats == null)
            Debug.LogError("[PlayerController] PlayerStats tidak ditemukan pada GameObject ini!");
    }

    private void Update()
    {
        HandleInteractInput();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Move();
    }

    // ─── Input (dipanggil oleh PlayerInput component / Input System) ────────

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed) TryInteract();
    }

    // ─── Movement ───────────────────────────────────────────────────────────

    private void Move()
    {
        float currentSpeed = baseSpeed;

        // Bonus kecepatan dari Reputation Tier
        if (stats != null)
            currentSpeed *= stats.SpeedMultiplier;

        if (isSprinting)
            currentSpeed *= sprintMultiplier;

        rb.linearVelocity = moveInput * currentSpeed;

        // Simpan arah terakhir untuk animasi idle
        if (moveInput != Vector2.zero)
            lastMoveDir = moveInput;
    }

    // ─── Animasi ────────────────────────────────────────────────────────────

    private void UpdateAnimator()
    {
        Vector2 dir = moveInput != Vector2.zero ? moveInput : lastMoveDir;

        animator.SetFloat(AnimMoveX, dir.x);
        animator.SetFloat(AnimMoveY, dir.y);
        animator.SetFloat(AnimSpeed, moveInput.magnitude);
        animator.SetBool(AnimCarrying, isCarryingNPC);
    }

    // ─── Interaksi ───────────────────────────────────────────────────────────

    private void HandleInteractInput() { /* ditangani oleh OnInteract callback */ }

    private void TryInteract()
    {
        // Cari objek interactable terdekat dalam radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayer);

        IInteractable closest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = interactable;
            }
        }

        closest?.Interact(this);
    }

    // ─── Pickup / Drop Sampah ────────────────────────────────────────────────

    public bool TryPickupTrash(TrashObject trash)
    {
        if (stats == null) return false;
        return stats.TryAddTrash(trash);
    }

    public void DropAllTrash()
    {
        stats?.ClearTrash();
    }

    // ─── NPC Escort ─────────────────────────────────────────────────────────

    public void SetCarryingNPC(bool value)
    {
        isCarryingNPC = value;
    }

    // ─── Gizmos Debug ───────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
