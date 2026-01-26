using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhaseSettings", menuName = "Shift25/Phase Settings")]
public class PhaseSettings : ScriptableObject
{
    public int phaseNumber;
    [Header("Scanning Difficulty")]
    public float minScanTime = 0.5f; // minimum time required for a perfect scan
    public float maxScanTime = 1.5f; // maximum time required for a perfect scan
    
    public float GetRandomScanTime() => Random.Range(minScanTime, maxScanTime);
}
