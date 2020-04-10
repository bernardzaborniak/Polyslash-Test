using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class informs the ElevatorController whether the player has entered the dangerous door area.
[RequireComponent(typeof(Collider))]
public class DoorPhototube : MonoBehaviour
{
    [SerializeField]
    ElevatorController elevatorController;

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            elevatorController.OnPlayerEntersDangerousArea();
        }
    }
}
