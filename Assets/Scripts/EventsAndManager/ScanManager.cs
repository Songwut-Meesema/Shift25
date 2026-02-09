using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;

namespace Shift25.Managers
{
    public class ScanManager : MonoBehaviour
    {
        public static ScanManager Instance { get; private set; }
        [SerializeField] private CinemachineVirtualCamera scanCamera;
        [SerializeField] private Transform[] spawnPoints; 

        private List<GameObject> _activeItems = new List<GameObject>();
        private int _itemsScannedInSession = 0;
        private int _totalItemsInSession = 0;
        private bool _isSessionActive = false; // [State] เช็คว่ากำลังสแกนอยู่ไหม

        private void Awake() => Instance = this;

        public async UniTask<bool> StartScanSession(List<ScanItemData> itemsToScan)
        {
            if (_isSessionActive) return false; // ป้องกันการกดซ้อน
            _isSessionActive = true;

            _itemsScannedInSession = 0;
            _totalItemsInSession = itemsToScan.Count;

            // [State Pattern] ล็อกตัวผู้เล่น
            PlayerStateManager.Instance.SwitchState(PlayerStateManager.PlayerState.Interacting); 
            scanCamera.Priority = 20;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // [Factory] สร้างของ
            for (int i = 0; i < itemsToScan.Count; i++)
            {
                if (i >= spawnPoints.Length) break;
                var item = Instantiate(itemsToScan[i].itemPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
                if (item.TryGetComponent<Shift25.Gameplay.ScannableItem>(out var scannable))
                    scannable.Initialize(itemsToScan[i]);
                _activeItems.Add(item);
            }

            // [UniTask] รอจนกว่าสแกนครบ
            await UniTask.WaitUntil(() => _itemsScannedInSession >= _totalItemsInSession);

            // [Logic] สแกนเสร็จแล้ว ต้องทำความสะอาดก่อนคืนค่า
            await UniTask.Delay(300); // รอจังหวะนิดนึงให้ Game Feel ดี
            EndScanSession();
            
            _isSessionActive = false;
            return true; 
        }

        public void ReportItemScanned() => _itemsScannedInSession++;

        private void EndScanSession()
        {
            foreach (var item in _activeItems) if(item != null) Destroy(item);
            _activeItems.Clear();

            scanCamera.Priority = 0;
            PlayerStateManager.Instance.SwitchState(PlayerStateManager.PlayerState.Roaming); 

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}