using UnityEngine;

/// <summary>
/// Pusat SFX gameplay. Buat satu GameObject "SFXManager" di scene,
/// lalu isi AudioClip dari Inspector.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.96f, 1.04f);

    [Header("Interaction")]
    [SerializeField] private AudioClip interactClip;
    [SerializeField] private AudioClip dialogOpenClip;
    [SerializeField] private AudioClip dialogCloseClip;
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Trash")]
    [SerializeField] private AudioClip trashPickupClip;
    [SerializeField] private AudioClip trashDumpClip;
    [SerializeField] private AudioClip actionFailedClip;

    [Header("Points & Progress")]
    [SerializeField] private AudioClip pointGainClip;
    [SerializeField] private AudioClip pointLoseClip;
    [SerializeField] private AudioClip reputationUpClip;
    [SerializeField] private AudioClip missionCompleteClip;
    [SerializeField] private AudioClip missionFailClip;

    [Header("Traffic")]
    [SerializeField] private AudioClip trafficLightChangeClip;
    [SerializeField] private AudioClip vehicleHitClip;

    private float defaultPitch = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        defaultPitch = audioSource.pitch;
    }

    public void PlayInteract() => Play(interactClip);
    public void PlayDialogOpen() => Play(dialogOpenClip);
    public void PlayDialogClose() => Play(dialogCloseClip);
    public void PlayButtonClick() => Play(buttonClickClip);
    public void PlayTrashPickup() => Play(trashPickupClip);
    public void PlayTrashDump() => Play(trashDumpClip);
    public void PlayActionFailed() => Play(actionFailedClip);
    public void PlayPointGain() => Play(pointGainClip);
    public void PlayPointLose() => Play(pointLoseClip);
    public void PlayReputationUp() => Play(reputationUpClip);
    public void PlayMissionComplete() => Play(missionCompleteClip);
    public void PlayMissionFail() => Play(missionFailClip);
    public void PlayTrafficLightChange() => Play(trafficLightChangeClip);
    public void PlayVehicleHit() => Play(vehicleHitClip);

    private void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.pitch = randomizePitch
            ? Random.Range(pitchRange.x, pitchRange.y)
            : defaultPitch;

        audioSource.PlayOneShot(clip, masterVolume);
    }
}
