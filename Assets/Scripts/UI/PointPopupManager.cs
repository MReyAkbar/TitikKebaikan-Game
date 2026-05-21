using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Menampilkan popup kecil di dekat player saat Poin Etika bertambah atau berkurang.
/// </summary>
public class PointPopupManager : MonoBehaviour
{
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

    private bool isSubscribed;
    private TMP_FontAsset generatedFontAsset;

    private void Update()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        if (!isSubscribed && EthicsManager.Instance != null)
        {
            EthicsManager.Instance.OnPointsDeltaChanged += ShowPointPopup;
            isSubscribed = true;
        }
    }

    private void OnDestroy()
    {
        if (EthicsManager.Instance != null)
            EthicsManager.Instance.OnPointsDeltaChanged -= ShowPointPopup;
    }

    private void ShowPointPopup(int delta, string reason)
    {
        if (playerTransform == null) return;

        GameObject popup = new GameObject("PointPopup");
        popup.transform.position = playerTransform.position + worldOffset +
                                   Vector3.right * Random.Range(-randomXOffset, randomXOffset);

        TextMeshPro text = popup.AddComponent<TextMeshPro>();
        text.text = $"{(delta > 0 ? "+" : "")}{delta}";
        text.fontSize = fontSize;
        text.font = GetPopupFont();
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.color = delta > 0 ? positiveColor : negativeColor;
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
