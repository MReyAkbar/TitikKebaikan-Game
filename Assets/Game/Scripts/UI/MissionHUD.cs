using UnityEngine;
using TMPro;

/// <summary>
/// MissionHUD v2 — tambah Instance agar bisa diakses dari NenekNPC.
/// </summary>
public class MissionHUD : MonoBehaviour
{
    public static MissionHUD Instance { get; private set; }

    [Header("UI Text")]
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI nenekMissionText;
    public TextMeshProUGUI pakRTMissionText;
    public TextMeshProUGUI trashText;
    public TextMeshProUGUI reputationText;

    [Header("Reputation Colors")]
    [SerializeField] private Color jelekColor = new Color(1f, 0.25f, 0.2f);
    [SerializeField] private Color lumayanColor = new Color(1f, 0.82f, 0.2f);
    [SerializeField] private Color bagusColor = new Color(0.25f, 0.9f, 0.35f);

    private bool isEthicsSubscribed = false;
    private bool isPlayerStatsSubscribed = false;
    private PlayerStats playerStats;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (!isEthicsSubscribed && EthicsManager.Instance != null)
        {
            EthicsManager.Instance.OnPointsChanged += UpdatePointsUI;
            isEthicsSubscribed = true;
            UpdatePointsUI(EthicsManager.Instance.GetPoints());
            SetNenekMissionText("Belum dimulai");
            SetPakRTMissionText("Belum dimulai");
        }

        if (!isPlayerStatsSubscribed)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnTrashChanged += UpdateTrashUI;
                playerStats.OnTierChanged += UpdateReputationUI;
                isPlayerStatsSubscribed = true;
                UpdateTrashUI(playerStats.CurrentTrash, playerStats.MaxTrash);
                UpdateReputationUI(playerStats.Tier);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (EthicsManager.Instance != null)
            EthicsManager.Instance.OnPointsChanged -= UpdatePointsUI;

        if (playerStats != null)
        {
            playerStats.OnTrashChanged -= UpdateTrashUI;
            playerStats.OnTierChanged -= UpdateReputationUI;
        }
    }

    public void UpdatePointsUI(int points)
    {
        if (pointsText != null)
            pointsText.text = $"Poin Etika: {points}";
    }

    public void SetMissionText(string text)
    {
        if (missionText != null)
            missionText.text = $"Misi: {text}";
    }

    public void SetNenekMissionText(string text)
    {
        if (nenekMissionText != null)
            nenekMissionText.text = $"Nenek: {text}";
        else
            SetMissionText($"Nenek - {text}");
    }

    public void SetPakRTMissionText(string text)
    {
        if (pakRTMissionText != null)
            pakRTMissionText.text = $"Pak RT: {text}";
        else
            SetMissionText($"Pak RT - {text}");
    }

    public void UpdateTrashUI(int current, int max)
    {
        if (trashText != null)
            trashText.text = $"Inventory: {current}/{max}";
    }

    public void UpdateReputationUI(PlayerStats.ReputationTier tier)
    {
        if (reputationText != null)
        {
            reputationText.text = $"Reputasi: {tier}";
            reputationText.color = GetReputationColor(tier);
        }
    }

    private Color GetReputationColor(PlayerStats.ReputationTier tier)
    {
        return tier switch
        {
            PlayerStats.ReputationTier.Lumayan => lumayanColor,
            PlayerStats.ReputationTier.Bagus => bagusColor,
            _ => jelekColor,
        };
    }
}
