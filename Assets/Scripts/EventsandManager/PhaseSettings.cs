using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhaseSettings", menuName = "Shift25/Phase Settings")]
public class PhaseSettings : ScriptableObject
{
    public int phaseNumber;
    public float durationInSeconds; // ระยะเวลาของ Phase นี้

    [Header("Scanning Logic")]
    public int minItemsPerCustomer; // สุ่มจำนวนชิ้นขั้นต่ำ
    public int maxItemsPerCustomer; // สุ่มจำนวนชิ้นสูงสุด
    public List<ScanItemData> availableItems; // รายการสินค้าที่อาจโผล่มาใน Phase นี้

    [Header("Customer Flow")]
    public float minSpawnInterval; // ลูกค้าจะเข้าเร็วสุดกี่วินาที
    public float maxSpawnInterval; // ลูกค้าจะเข้าช้าสุดกี่วินาที
}