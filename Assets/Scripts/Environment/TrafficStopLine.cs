using UnityEngine;

/// <summary>
/// Marker garis berhenti kendaraan untuk sebuah TrafficLight.
/// Letakkan di depan zebra cross/lampu pada lane kendaraan.
/// </summary>
public class TrafficStopLine : MonoBehaviour
{
    [SerializeField] private TrafficLight linkedTrafficLight;

    public TrafficLight LinkedTrafficLight => linkedTrafficLight;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.4f, 1.5f, 0f));
    }
}
