using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalPointController : MonoBehaviour
{
    public static FinalPointController instance;

    public FinalPointP1 finalPointP1;
    public FinalPointP2 finalPointP2;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void CheckIfBothPlayersReachedCheckpoint()
    {
        if (finalPointP1 != null && finalPointP2 != null)
        {
            if (finalPointP1.player1IsOnPoint && finalPointP2.player2IsOnPoint)
            {
                Debug.Log("�Ambos jugadores han llegado al checkpoint!");
                // Aqu� puedes poner la l�gica que quieres ejecutar cuando ambos jugadores lleguen al checkpoint
            }
        }
    }
}
