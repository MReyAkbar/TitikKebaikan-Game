using UnityEngine;

/// <summary>
/// Kendaraan sederhana untuk tahap berikutnya. Dipertahankan agar VehicleSpawner tidak putus.
/// </summary>
public class VehicleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float speedVariance = 1f;
    [SerializeField] private Vector2 moveDirection = Vector2.right;
    [SerializeField] private float stopDistance = 0.6f;
    [SerializeField] private float stopLineSearchDistance = 25f;
    [SerializeField] private float passedStopLineTolerance = 0.15f;

    [Header("Traffic")]
    [SerializeField] private TrafficStopLine assignedStopLine;
    [SerializeField] private TrafficStopLine[] assignedStopLines;

    [Header("Car Following")]
    [SerializeField] private bool avoidVehiclesAhead = true;
    [SerializeField] private float followDistance = 2.5f;
    [SerializeField] private float minimumGap = 1.2f;
    [SerializeField] private float laneTolerance = 0.45f;

    [Header("Despawn")]
    [SerializeField] private float despawnDistance = 35f;

    [Header("Penalty")]
    [SerializeField] private int playerHitPenalty = 10;

    private TrafficStopLine targetStopLine;
    private Rigidbody2D rb;
    private Vector2 spawnPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPoint = transform.position;
        speed += Random.Range(-speedVariance, speedVariance);
    }

    private void Start()
    {
        targetStopLine = FindNextStopLine();
    }

    public void SetStopLine(TrafficStopLine stopLine)
    {
        assignedStopLine = stopLine;
        assignedStopLines = null;
        targetStopLine = stopLine;
    }

    public void SetStopLines(TrafficStopLine[] stopLines)
    {
        assignedStopLines = stopLines;
        assignedStopLine = null;
        targetStopLine = FindNextStopLine();
    }

    public void SetMoveDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        moveDirection = direction.normalized;
        targetStopLine = FindNextStopLine();
    }

    public void SetSpawnPoint(Vector2 position)
    {
        spawnPoint = position;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (targetStopLine == null || HasPassedTargetStopLine())
            targetStopLine = FindNextStopLine();

        if (ShouldStopAtLine())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection.normalized * GetSafeSpeed();

        if (ShouldDespawn())
            Destroy(gameObject);
    }

    private bool ShouldDespawn()
    {
        Vector2 fromSpawn = (Vector2)transform.position - spawnPoint;
        float forwardDistance = Vector2.Dot(fromSpawn, moveDirection.normalized);
        return forwardDistance >= despawnDistance;
    }

    private bool ShouldStopAtLine()
    {
        if (targetStopLine == null) return false;
        if (targetStopLine.LinkedTrafficLight == null) return false;
        if (!targetStopLine.LinkedTrafficLight.IsRedLight) return false;

        Vector2 toStopLine = (Vector2)targetStopLine.transform.position - rb.position;
        float forwardDistance = Vector2.Dot(toStopLine, moveDirection.normalized);

        return forwardDistance >= -passedStopLineTolerance && forwardDistance <= stopDistance;
    }

    private bool HasPassedTargetStopLine()
    {
        if (targetStopLine == null) return false;

        Vector2 toStopLine = (Vector2)targetStopLine.transform.position - rb.position;
        float forwardDistance = Vector2.Dot(toStopLine, moveDirection.normalized);

        return forwardDistance < -passedStopLineTolerance;
    }

    private float GetSafeSpeed()
    {
        if (!avoidVehiclesAhead) return speed;

        VehicleController[] vehicles = FindObjectsByType<VehicleController>(FindObjectsSortMode.None);
        Vector2 direction = moveDirection.normalized;
        Vector2 sideDirection = new Vector2(-direction.y, direction.x);
        float nearestForwardDistance = float.MaxValue;

        foreach (VehicleController other in vehicles)
        {
            if (other == null || other == this) continue;
            if (Vector2.Dot(other.moveDirection.normalized, direction) < 0.85f) continue;

            Vector2 toOther = (Vector2)other.transform.position - rb.position;
            float forwardDistance = Vector2.Dot(toOther, direction);
            if (forwardDistance <= 0f || forwardDistance > followDistance) continue;

            float sideDistance = Mathf.Abs(Vector2.Dot(toOther, sideDirection));
            if (sideDistance > laneTolerance) continue;

            if (forwardDistance < nearestForwardDistance)
                nearestForwardDistance = forwardDistance;
        }

        if (nearestForwardDistance == float.MaxValue) return speed;
        if (nearestForwardDistance <= minimumGap) return 0f;

        float speedRatio = Mathf.InverseLerp(minimumGap, followDistance, nearestForwardDistance);
        return speed * speedRatio;
    }

    private TrafficStopLine FindNextStopLine()
    {
        TrafficStopLine[] stopLines = GetAvailableStopLines();
        TrafficStopLine nearest = null;
        float nearestForwardDistance = float.MaxValue;
        Vector2 direction = moveDirection.normalized;

        foreach (TrafficStopLine stopLine in stopLines)
        {
            Vector2 toStopLine = (Vector2)stopLine.transform.position - rb.position;
            float forwardDistance = Vector2.Dot(toStopLine, direction);
            if (forwardDistance < 0f || forwardDistance > stopLineSearchDistance) continue;

            if (forwardDistance < nearestForwardDistance)
            {
                nearestForwardDistance = forwardDistance;
                nearest = stopLine;
            }
        }

        return nearest;
    }

    private TrafficStopLine[] GetAvailableStopLines()
    {
        if (assignedStopLines != null && assignedStopLines.Length > 0)
            return assignedStopLines;

        if (assignedStopLine != null)
            return new[] { assignedStopLine };

        return FindObjectsByType<TrafficStopLine>(FindObjectsSortMode.None);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        NenekNPC nenek = other.GetComponent<NenekNPC>() ?? other.GetComponentInParent<NenekNPC>();
        if (nenek != null)
        {
            Debug.Log("[Vehicle] Menabrak Nenek!");
            SFXManager.Instance?.PlayVehicleHit();
            nenek.OnHitByVehicle();
            return;
        }

        if (!other.CompareTag("Player")) return;

        Debug.Log("[Vehicle] Menabrak pemain!");
        SFXManager.Instance?.PlayVehicleHit();
        EthicsManager.Instance?.AddPoints(-playerHitPenalty, "Tertabrak kendaraan");
        CheckpointManager.Instance?.RespawnPlayer(other.GetComponent<PlayerController>());
    }
}
