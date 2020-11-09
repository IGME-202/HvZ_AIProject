using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Zombie : Vehicle
{
    List<GameObject> activeHumans;
    float targetDist;
    GameObject targetHuman;

    public Material debugForward;
    public Material debugRight;
    public Material debugTarget;
    public float debugMultiplier = 1;

    public float stopRadius = 0;
    public float slowRadius = 10;

    protected override void Start()
    {
        base.Start();

        activeHumans = HvZ_Manager.Instance.activeHumans;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.D))
        {
            debugEnabled = !debugEnabled;
        }
    }

    protected override void CalcSteeringForces()
    {
        // If there are humans in the scene, zombie will chase the neareast human using seek
        if (activeHumans.Count > 0)
        {
            targetHuman = null;
            targetDist = Mathf.Infinity;

            foreach (GameObject human in activeHumans)
            {
                Vector3 vectorToTarget = human.GetComponent<Human>().planarPosition - planarPosition;
                float distance = vectorToTarget.magnitude;
                if (distance < targetDist)
                {
                    targetDist = distance;
                    targetHuman = human;
                }
            }

            // Every frame the zombie is just chasing the closest human to it
            ApplyForce(Seek(targetHuman));
        }
        // If there are no more humans in the scene, zombies will all arrive at the center of the platform (temporary)
        else
        {
            targetHuman = null;

            ApplyForce(Arrive(Vector3.zero, slowRadius, stopRadius));

            if (velocity.magnitude <= 0.01f)
            {
                velocity = Vector3.zero;
            }
        }
    }

    private void OnRenderObject()
    {
        if (debugEnabled)
        {
            // Set the material to be used for the forward debug line
            debugForward.SetPass(0);

            Vector3 forward = direction * debugMultiplier;

            // Draw the forward debug line
            GL.Begin(GL.LINES);
            GL.Vertex(position);
            GL.Vertex(position + forward);
            GL.End();

            // Set the material to be used for the right debug line
            debugRight.SetPass(0);

            Vector3 right = Quaternion.Euler(0, 90, 0) * direction;
            right *= debugMultiplier;

            // Draw the right debug line
            GL.Begin(GL.LINES);
            GL.Vertex(position);
            GL.Vertex(position + right);
            GL.End();

            if (targetHuman != null)
            {
                // Set the material to be used for the target human debug line
                debugTarget.SetPass(0);

                // Draw the target human debug line
                GL.Begin(GL.LINES);
                GL.Vertex(position);
                GL.Vertex(targetHuman.transform.position);
                GL.End();
            }
        }
    }
}
