using UnityEngine;
using System;
using System.Collections.Generic;
using Random = System.Random;
using System.Linq;

enum Rotation
{
    Right,
    Down,
    Left,
    Up
}

public class MapGenerator : MonoBehaviour
{
    [Header("Room Content Settings")]
    [Tooltip("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private GameObject cube1_1;
    [SerializeField] private GameObject cube2_1;
    [SerializeField] private GameObject cube3_1;
    [SerializeField] private GameObject cube3_2;
    [SerializeField] private GameObject cube4_1;
    [SerializeField] private GameObject cube4_2;
    [SerializeField] private GameObject cube4_3;
    [SerializeField] private GameObject cube4_4;
    [SerializeField] private GameObject cube4_5;
    [SerializeField] private GameObject passageOpen;
    [SerializeField] private GameObject passageClosed;
    [SerializeField] private GameObject character;
    public Color[] colors;
    [SerializeField] private int size = 3;
    [SerializeField] int[,] matrix;
    int startX = 0; // Start points from up left corner
    int startY = 0;
    [SerializeField] int tileSize = 30;
    bool stop = false;
    List<Cell> availableCells = new List<Cell>();
    int roomsCount = 0;
    List<Passage> availablePassages = new List<Passage>();
    List<Passage> requiredPassages = new List<Passage>();
    List<Passage> unnecessaryPassages = new List<Passage>();
    private Dictionary<int, Room> rooms = new Dictionary<int, Room>();
    public Room currentActiveRoom;
    public GameObject currentActivePassage;

    public static MapGenerator instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        matrix = new int[size, size];
        FillMatrix();
        CreatePassages();
        CheckPassages();
        SpawnPassages();
        StartGame();
    }

    void CheckPassages()
    {
        while (availablePassages.Count > 0)
        {
            Random random = new Random();
            List<Passage> chosenPassages = new List<Passage>(availablePassages);

            int[] uniqueRooms = chosenPassages.SelectMany(p => p.Rooms).Distinct().ToArray();
            int randomIndex = random.Next(uniqueRooms.GetLength(0));
            chosenPassages.RemoveAll(p => !Array.Exists(p.Rooms, room => room == uniqueRooms[randomIndex]));

            randomIndex = random.Next(chosenPassages.Count);
            Passage chosenPassage = chosenPassages[randomIndex];
            availablePassages.RemoveAll(p => p.ComparePassages(chosenPassage));
            if (!IsEveryRoomAvailable())
            {
                requiredPassages.Add(chosenPassage);
            } else
            {
                unnecessaryPassages.Add(chosenPassage);
            }
        }
    }

    bool IsEveryRoomAvailable()
    {
        HashSet<int> accessibleRooms = new HashSet<int> { 1 };
        List<Passage> allPassages = availablePassages.Concat(requiredPassages).ToList();

        while (true)
        {
            int initialCount = accessibleRooms.Count;
            foreach (Passage passage in allPassages.Where(p => p.Rooms.Any(r => accessibleRooms.Contains(r))))
            {
                accessibleRooms.UnionWith(passage.Rooms);
            }

            if (accessibleRooms.Count == roomsCount) return true;
            if (initialCount == accessibleRooms.Count) return false;
        }
    }


    void FillMatrix()
    {
        Random random = new Random();
        int cellNumber = 1;
        while (!stop)
        {
            FindEmptyCell();
            if (stop) return;
            CheckAvailablePatterns();

            int[] sizesArray = availableCells.Select(cell => cell.CellSize).Distinct().ToArray(); // Choose random size
            int chosenSize = random.Next(1, sizesArray.GetLength(0) + 1);
            availableCells.RemoveAll(cell => cell.CellSize != chosenSize);

            int[] typesArray = availableCells.Select(cell => cell.Type).Distinct().ToArray(); // Choose random type
            int chosenType = random.Next(typesArray.GetLength(0));
            availableCells.RemoveAll(cell => cell.Type != typesArray[chosenType]);

            int randomIndex = random.Next(availableCells.Count); // Choose random figure
            Cell randomCell = availableCells[randomIndex];

            FillMatrixCell(cellNumber, randomCell.CellSize, randomCell.StartX, randomCell.StartY, randomCell.Type, randomCell.Flip, randomCell.Rotate);
            roomsCount = cellNumber++;
            availableCells.Clear();
        }
    }

    void FillMatrixCell(int cellNumber, int cellSize, int startX, int startY, int type, int flip, int rotate)
    {
        (int a, int b) = GetRotationOffset((Rotation)rotate);
        GameObject spawnedObject;
        GameObject objectType = cube1_1;
        Quaternion rotation = Quaternion.Euler(0, (rotate) * 90, 0);
        Debug.Log($"CeLL: {cellNumber}, SIZE: {cellSize}, X: {startX}, Y: {startY}, TYPE: {type}, FLIP: {flip}, ROTATION: {rotate}");
        switch (cellSize)
        {
            case 1:
                matrix[startY, startX] = cellNumber;
                break;
            case 2:
                matrix[startY, startX] = matrix[startY + 1 * a, startX + 1 * b] = cellNumber;
                objectType = cube2_1;
                break;
            case 3:
                switch (type)
                {
                    case 0:
                        matrix[startY, startX] = matrix[startY + 1 * a, startX + 1 * b] = matrix[startY + 2 * a, startX + 2 * b] = cellNumber;
                        objectType = cube3_1;
                        break;
                    default:
                        matrix[startY, startX] = matrix[startY + a, startX + b] = matrix[startY + 1 * b - (2 * flip * b) + 1 * a, startX + 1 * b - 1 * a + (2 * flip * a)] = cellNumber;
                        objectType = cube3_2;
                        break;
                }
                break;
            case 4:
                switch (type)
                {
                    case 0:
                        matrix[startY, startX] = matrix[startY + 1 * a, startX + 1 * b] = matrix[startY + 2 * a, startX + 2 * b] = matrix[startY + 3 * a, startX + 3 * b] = cellNumber;
                        objectType = cube4_1;
                        break;
                    case 1:
                        matrix[startY, startX] = matrix[startY + a, startX + b] = matrix[startY + 1 * b - (2 * flip * b) + 1 * a, startX + 1 * b - 1 * a + (2 * flip * a)] = matrix[startY + 2 * a, startX + 2 * b] = cellNumber;
                        objectType = cube4_2;
                        break;
                    case 2:
                        matrix[startY, startX] = matrix[startY + b + a, startX] = matrix[startY, startX + b - a] = matrix[startY + a + b, startX - a + b] = cellNumber;
                        objectType = cube4_3;
                        break;
                    case 3:
                        matrix[startY, startX] = matrix[startY + 1 * a, startX + 1 * b] = matrix[startY + 2 * a, startX + 2 * b] = matrix[startY + 1 * b - (2 * flip * b) + 2 * a, startX + 2 * b - 1 * a + (2 * flip * a)] = cellNumber;
                        objectType = cube4_4;
                        break;
                    case 4:
                        matrix[startY, startX] = matrix[startY + a, startX + b] = matrix[startY + 1 * b - (2 * flip * b) + 1 * a, startX + 1 * b - 1 * a + (2 * flip * a)] = matrix[startY + 1 * b - (2 * flip * b) + 2 * a, startX + 2 * b - 1 * a + (2 * flip * a)] = cellNumber;
                        objectType = cube4_5;
                        break;
                }
                break;
        }
        spawnedObject = Instantiate(objectType, new Vector3(startY * 2 * tileSize, 0, startX * 2 * tileSize), rotation);
        Room roomComponent = spawnedObject.GetComponent<Room>();
        roomComponent.roomId = cellNumber;
        roomComponent.SetupRoom(enemyPrefab);
        rooms[cellNumber] = roomComponent;

        RoomTriggerInfo triggerInfo = spawnedObject.GetComponent<RoomTriggerInfo>();
        triggerInfo.roomId = cellNumber;
        if (spawnedObject.GetComponent<Collider>() == null)
        {
            BoxCollider collider = spawnedObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(tileSize * 1.8f, 5, tileSize * 1.8f);
        }
        else
        {
            spawnedObject.GetComponent<Collider>().isTrigger = true;
        }
        spawnedObject.transform.localScale = new Vector3(1 + -2 * flip, 1, 1); 
    }

    void FindEmptyCell()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (matrix[i, j] == 0)
                {
                    startX = j;
                    startY = i;
                    return;
                }
            }
        }
        stop = true;

    }

    void CheckAvailablePatterns() // "Trace" method
    {
        int maxTypes = 5;
        int maxRotation = 1;
        int maxFlip = 1;
        for (int s = 1; s < 5; s++)
        {
            switch (s)
            {
                case 1:
                    maxTypes = 1;
                    maxRotation = 1;
                    maxFlip = 1;
                    break;
                case 2:
                    maxTypes = 1;
                    maxRotation = 4;
                    maxFlip = 1;
                    break;
                case 3:
                    maxTypes = 2;
                    maxRotation = 4;
                    break;
                case 4:
                    maxTypes = 5;
                    maxRotation = 4;
                    break;
            }
            for (int t = 0; t < maxTypes; t++)
            {
                if (t == 0 || t == 2) { maxFlip = 1; }
                else maxFlip = 2;
                for (int f = 0; f < maxFlip; f++)
                {
                    for (int r = 0; r < maxRotation; r++)
                    {
                        (int a, int b) = GetRotationOffset((Rotation)r);
                        try
                        {
                            CheckPatterns(s, a, b, t, f, r);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
    }

    void CheckPatterns(int size, int a, int b, int type, int flip, int rotation)
    {
        switch (size)
        {
            case 1:
                if (matrix[startY, startX] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                break;
            case 2:
                if (matrix[startY, startX] == 0 && matrix[startY + 1 * a, startX + 1 * b] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                break;
            case 3:
                switch (type)
                {
                    case 0:
                        if (matrix[startY, startX] == 0 && matrix[startY + 1 * a, startX + 1 * b] == 0 && matrix[startY + 2 * a, startX + 2 * b] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                        break;
                    default:
                        if (matrix[startY, startX] == 0 && matrix[startY + a, startX + b] == 0 && matrix[startY + 1 * b - (2 * flip * b) + 1 * a, startX + 1 * b - 1 * a + (2 * flip * a)] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                        break;
                }
                break;
            case 4:
                switch (type)
                {
                    case 0:
                        if (matrix[startY, startX] == 0 && matrix[startY + 1 * a, startX + 1 * b] == 0 && matrix[startY + 2 * a, startX + 2 * b] == 0 && matrix[startY + 3 * a, startX + 3 * b] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                        break;
                    case 1:
                        if (matrix[startY, startX] == 0 && matrix[startY + a, startX + b] == 0 && matrix[startY + 1 * b - (2 * flip * b) + 1 * a, startX + 1 * b - 1 * a + (2 * flip * a)] == 0 && matrix[startY + 2 * a, startX + 2 * b] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                        break;
                    case 2:
                        if (matrix[startY, startX] == 0 && matrix[startY + b + a, startX] == 0 && matrix[startY, startX + b - a] == 0 && matrix[startY + a + b, startX - a + b] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                        break;
                    case 3:
                        if (matrix[startY, startX] == 0 && matrix[startY + 1 * a, startX + 1 * b] == 0 && matrix[startY + 2 * a, startX + 2 * b] == 0 && matrix[startY + 1 * b - (2 * flip * b) + 2 * a, startX + 2 * b - 1 * a + (2 * flip * a)] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                        break;
                    case 4:
                        if (matrix[startY, startX] == 0 && matrix[startY + a, startX + b] == 0 && matrix[startY + 1 * b - (2 * flip * b) + 1 * a, startX + 1 * b - 1 * a + (2 * flip * a)] == 0 && matrix[startY + 1 * b - (2 * flip * b) + 2 * a, startX + 2 * b - 1 * a + (2 * flip * a)] == 0) availableCells.Add(new Cell(size, startX, startY, type, flip, rotation));
                        break;
                }
                break;
        }
    }

    void CreatePassages()
    {
        for (int i = 0; i < size; i++) // Horizontal
        {
            for (int j = 0; j < size - 1; j++)
            {
                if (matrix[i, j] != matrix[i, j + 1])
                {
                    availablePassages.Add(new Passage(j, i, j + 1, i, matrix[i, j], matrix[i, j + 1]));
                }
            }
        }

        for (int i = 0; i < size - 1; i++) // Vertical
        {
            for (int j = 0; j < size; j++)
            {
                if (matrix[i, j] != matrix[i + 1, j])
                {
                    availablePassages.Add(new Passage(j, i, j, i + 1, matrix[i, j], matrix[i + 1, j]));
                }
            }
        }
    }

    void SpawnPassages()
    {
        foreach (Passage passage in requiredPassages)
        {
            Quaternion rotation = Quaternion.Euler(0, (passage.coords[0, 1] - passage.coords[1, 1]) * 90, 0);
            GameObject passageObj = Instantiate(passageOpen, new Vector3((passage.coords[0, 1] + passage.coords[1, 1]) * tileSize, 0, (passage.coords[0, 0] + passage.coords[1, 0]) * tileSize), rotation);
            rooms[passage.Rooms[0]].associatedPassages.Add(passageObj);
            rooms[passage.Rooms[1]].associatedPassages.Add(passageObj);
        }
        foreach (Passage passage in unnecessaryPassages)
        {
            Quaternion rotation = Quaternion.Euler(0, (passage.coords[0, 1] - passage.coords[1, 1]) * 90, 0);
            GameObject passageObj = Instantiate(passageClosed, new Vector3((passage.coords[0, 1] + passage.coords[1, 1]) * tileSize, 0, (passage.coords[0, 0] + passage.coords[1, 0]) * tileSize), rotation);
            rooms[passage.Rooms[0]].associatedPassages.Add(passageObj);
            rooms[passage.Rooms[1]].associatedPassages.Add(passageObj);
        }
        for (int i = 0; i < size; i++)
        {
            Instantiate(passageClosed, new Vector3((-1), 0, i * 2) * tileSize, Quaternion.Euler(0, 90, 0));
            Instantiate(passageClosed, new Vector3((size * 2 - 1) * tileSize, 0, i * 2 * tileSize), Quaternion.Euler(0, 90, 0));
            Instantiate(passageClosed, new Vector3(i * 2, 0, -1) * tileSize, Quaternion.Euler(0, 0, 0));
            Instantiate(passageClosed, new Vector3(i * 2 * tileSize, 0, (size * 2 - 1) * tileSize), Quaternion.Euler(0, 0, 0));
        }
    }

    private void StartGame()
    {
        Instantiate(character, new Vector3(0, 1.5f, 0), Quaternion.Euler(0, 0, 0));
        foreach (var room in rooms.Values)
        {
            room.Deactivate();
        }

        if (rooms.ContainsKey(1))
        {
            UpdateActiveRoom(1);
        }
    }

    public void UpdateActiveRoom(int newRoomId)
    {
        if (rooms.ContainsKey(newRoomId))
        {
            Room newRoom = rooms[newRoomId];

            if (newRoom == currentActiveRoom) return;

            if (currentActiveRoom != null)
            {
                currentActiveRoom.activePassage = currentActivePassage;
                currentActiveRoom.Deactivate();
            }

            newRoom.Activate();
            currentActiveRoom = newRoom;
        }
    }

    (int a, int b) GetRotationOffset(Rotation rotation)
    {
        return rotation switch
        {
            Rotation.Right => (0, 1),
            Rotation.Down => (1, 0),
            Rotation.Left => (0, -1),
            Rotation.Up => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), "Invalid rotation value")
        };
    }


    struct Cell
    {
        public int CellSize;
        public int StartX;
        public int StartY;
        public int Type;
        public int Flip;
        public int Rotate;

        public Cell(int cellSize, int startX, int startY, int type, int flip, int rotate)
        {
            CellSize = cellSize;
            StartX = startX;
            StartY = startY;
            Type = type;
            Flip = flip;
            Rotate = rotate;
        }

        public void PrintInfo()
        {
            Debug.Log($"\nCell Size ={CellSize}, Start=({StartX},{StartY}), Type={Type}, Flip={Flip}, Rotate={Rotate}");
        }
    }

    struct Passage
    {
        public int[,] coords;
        public int[] Rooms;

        public Passage(int x1, int y1, int x2, int y2, int room1, int room2)
        {
            coords = new int[2, 2];
            Rooms = new int[2];
            coords[0, 0] = x1;
            coords[0, 1] = y1;
            coords[1, 0] = x2;
            coords[1, 1] = y2;
            Rooms[0] = room1;
            Rooms[1] = room2;
        }
        public bool ComparePassages(Passage otherPassage)
        {
            if (coords[0, 0] == otherPassage.coords[0, 0] &&
            coords[0, 1] == otherPassage.coords[0, 1] &&
            coords[1, 0] == otherPassage.coords[1, 0] &&
            coords[1, 1] == otherPassage.coords[1, 1] &&
            Rooms[0] == otherPassage.Rooms[0] &&
            Rooms[1] == otherPassage.Rooms[1])
            {
                return true;
            }
            else return false;
        }

        public void Show()
        {
            Debug.Log($"\nPassage Start=({coords[0, 0]}, {coords[0, 1]}), End=({coords[1, 0]}, {coords[1, 1]}) Room1 ({Rooms[0]}) Room2 ({Rooms[1]})");
        }
    }



    void Update()
    {

    }
}
