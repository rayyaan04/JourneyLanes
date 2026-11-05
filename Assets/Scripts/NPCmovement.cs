using UnityEngine;

public class NPCmovement : MonoBehaviour
{
    public Transform[] Waypoints;
    public float NPCspeed = 10f;
    private int currentWaypointIndex = 0;  // Track current waypoint NPC is going to
    void Update()
    {
        if (currentWaypointIndex < Waypoints.Length)
        { // continually makes the MoveToNextWaypoint() function run until the object reaches the last indexed waypoint
            MoveToNextWaypoint();
        }
        else
        { // makes NPC dissappear when it reaches last waypoint in the array
            Destroy(gameObject);
        }
    }
    void MoveToNextWaypoint()
    {
        Transform targetWaypoint = Waypoints[currentWaypointIndex];
        // sets the next waypoint/target waypoint to the one thats loaded into the index.
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, NPCspeed * Time.deltaTime);
        // moves toward current position (transform.position) to the next waypoint (targetWaypoint.position), with a speed of NPCspeed

        Vector3 direction = Waypoints[currentWaypointIndex].position - transform.position; // tells the game object which way to face when in order to move to that way point
        Quaternion targetRotation = Quaternion.LookRotation(direction); // sets the target rotation of the car to the next waypoint to whatever the rotation is needed to reach direction
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 125 * Time.deltaTime);
        // rotates the car from its current rotation values (transform.position) to the target rotation values (targetRotation) multiplied by how fast it should do it (100)

        // checks if the distance between the car and waypoint is less than 0.1 units, and adds 1 to index if so
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f) 
        {
            currentWaypointIndex = currentWaypointIndex + 1;
        }
    }
}
