using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift25.Managers // [Namespace Fix] ให้อยู่บ้านเดียวกับ GamePhaseManager
{
    [CreateAssetMenu(fileName = "PhaseSettings", menuName = "Shift25/Phase Settings")]
    public class PhaseSettings : ScriptableObject
    {
        public int phaseNumber;
        public float durationInSeconds;

        [Header("NPC & Queue Capacity")]
        public int maxNPCInStore; 
        public float minSpawnInterval;
        public float maxSpawnInterval;

        [Header("Item Scanning Data")]
        public int minItemsPerCustomer;
        public int maxItemsPerCustomer;
        public List<ScanItemData> availableItems; // ลาก ScanItemData อะไรใส่ก็ได้

        [Header("Microwave Data")]
        public List<Shift25.Gameplay.MicrowaveRequestData> availableMicrowaveRequests;
    }
}