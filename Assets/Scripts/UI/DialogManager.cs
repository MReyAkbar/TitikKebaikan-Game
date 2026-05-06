using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Menampilkan dialog NPC dalam sebuah dialog box.
/// Pemain bisa menutup dialog dengan menekan E atau klik.
/// </summary>
public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [Header("Dialog UI")]
    [SerializeField] private GameObject          dialogPanel;
    [SerializeField] private TextMeshProUGUI     speakerText;
    [SerializeField] private TextMeshProUGUI     dialogText;
    [SerializeField] private float               typewriterSpeed = 0.03f; // detik per karakter

    private bool    isTyping   = false;
    private string  fullText   = "";
    private Coroutine typeCoroutine;

    // ─── Singleton ───────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        dialogPanel?.SetActive(false);
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    public void ShowDialog(string speaker, string text)
    {
        dialogPanel?.SetActive(true);

        if (speakerText != null) speakerText.text = speaker;

        fullText = text;

        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypewriterEffect(text));
    }

    public void CloseDialog()
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        isTyping = false;
        dialogPanel?.SetActive(false);
    }

    // ─── Input: skip / close ─────────────────────────────────────────────────

    private void Update()
    {
        if (!dialogPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
                SkipTypewriter();
            else
                CloseDialog();
        }
    }

    // ─── Typewriter Effect ───────────────────────────────────────────────────

    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        if (dialogText != null) dialogText.text = "";

        foreach (char c in text)
        {
            if (dialogText != null) dialogText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
    }

    private void SkipTypewriter()
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        isTyping = false;
        if (dialogText != null) dialogText.text = fullText;
    }
}
