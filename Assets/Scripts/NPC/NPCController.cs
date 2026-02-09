using UnityEngine;
using UnityEngine.AI;
using Shift25.Managers; // เรียกใช้ Manager ทั้งหมด
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Pool;
using System.Threading;

namespace Shift25.Gameplay
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCController : MonoBehaviour, IInteractable
    {
        public enum NPCState { Browsing, MovingToQueue, WaitingInQueue, WaitingForFood, Scanning, Leaving }
        private NPCState _currentState;

        private NavMeshAgent _agent;
        private Transform _exitPoint;
        private List<ScanItemData> _shoppingCart = new List<ScanItemData>();
        private IObjectPool<NPCController> _pool;
        private List<Transform> _browsingPoints;
        private CancellationTokenSource _cts;

        private MicrowaveRequestData _mySelectedRequest; 
        private bool _needsMicrowave = false;

        // [Logic] แสดงปุ่ม Interact เฉพาะคิวแรกที่เข้าตำแหน่ง
        public string InteractionPrompt 
        {
            get {
                if (_currentState == NPCState.Scanning || _currentState == NPCState.Leaving) return "";
                
                QueueManager.Instance.GetTargetPoint(this, out int index);
                if (index != 0) return ""; 

                float dist = Vector3.Distance(transform.position, QueueManager.Instance.GetTargetPoint(this, out _).position);
                if (dist > 1.0f) return "";

                if (_currentState == NPCState.WaitingInQueue || _currentState == NPCState.MovingToQueue) return "Press E to Scan Items";
                if (_currentState == NPCState.WaitingForFood && MicrowaveManager.Instance.CurrentState == MicrowaveManager.MicrowaveState.Done) return "Press E to Give Food";
                
                return "";
            }
        }

        private void Awake() => _agent = GetComponent<NavMeshAgent>();

        // [Initialize Pattern] เรียกใช้เมื่อลูกค้าเกิดจาก Pool
        public void Initialize(Transform exit, IObjectPool<NPCController> pool, List<Transform> browsePts)
        {
            _exitPoint = exit;
            _pool = pool;
            _browsingPoints = browsePts;
            _shoppingCart.Clear();
            _mySelectedRequest = null;
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            // [Data-Driven] ดึงข้อมูลจากกะปัจจุบัน
            var currentPhase = GamePhaseManager.Instance.CurrentPhase;
            if (currentPhase == null) return;

            // 1. สุ่มสินค้าใส่รถเข็น
            int count = Random.Range(currentPhase.minItemsPerCustomer, currentPhase.maxItemsPerCustomer + 1);
            for (int i = 0; i < count; i++)
            {
                if (currentPhase.availableItems.Count > 0)
                    _shoppingCart.Add(currentPhase.availableItems[Random.Range(0, currentPhase.availableItems.Count)]);
            }

            // 2. สุ่มความต้องการไมโครเวฟ
            _needsMicrowave = (Random.value < 0.3f);
            if (_needsMicrowave && currentPhase.availableMicrowaveRequests.Count > 0)
            {
                _mySelectedRequest = currentPhase.availableMicrowaveRequests[Random.Range(0, currentPhase.availableMicrowaveRequests.Count)];
            }

            SwitchState(NPCState.Browsing);
        }

        private void OnDisable() => _cts?.Cancel();

        public void SwitchState(NPCState newState)
        {
            _currentState = newState;
            if (_agent == null || !gameObject.activeInHierarchy) return;

            switch (_currentState)
            {
                case NPCState.Browsing: HandleBrowsing(_cts.Token).Forget(); break;
                case NPCState.MovingToQueue: QueueManager.Instance.JoinQueue(this); break;
                case NPCState.Leaving: 
                    _agent.isStopped = false;
                    _agent.SetDestination(_exitPoint.position);
                    CheckExitDistance(_cts.Token).Forget(); 
                    break;
            }
        }

        private async UniTaskVoid HandleBrowsing(CancellationToken token)
        {
            try {
                int visits = Random.Range(1, 4);
                for (int i = 0; i < visits; i++) {
                    if (token.IsCancellationRequested) return;
                    if (_browsingPoints.Count > 0) {
                        _agent.SetDestination(_browsingPoints[Random.Range(0, _browsingPoints.Count)].position);
                        await UniTask.WaitUntil(() => !_agent.pathPending && _agent.remainingDistance < 0.6f, cancellationToken: token);
                        await UniTask.Delay(Random.Range(2000, 5000), cancellationToken: token);
                    }
                }
                SwitchState(NPCState.MovingToQueue);
            } catch { }
        }

        public void RefreshQueuePosition()
        {
            if (_agent == null || !_agent.isOnNavMesh || _currentState == NPCState.Scanning) return;
            Transform target = QueueManager.Instance.GetTargetPoint(this, out int index);
            if (target != null) _agent.SetDestination(target.position);
        }

        public void Interact()
        {
            QueueManager.Instance.GetTargetPoint(this, out int index);
            if (index != 0) return;

            if (_currentState == NPCState.WaitingInQueue || _currentState == NPCState.MovingToQueue)
            {
                StartScanning().Forget();
            }
            else if (_currentState == NPCState.WaitingForFood && MicrowaveManager.Instance.CurrentState == MicrowaveManager.MicrowaveState.Done)
            {
                CompleteHandover();
            }
        }

        private async UniTaskVoid StartScanning()
        {
            _currentState = NPCState.Scanning;
            _agent.isStopped = true;

            // [UniTask] ส่งสินค้าของคุณเข้าเครื่องสแกน
            bool scanFinished = await ScanManager.Instance.StartScanSession(_shoppingCart);
            
            if (scanFinished) {
                if (_needsMicrowave && _mySelectedRequest != null) {
                    MicrowaveManager.Instance.AssignPendingRequest(_mySelectedRequest);
                    _currentState = NPCState.WaitingForFood;
                    _agent.isStopped = false;
                } else {
                    QueueManager.Instance.ShiftQueue();
                    SwitchState(NPCState.Leaving);
                }
            }
        }

        private void CompleteHandover()
        {
            float pressure = MicrowaveManager.Instance.GetResultAndReset();
            PressureManager.Instance.AddPressure(pressure);
            QueueManager.Instance.ShiftQueue();
            SwitchState(NPCState.Leaving);
        }

        private async UniTaskVoid CheckExitDistance(CancellationToken token)
        {
            try {
                while (_currentState == NPCState.Leaving) {
                    if (token.IsCancellationRequested) return;
                    if (!_agent.pathPending && _agent.remainingDistance < 0.8f) {
                        _pool.Release(this);
                        break;
                    }
                    await UniTask.Delay(500, cancellationToken: token);
                }
            } catch { }
        }
    }
}