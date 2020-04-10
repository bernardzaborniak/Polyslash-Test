using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * represents elevator stops or the floors an elevator can stop on
 */
public class ElevatorStop : MonoBehaviour
{
    public Door door;
    [SerializeField]
    Mesh gizmoMeshRepresentation;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 0.7f, 0.5f);

        Gizmos.DrawMesh(gizmoMeshRepresentation, transform.position, transform.rotation, Vector3.one);
    }
}
