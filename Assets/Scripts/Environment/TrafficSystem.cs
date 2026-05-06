using UnityEngine;

// ════════════════════════════════════════════════════════════════
// ZebraCross.cs - Area penyeberangan zebra
// ════════════════════════════════════════════════════════════════

/// <summary>
/// Mendeteksi ketika pemain (dan Nenek jika misi aktif) melewati zebra cross.
/// Memberikan bonus Poin Etika.
/// Pastikan memiliki Collider2D dengan isTrigger = true.
/// </summary>
public class ZebraCross : MonoBehaviour
{
    [SerializeField] private int bonusPointsPlayer = 5;  // poin jika pemain saja
    private bool playerInZebra = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZebra = true;

            // Cek apakah pemain sedang escort Nenek
            NPCNenek nenek = FindFirstObjectByType<NPCNenek>();
            if (nenek != null && nenek.IsMissionActive)
            {
                nenek.OnCrossedZebra(); // Nenek dapat bonus lebih besar
            }
            else
            {
                // Poin kecil untuk patuh lalu lintas meski tidak ada misi
                PlayerStats stats = other.GetComponent<PlayerStats>();
                stats?.AddEthicsPoints(bonusPointsPlayer, "Menyeberang di zebra cross");
                GameHUD.Instance?.ShowFloatingText($"+{bonusPointsPlayer}", other.transform.position);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInZebra = false;
    }
}


// ════════════════════════════════════════════════════════════════
// TrafficLight.cs - Lampu merah/hijau
// ════════════════════════════════════════════════════════════════

/// <summary>
/// Lampu lalu lintas dengan siklus hijau-kuning-merah.
/// Pemain mendapat bonus poin saat menunggu lampu merah dan menyeberang saat hijau.
/// Kendaraan berhenti saat merah.
/// </summary>
public class TrafficLight : MonoBehaviour
{
    public enum LightState { Green, Yellow, Red }

    [Header("Timing (detik)")]
    [SerializeField] private float greenDuration  = 6f;
    [SerializeField] private float yellowDuration = 2f;
    [SerializeField] private float redDuration    = 5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer lightRenderer;
    [SerializeField] private Color colorGreen  = Color.green;
    [SerializeField] private Color colorYellow = Color.yellow;
    [SerializeField] private Color colorRed    = Color.red;

    [Header("Poin")]
    [SerializeField] private int obeyBonus = 8;

    // State
    private LightState currentState = LightState.Green;
    private float timer;

    public LightState State     => currentState;
    public bool IsRedLight      => currentState == LightState.Red;
    public bool IsGreenLight    => currentState == LightState.Green;

    // ─── Unity Lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        timer = greenDuration;
        UpdateVisual();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0) CycleLight();
    }

    private void CycleLight()
    {
        currentState = currentState switch
        {
            LightState.Green  => LightState.Yellow,
            LightState.Yellow => LightState.Red,
            LightState.Red    => LightState.Green,
            _                 => LightState.Green,
        };

        timer = currentState switch
        {
            LightState.Green  => greenDuration,
            LightState.Yellow => yellowDuration,
            LightState.Red    => redDuration,
            _                 => greenDuration,
        };

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (lightRenderer == null) return;
        lightRenderer.color = currentState switch
        {
            LightState.Green  => colorGreen,
            LightState.Yellow => colorYellow,
            LightState.Red    => colorRed,
            _                 => colorGreen,
        };
    }

    // ─── Pemain menerobos lampu merah ────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (IsRedLight)
        {
            stats.AddEthicsPoints(-10, "Menerobos lampu merah");
            GameHUD.Instance?.ShowFloatingText("-10 Melanggar lampu!", other.transform.position);
        }
        else if (IsGreenLight)
        {
            stats.AddEthicsPoints(obeyBonus, "Menyeberang saat lampu hijau");
            GameHUD.Instance?.ShowFloatingText($"+{obeyBonus} Patuh!", other.transform.position);
        }
    }
}


// ════════════════════════════════════════════════════════════════
// VehicleController.cs - Kendaraan yang bergerak di jalanan
// ════════════════════════════════════════════════════════════════

/// <summary>
/// Kendaraan yang bergerak di jalan raya sebagai rintangan dinamis.
/// Berhenti di lampu merah. Mematikan (damage) jika menabrak pemain/nenek.
/// </summary>
public class VehicleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed        = 4f;
    [SerializeField] private float speedVariance = 1f; // variasi kecepatan antar kendaraan
    [SerializeField] private Vector2 moveDirection = Vector2.right;

    [Header("Despawn")]
    [SerializeField] private float despawnX = 20f; // X position untuk destroy

    // State
    private TrafficLight linkedLight;
    private bool isStopped;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        speed += Random.Range(-speedVariance, speedVariance);
    }

    private void Start()
    {
        // Cari traffic light terdekat di jalur ini
        linkedLight = FindNearestTrafficLight();
    }

    private void FixedUpdate()
    {
        if (isStopped)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Cek lampu merah
        if (linkedLight != null && linkedLight.IsRedLight &&
            Vector2.Distance(transform.position, linkedLight.transform.position) < 3f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection.normalized * speed;

        // Despawn jika sudah melewati batas
        if (Mathf.Abs(transform.position.x) > Mathf.Abs(despawnX))
            Destroy(gameObject);
    }

    private TrafficLight FindNearestTrafficLight()
    {
        TrafficLight[] lights = FindObjectsByType<TrafficLight>(FindObjectsSortMode.None);
        TrafficLight nearest  = null;
        float minDist         = float.MaxValue;

        foreach (var light in lights)
        {
            float d = Vector2.Distance(transform.position, light.transform.position);
            if (d < minDist) { minDist = d; nearest = light; }
        }
        return nearest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kendaraan menabrak pemain
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Vehicle] Menabrak pemain!");
            CheckpointManager.Instance?.RespawnPlayer(other.GetComponent<PlayerController>());
        }
    }
}
