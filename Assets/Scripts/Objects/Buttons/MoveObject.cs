    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] private Animator objectToMove;
    private bool animationDone = false;
    [SerializeField] private string animationName = "move";

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !animationDone)
        {
            objectToMove.SetBool(animationName, true);
            animationDone = true;
        }
    }
}
