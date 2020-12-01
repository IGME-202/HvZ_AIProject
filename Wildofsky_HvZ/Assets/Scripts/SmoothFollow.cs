using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This camera smoothes out rotation around the y-axis and height.
// Horizontal Distance to the target is always fixed.
// For every one of those smoothed values, calculate the wanted value and the current value.
// Smooth it using the Lerp function and apply the smoothed values to the transform's position.
public class SmoothFollow : MonoBehaviour
{
	public Transform target;
	public int targetIndex = 0;
	public float distance = 3.0f;
	public float height = 1.50f;
	public float heightDamping = 2.0f;
	public float positionDamping = 2.0f;
	public float rotationDamping = 2.0f;

	// Update is called once per frame
	private void Update()
    {
		// Allow the user to then look at the next and previous vehicles in the list
		if (Input.GetMouseButtonDown(0))
		{
			targetIndex++;
			if (targetIndex >= HvZ_Manager.Instance.allVehicles.Count)
			{
				targetIndex = 0;
			}
		}
		else if (Input.GetMouseButtonDown(1))
		{
			targetIndex--;
			if (targetIndex < 0)
			{
				targetIndex = HvZ_Manager.Instance.allVehicles.Count - 1;
			}
		}
		// Apply the users target selection
		target = HvZ_Manager.Instance.allVehicles[targetIndex].transform;
	}

    void LateUpdate()
	{
		// Early out if we don't have a target
		if (!target) return;

		float wantedHeight = target.position.y + height;
		float currentHeight = transform.position.y;

		// Damp the height
		currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

		// Set the position of the camera 
		Vector3 wantedPosition = target.position - target.forward * distance;
		transform.position = Vector3.Lerp(transform.position, wantedPosition,
										 Time.deltaTime * positionDamping);

		// Adjust the height of the camera
		transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

		// Set the forward to rotate with time
		transform.forward = Vector3.Lerp(transform.forward, target.forward,
		  Time.deltaTime * rotationDamping);
	}
}

