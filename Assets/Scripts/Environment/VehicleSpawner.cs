using UnityEngine;

/// <summary>
/// Spawner kendaraan untuk jalan raya.
/// Letakkan di tepi kiri/kanan jalan, arahkan ke kanan/kiri.
/// Kendaraan di-spawn secara acak dalam interval tertentu.
/// </summary>
public class VehicleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] vehiclePrefabs;       // berbagai tipe kendaraan
    [SerializeField] private float        spawnIntervalMin = 2f;
    [SerializeField] private float        spawnIntervalMax = 5f;
    [SerializeField] private Vector2      spawnDirection   = Vector2.right;
    [SerializeField] private float        laneWidth        = 0.5f; // variasi Y spawn

    private float nextSpawnTime;

    private void Start() => ScheduleNextSpawn();

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnVehicle();
            ScheduleNextSpawn();
        }
    }

    private void SpawnVehicle()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0) return;

        // Pilih kendaraan acak
        GameObject prefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];

        // Variasi posisi Y agar terlihat natural
        Vector3 spawnPos = transform.position + Vector3.up * Random.Range(-laneWidth, laneWidth);

        GameObject vehicle = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Set arah gerakan
        VehicleController vc = vehicle.GetComponent<VehicleController>();
        // moveDirection adalah [SerializeField] – kita set via reflection atau cara lain
        // Cara sederhana: flip scale jika arah kiri
        if (spawnDirection.x < 0)
            vehicle.transform.localScale = new Vector3(-1, 1, 1);
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, laneWidth * 2f, 0));
        Gizmos.DrawRay(transform.position, (Vector3)spawnDirection * 2f);
    }
}
