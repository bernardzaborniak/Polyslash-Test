using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator animator;

    public void Open()
    {
        animator.SetTrigger("open");
    }

    public void Close()
    {
        animator.SetTrigger("close");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Open();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Close();
        }
    }
}
