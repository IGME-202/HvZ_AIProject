using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Human : Vehicle
{
    List<GameObject> activeZombies;
    public float detectionRange = 5f;

    public Material debugForward;
    public Material debugRight;
    public Material debugPrediction;

    public float evadeZombiesFactor = 1f;

    protected override void Start()
    {
        base.Start();

        activeZombies = HvZ_Manager.Instance.activeZombies;
    }

    protected override void CalcSteeringForces()
    {
        // Accumulate all forces before applying them to the object
        Vector3 netForce = Vector3.zero;

        netForce += ObstacleAvoidance() * obstacleAvoidanceFactor;

        // Humans seek the treasure
        //netForce += Seek(treasure.gameObject) * seekTreasureFactor;
        netForce += BoundaryEvasion(predictionFactor) * boundaryEvasionFactor;

        float numZombiesDetected = 0;
        foreach (GameObject zombie in activeZombies)
        {
            // If within the human's detection range, humans flee zombies
            if (Vector3.Distance(planarPosition, zombie.GetComponent<Zombie>().planarPosition) < detectionRange)
            {
                netForce += Evade(zombie, predictionFactor) * evadeZombiesFactor;
                numZombiesDetected++;
            }
        }
        if (numZombiesDetected == 0)
        {
            netForce += Wandering() * wanderingFactor;
        }

        netForce += Separation() * separationFactor;

        // Clamp the net force to a max force to keep the simulation reasonable
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.D))
        {
            debugEnabled = !debugEnabled;
        }
    }

    // When the human object is selected in Unity editor, display zombie detection range and grab radius as gizmos
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);
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

            Vector3 scale = new Vector3(0.4f, 0.4f, 0.4f);
            Matrix4x4 debugPos = Matrix4x4.TRS(futurePosition, Quaternion.identity, scale);

            debugPrediction.SetPass(0);
            Graphics.DrawMeshNow(futurePosMesh, debugPos);
        }
    }
}
