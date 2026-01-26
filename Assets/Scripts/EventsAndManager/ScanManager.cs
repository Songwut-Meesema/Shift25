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

        private void Awake() => Instance = this;

        public async UniTask StartScanSession(List<ScanItemData> itemsToScan)
        {
            _itemsScannedInSession = 0;
            _totalItemsInSession = itemsToScan.Count;

            PlayerStateManager.Instance.SwitchState(PlayerStateManager.PlayerState.Interacting); 
            scanCamera.Priority = 20;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            for (int i = 0; i < itemsToScan.Count; i++)
            {
                if (i >= spawnPoints.Length) break;
                
                var item = Instantiate(itemsToScan[i].itemPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
                
                // [Generics] ดึงสคริปต์มาเพื่อเริ่มระบบ
                if (item.TryGetComponent<Shift25.Gameplay.ScannableItem>(out var scannable))
                {
                    // [Fix CS1503] ส่งตัวแปร ScanItemData (itemsToScan[i]) เข้าไปให้ถูก Type
                    scannable.Initialize(itemsToScan[i]);
                }
                
                _activeItems.Add(item);
            }

            await UniTask.WaitUntil(() => _itemsScannedInSession >= _totalItemsInSession);
            EndScanSession();
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