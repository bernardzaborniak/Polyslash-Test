using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ElevatorButton : MonoBehaviour, IInteractable
{

    public ElevatorStop targetStop;

    [SerializeField]
    UnityEvent onPress; 

    [Header("Audio-Visual Feedback")]
    [SerializeField]
    MeshRenderer meshRenderer;
    [SerializeField]
    Material normalMaterial;
    [SerializeField]
    Material pressedMaterial;
    [SerializeField]
    Animator buttonAnimator;
    [SerializeField]
    AudioSource buttonAudioSource;


    public void Interact()
    {
        if (buttonAnimator)
        {
            buttonAnimator.SetTrigger("press");
        }
        if (buttonAudioSource)
        {
            buttonAudioSource.Play();
        }
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
