using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class Room : MonoBehaviour
{


    [SerializeField] public int roomId;
    [SerializeField] private GameObject visualsContainer;
    [SerializeField] public List<GameObject> associatedPassages = new List<GameObject>();


    public GameObject activePassage;

    [Header("Room Clearing Logic")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfEnemies = 3;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Gate Open Delay")]
    [Tooltip("Delay in seconds before opening the gates after the room is cleared.")]
    [SerializeField] private float gateOpenDelay = 1.5f;
    [Tooltip("Delay in seconds before closing the gates when entering a room with enemies.")]
    [SerializeField] private float gateCloseDelay = 0.5f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    public bool isCleared { get; private set; } = false;
    private bool enemiesSpawned = false;

    void Awake()
    {
        if (visualsContainer == null)
        {
            Transform visualsTransform = transform.Find("Visuals");
            if (visualsTransform != null)
            {
                visualsContainer = visualsTransform.gameObject;
            }
        }
    }

    public void SetupRoom(GameObject enemyPrefabToSpawn)
    {
        this.enemyPrefab = enemyPrefabToSpawn;
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = GetComponentsInChildren<Transform>().Where(t => t.CompareTag("SpawnPoint")).ToArray();
            if (spawnPoints.Length == 0)
            {
                spawnPoints = new Transform[] { this.transform };
            }
        }
    }

    public void Activate()
    {
        if (visualsContainer != null) visualsContainer.SetActive(true);
        foreach (var passage in associatedPassages)
        {
            if (passage != null) passage.SetActive(true);
        }


        if (isCleared)
        {
            OpenAllPassages();
        }
        else
        {
            StartCoroutine(DelayedGateClose());

            if (!enemiesSpawned)
            {
                SpawnEnemies();
            }
        }
    }

    public void Deactivate()
    {
        if (visualsContainer != null) visualsContainer.SetActive(false);
        foreach (var passage in associatedPassages)
        {

            if (passage != null && passage != activePassage)
            {
                passage.SetActive(false);
            }
        }
    }

    private void SpawnEnemies()
    {
        if (numberOfEnemies <= 0)
        {
            isCleared = true;
            enemiesSpawned = true;
            StartCoroutine(DelayedGateOpen());
            return;
        }

        if (enemyPrefab == null) return;

        enemiesSpawned = true;

        for (int i = 0; i < numberOfEnemies; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.parentRoom = this;
            }

            activeEnemies.Add(enemyInstance);
        }
    }

    public void EnemyWasDefeated(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

        if (activeEnemies.Count == 0 && !isCleared)
        {
            isCleared = true;
            StartCoroutine(DelayedGateOpen());
        }
    }

    private IEnumerator DelayedGateOpen()
    {
        yield return new WaitForSeconds(gateOpenDelay);
        OpenAllPassages();
    }

    private IEnumerator DelayedGateClose()
    {
        yield return new WaitForSeconds(gateCloseDelay);
        CloseAllPassages();
    }

    private void OpenAllPassages()
    {
        foreach (var passageGO in associatedPassages)
        {
            if (passageGO != null && passageGO.TryGetComponent<Passage>(out var gate))
            {
                gate.Open();
            }
        }
    }

    private void CloseAllPassages()
    {
        foreach (var passageGO in associatedPassages)
        {
            if (passageGO != null && passageGO.TryGetComponent<Passage>(out var gate))
            {
                gate.Close();
            }
        }
    }
}