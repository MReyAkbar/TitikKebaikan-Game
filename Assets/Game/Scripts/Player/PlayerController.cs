using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mengatur pergerakan pemain, animasi, dan interaksi dasar.
/// Mendukung WASD / Arrow Keys sesuai GDD Titik Kebaikan.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 4f;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // Komponen
    private Rigidbody2D rb;
    private PlayerStats stats;

    // State
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down; // default facing down
    private bool isCarryingNPC;

    // Animator parameter hashes (lebih efisien dari string)
    private static readonly int AnimMoveX    = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveY    = Animator.StringToHash("MoveY");
    private static readonly int AnimLastInputX = Animator.StringToHash("LastInputX");
    private static readonly int AnimLastInputY = Animator.StringToHash("LastInputY");
    private static readonly int AnimIsWalking = Animator.StringToHash("IsWalking");

    // ─── Unity Lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        rb    = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = FindAnimator();

        if (animator != null && animator.runtimeAnimatorController == null)
            animator = FindAnimator();

        stats = GetComponent<PlayerStats>();

        if (stats == null)
            Debug.LogError("[PlayerController] PlayerStats tidak ditemukan pada GameObject ini!");

        if (animator == null)
            Debug.LogError("[PlayerController] Animator tidak ditemukan pada Player atau child-nya!");
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
        // Sprint manual dimatikan agar bonus kecepatan dari reputasi lebih terasa.
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

        rb.linearVelocity = moveInput * currentSpeed;

        // Simpan arah terakhir untuk animasi idle
        if (moveInput != Vector2.zero)
            lastMoveDir = moveInput;
    }

    // ─── Animasi ────────────────────────────────────────────────────────────

    private void UpdateAnimator()
    {
        if (animator == null) return;

        bool isWalking = moveInput.sqrMagnitude > 0.001f;

        animator.SetFloat(AnimMoveX, moveInput.x);
        animator.SetFloat(AnimMoveY, moveInput.y);
        animator.SetFloat(AnimLastInputX, lastMoveDir.x);
        animator.SetFloat(AnimLastInputY, lastMoveDir.y);
        animator.SetBool(AnimIsWalking, isWalking);
    }

    private Animator FindAnimator()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator candidate in animators)
        {
            if (candidate != null && candidate.runtimeAnimatorController != null)
                return candidate;
        }

        return animators.Length > 0 ? animators[0] : null;
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
