using UnityEngine;
using System.Collections.Generic;

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
    [SerializeField] private bool         spawnContinuously = true;
    [SerializeField] private bool         limitActiveVehicles = false;
    [SerializeField] private int          maxActiveVehicles = 6;
    [SerializeField] private TrafficStopLine stopLine;
    [SerializeField] private TrafficStopLine[] stopLines;

    [Header("Visual Polish")]
    [SerializeField] private Sprite[] vehicleSprites;
    [SerializeField] private Color[] vehicleColors;
    [SerializeField] private Vector2 randomScaleRange = new Vector2(1f, 1f);

    private float nextSpawnTime;
    private readonly List<GameObject> activeVehicles = new List<GameObject>();

    private void Start() => ScheduleNextSpawn();

    private void Update()
    {
        if (!spawnContinuously) return;

        if (limitActiveVehicles)
            CleanupActiveVehicles();

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnVehicle();
            ScheduleNextSpawn();
        }
    }

    private void TrySpawnVehicle()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0) return;
        if (limitActiveVehicles)
        {
            CleanupActiveVehicles();
            if (maxActiveVehicles > 0 && activeVehicles.Count >= maxActiveVehicles) return;
        }

        Vector3 spawnPos = transform.position + Vector3.up * Random.Range(-laneWidth, laneWidth);

        GameObject vehicle = InstantiateRandomVehicle(spawnPos);
        if (vehicle == null) return;

        if (limitActiveVehicles)
            activeVehicles.Add(vehicle);

        ApplyVisualVariation(vehicle);

        // Set arah gerakan
        VehicleController vc = vehicle.GetComponent<VehicleController>();
        if (vc != null)
        {
            TrafficStopLine[] validStopLines = GetValidStopLines();
            if (validStopLines.Length > 0)
                vc.SetStopLines(validStopLines);
            else if (stopLine != null)
                vc.SetStopLine(stopLine);

            vc.SetMoveDirection(spawnDirection);
            vc.SetSpawnPoint(spawnPos);
        }
        // moveDirection adalah [SerializeField] – kita set via reflection atau cara lain
        // Cara sederhana: flip scale jika arah kiri
        if (spawnDirection.x < 0)
            vehicle.transform.localScale = new Vector3(-Mathf.Abs(vehicle.transform.localScale.x), vehicle.transform.localScale.y, vehicle.transform.localScale.z);
    }

    private GameObject InstantiateRandomVehicle(Vector3 spawnPos)
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0) return null;

        int startIndex = Random.Range(0, vehiclePrefabs.Length);
        for (int offset = 0; offset < vehiclePrefabs.Length; offset++)
        {
            GameObject prefab = vehiclePrefabs[(startIndex + offset) % vehiclePrefabs.Length];
            if (prefab == null) continue;

            try
            {
                return Instantiate(prefab, spawnPos, Quaternion.identity);
            }
            catch (MissingReferenceException)
            {
                vehiclePrefabs[(startIndex + offset) % vehiclePrefabs.Length] = null;
            }
        }

        Debug.LogWarning($"[{name}] Tidak ada prefab kendaraan valid untuk di-spawn.", this);
        return null;
    }

    private void ApplyVisualVariation(GameObject vehicle)
    {
        SpriteRenderer spriteRenderer = vehicle.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) return;

        if (vehicleSprites != null && vehicleSprites.Length > 0)
            spriteRenderer.sprite = vehicleSprites[Random.Range(0, vehicleSprites.Length)];

        if (vehicleColors != null && vehicleColors.Length > 0)
            spriteRenderer.color = vehicleColors[Random.Range(0, vehicleColors.Length)];

        float scaleMin = Mathf.Min(randomScaleRange.x, randomScaleRange.y);
        float scaleMax = Mathf.Max(randomScaleRange.x, randomScaleRange.y);
        float randomScale = Random.Range(scaleMin, scaleMax);
        vehicle.transform.localScale = new Vector3(
            vehicle.transform.localScale.x * randomScale,
            vehicle.transform.localScale.y * randomScale,
            vehicle.transform.localScale.z
        );
    }

    private void CleanupActiveVehicles()
    {
        for (int i = activeVehicles.Count - 1; i >= 0; i--)
        {
            if (activeVehicles[i] == null)
                activeVehicles.RemoveAt(i);
        }
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    private TrafficStopLine[] GetValidStopLines()
    {
        if (stopLines == null || stopLines.Length == 0)
            return System.Array.Empty<TrafficStopLine>();

        List<TrafficStopLine> validStopLines = new List<TrafficStopLine>();
        foreach (TrafficStopLine candidate in stopLines)
        {
            if (candidate != null)
                validStopLines.Add(candidate);
        }

        return validStopLines.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, laneWidth * 2f, 0));
        Gizmos.DrawRay(transform.position, (Vector3)spawnDirection * 2f);
    }
}
