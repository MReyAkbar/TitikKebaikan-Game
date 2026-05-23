using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Mengarahkan kamera sebentar ke target penting, lalu kembali mengikuti player.
/// Cocok untuk memperlihatkan destinasi misi tanpa memindahkan player.
/// </summary>
public class CameraFocusCue : MonoBehaviour
{
    public static CameraFocusCue Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform playerTarget;

    [Header("Cue Timing")]
    [SerializeField] private float holdDuration = 1.4f;
    [SerializeField] private float returnDelay = 0.15f;

    private Coroutine cueRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (cinemachineCamera == null)
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

        if (cameraFollow == null)
            cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTarget = player.transform;
        }
    }

    public void ShowTarget(Transform target)
    {
        if (target == null) return;

        if (cueRoutine != null)
            StopCoroutine(cueRoutine);

        cueRoutine = StartCoroutine(ShowTargetRoutine(target));
    }

    private IEnumerator ShowTargetRoutine(Transform target)
    {
        Transform originalCinemachineTarget = cinemachineCamera != null ? cinemachineCamera.Follow : null;

        SetCameraTarget(target);
        yield return new WaitForSeconds(holdDuration);

        if (returnDelay > 0f)
            yield return new WaitForSeconds(returnDelay);

        Transform returnTarget = playerTarget != null ? playerTarget : originalCinemachineTarget;
        SetCameraTarget(returnTarget);

        cueRoutine = null;
    }

    private void SetCameraTarget(Transform target)
    {
        if (target == null) return;

        if (cinemachineCamera != null)
            cinemachineCamera.Follow = target;

        if (cameraFollow != null)
            cameraFollow.SetTarget(target);
    }
}
