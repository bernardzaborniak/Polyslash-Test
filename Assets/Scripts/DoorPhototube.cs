using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * informs the elevator controller, if the player has entered the dangerous door area
 */
 [RequireComponent(typeof(Collider))]
public class DoorPhototube : MonoBehaviour
{
    public ElevatorController elevatorController;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            elevatorController.OnPlayerEntersDangerousArea();
        }
    }
}
