using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewScanItem", menuName = "Shift25/Scan Item Data")]
public class ScanItemData : ScriptableObject
{
    public string itemName;
    public GameObject itemPrefab;
    public float baseScanTime = 0.8f; //time to hover berfore click
}
