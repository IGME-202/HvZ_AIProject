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
    public Material debugPrediction;

    public float pursueHumanFactor = 1f;

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
        // Accumulate all forces before applying them to the object
        Vector3 netForce = Vector3.zero;

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
            netForce += Pursue(targetHuman, predictionFactor) * pursueHumanFactor;
        }
        // If there are no more humans in the scene, zombies will all arrive at the center of the platform (temporary)
        else
        {
            targetHuman = null;

            netForce += Wandering() * wanderingFactor;
        }

        netForce += ObstacleAvoidance() * obstacleAvoidanceFactor;
        netForce += BoundaryEvasion(predictionFactor) * boundaryEvasionFactor;
        netForce += Separation() * separationFactor;
        // Clamp the net force to a max force to keep the simulation reasonable
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }

    private void OnRenderObject()
    {
        if (debugEnabled)
        {
            // Set the material to be used for the forward debug line
            debugForward.SetPass(0);

            // Draw the forward debug line
            GL.Begin(GL.LINES);
            GL.Vertex(position);
            GL.Vertex(position + forward * debugMultiplier);
            GL.End();

            // Set the material to be used for the right debug line
            debugRight.SetPass(0);


            // Draw the right debug line
            GL.Begin(GL.LINES);
            GL.Vertex(position);
            GL.Vertex(position + right * debugMultiplier);
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

            Vector3 scale = new Vector3(0.4f, 0.4f, 0.4f);
            Matrix4x4 debugPos = Matrix4x4.TRS(futurePosition, Quaternion.identity, scale);

            debugPrediction.SetPass(0);
            Graphics.DrawMeshNow(futurePosMesh, debugPos);
        }
    }
}
