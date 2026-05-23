using UnityEngine;

/// <summary>
/// Menyimpan dan mengelola Poin Etika pemain.
/// Singleton sederhana, tidak bergantung script lain.
/// </summary>
public class EthicsManager : MonoBehaviour
{
    public static EthicsManager Instance { get; private set; }

    [Header("Poin Awal")]
    public int ethicsPoints = 0;

    public event System.Action<int> OnPointsChanged;
    public event System.Action<int, string> OnPointsDeltaChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddPoints(int amount, string reason = "")
    {
        int previousPoints = ethicsPoints;
        ethicsPoints = Mathf.Max(0, ethicsPoints + amount);
        int actualDelta = ethicsPoints - previousPoints;

        if (!string.IsNullOrEmpty(reason))
            Debug.Log($"[Poin] {(actualDelta >= 0 ? "+" : "")}{actualDelta} - {reason}. Total: {ethicsPoints}");

        OnPointsChanged?.Invoke(ethicsPoints);

        if (actualDelta != 0)
        {
            if (actualDelta > 0)
                SFXManager.Instance?.PlayPointGain();
            else
                SFXManager.Instance?.PlayPointLose();

            OnPointsDeltaChanged?.Invoke(actualDelta, reason);
        }
    }

    public int GetPoints() => ethicsPoints;
}
