using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Mission Status Banner")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private string defaultStatusText = "ayo interaksi dengan warga sekitar";
    [SerializeField] private string allMissionsCompleteText = "semua misi sudah selesai";

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Fallback")]
    [SerializeField] private bool handleEscapeWithoutGameManager = true;

    private GameManager gameManager;
    private MissionManager missionManager;
    private NenekNPC nenekNpc;
    private NPCPakRT pakRtNpc;
    private bool fallbackPaused;

    private void Awake()
    {
        SetPanelVisible(false);
        RefreshStatusText();
    }

    private void OnEnable()
    {
        TryConnectManagers();
        RefreshStatusText();
    }

    private void OnDisable()
    {
        DisconnectGameManager();
        DisconnectMissionManager();
    }

    private void Start()
    {
        CacheMissionNpcs();
        TryConnectManagers();
        SyncPanelWithGameState();
        RefreshStatusText();
    }

    private void Update()
    {
        TryConnectManagers();

        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (gameManager != null)
            ToggleGameManagerPause();
        else if (handleEscapeWithoutGameManager)
            ToggleFallbackPause();
    }

    public void ResumeGame()
    {
        TryConnectManagers();

        if (gameManager != null)
            gameManager.ResumeGame();
        else
            SetFallbackPaused(false);

        ClearSelection();
    }

    public void BackToMainMenu()
    {
        SetPanelVisible(false);
        Time.timeScale = 1f;

        if (gameManager != null)
            gameManager.ReturnToMainMenu();
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RefreshStatusText()
    {
        if (statusText == null) return;

        statusText.text = GetCurrentMissionStatusText();
    }

    private void TryConnectManagers()
    {
        if (gameManager == null && GameManager.Instance != null)
        {
            gameManager = GameManager.Instance;
            gameManager.OnStateChanged += OnGameStateChanged;
        }

        if (missionManager == null && MissionManager.Instance != null)
        {
            missionManager = MissionManager.Instance;
            missionManager.OnMissionStarted += OnMissionStarted;
            missionManager.OnMissionCompleted += OnMissionCompleted;
            missionManager.OnMissionFailed += OnMissionFailed;
        }
    }

    private void DisconnectGameManager()
    {
        if (gameManager == null) return;

        gameManager.OnStateChanged -= OnGameStateChanged;
        gameManager = null;
    }

    private void DisconnectMissionManager()
    {
        if (missionManager == null) return;

        missionManager.OnMissionStarted -= OnMissionStarted;
        missionManager.OnMissionCompleted -= OnMissionCompleted;
        missionManager.OnMissionFailed -= OnMissionFailed;
        missionManager = null;
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        SetPanelVisible(state == GameManager.GameState.Paused);
    }

    private void OnMissionStarted(MissionType type)
    {
        RefreshStatusText();
    }

    private void OnMissionCompleted(MissionType type)
    {
        RefreshStatusText();
    }

    private void OnMissionFailed(MissionType type, string reason)
    {
        RefreshStatusText();
    }

    private void SyncPanelWithGameState()
    {
        if (gameManager != null)
            SetPanelVisible(gameManager.State == GameManager.GameState.Paused);
    }

    private void SetPanelVisible(bool visible)
    {
        if (pausePanel != null)
            pausePanel.SetActive(visible);

        if (visible)
        {
            RefreshStatusText();
            SelectFirstButton();
        }
    }

    private void ToggleFallbackPause()
    {
        SetFallbackPaused(!fallbackPaused);
    }

    private void ToggleGameManagerPause()
    {
        if (gameManager.State == GameManager.GameState.Playing)
            gameManager.PauseGame();
        else if (gameManager.State == GameManager.GameState.Paused)
            gameManager.ResumeGame();
    }

    private void SetFallbackPaused(bool paused)
    {
        fallbackPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        SetPanelVisible(paused);

        if (!paused)
            ClearSelection();
    }

    private string GetCurrentMissionStatusText()
    {
        CacheMissionNpcs();

        string npcStatus = GetNpcMissionStatusText();
        if (!string.IsNullOrWhiteSpace(npcStatus))
            return npcStatus;

        string managerStatus = GetMissionManagerStatusText();
        if (!string.IsNullOrWhiteSpace(managerStatus))
            return managerStatus;

        return defaultStatusText;
    }

    private void CacheMissionNpcs()
    {
        if (nenekNpc == null)
            nenekNpc = FindFirstObjectByType<NenekNPC>();

        if (pakRtNpc == null)
            pakRtNpc = FindFirstObjectByType<NPCPakRT>();
    }

    private string GetNpcMissionStatusText()
    {
        if (nenekNpc != null)
        {
            if (nenekNpc.State == NenekNPC.MissionState.Talking)
                return "Misi Nenek: dengarkan permintaan Nenek";

            if (nenekNpc.State == NenekNPC.MissionState.Active)
                return "Misi Nenek: antar Nenek ke pasar";
        }

        if (pakRtNpc != null)
        {
            if (pakRtNpc.State == NPCPakRT.MissionState.Talking)
                return "Misi Pak RT: dengarkan permintaan Pak RT";

            if (pakRtNpc.State == NPCPakRT.MissionState.Active)
                return $"Misi Pak RT: kumpulkan sampah {pakRtNpc.TrashDelivered}/{pakRtNpc.RequiredTrashCount}";

            if (pakRtNpc.State == NPCPakRT.MissionState.ReadyToReport)
                return "Misi Pak RT: lapor kembali ke Pak RT";
        }

        return string.Empty;
    }

    private string GetMissionManagerStatusText()
    {
        if (missionManager == null) return string.Empty;

        if (missionManager.GetStatus(MissionType.EscortNenek) == MissionManager.MissionStatus.Active)
            return "Misi Nenek: antar Nenek ke pasar";

        if (missionManager.GetStatus(MissionType.CleanEnvironment) == MissionManager.MissionStatus.Active)
            return "Misi Pak RT: kumpulkan dan buang sampah";

        if (missionManager.IsAllCompleted())
            return allMissionsCompleteText;

        return string.Empty;
    }

    private void SelectFirstButton()
    {
        if (firstSelectedButton == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    private void ClearSelection()
    {
        if (EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
    }
}
