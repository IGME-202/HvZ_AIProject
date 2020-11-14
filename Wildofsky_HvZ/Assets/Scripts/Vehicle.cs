using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Vehicle : MonoBehaviour
{
    protected Vector3 position;
    public Vector3 planarPosition;
    protected Vector3 direction;
    protected Vector3 velocity;
    protected Vector3 acceleration;

    protected Vector3 forward;
    protected Vector3 right;

    [Min(0.00001f)]
    public float mass = 1f;
    public float radius = 1f;
    public float maxSpeed = 1f;
    public float maxForce = 1f;
    public float timeCoeff = 1.5f;

    protected MeshRenderer floor;

    protected bool debugEnabled;

    // Keep a list of all of the obstacles in the scene and which ones to avoid
    protected List<GameObject> obstacles;
    protected List<GameObject> avoidList;
    // Give each moving object circular range to detect obstacles around them
    public float avoidanceRange = 8;
    // Use a mesh box to display a moving object's path so it can be rotated with the object
    public Mesh pathMesh;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        position = transform.position;
        direction = Vector3.right;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        // Initialize a reference to the floor's mesh renderer in start so Unity does not have to find the floor every frame in Bounce()
        floor = GameObject.FindWithTag("Floor").GetComponent<MeshRenderer>();
        avoidList = new List<GameObject>();

        debugEnabled = false;
        // Make sure every zombie generated during runtime has the same value as the zombies already in the scene
        foreach (GameObject obj in HvZ_Manager.Instance.activeZombies)
        {
            if (obj.GetComponent<Vehicle>().debugEnabled)
            {
                debugEnabled = true;
                return;
            }
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Reset acceleration for each frame
        acceleration = Vector3.zero;
        planarPosition = new Vector3(position.x, 0, position.z);

        forward = direction;
        right = Quaternion.Euler(0, 90, 0) * direction;

        // Let the child classes determine what this does
        CalcSteeringForces();
        UpdatePosition();
        //Bounce();
        SetTransform();
    }

    protected void UpdatePosition()
    {
        // Multiply by delta time for each calculation to make velocity and acceleration units per second instead of per frame
        // Use acceleration to change object's velocity
        velocity += acceleration * Time.deltaTime;

        // Use velocity to change object's position
        position += velocity * Time.deltaTime;

        // Continuously keep track of direction through the vehicle's current velocity
        direction = velocity.normalized;
    }

    void Bounce()
    {
        Vector3 max = floor.bounds.max;
        Vector3 min = floor.bounds.min;

        // Reverse the velocity of the object when it hits the edge of the floor to make it bounce
        if (position.x > max.x)
        {
            position.x = max.x;
            velocity.x *= -1;
        }
        if (position.x < min.x)
        {
            position.x = min.x;
            velocity.x *= -1;
        }
        if (position.z > max.z)
        {
            position.z = max.z;
            velocity.z *= -1;
        }
        if (position.z < min.z)
        {
            position.z = min.z;
            velocity.z *= -1;
        }
    }

    protected void SetTransform()
    {
        // Update position
        transform.position = position;
    }

    protected void ApplyForce(Vector3 force)
    {
        // Add every force that relies on mass to acceleration in this way
        acceleration += force / mass;
        // Keep object's movement only in the x-z plane
        acceleration.y = 0;
    }

    protected void ApplyFriction(float coeff)
    {
        // Apply friction based off a coefficient and the opposite direction of curent velocity
        Vector3 friction = velocity * -1;
        friction.Normalize();
        friction = friction * coeff;
        acceleration += friction;
        // Keep object's movement only in the x-z plane
        acceleration.y = 0;
    }

    // Require every child object of this class to have an override of this method
    protected abstract void CalcSteeringForces();

    #region Seek Logic
    protected Vector3 Seek(Vector3 targetPos)
    {
        // Calculate desired velocity
        Vector3 desiredVelocity = targetPos - position;
        // Set desired = max speed
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        // Calculate seek steering force
        Vector3 seekingForce = desiredVelocity - velocity;
        // Return seek steering force
        return seekingForce;
    }

    // Helpful overload that allows for either a Vector3 or GameObject to be passed into Seek 
    protected Vector3 Seek(GameObject target)
    {
        return Seek(target.transform.position);
    }
    #endregion

    #region Flee Logic
    protected Vector3 Flee(Vector3 targetPos)
    {
        // Calculate desired velocity
        Vector3 desiredVelocity = position - targetPos;
        // Set desired = max speed
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        // Calculate flee steering force
        Vector3 fleeingForce = desiredVelocity - velocity;
        // Return flee steering force
        return fleeingForce;
    }

    // Helpful overload that allows for either a Vector3 or GameObject to be passed into Flee 
    protected Vector3 Flee(GameObject target)
    {
        return Flee(target.transform.position);
    }
    #endregion

    protected Vector3 Arrive(Vector3 targetPos, float slowDown, float stop)
    {
        // Calculate desired velocity
        Vector3 desiredVelocity = targetPos - position;
        // Calculate the distance from the target from the desired velocity
        float distance = desiredVelocity.magnitude;
        desiredVelocity = desiredVelocity.normalized;

        if (distance < slowDown)
        {
            float arriveSpeed = Remap(distance, stop, slowDown, 0, maxSpeed);
            desiredVelocity *= arriveSpeed;
        }
        else
        {
            desiredVelocity *= maxSpeed;
        }

        // Calculate arrive steering force
        Vector3 seekingForce = desiredVelocity - velocity;
        // Return arrive steering force
        return seekingForce;
    }

    // This method is derived from Jessy's Remap method on the Unity forums here:
    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    /// <summary>
    /// Re-maps a number from one range to another
    /// </summary>
    /// <param name="value">The number to change through Remap</param>
    /// <param name="range1Start">The beginning number in the first range</param>
    /// <param name="range1End">The ending number in the first range</param>
    /// <param name="range2Start">The beginning number in the second range</param>
    /// <param name="range2End">The ending number in the second range</param>
    /// <returns>A float number that is the remapped version on the value inputted</returns>
    protected float Remap(float value, float range1Start, float range1End, float range2Start, float range2End)
    {
        return (value - range1Start) / (range1End - range1Start) * (range2End - range2Start) + range2Start;
    }

    protected Vector3 BoundaryEvasion(float timeCoeff)
    {
        Vector3 max = floor.bounds.max;
        Vector3 min = floor.bounds.min;

        Vector3 predictedPos = velocity * timeCoeff + position;
        Vector3 desiredPosition = Vector3.zero;
        Vector3 boundaryForce = Vector3.zero;
        float distance = 1;
        bool atBoundary = false;

        if (predictedPos.x > max.x)
        {
            distance = max.x - position.x;
            desiredPosition = velocity;
            desiredPosition.x *= -1;
            atBoundary = true;
        }
        if (predictedPos.x < min.x)
        {
            distance = position.x - min.x;
            desiredPosition = velocity;
            desiredPosition.x *= -1;
            atBoundary = true;
        }
        if (predictedPos.z > max.z)
        {
            distance = max.z - position.z;
            if (desiredPosition.magnitude == 0)
            {
                desiredPosition = velocity;
            }
            desiredPosition.z *= -1;
            atBoundary = true;
        }
        if (predictedPos.z < min.z)
        {
            distance = position.z - min.z;
            if (desiredPosition.magnitude == 0)
            {
                desiredPosition = velocity;
            }
            desiredPosition.z *= -1;
            atBoundary = true;
        }

        if (distance <= 0)
        {
            distance = 0.0001f;
        }
        if (atBoundary)
        {
            boundaryForce = Seek(desiredPosition) / distance;
        }
        return boundaryForce;
    }

    /*protected bool CheckOutOfBounds()
    {
        Vector3 max = floor.bounds.max;
        Vector3 min = floor.bounds.min;
        bool outOfBounds = false;

        if (position.x > max.x)
        {
            outOfBounds = true;
            if (position.x > max.x + 0.5f)
            {
                position.x = max.x + 0.5f;
            }
        }
        if (position.x < min.x)
        {
            outOfBounds = true;
            if (position.x < min.x - 0.5f)
            {
                position.x = min.x - 0.5f;
            }
        }
        if (position.z > max.z)
        {
            outOfBounds = true;
            if (position.x > max.z + 0.5f)
            {
                position.x = max.z + 0.5f;
            }
        }
        if (position.z < min.z)
        {
            outOfBounds = true;
            if (position.x < min.z - 0.5f)
            {
                position.x = min.z - 0.5f;
            }
        }

        return outOfBounds;
    }*/

    protected Vector3 ObstacleAvoidance()
    {
        Vector3 avoidanceSteering = Vector3.zero;
        Vector3 toObstacle;
        float dotProduct;
        float obstacleDistance;
        avoidList.Clear();

        foreach (GameObject obstacle in obstacles)
        {
            // Vector from moving object to obstacle
            toObstacle = obstacle.transform.position - position;
            dotProduct = Vector3.Dot(velocity, toObstacle);
            obstacleDistance = toObstacle.magnitude;
            
            // If an object happens to go inside an obstacle, make the distance value extremely small so the force to get out of the obstacle is strong
            if (obstacleDistance <= obstacle.GetComponent<Obstacle>().radius + radius)
            {
                obstacleDistance = 0.00001f;
            }

            // If an obstacle is in front of a moving object and within the object's detection range
            if (dotProduct > 0 && 
                obstacleDistance < avoidanceRange + obstacle.GetComponent<Obstacle>().radius)
            {
                // Change the dot product value to a projection of the vector to the obstacle onto the objects right vector(already normalized)
                dotProduct = Vector3.Dot(right, toObstacle);

                // If the obstacle is within a moving object's path
                if (Mathf.Abs(dotProduct) <= radius + obstacle.GetComponent<Obstacle>().radius)
                {
                    // Add the obstacle to the list of obstacles to avoid
                    avoidList.Add(obstacle);

                    // On right, steer left
                    if (dotProduct >= 0)
                    {
                        // Steer an object away from an obstacle based on which side of the object it is on and the closer the object is to the obstacle, the greater the force
                        // Distance rarely ever goes below the radius of the obstacle plus the radius of the object and is at max the detection range(plus the obstacles radius)
                        // So remap changes a number from that range to a number of equivalent distance from the start and end values of a new range
                        // In this case it allows me to divide the steering force by distance and actually have it make the force large when distance is low
                        avoidanceSteering += -right / Remap(obstacleDistance, obstacle.GetComponent<Obstacle>().radius + radius, avoidanceRange, 0.001f, 1);
                    }
                    // On left, steer right
                    else
                    {
                        avoidanceSteering += right / Remap(obstacleDistance, obstacle.GetComponent<Obstacle>().radius + radius, avoidanceRange, 0.001f, 1);
                    }
                }
            }
        }

        return avoidanceSteering;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Draw avoidance area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(position, avoidanceRange);

        // Dar future position
        Gizmos.color = Color.green;
        Vector3 futurePos = position + velocity;
        Gizmos.DrawWireSphere(futurePos, radius);

        // Draw velocity path area
        Gizmos.color = Color.red;
        Vector3 boxSize = new Vector3(radius * 2f, radius * 2f, Vector3.Distance(position, futurePos) + radius);
        Vector3 boxCenter = (futurePos - position).normalized * (boxSize.z / 2f);
        Gizmos.DrawWireMesh(pathMesh, position + boxCenter, Quaternion.LookRotation(velocity, Vector3.up), boxSize);

        // Draw lines to obstacles to avoid
        Gizmos.color = Color.white;
        foreach (GameObject obstacle in avoidList)
        {
            Gizmos.DrawLine(position, obstacle.transform.position);
        }
    }
}