using UnityEngine;

/// <summary>
/// Lampu lalu lintas sederhana dengan siklus hijau -> kuning -> merah.
/// ZebraCross bisa membaca state lampu ini untuk memberi bonus atau penalti.
/// </summary>
public class TrafficLight : MonoBehaviour
{
    public enum LightState { Green, Yellow, Red }

    [Header("Timing")]
    [SerializeField] private float greenDuration = 6f;
    [SerializeField] private float yellowDuration = 2f;
    [SerializeField] private float redDuration = 5f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer redRenderer;
    [SerializeField] private SpriteRenderer yellowRenderer;
    [SerializeField] private SpriteRenderer greenRenderer;
    [SerializeField] private Color activeRed = Color.red;
    [SerializeField] private Color activeYellow = Color.yellow;
    [SerializeField] private Color activeGreen = Color.green;
    [SerializeField] private Color inactiveColor = new Color(0.18f, 0.18f, 0.18f, 1f);

    [Header("Initial State")]
    [SerializeField] private LightState initialState = LightState.Green;

    private LightState currentState;
    private float timer;
    private bool hasInitializedState;

    public LightState State => currentState;
    public bool IsGreenLight => currentState == LightState.Green;
    public bool IsYellowLight => currentState == LightState.Yellow;
    public bool IsRedLight => currentState == LightState.Red;

    private void Start()
    {
        SetState(initialState);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            CycleLight();
    }

    private void CycleLight()
    {
        LightState nextState = currentState switch
        {
            LightState.Green => LightState.Yellow,
            LightState.Yellow => LightState.Red,
            LightState.Red => LightState.Green,
            _ => LightState.Green,
        };

        SetState(nextState);
    }

    private void SetState(LightState state)
    {
        LightState previousState = currentState;
        currentState = state;
        timer = state switch
        {
            LightState.Green => greenDuration,
            LightState.Yellow => yellowDuration,
            LightState.Red => redDuration,
            _ => greenDuration,
        };

        UpdateVisual();

        if (hasInitializedState && previousState != currentState)
            SFXManager.Instance?.PlayTrafficLightChange();

        hasInitializedState = true;
    }

    private void UpdateVisual()
    {
        if (redRenderer != null)
            redRenderer.color = currentState == LightState.Red ? activeRed : inactiveColor;

        if (yellowRenderer != null)
            yellowRenderer.color = currentState == LightState.Yellow ? activeYellow : inactiveColor;

        if (greenRenderer != null)
            greenRenderer.color = currentState == LightState.Green ? activeGreen : inactiveColor;
    }
}
