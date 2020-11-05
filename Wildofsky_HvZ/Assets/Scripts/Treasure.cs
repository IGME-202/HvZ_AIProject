using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    public void OnGrab()
    {
        MeshRenderer floor = GameObject.FindWithTag("Floor").GetComponent<MeshRenderer>();
        Vector3 max = floor.bounds.max;
        Vector3 min = floor.bounds.min;

        // When the treasure is grabbed by the human, give it a new random location within the floor object
        transform.position = new Vector3(Random.Range(min.x, max.x), GetComponent<MeshRenderer>().bounds.extents.y, Random.Range(min.z, max.z));
    }
}
