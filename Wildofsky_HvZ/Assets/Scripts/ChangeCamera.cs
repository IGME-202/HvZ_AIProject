using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    // Camera array that holds a reference to every camera in the scene
    public Camera[] cameras;

    // Current camera 
    private int currentCameraIndex;


    // Start is called before the first frame update
    void Start()
    {
        currentCameraIndex = 0;

        // Turn all cameras off, except the first default one
        for (int i = 1; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        // If any cameras were added to the controller, enable the first one
        if (cameras.Length > 0)
        {
            cameras[0].gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Press the 'C' key to cycle through cameras in the array
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Turn off placement of objects
            HvZ_Manager.Instance.placingObstacles = false;
            HvZ_Manager.Instance.placingHumans = false;
            HvZ_Manager.Instance.placingZombies = false;

            // Cycle to the next camera
            currentCameraIndex++;

            // If cameraIndex is in bounds, set this camera active and last one inactive
            if (currentCameraIndex < cameras.Length)
            {
                cameras[currentCameraIndex - 1].gameObject.SetActive(false);
                cameras[currentCameraIndex].gameObject.SetActive(true);
            }
            // If last camera, cycle back to first camera
            else
            {
                cameras[currentCameraIndex - 1].gameObject.SetActive(false);
                currentCameraIndex = 0; cameras[currentCameraIndex].gameObject.SetActive(true);
            }

            GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>().worldCamera = cameras[currentCameraIndex];
        }
    }

    public void SwitchToCameraIndex(int index)
    {
        if (cameras != null && cameras.Length > 0)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                if (i == index)
                {
                    cameras[index].gameObject.SetActive(true);
                }
                else
                {
                    cameras[i].gameObject.SetActive(false);
                }
            }

            currentCameraIndex = index;
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>().worldCamera = cameras[index];
        }
    }
}
