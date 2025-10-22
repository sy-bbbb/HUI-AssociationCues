using System.Collections.Generic;
using UnityEngine;

public struct ObjectTransform
{
    public Vector3 Position;
    public Quaternion Rotation;
}

public struct RootTransform
{
    public Vector3 Position;
    public Vector3 Scale;
}

public class TaskData
{
    public List<List<string>> BlockPrefabNames;
    public RootTransform TaskRootTransform;
    public List<ObjectTransform> PrefabTransforms;
}


public class BlockDataManager : MonoBehaviour
{
    public static BlockDataManager Instance { get; private set; }

    private Dictionary<StudySettings.Task, TaskData> allTaskData;
    private List<ObjectTransform> prefabTransforms;
    private Vector3 originTask = Vector3.zero;
    private int nObjects = 6;

    [Header("Task 1 Settings")]
    [SerializeField] private float radiusTask1 = 1.5f;
    [SerializeField] private float startAngleTask1 = -90f;
    [SerializeField] private float endAngleTask1 = 90f;

    [Header("Task 2 Settings")]
    [SerializeField] private float radiusTask2 = 1.5f;
    [SerializeField] private float startAngleTask2 = -25f;
    [SerializeField] private float endAngleTask2 = 25f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            InitializeTaskData();
            //InitializePrefabTransforms();
        }
    }

    public RootTransform GetRootTransformForTask(StudySettings.Task task)
    {
        if (allTaskData.TryGetValue(task, out TaskData data))
            return data.TaskRootTransform;
        return new RootTransform { Position = Vector3.zero, Scale = Vector3.one }; // Return default
    }

    public List<string> GetPrefabNamesForBlock(StudySettings.Task task, int blockId)
    {
        if (allTaskData.TryGetValue(task, out TaskData data))
        {
            int blockIndex = blockId - 1;
            if (blockIndex >= 0 && blockIndex < data.BlockPrefabNames.Count)
                return data.BlockPrefabNames[blockIndex];
        }
        return null;
    }

    public List<ObjectTransform> GetPrefabTransforms(StudySettings.Task task)
    {
        if (allTaskData.TryGetValue(task, out TaskData data))
            return data.PrefabTransforms;
        return new List<ObjectTransform>();
    }
    private void InitializeTaskData()
    {
        allTaskData = new Dictionary<StudySettings.Task, TaskData>
        {
            {
                StudySettings.Task.task1, new TaskData
                {
                    TaskRootTransform = new RootTransform { Position = new Vector3 (0f, -0.4f, 0f), Scale = Vector3.one },
                    BlockPrefabNames = new List<List<string>>
                    {
                        new List<string> { "OrangeClownfish", "PurpleTang", "FlameAngelfish", "NapoleonWrasse", "Boxfish", "Bannerfish" },
                        new List<string> { "YellowTang", "LinedSurgeon", "AngelfishBlueface", "Threadfin", "BlackDurgon", "SunfishMolaMola" },
                        new List<string> { "ClownTriggerfish", "BlueTang", "AngelfishMagestic", "BanggaiCardinalfish", "ClownfishBlack", "Discus" },
                        new List<string> { "EmperorAngelfish", "RoyalAngelfish", "ScrawledFilefish", "AngelfishMultibarred", "AngelfishQueen", "GobyNemateleotris" },
                    },
                    PrefabTransforms = GenerateTask1Positions()
                }
            },
            {
                StudySettings.Task.task2, new TaskData
                {
                    TaskRootTransform = new RootTransform { Position =  new Vector3 (0f, -0.4f, 0f), Scale = Vector3.one * 0.5f },
                    BlockPrefabNames = new List<List<string>>
                    {
                        new List<string> { "BlackDurgon", "FlameAngelfish", "AngelfishMultibarred", "Threadfin", "Bannerfish", "ClownfishBlack" },
                        new List<string> { "SunfishMolaMola", "YellowTang", "BlueTang", "EmperorAngelfish", "Boxfish", "RoyalAngelfish" },
                        new List<string> { "OrangeClownfish", "PurpleTang", "GobyNemateleotris", "BanggaiCardinalfish", "LinedSurgeon", "Discus" },
                        new List<string> { "ScrawledFilefish", "NapoleonWrasse", "AngelfishBlueface", "AngelfishMagestic", "ClownTriggerfish", "AngelfishQueen" },
                    },
                    PrefabTransforms = GenerateTask2Positions()
                }
            },
            {
                StudySettings.Task.practice, new TaskData
                {
                    TaskRootTransform = new RootTransform { Position =  new Vector3 (0f, -0.4f, 0f), Scale = Vector3.one },
                    BlockPrefabNames = new List<List<string>>
                    {
                        new List<string> { "DoubleSaddle", "Copperband", "Fusilier", "AngelfishFlagfin", "MoorishIdol", "Mandarinfish" },
                        new List<string> { "DoubleSaddle", "Copperband", "Fusilier", "AngelfishFlagfin", "MoorishIdol", "Mandarinfish" },
                        new List<string> { "DoubleSaddle", "Copperband", "Fusilier", "AngelfishFlagfin", "MoorishIdol", "Mandarinfish" },
                        new List<string> { "DoubleSaddle", "Copperband", "Fusilier", "AngelfishFlagfin", "MoorishIdol", "Mandarinfish" },
                    },
                    PrefabTransforms = GeneratePracticePositions()
                }
            }
        };
    }

    private List<ObjectTransform> GenerateTask1Positions()
    {
        List<ObjectTransform> transforms = new List<ObjectTransform>();

        for (int i = 0; i < nObjects; i++)
        {
            Vector3 position = GetCoordinates(i, nObjects, radiusTask1, originTask, startAngleTask1, endAngleTask1);
            transforms.Add(new ObjectTransform
            {
                Position = position,
                Rotation = GetTask1Rotation(i)
            });
        }
        ShuffleList(transforms);

        return transforms;
    }

    private List<ObjectTransform> GenerateTask2Positions()
    {
        List<ObjectTransform> transforms = new List<ObjectTransform>();

        for (int i = 0; i < nObjects; i++)
        {
            Vector3 position = GetCoordinates(i, nObjects, radiusTask2, originTask, startAngleTask2, endAngleTask2);
            transforms.Add(new ObjectTransform
            {
                Position = position,
                Rotation = GetTask2Rotation(i)
            });
        }
        ShuffleList(transforms);

        return transforms;
    }

    private List<ObjectTransform> GeneratePracticePositions()
    {
        List<ObjectTransform> transforms = new List<ObjectTransform>();

        for (int i = 0; i < nObjects; i++)
        {
            Vector3 position = GetCoordinates(i, nObjects, 1.5f, originTask, -60f, 60f);
            transforms.Add(new ObjectTransform
            {
                Position = position,
                Rotation = GetTask2Rotation(i)
            });
        }
        ShuffleList(transforms);

        return transforms;
    }
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }


    private Vector3 GetCoordinates(int index, int totalObjects, float radius, Vector3 origin, float startAngle, float endAngle)
    {
        float angleDeg = startAngle + index * (endAngle - startAngle) / (totalObjects - 1);
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float x = origin.x + radius * Mathf.Sin(angleRad);
        float z = origin.z + radius * Mathf.Cos(angleRad);
        float y = origin.y;

        return new Vector3(x, y, z);
    }

    private Quaternion GetTask1Rotation(int index)
    {
        float angleDeg = startAngleTask1 + index * (endAngleTask1 - startAngleTask1) / (nObjects - 1);
        return Quaternion.Euler(0, angleDeg + 180f, 0);
    }

    private Quaternion GetTask2Rotation(int index)
    {
        float angleDeg = startAngleTask2 + index * (endAngleTask2 - startAngleTask2) / (nObjects - 1);
        return Quaternion.Euler(0, angleDeg, 0);
    }
}

