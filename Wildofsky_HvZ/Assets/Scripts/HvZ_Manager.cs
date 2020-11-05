using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HvZ_Manager : MonoBehaviour
{
    // This class is a basic singleton and will be accessed through a single instance
    private static HvZ_Manager mgrInstance;
    public static HvZ_Manager Instance
    {
        get
        {
            return mgrInstance;
        }
    }

    public GameObject human;
    public GameObject zombie;
    public GameObject treasure;
    [NonSerialized]
    public List<GameObject> activeHumans;
    [NonSerialized]
    public List<GameObject> activeZombies;
    [NonSerialized]
    public GameObject activeTreasure;
    public int numHumans = 10;
    public int numZombies = 5;

    MeshRenderer floor;
    Vector3 max;
    Vector3 min;

    private void Awake()
    {
        mgrInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        activeHumans = new List<GameObject>();
        activeZombies = new List<GameObject>();

        floor = GameObject.FindWithTag("Floor").GetComponent<MeshRenderer>();
        max = floor.bounds.max;
        min = floor.bounds.min;

        InstantiateAllInto(human, activeHumans, numHumans);
        InstantiateAllInto(zombie, activeZombies, numZombies);
        activeTreasure = Instantiate(treasure, GetRandomPosition(treasure), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        ZombieConversion();
    }

    private void InstantiateAllInto(GameObject obj, List<GameObject> objList, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            objList.Add(Instantiate(obj, GetRandomPosition(obj), Quaternion.identity));
        }
    }

    private void AddToScene(GameObject obj, List<GameObject> objList, Vector3 position)
    {
        position.y = obj.GetComponent<MeshRenderer>().bounds.extents.y;

        objList.Add(Instantiate(obj, position, Quaternion.identity));
    }

    private Vector3 GetRandomPosition(GameObject obj)
    {
        return new Vector3(UnityEngine.Random.Range(min.x, max.x), obj.GetComponent<MeshRenderer>().bounds.extents.y, UnityEngine.Random.Range(min.z, max.z));
    }

    private void ZombieConversion()
    {
        for (int i = 0; i < activeZombies.Count; i++)
        {
            for (int j = 0; j < activeHumans.Count; j++)
            {
                if (CollisionCheck(activeZombies[i], activeHumans[j]))
                {
                    GameObject humanToConvert = activeHumans[j];
                    Vector3 convertPos = humanToConvert.transform.position;
                    activeHumans.RemoveAt(j);
                    Destroy(humanToConvert);
                    AddToScene(zombie, activeZombies, convertPos);
                    j = -1;
                }
            }
        }
    }

    private bool CollisionCheck(GameObject zombie, GameObject human)
    {
        bool isColliding = false;
        MeshRenderer humanMesh = human.GetComponent<MeshRenderer>();
        MeshRenderer zombieMesh = zombie.GetComponent<MeshRenderer>();
        Human humanProperties = human.GetComponent<Human>();
        Zombie zombieProperties = zombie.GetComponent<Zombie>();

        if ((zombieMesh.bounds.center - humanMesh.bounds.center).magnitude < zombieProperties.radius + humanProperties.radius)
        {
            isColliding = true;
        }

        return isColliding;
    }
}
