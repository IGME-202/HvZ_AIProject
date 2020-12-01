using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public GameObject obstacle;
    public GameObject humanParent;
    public GameObject zombieParent;
    public GameObject obstacleParent;
    [NonSerialized]
    public List<GameObject> activeHumans;
    [NonSerialized]
    public List<GameObject> activeZombies;
    [NonSerialized]
    public List<GameObject> allVehicles;
    [NonSerialized]
    public List<GameObject> activeObstacles;
    public int numHumans = 10;
    public int numZombies = 5;
    public int numObstacles = 20;
    public Text humanCount;
    public Text zombieCount;
    public Text obstacleCount;

    MeshRenderer floor;
    Vector3 max;
    Vector3 min;

    [NonSerialized]
    public bool placingObstacles = false;
    [NonSerialized]
    public bool placingHumans = false;
    [NonSerialized]
    public bool placingZombies = false;
    public Image placementMarker;

    private void Awake()
    {
        mgrInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        activeHumans = new List<GameObject>();
        activeZombies = new List<GameObject>();
        activeObstacles = new List<GameObject>();
        allVehicles = new List<GameObject>();

        floor = GameObject.FindWithTag("Floor").GetComponent<MeshRenderer>();
        max = floor.bounds.max;
        min = floor.bounds.min;

        InstantiateAllInto(human, activeHumans, numHumans, humanParent);
        InstantiateAllInto(zombie, activeZombies, numZombies, zombieParent);
        InstantiateAllInto(obstacle, activeObstacles, numObstacles, obstacleParent);
        allVehicles.AddRange(activeHumans);
        allVehicles.AddRange(activeZombies);
    }

    // Update is called once per frame
    void Update()
    {
        ZombieConversion();

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync("HumansVsZombies");
        }

        PlaceObjects();

        humanCount.text = activeHumans.Count.ToString();
        zombieCount.text = activeZombies.Count.ToString();
        obstacleCount.text = activeObstacles.Count.ToString();
    }

    private void InstantiateAllInto(GameObject obj, List<GameObject> objList, int iterations, GameObject parent)
    {
        for (int i = 0; i < iterations; i++)
        {
            objList.Add(Instantiate(obj, GetRandomPosition(obj), Quaternion.identity, parent.transform));
        }
    }

    private void AddToScene(GameObject obj, List<GameObject> objList, Vector3 position, GameObject parent, bool isVehicle)
    {
        position.y = obj.GetComponent<MeshRenderer>().bounds.extents.y;

        GameObject objToAdd = Instantiate(obj, position, Quaternion.identity, parent.transform);
        objList.Add(objToAdd);
        if (isVehicle)
        {
            allVehicles.Add(objToAdd);
        }
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
                    allVehicles.Remove(humanToConvert);
                    Destroy(humanToConvert);
                    AddToScene(zombie, activeZombies, convertPos, zombieParent, true);
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

    public void ToggleObstaclePlacement()
    {
        placingObstacles = !placingObstacles;
        placingHumans = false;
        placingZombies = false;
    }

    public void ToggleHumanPlacement()
    {
        placingHumans = !placingHumans;
        placingObstacles = false;
        placingZombies = false;
    }

    public void ToggleZombiePlacement()
    {
        placingZombies = !placingZombies;
        placingObstacles = false;
        placingHumans = false;
    }

    private void PlaceObjects()
    {
        if (placingObstacles || placingHumans || placingZombies)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool withinBounds = true;
            placementMarker.gameObject.SetActive(true);

            if (mousePosition.x > max.x || mousePosition.x < min.x || mousePosition.z > max.z || mousePosition.z < min.z)
            {
                withinBounds = false;
            }

            if (withinBounds)
            {
                // Match the marker position to the mouse position relative to the canvas UI
                Vector3 markerPosition = new Vector3(0, placementMarker.transform.position.y, 0);
                markerPosition.x = mousePosition.x;
                markerPosition.z = mousePosition.z;
                placementMarker.transform.position = markerPosition;
            }

            if (Input.GetMouseButtonDown(0) && withinBounds && placingObstacles)
            {
                AddToScene(obstacle, activeObstacles, mousePosition, obstacleParent, false);
            }
            else if (Input.GetMouseButtonDown(0) && withinBounds && placingHumans)
            {
                AddToScene(human, activeHumans, mousePosition, humanParent, true);
            }
            else if (Input.GetMouseButtonDown(0) && withinBounds && placingZombies)
            {
                AddToScene(zombie, activeZombies, mousePosition, zombieParent, true);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                placingObstacles = false;
                placingHumans = false;
                placingZombies = false;
            }
        }
        else
        {
            placementMarker.gameObject.SetActive(false);
        }
    }
}
