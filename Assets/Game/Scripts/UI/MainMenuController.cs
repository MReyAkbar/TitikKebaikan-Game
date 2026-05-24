using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject howToPanel;
    [SerializeField] private GameObject aboutPanel;

    [Header("How To Play Pages")]
    [SerializeField] private Image howToPageImage;
    [SerializeField] private Button howToNextButton;
    [SerializeField] private Sprite[] howToPages = new Sprite[0];
    [SerializeField] private string howToResourcesFolder = "HowToPlay";
    [SerializeField] private bool closeHowToAfterLastPage = true;
    [SerializeField] private bool hideNextButtonOnLastPage;

    [Header("About")]
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button aboutBackButton;
    [SerializeField] private Image aboutPageImage;
    [SerializeField] private Sprite aboutButtonSprite;
    [SerializeField] private Sprite aboutPageSprite;
    [SerializeField] private Sprite aboutBackButtonSprite;

    [Header("Objects Hidden While Menu Is Open")]
    [SerializeField] private GameObject[] hideWhileMenuOpen;

    [Header("Game Flow")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private bool loadGameSceneOnStart = true;
    [SerializeField] private bool freezeGameplayWhileMenuOpen = true;

    [Header("Selection")]
    [SerializeField] private GameObject firstSelectedButton;

    private bool hasStartedGame;
    private int currentHowToIndex;
    private bool generatedAboutButton;

    private void Start()
    {
        SetupHowToPages();
        SetupAbout();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        hasStartedGame = false;
        SetMenuVisible(true);
        SetHowToVisible(false);
        SetAboutVisible(false);
        SetHiddenObjectsVisible(false);

        if (freezeGameplayWhileMenuOpen)
            Time.timeScale = 0f;

        ClearSelection();
    }

    public void StartGame()
    {
        if (hasStartedGame) return;

        hasStartedGame = true;
        SetHowToVisible(false);
        SetAboutVisible(false);
        SetMenuVisible(false);
        SetHiddenObjectsVisible(true);
        Time.timeScale = 1f;

        if (loadGameSceneOnStart)
        {
            GameManager manager = GetOrCreateGameManager();
            if (manager != null)
                manager.StartGame(gameSceneName);
            else if (!string.IsNullOrWhiteSpace(gameSceneName))
                SceneManager.LoadScene(gameSceneName);

            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.StartGameInCurrentScene();
    }

    public void ShowHowTo()
    {
        SetAboutVisible(false);
        SetHowToVisible(true);
        ShowHowToPage(0);
        SelectHowToButton();
    }

    public void HideHowTo()
    {
        SetHowToVisible(false);
        currentHowToIndex = 0;
        ClearSelection();
    }

    public void ShowAbout()
    {
        SetHowToVisible(false);
        SetAboutVisible(true);
        SelectAboutButton();
    }

    public void HideAbout()
    {
        SetAboutVisible(false);
        ClearSelection();
    }

    public void NextHowTo()
    {
        if (!HasHowToPages()) return;

        if (currentHowToIndex >= howToPages.Length - 1)
        {
            if (closeHowToAfterLastPage)
                HideHowTo();

            return;
        }

        ShowHowToPage(currentHowToIndex + 1);
    }

    public void PreviousHowTo()
    {
        if (!HasHowToPages()) return;

        ShowHowToPage(currentHowToIndex - 1);
    }

    public void RestartHowTo()
    {
        ShowHowToPage(0);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private GameManager GetOrCreateGameManager()
    {
        if (GameManager.Instance != null)
            return GameManager.Instance;

        GameObject managerObject = new GameObject("GameManager");
        return managerObject.AddComponent<GameManager>();
    }

    private void SetMenuVisible(bool visible)
    {
        if (menuRoot != null)
            menuRoot.SetActive(visible);
    }

    private void SetHowToVisible(bool visible)
    {
        if (howToPanel != null)
            howToPanel.SetActive(visible);
    }

    private void SetAboutVisible(bool visible)
    {
        if (aboutPanel != null)
            aboutPanel.SetActive(visible);
    }

    private void SetupHowToPages()
    {
        LoadHowToPagesFromResourcesIfNeeded();

        if (howToPanel != null)
        {
            if (howToPageImage == null)
                howToPageImage = FindHowToPageImage();

            if (howToPageImage != null)
                howToPageImage.raycastTarget = false;

            if (howToNextButton == null)
                howToNextButton = FindHowToNextButton();
        }

        if (howToNextButton != null)
        {
            howToNextButton.onClick.RemoveListener(NextHowTo);

            if (!HasPersistentButtonListener(howToNextButton, nameof(NextHowTo)))
                howToNextButton.onClick.AddListener(NextHowTo);
        }
    }

    private void SetupAbout()
    {
        FindExistingAboutReferences();
        EnsureAboutButtonExists();
        EnsureAboutPanelExists();

        if (aboutPageImage != null)
            aboutPageImage.raycastTarget = false;

        if (aboutButton != null)
        {
            aboutButton.onClick.RemoveListener(ShowAbout);

            if (!HasPersistentButtonListener(aboutButton, nameof(ShowAbout)))
                aboutButton.onClick.AddListener(ShowAbout);
        }

        if (aboutBackButton != null)
        {
            aboutBackButton.onClick.RemoveListener(HideAbout);

            if (!HasPersistentButtonListener(aboutBackButton, nameof(HideAbout)))
                aboutBackButton.onClick.AddListener(HideAbout);
        }
    }

    private void EnsureAboutButtonExists()
    {
        if (aboutButton != null || menuRoot == null) return;

        RectTransform buttonTransform = CreateUIObject<RectTransform>("AboutButton", menuRoot.transform);
        buttonTransform.anchorMin = new Vector2(0.5f, 0.5f);
        buttonTransform.anchorMax = new Vector2(0.5f, 0.5f);
        buttonTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonTransform.anchoredPosition = new Vector2(-707f, -351.24f);
        buttonTransform.sizeDelta = new Vector2(290.3222f, 95.4438f);

        Image image = buttonTransform.gameObject.AddComponent<Image>();
        image.sprite = aboutButtonSprite;
        image.preserveAspect = true;
        image.raycastTarget = true;

        aboutButton = buttonTransform.gameObject.AddComponent<Button>();
        aboutButton.targetGraphic = image;
        generatedAboutButton = true;

        MoveExitButtonForAbout();
    }

    private void MoveExitButtonForAbout()
    {
        if (!generatedAboutButton || menuRoot == null) return;

        Transform exitTransform = menuRoot.transform.Find("ExitButton");
        RectTransform exitRect = exitTransform != null ? exitTransform as RectTransform : null;

        if (exitRect != null)
            exitRect.anchoredPosition = new Vector2(exitRect.anchoredPosition.x, -470f);
    }

    private void EnsureAboutPanelExists()
    {
        if (aboutPanel != null) return;

        Transform parent = GetMenuCanvasTransform();
        if (parent == null) return;

        RectTransform panelTransform = CreateUIObject<RectTransform>("AboutPanel", parent);
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.pivot = new Vector2(0.5f, 0.5f);
        panelTransform.anchoredPosition = Vector2.zero;
        panelTransform.sizeDelta = Vector2.zero;

        Image panelImage = panelTransform.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.65f);
        panelImage.raycastTarget = true;

        aboutPanel = panelTransform.gameObject;

        RectTransform pageTransform = CreateUIObject<RectTransform>("AboutPageImage", panelTransform);
        pageTransform.anchorMin = new Vector2(0.5f, 0.5f);
        pageTransform.anchorMax = new Vector2(0.5f, 0.5f);
        pageTransform.pivot = new Vector2(0.5f, 0.5f);
        pageTransform.anchoredPosition = Vector2.zero;
        pageTransform.sizeDelta = new Vector2(1536f, 1024f);

        aboutPageImage = pageTransform.gameObject.AddComponent<Image>();
        aboutPageImage.sprite = aboutPageSprite;
        aboutPageImage.preserveAspect = true;
        aboutPageImage.raycastTarget = false;

        RectTransform backTransform = CreateUIObject<RectTransform>("AboutBackButton", panelTransform);
        backTransform.anchorMin = new Vector2(0.5f, 0.5f);
        backTransform.anchorMax = new Vector2(0.5f, 0.5f);
        backTransform.pivot = new Vector2(0.5f, 0.5f);
        backTransform.anchoredPosition = new Vector2(654f, -460f);
        backTransform.sizeDelta = new Vector2(303.6747f, 79.977f);

        Image backImage = backTransform.gameObject.AddComponent<Image>();
        backImage.sprite = aboutBackButtonSprite;
        backImage.preserveAspect = true;
        backImage.raycastTarget = true;

        aboutBackButton = backTransform.gameObject.AddComponent<Button>();
        aboutBackButton.targetGraphic = backImage;

        CreateButtonText("Kembali", backTransform);
        aboutPanel.SetActive(false);
    }

    private void FindExistingAboutReferences()
    {
        if (menuRoot != null && aboutButton == null)
        {
            Transform foundButton = menuRoot.transform.Find("AboutButton");
            if (foundButton != null)
                aboutButton = foundButton.GetComponent<Button>();
        }

        Transform canvasTransform = GetMenuCanvasTransform();

        if (canvasTransform != null && aboutPanel == null)
        {
            Transform foundPanel = canvasTransform.Find("AboutPanel");
            if (foundPanel != null)
                aboutPanel = foundPanel.gameObject;
        }

        if (aboutPanel == null) return;

        if (aboutPageImage == null)
        {
            Transform foundPageImage = aboutPanel.transform.Find("AboutPageImage");
            if (foundPageImage != null)
                aboutPageImage = foundPageImage.GetComponent<Image>();
        }

        if (aboutBackButton == null)
        {
            Transform foundBackButton = aboutPanel.transform.Find("AboutBackButton");
            if (foundBackButton != null)
                aboutBackButton = foundBackButton.GetComponent<Button>();
        }
    }

    private Transform GetMenuCanvasTransform()
    {
        if (menuRoot != null && menuRoot.transform.parent != null)
            return menuRoot.transform.parent;

        if (howToPanel != null && howToPanel.transform.parent != null)
            return howToPanel.transform.parent;

        return transform;
    }

    private T CreateUIObject<T>(string objectName, Transform parent) where T : Component
    {
        GameObject item = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
        item.layer = LayerMask.NameToLayer("UI");
        item.transform.SetParent(parent, false);
        return item.GetComponent<T>();
    }

    private void CreateButtonText(string text, RectTransform parent)
    {
        RectTransform textTransform = CreateUIObject<RectTransform>("Text", parent);
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.pivot = new Vector2(0.5f, 0.5f);
        textTransform.anchoredPosition = Vector2.zero;
        textTransform.sizeDelta = Vector2.zero;

        TextMeshProUGUI label = textTransform.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 24f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.196f, 0.196f, 0.196f, 1f);
        label.raycastTarget = false;
    }

    private void LoadHowToPagesFromResourcesIfNeeded()
    {
        if (HasHowToPages() || string.IsNullOrWhiteSpace(howToResourcesFolder)) return;

        howToPages = Resources.LoadAll<Sprite>(howToResourcesFolder);
        Array.Sort(howToPages, CompareSpriteNames);
    }

    private Image FindHowToPageImage()
    {
        if (howToPanel == null) return null;

        Image[] images = howToPanel.GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            if (image.gameObject == howToPanel) continue;
            if (image.GetComponent<Button>() != null) continue;
            if (image.GetComponentInParent<Button>() != null) continue;

            return image;
        }

        return null;
    }

    private Button FindHowToNextButton()
    {
        if (howToPanel == null) return null;

        Button[] buttons = howToPanel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            string buttonName = button.name.ToLowerInvariant();

            if (buttonName.Contains("next") || buttonName.Contains("lanjut"))
                return button;
        }

        return null;
    }

    private bool HasPersistentButtonListener(Button button, string methodName)
    {
        if (button == null) return false;

        int listenerCount = button.onClick.GetPersistentEventCount();

        for (int i = 0; i < listenerCount; i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this &&
                button.onClick.GetPersistentMethodName(i) == methodName)
            {
                return true;
            }
        }

        return false;
    }

    private void ShowHowToPage(int pageIndex)
    {
        if (!HasHowToPages())
        {
            UpdateHowToNextButton();
            return;
        }

        currentHowToIndex = Mathf.Clamp(pageIndex, 0, howToPages.Length - 1);

        if (howToPageImage != null)
        {
            howToPageImage.sprite = howToPages[currentHowToIndex];
            howToPageImage.preserveAspect = true;
            howToPageImage.raycastTarget = false;
        }

        UpdateHowToNextButton();
    }

    private void UpdateHowToNextButton()
    {
        if (howToNextButton == null) return;

        bool hasPages = HasHowToPages();
        bool isLastPage = hasPages && currentHowToIndex >= howToPages.Length - 1;

        if (hideNextButtonOnLastPage)
            howToNextButton.gameObject.SetActive(hasPages && !isLastPage);

        howToNextButton.interactable = hasPages && (!isLastPage || closeHowToAfterLastPage);
    }

    private void SelectHowToButton()
    {
        if (howToNextButton == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(howToNextButton.gameObject);
    }

    private void SelectAboutButton()
    {
        if (aboutBackButton == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(aboutBackButton.gameObject);
    }

    private bool HasHowToPages()
    {
        return howToPages != null && howToPages.Length > 0;
    }

    private int CompareSpriteNames(Sprite first, Sprite second)
    {
        int firstNumber = GetTrailingNumber(first != null ? first.name : string.Empty);
        int secondNumber = GetTrailingNumber(second != null ? second.name : string.Empty);

        int numberComparison = firstNumber.CompareTo(secondNumber);
        if (numberComparison != 0)
            return numberComparison;

        string firstName = first != null ? first.name : string.Empty;
        string secondName = second != null ? second.name : string.Empty;
        return string.Compare(firstName, secondName, StringComparison.OrdinalIgnoreCase);
    }

    private int GetTrailingNumber(string text)
    {
        if (string.IsNullOrEmpty(text)) return int.MaxValue;

        int multiplier = 1;
        int number = 0;
        bool foundDigit = false;

        for (int i = text.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(text[i]))
                break;

            foundDigit = true;
            number += (text[i] - '0') * multiplier;
            multiplier *= 10;
        }

        return foundDigit ? number : int.MaxValue;
    }

    private void SetHiddenObjectsVisible(bool visible)
    {
        if (hideWhileMenuOpen == null) return;

        foreach (GameObject item in hideWhileMenuOpen)
        {
            if (item != null)
                item.SetActive(visible);
        }
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
