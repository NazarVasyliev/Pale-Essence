using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableItem
    {
        [Tooltip("Prefab of the object to spawn.")]
        public GameObject prefab;

        [Tooltip("Spawn chance in percent (e.g., 90.0 for 90%).")]
        [Range(0.01f, 100f)]
        public float spawnChance = 100f;
    }

    [Header("Spawn Settings")]
    [Tooltip("List of objects that can be spawned and their chances.")]
    [SerializeField] private List<SpawnableItem> itemsToSpawn;

    [Tooltip("Total number of objects to spawn.")]
    [SerializeField] private int numberOfItemsToCreate = 2;

    [Tooltip("Position where the objects will appear.")]
    [SerializeField] private Transform spawnPoint;

    void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        if (itemsToSpawn == null || itemsToSpawn.Count == 0)
            return;
    }

    public void SpawnItems()
    {
        List<SpawnableItem> remainingItems = new List<SpawnableItem>(itemsToSpawn);
        int spawnedCount = 0;

        while (spawnedCount < numberOfItemsToCreate && remainingItems.Count > 0)
        {
            float totalChance = remainingItems.Sum(item => item.spawnChance);
            float randomValue = Random.Range(0f, totalChance);
            SpawnableItem itemToSpawn = null;
            float cumulativeChance = 0f;

            foreach (var item in remainingItems)
            {
                cumulativeChance += item.spawnChance;
                if (randomValue <= cumulativeChance)
                {
                    itemToSpawn = item;
                    break;
                }
            }

            if (itemToSpawn != null)
            {
                if (Random.Range(0f, 100f) <= itemToSpawn.spawnChance)
                {
                    Instantiate(itemToSpawn.prefab, spawnPoint.position, spawnPoint.rotation);
                    spawnedCount++;
                }

                remainingItems.Remove(itemToSpawn);
            }

        }
    }
}