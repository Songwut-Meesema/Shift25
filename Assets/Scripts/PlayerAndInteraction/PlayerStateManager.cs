using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    public enum PlayerState { Roaming, Interacting, Locked }
    public PlayerState CurrentState { get; private set; }

    private void Awake() => Instance = this; 

    public void SwitchState(PlayerState newState) 
    {
        CurrentState = newState;
        Debug.Log($"Player State Switched to: {newState}");
        
    }
}