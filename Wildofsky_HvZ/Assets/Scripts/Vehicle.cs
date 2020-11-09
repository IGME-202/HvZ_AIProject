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

    [Min(0.00001f)]
    public float mass = 1f;
    public float radius = 1f;
    public float maxSpeed = 1f;
    public float maxForce = 1f;

    protected MeshRenderer floor;

    protected bool debugEnabled;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        position = transform.position;
        direction = Vector3.right;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        // Initialize a reference to the floor's mesh renderer in start so Unity does not have to find the floor every frame in Bounce()
        floor = GameObject.FindWithTag("Floor").GetComponent<MeshRenderer>();
        
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

        // Let the child classes determine what this does
        CalcSteeringForces();
        UpdatePosition();
        Bounce();
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
            float arriveSpeed = Remap(distance, stop, slowDown, 0, maxSpeed / 2);
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
}
