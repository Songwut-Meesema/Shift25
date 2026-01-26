using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PressureManager : MonoBehaviour
{
    //Singleton instance
    public static PressureManager Instance { get; private set; }

    [Header("Pressure Stats")]
    [SerializeField] private float currentPressure = 0f;
    [SerializeField] private float maxPressure = 100f;

    private bool isGameRunning = true;

    private void Awake()
    {
        // Implementing Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //Unitask to handle pressure increase over time
        StartPressureTick().Forget();
    }

    private async UniTaskVoid StartPressureTick()
    {
        while (isGameRunning)
        {
            AddPressure(0.1f); // Initial pressure increment
            await UniTask.Delay(1000); // Wait for 1 second
        }
    }

    public void AddPressure(float amount)
    {
        currentPressure = Mathf.Clamp(currentPressure + amount, 0, maxPressure);

        //for observer to UI or any visual
        GameEvents.RaisePressureChanged(currentPressure, maxPressure);

        if (currentPressure >= maxPressure)
        {
           TriggerMentalCollapse();
        }
    }

    private void TriggerMentalCollapse()
    {
        isGameRunning = false;
        Debug.Log("Mental Collapse Triggered! : Game Ended");
    }
}
