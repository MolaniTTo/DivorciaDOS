using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalPoint : MonoBehaviour
{
    public bool playerIsOnPoint = false;
    public FinalPoint otherFinalPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerIsOnPoint = true;
            Debug.Log("Un player ha llegado al punto.");

            if (FinalPointController.instance != null)
            {
                Debug.Log("Llamando a CheckIfBothPlayersReachedCheckpoint desde FinalPoint.");
                FinalPointController.instance.CheckIfBothPlayersReachedCheckpoint();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            playerIsOnPoint = false;
            Debug.Log("Player 1 ha salido del punto.");
        }
    }
}
