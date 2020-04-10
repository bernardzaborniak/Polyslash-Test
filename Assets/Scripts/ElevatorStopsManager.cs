using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * This class responsible for keeeping track of the elevator stops
 * and draws the in the scene view for simpler level design.
 * It can be seen as a helper class for the Elevator Controller
 */
public class ElevatorStopsManager : MonoBehaviour
{
    [Tooltip("the order of the stops is important")]
    public ElevatorStop[] elevatorStops;

    public Mesh gizmoMesh;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public Vector3 GetStopPostion(int floorID)
    {
        return elevatorStops[floorID].transform.position;
    }

    public Door GetOuterElevatorDoor(int floorID)
    {
        return elevatorStops[floorID].door;
    }


    //draws the Elevator stops
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0,0,0.7f,0.5f);
        for (int i = 0; i < elevatorStops.Length; i++)
        {
            // Gizmos.DrawCube(elevatorStops[i].position, Vector3.one);
            Gizmos.DrawMesh(gizmoMesh, elevatorStops[i].transform.position, elevatorStops[i].transform.rotation, Vector3.one);
        }
    }
}
