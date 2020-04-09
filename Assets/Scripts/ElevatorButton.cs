using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ElevatorButton : MonoBehaviour, IInteractable
{
    public UnityEvent onPress;

    public int targetFloorID;
    public MeshRenderer meshRenderer;
    public Material normalMaterial;
    public Material pressedMaterial;

    public void Interact()
    {
        onPress.Invoke();
    }

    //the button will change color to highligth that it has been pressed
    public void SetPressed()
    {
        meshRenderer.material = pressedMaterial;
    }

    //the button will change color to highligth that it waits to be pressed
    public void SetReadyToBePressed()
    {
        meshRenderer.material = normalMaterial;
    }
}
