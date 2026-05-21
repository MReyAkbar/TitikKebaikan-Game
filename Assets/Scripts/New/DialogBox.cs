using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menampilkan kotak dialog NPC.
/// Attach ke GameObject Canvas yang berisi panel dialog.
/// </summary>
public class DialogBox : MonoBehaviour
{
    public static DialogBox Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject    dialogPanel;   // panel kotak dialog
    public TextMeshProUGUI speakerText; // nama NPC
    public TextMeshProUGUI dialogText;  // isi dialog

    [Header("Tombol Lanjut")]
    public Button continueButton;       // tombol "Lanjut" / "Oke"

    // Callback dipanggil setelah pemain klik Lanjut
    private System.Action onConfirm;
    private int shownFrame = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Sembunyikan dialog di awal
        dialogPanel.SetActive(false);

        // Hubungkan tombol
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    /// <summary>
    /// Tampilkan dialog.
    /// onConfirmCallback = fungsi yang dipanggil saat pemain klik Lanjut.
    /// </summary>
    public void Show(string speaker, string message, System.Action onConfirmCallback = null)
    {
        speakerText.text = speaker;
        dialogText.text  = message;
        onConfirm        = onConfirmCallback;
        dialogPanel.SetActive(true);
        shownFrame = Time.frameCount;

        // Pause game saat dialog
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        dialogPanel.SetActive(false);
        Time.timeScale = 1f; // resume game
    }

    private void OnContinueClicked()
    {
        Hide();
        onConfirm?.Invoke(); // jalankan callback
    }

    // Bisa juga tutup dengan tekan E
    private void Update()
    {
        if (dialogPanel.activeSelf && Time.frameCount > shownFrame && Input.GetKeyDown(KeyCode.E))
            OnContinueClicked();
    }
}
