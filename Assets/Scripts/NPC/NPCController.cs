using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // [NavMesh] ใช้จัดการการเดิน
using Shift25.Managers;

namespace Shift25.Gameplay
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCController : MonoBehaviour
    {
        // [State Pattern] กำหนดสถานะของลูกค้า
        public enum NPCState { Spawning, Browsing, MovingToQueue, WaitingInQueue, Leaving }
        private NPCState _currentState;

        private NavMeshAgent _agent;
        private Transform _exitPoint;
        private Vector3 _queuePosition;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Initialize(Transform exit, NPCState startState = NPCState.Browsing)
        {
            _exitPoint = exit;
            SwitchState(startState);
        }

        public void SwitchState(NPCState newState)
        {
            _currentState = newState;
            
            // [Logic] ปฏิบัติตามสถานะใหม่
            switch (_currentState)
            {
                case NPCState.Browsing:
                    HandleBrowsing();
                    break;
                case NPCState.MovingToQueue:
                    // จะถูกเรียกโดย QueueManager ในอนาคต
                    break;
                case NPCState.Leaving:
                    _agent.SetDestination(_exitPoint.position);
                    break;
            }
        }

        private async void HandleBrowsing()
        {
            // [Logic] สุ่มเดินไปที่ชั้นวางสินค้า
            // สุ่มจุดบน NavMesh และรอสักพัก (Surreal Delay)
            // เมื่อพอใจแล้วจะเปลี่ยนสถานะไปต่อคิว
            
            // [UniTask] สามารถใช้รอเวลาได้โดยไม่กินเฟรม
            // await UniTask.Delay(Random.Range(2000, 5000));
            
            SwitchState(NPCState.MovingToQueue);
        }

        private void Update()
        {
            if (_currentState == NPCState.Leaving && _agent.remainingDistance < 0.5f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}