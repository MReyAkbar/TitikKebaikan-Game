using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// GameHUD: mengelola semua elemen UI dalam game.
/// - Poin Etika bar (pojok kiri atas)
/// - Reputation tier badge
/// - Timer misi
/// - Daftar misi
/// - Inventory sampah
/// - Floating text
/// - Notifikasi
/// Semua metode aman dipanggil meski referensi null (tidak crash).
/// </summary>
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    // ─── UI References ───────────────────────────────────────────────────────

    [Header("Ethics Points")]
    [SerializeField] private Slider    ethicsBar;
    [SerializeField] private TextMeshProUGUI ethicsText;
    [SerializeField] private int       maxDisplayPoints = 200;

    [Header("Reputation Tier")]
    [SerializeField] private TextMeshProUGUI tierText;
    [SerializeField] private Image           tierIcon;
    [SerializeField] private Sprite[]        tierSprites; // [0]=Jelek, [1]=Lumayan, [2]=Bagus

    [Header("Mission Panel (kanan atas)")]
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject      timerPanel;

    [Header("Trash Inventory (kanan bawah)")]
    [SerializeField] private TextMeshProUGUI trashCountText;
    [SerializeField] private Image[]         trashSlots; // ikon slot sampah

    [Header("Floating Text")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Canvas      worldCanvas;

    [Header("Notifications")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float           notificationDuration = 2.5f;

    [Header("End Screen")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TextMeshProUGUI finalPointsText;
    [SerializeField] private TextMeshProUGUI finalTierText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    // ─── Singleton ───────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Subscribe ke PlayerStats
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.OnPointsChanged += UpdateEthicsBar;
            stats.OnTierChanged   += UpdateTierDisplay;
            stats.OnTrashChanged  += UpdateTrashUI;
        }

        // Initial state
        endScreenPanel?.SetActive(false);
        pauseMenuPanel?.SetActive(false);
        timerPanel?.SetActive(false);

        GameManager.Instance?.OnStateChanged += OnGameStateChanged;
    }

    // ─── Ethics Points ───────────────────────────────────────────────────────

    public void UpdateEthicsBar(int points)
    {
        if (ethicsBar  != null) ethicsBar.value  = (float)points / maxDisplayPoints;
        if (ethicsText != null) ethicsText.text  = $"Poin Etika: {points}";
    }

    // ─── Reputation Tier ─────────────────────────────────────────────────────

    public void UpdateTierDisplay(PlayerStats.ReputationTier tier)
    {
        if (tierText != null)
        {
            tierText.text = tier switch
            {
                PlayerStats.ReputationTier.Lumayan => "⭐ Lumayan",
                PlayerStats.ReputationTier.Bagus   => "⭐⭐ Bagus",
                _                                  => "😐 Jelek",
            };
        }

        if (tierIcon != null && tierSprites != null && (int)tier < tierSprites.Length)
            tierIcon.sprite = tierSprites[(int)tier];

        // Flash animasi naik tier
        ShowNotification($"Reputasi naik! Kamu sekarang: {tier}");
    }

    // ─── Timer ───────────────────────────────────────────────────────────────

    public void UpdateTimer(float seconds)
    {
        timerPanel?.SetActive(seconds > 0);

        if (timerText != null)
        {
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            timerText.text = $"{m:00}:{s:00}";

            // Warna merah saat waktu hampir habis
            timerText.color = seconds < 20f ? Color.red : Color.white;
        }
    }

    // ─── Mission Text ─────────────────────────────────────────────────────────

    public void UpdateMissionText(string text)
    {
        if (missionText != null) missionText.text = text;
    }

    public void UpdateMissionStatus(MissionType type, MissionManager.MissionStatus status)
    {
        string icon = status switch
        {
            MissionManager.MissionStatus.Active    => "🔵",
            MissionManager.MissionStatus.Completed => "✅",
            MissionManager.MissionStatus.Failed    => "❌",
            _                                      => "⚪",
        };
        UpdateMissionText($"{icon} {type}: {status}");
    }

    // ─── Trash Inventory ─────────────────────────────────────────────────────

    public void UpdateTrashUI(int current, int max)
    {
        if (trashCountText != null)
            trashCountText.text = $"Sampah: {current}/{max}";

        // Update slot icons
        if (trashSlots != null)
        {
            for (int i = 0; i < trashSlots.Length; i++)
                trashSlots[i].color = i < current ? Color.green : Color.gray;
        }
    }

    // ─── Floating Text ────────────────────────────────────────────────────────

    public void ShowFloatingText(string text, Vector3 worldPos)
    {
        if (floatingTextPrefab == null || worldCanvas == null) return;

        GameObject go = Instantiate(floatingTextPrefab, worldCanvas.transform);
        var tmp       = go.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;

        // Konversi world position ke screen position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        go.GetComponent<RectTransform>().position = screenPos;

        StartCoroutine(FloatAndFade(go));
    }

    private IEnumerator FloatAndFade(GameObject go)
    {
        var tmp  = go.GetComponent<TextMeshProUGUI>();
        var rect = go.GetComponent<RectTransform>();
        float t  = 0f;

        while (t < 1.2f)
        {
            t += Time.deltaTime;
            rect.anchoredPosition += Vector2.up * 40f * Time.deltaTime;
            if (tmp != null) tmp.alpha = 1f - (t / 1.2f);
            yield return null;
        }

        Destroy(go);
    }

    // ─── Notification ─────────────────────────────────────────────────────────

    public void ShowNotification(string text)
    {
        if (notificationText == null) return;
        StopCoroutine(nameof(HideNotification));
        notificationText.text = text;
        notificationText.gameObject.SetActive(true);
        StartCoroutine(HideNotification());
    }

    private IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(notificationDuration);
        notificationText?.gameObject.SetActive(false);
    }

    // ─── End Screen ───────────────────────────────────────────────────────────

    public void ShowEndScreen()
    {
        endScreenPanel?.SetActive(true);

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats == null) return;

        if (finalPointsText != null) finalPointsText.text = $"Poin Etika: {stats.EthicsPoints}";
        if (finalTierText   != null) finalTierText.text   = $"Status: {stats.Tier}";
    }

    // ─── Pause ───────────────────────────────────────────────────────────────

    private void OnGameStateChanged(GameManager.GameState state)
    {
        pauseMenuPanel?.SetActive(state == GameManager.GameState.Paused);
    }

    // ─── Button Callbacks (dihubungkan via Inspector) ────────────────────────

    public void OnResumeButton()  => GameManager.Instance?.ResumeGame();
    public void OnRestartButton() => GameManager.Instance?.RestartGame();
    public void OnMenuButton()    => GameManager.Instance?.ReturnToMainMenu();
}
