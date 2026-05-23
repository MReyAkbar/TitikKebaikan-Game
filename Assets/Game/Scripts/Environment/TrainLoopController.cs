using UnityEngine;

/// <summary>
/// Obstacle kereta yang bergerak di rel, loop, lalu memberi jeda kosong untuk pemain lewat.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TrainLoopController : MonoBehaviour
{
    [Header("Route")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private Vector2 moveDirection = Vector2.left;
    [SerializeField] private float travelDistance = 45f;

    [Header("Timing")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float gapDuration = 3f;
    [SerializeField] private float arrivalTolerance = 0.03f;
    [SerializeField] private bool hideDuringGap = true;

    [Header("Collision")]
    [SerializeField] private bool autoConfigureCollider = true;
    [SerializeField] private string vehicleTag = "Vehicle";
    [SerializeField] private string vehicleLayerName = "Vehicle";
    [SerializeField] private int playerHitPenalty = 10;
    [SerializeField] private float hitCooldown = 0.5f;

    private Rigidbody2D rb;
    private Renderer[] renderers;
    private Collider2D[] colliders;
    private Vector2 runtimeStart;
    private Vector2 runtimeEnd;
    private float gapTimer;
    private bool isMoving;
    private int lastHitInstanceId;
    private float lastHitTime = -999f;

    private void Reset()
    {
        moveDirection = Vector2.left;
        travelDistance = 45f;
        speed = 7f;
        gapDuration = 3f;
        hideDuringGap = true;
        autoConfigureCollider = true;
        vehicleTag = "Vehicle";
        vehicleLayerName = "Vehicle";
        playerHitPenalty = 10;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body == null)
            body = gameObject.AddComponent<Rigidbody2D>();

        ConfigureRigidbody(body);

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider2D>();

        box.isTrigger = true;
        FitBoxColliderToSprite(box);
        ApplyVehicleIdentity();
    }

    private void OnValidate()
    {
        if (moveDirection == Vector2.zero)
            moveDirection = Vector2.left;

        moveDirection = moveDirection.normalized;
        travelDistance = Mathf.Max(0f, travelDistance);
        speed = Mathf.Max(0f, speed);
        startDelay = Mathf.Max(0f, startDelay);
        gapDuration = Mathf.Max(0f, gapDuration);
        arrivalTolerance = Mathf.Max(0.001f, arrivalTolerance);
        hitCooldown = Mathf.Max(0f, hitCooldown);

        if (!Application.isPlaying && autoConfigureCollider)
        {
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null)
                FitBoxColliderToSprite(box);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ConfigureRigidbody(rb);

        if (autoConfigureCollider)
        {
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null)
                FitBoxColliderToSprite(box);
        }

        CacheRenderersAndColliders();
        ApplyVehicleIdentity();
    }

    private void Start()
    {
        RefreshRuntimeRoute();

        if (startDelay > 0f)
            BeginGap(startDelay);
        else
            BeginRun();
    }

    private void FixedUpdate()
    {
        if (!isMoving)
        {
            TickGap();
            return;
        }

        if (rb == null) return;

        Vector2 nextPosition = Vector2.MoveTowards(rb.position, runtimeEnd, speed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);

        if (Vector2.Distance(nextPosition, runtimeEnd) <= arrivalTolerance)
            BeginGap(gapDuration);
    }

    private void BeginRun()
    {
        RefreshRuntimeRoute();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = runtimeStart;
        }
        else
        {
            transform.position = runtimeStart;
        }

        SetObstacleActive(true);
        isMoving = true;
        gapTimer = 0f;
    }

    private void BeginGap(float duration)
    {
        isMoving = false;
        gapTimer = Mathf.Max(0f, duration);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = runtimeStart;
        }
        else
        {
            transform.position = runtimeStart;
        }

        SetObstacleActive(!hideDuringGap);

        if (gapTimer <= 0f)
            BeginRun();
    }

    private void TickGap()
    {
        if (gapTimer <= 0f) return;

        gapTimer -= Time.fixedDeltaTime;
        if (gapTimer <= 0f)
            BeginRun();
    }

    private void RefreshRuntimeRoute()
    {
        runtimeStart = startPoint != null ? (Vector2)startPoint.position : (Vector2)transform.position;
        runtimeEnd = endPoint != null ? (Vector2)endPoint.position : runtimeStart + moveDirection.normalized * travelDistance;
    }

    private void ConfigureRigidbody(Rigidbody2D target)
    {
        if (target == null) return;

        target.bodyType = RigidbodyType2D.Kinematic;
        target.gravityScale = 0f;
        target.constraints = RigidbodyConstraints2D.FreezeRotation;
        target.linearVelocity = Vector2.zero;
        target.angularVelocity = 0f;
    }

    private void CacheRenderersAndColliders()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    private void SetObstacleActive(bool active)
    {
        if (renderers != null)
        {
            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer != null)
                    targetRenderer.enabled = active;
            }
        }

        if (colliders != null)
        {
            foreach (Collider2D targetCollider in colliders)
            {
                if (targetCollider != null)
                    targetCollider.enabled = active;
            }
        }
    }

    private void FitBoxColliderToSprite(BoxCollider2D box)
    {
        if (box == null) return;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        Bounds spriteBounds = spriteRenderer.bounds;
        Vector3 localCenter = transform.InverseTransformPoint(spriteBounds.center);
        Vector3 localSize = transform.InverseTransformVector(spriteBounds.size);

        box.offset = new Vector2(localCenter.x, localCenter.y);
        box.size = new Vector2(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y));
        box.isTrigger = true;
    }

    private void ApplyVehicleIdentity()
    {
        int vehicleLayer = LayerMask.NameToLayer(vehicleLayerName);
        if (vehicleLayer >= 0)
            SetLayerRecursively(transform, vehicleLayer);

        TryApplyVehicleTag();
    }

    private void SetLayerRecursively(Transform target, int layer)
    {
        target.gameObject.layer = layer;

        foreach (Transform child in target)
            SetLayerRecursively(child, layer);
    }

    private void TryApplyVehicleTag()
    {
        if (string.IsNullOrWhiteSpace(vehicleTag)) return;

        try
        {
            if (!CompareTag(vehicleTag))
                gameObject.tag = vehicleTag;
        }
        catch (UnityException)
        {
            Debug.LogWarning($"[{name}] Tag '{vehicleTag}' belum ada di Tag Manager.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (other == null) return;
        if (!isMoving && hideDuringGap) return;
        if (IsOnCooldown(other)) return;

        NenekNPC nenek = other.GetComponent<NenekNPC>() ?? other.GetComponentInParent<NenekNPC>();
        if (nenek != null)
        {
            Debug.Log("[Train] Menabrak Nenek!");
            SFXManager.Instance?.PlayVehicleHit();
            nenek.OnHitByVehicle();
            RegisterHit(other);
            return;
        }

        if (!other.CompareTag("Player")) return;

        Debug.Log("[Train] Menabrak pemain!");
        SFXManager.Instance?.PlayVehicleHit();
        EthicsManager.Instance?.AddPoints(-playerHitPenalty, "Tertabrak kereta");
        CheckpointManager.Instance?.RespawnPlayer(other.GetComponent<PlayerController>());
        RegisterHit(other);
    }

    private bool IsOnCooldown(Collider2D other)
    {
        int instanceId = other.GetInstanceID();
        return instanceId == lastHitInstanceId && Time.time < lastHitTime + hitCooldown;
    }

    private void RegisterHit(Collider2D other)
    {
        lastHitInstanceId = other.GetInstanceID();
        lastHitTime = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 previewStart = startPoint != null ? (Vector2)startPoint.position : (Vector2)transform.position;
        Vector2 previewEnd = endPoint != null ? (Vector2)endPoint.position : previewStart + moveDirection.normalized * travelDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(previewStart, previewEnd);
        Gizmos.DrawWireSphere(previewStart, 0.25f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(previewEnd, 0.25f);
    }
}
