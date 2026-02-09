using System.Collections.Generic;
using UnityEngine;
using Shift25.Gameplay;

namespace Shift25.Managers
{
    public class QueueManager : MonoBehaviour
    {
        public static QueueManager Instance { get; private set; }
        [SerializeField] private List<Transform> queuePoints; 
        private List<NPCController> _npcInQueue = new List<NPCController>();

        private void Awake() => Instance = this;

        public void JoinQueue(NPCController npc)
        {
            if (!_npcInQueue.Contains(npc))
            {
                _npcInQueue.Add(npc);
                npc.RefreshQueuePosition();
            }
        }

        public Transform GetTargetPoint(NPCController npc, out int index)
        {
            index = _npcInQueue.IndexOf(npc);
            if (index == -1) return null;
            int pointIndex = Mathf.Clamp(index, 0, queuePoints.Count - 1);
            return queuePoints[pointIndex];
        }

        // [Logic] ฟังก์ชันตรวจสอบว่า NPC ตัวนี้คือคิวแรกที่ถึงจุดหมายหรือยัง
        public bool IsFirstInLineAndReady(NPCController npc)
        {
            if (_npcInQueue.Count == 0) return false;
            return _npcInQueue[0] == npc;
        }

        public void ShiftQueue()
        {
            if (_npcInQueue.Count > 0)
            {
                _npcInQueue.RemoveAt(0);
                // [Observer Pattern Concept] แจ้งทุกคนในแถวให้ขยับ
                foreach (var npc in _npcInQueue) npc.RefreshQueuePosition();
            }
        }
    }
}