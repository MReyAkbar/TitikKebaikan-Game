using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menampilkan popup kecil di dekat player saat Poin Etika bertambah atau berkurang.
/// </summary>
public class PointPopupManager : MonoBehaviour
{
    public static PointPopupManager Instance { get; private set; }

    [Header("Target")]
    [SerializeField] private Transform playerTransform;

    [Header("Popup")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float randomXOffset = 0.25f;
    [SerializeField] private float floatDistance = 0.8f;
    [SerializeField] private float duration = 1.1f;
    [SerializeField] private float fontSize = 4f;
    [SerializeField] private TMP_FontAsset popupFontAsset;
    [SerializeField] private Font trueTypeFont;
    [SerializeField] private string sortingLayerName = "UI";
    [SerializeField] private int sortingOrder = 1000;
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField, Range(0f, 1f)] private float outlineWidth = 0.25f;
    [SerializeField] private Color positiveColor = new Color(0.25f, 1f, 0.35f, 1f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.25f, 0.25f, 1f);

    [Header("Progress Feedback")]
    [SerializeField] private Vector3 feedbackWorldOffset = new Vector3(0f, 1.65f, 0f);
    [SerializeField] private Color tierLumayanColor = new Color(1f, 0.82f, 0.2f, 1f);
    [SerializeField] private Color tierBagusColor = new Color(0.25f, 1f, 0.35f, 1f);
    [SerializeField] private Color missionCompleteColor = new Color(0.35f, 0.9f, 1f, 1f);

    [Header("Tier Notification UI")]
    [SerializeField] private GameObject tierNotificationPanel;
    [SerializeField] private TextMeshProUGUI tierNotificationHeaderText;
    [SerializeField] private TextMeshProUGUI tierNotificationMessageText;
    [SerializeField] private CanvasGroup tierNotificationCanvasGroup;
    [SerializeField] private string tierNotificationHeader = "Petunjuk Sistem";
    [SerializeField] private float notificationDuration = 2.4f;
    [SerializeField] private Vector2 notificationSize = new Vector2(520f, 92f);
    [SerializeField] private Vector2 notificationAnchoredPosition = new Vector2(0f, -72f);
    [SerializeField] private Color notificationBackgroundColor = new Color(0.06f, 0.07f, 0.08f, 0.88f);
    [SerializeField] private Color notificationHeaderColor = new Color(0.85f, 0.95f, 1f, 1f);
    [SerializeField] private float notificationHeaderFontSize = 20f;
    [SerializeField] private float notificationMessageFontSize = 28f;

    private bool isSubscribed;
    private TMP_FontAsset generatedFontAsset;
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (tierNotificationPanel != null)
            tierNotificationPanel.SetActive(false);
    }

    private void Update()
    {
        FindPlayerIfNeeded();

        if (!isSubscribed && EthicsManager.Instance != null)
        {
            EthicsManager.Instance.OnPointsDeltaChanged += ShowPointPopup;
            isSubscribed = true;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (EthicsManager.Instance != null)
            EthicsManager.Instance.OnPointsDeltaChanged -= ShowPointPopup;
    }

    public void ShowTierUpFeedback(PlayerStats.ReputationTier tier)
    {
        if (tier == PlayerStats.ReputationTier.Jelek) return;

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        float speedMultiplier = stats != null ? stats.SpeedMultiplier : 1f;

        Color color = tier == PlayerStats.ReputationTier.Bagus ? tierBagusColor : tierLumayanColor;
        ShowTierNotification($"Reputasi naik: {tier}\nKecepatan bertambah x{speedMultiplier:0.##}", color);
    }

    public void ShowMissionCompleteFeedback(MissionType missionType)
    {
        ShowFeedbackPopup($"Misi selesai: {GetMissionLabel(missionType)}", missionCompleteColor);
    }

    private void ShowPointPopup(int delta, string reason)
    {
        ShowPopup($"{(delta > 0 ? "+" : "")}{delta}", delta > 0 ? positiveColor : negativeColor, worldOffset);
    }

    private void ShowFeedbackPopup(string message, Color color)
    {
        ShowPopup(message, color, feedbackWorldOffset);
    }

    private void ShowTierNotification(string message, Color messageColor)
    {
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(ShowTierNotificationRoutine(message, messageColor));
    }

    private IEnumerator ShowTierNotificationRoutine(string message, Color messageColor)
    {
        GameObject notification = tierNotificationPanel != null
            ? SetupAssignedNotification(message, messageColor)
            : CreateNotificationObject(message, messageColor);

        CanvasGroup canvasGroup = tierNotificationPanel != null
            ? tierNotificationCanvasGroup
            : notification.GetComponent<CanvasGroup>();

        float elapsed = 0f;
        while (elapsed < notificationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / notificationDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = t < 0.8f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.8f) / 0.2f);

            yield return null;
        }

        if (tierNotificationPanel != null)
            tierNotificationPanel.SetActive(false);
        else
            Destroy(notification);

        notificationCoroutine = null;
    }

    private GameObject SetupAssignedNotification(string message, Color messageColor)
    {
        if (tierNotificationCanvasGroup == null)
            tierNotificationCanvasGroup = tierNotificationPanel.GetComponent<CanvasGroup>();

        if (tierNotificationCanvasGroup != null)
        {
            tierNotificationCanvasGroup.alpha = 1f;
            tierNotificationCanvasGroup.interactable = false;
            tierNotificationCanvasGroup.blocksRaycasts = false;
        }

        if (tierNotificationHeaderText != null)
        {
            tierNotificationHeaderText.text = tierNotificationHeader;
            tierNotificationHeaderText.color = notificationHeaderColor;
        }

        if (tierNotificationMessageText != null)
        {
            tierNotificationMessageText.text = message;
            tierNotificationMessageText.color = messageColor;
        }

        tierNotificationPanel.SetActive(true);
        return tierNotificationPanel;
    }

    private GameObject CreateNotificationObject(string message, Color messageColor)
    {
        GameObject canvasObject = new GameObject("TierNotification");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 10;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();
        CanvasGroup canvasGroup = canvasObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = notificationAnchoredPosition;
        panelRect.sizeDelta = notificationSize;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = notificationBackgroundColor;

        CreateNotificationText(panel.transform, "Header", "Petunjuk Sistem", notificationHeaderColor,
            notificationHeaderFontSize, new Vector2(0f, -14f), new Vector2(-32f, 28f));
        CreateNotificationText(panel.transform, "Message", message, messageColor,
            notificationMessageFontSize, new Vector2(0f, -48f), new Vector2(-32f, 42f));

        return canvasObject;
    }

    private void CreateNotificationText(Transform parent, string name, string textValue, Color color,
        float textSize, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = notificationSize + sizeDelta;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.font = GetPopupFont();
        text.fontSize = textSize;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.raycastTarget = false;
    }

    private void ShowPopup(string message, Color color, Vector3 offset)
    {
        FindPlayerIfNeeded();
        if (playerTransform == null) return;

        GameObject popup = new GameObject("GameplayFeedbackPopup");
        popup.transform.position = playerTransform.position + offset +
                                   Vector3.right * Random.Range(-randomXOffset, randomXOffset);

        TextMeshPro text = popup.AddComponent<TextMeshPro>();
        text.text = message;
        text.fontSize = fontSize;
        text.font = GetPopupFont();
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.outlineColor = outlineColor;
        text.outlineWidth = outlineWidth;

        Renderer textRenderer = popup.GetComponent<Renderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = sortingLayerName;
            textRenderer.sortingOrder = sortingOrder;
        }

        StartCoroutine(AnimatePopup(popup, text));
    }

    private void FindPlayerIfNeeded()
    {
        if (playerTransform != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    private string GetMissionLabel(MissionType missionType)
    {
        return missionType switch
        {
            MissionType.EscortNenek => "Antar Nenek",
            MissionType.CleanEnvironment => "Bersih Lingkungan",
            _ => missionType.ToString(),
        };
    }

    private TMP_FontAsset GetPopupFont()
    {
        if (popupFontAsset != null)
            return popupFontAsset;

        if (generatedFontAsset == null && trueTypeFont != null)
            generatedFontAsset = TMP_FontAsset.CreateFontAsset(trueTypeFont);

        return generatedFontAsset;
    }

    private IEnumerator AnimatePopup(GameObject popup, TextMeshPro text)
    {
        Vector3 startPosition = popup.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * floatDistance;
        Color startColor = text.color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            popup.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            yield return null;
        }

        Destroy(popup);
    }
}
