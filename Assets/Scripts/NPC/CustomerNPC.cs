using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shift25.Managers; 

namespace Shift25.NPC
{
    public class CustomerNPC : MonoBehaviour, IInteractable
    {
        // [Data-Driven] รายการสินค้าที่ลูกค้าคนนี้ถืออยู่
        private List<ScanItemData> _randomizedCart = new List<ScanItemData>();
        
        [SerializeField] private string prompt = "Press E to Scan Items";

        // [Lambda] ส่งค่า Prompt ไปแสดงที่หน้าจอ
        public string InteractionPrompt => prompt;

        // เมื่อ NPC ถูกเปิดใช้งาน (ใช้ร่วมกับ Object Pooling)
        private void OnEnable()
        {
            // ล้างข้อมูลเก่าทิ้งทุกครั้งที่ลูกค้าคนใหม่ (หรือตัวเดิมที่วนกลับมา) ปรากฏตัว
            _randomizedCart.Clear();
        }

        // [Logic] ฟังก์ชันสุ่มสินค้าใหม่โดยอิงตาม Phase ปัจจุบัน
        private void GenerateRandomCartForCurrentPhase()
        {
            // [Singleton] ดึงข้อมูลจาก Phase Manager
            var currentPhase = GamePhaseManager.Instance?.CurrentPhase;
            
            if (currentPhase == null)
            {
                Debug.LogError("[CustomerNPC] GamePhaseManager or CurrentPhase is null!");
                return;
            }

            // ล้างตะกร้าเก่าก่อนสุ่มใหม่
            _randomizedCart.Clear();

            // [Logic] สุ่มจำนวนสินค้าตามค่า Min/Max ของ Phase ปัจจุบัน
            int itemCount = Random.Range(currentPhase.minItemsPerCustomer, currentPhase.maxItemsPerCustomer + 1);

            for (int i = 0; i < itemCount; i++)
            {
                // [Logic] สุ่มเลือกไอเทมจากรายการที่มีให้ใน Phase นี้
                if (currentPhase.availableItems.Count > 0)
                {
                    int randomIndex = Random.Range(0, currentPhase.availableItems.Count);
                    _randomizedCart.Add(currentPhase.availableItems[randomIndex]);
                }
            }

            Debug.Log($"[CustomerNPC] Phase {currentPhase.phaseNumber}: Randomized {itemCount} items.");
        }

        public void Interact()
        {
            // [Critical Logic] สุ่มของใหม่ "ทันที" ที่มีการ Interact 
            // เพื่อให้แน่ใจว่าจำนวนสินค้าอัปเดตตาม Phase ล่าสุดจริงๆ
            GenerateRandomCartForCurrentPhase();

            if (_randomizedCart.Count == 0)
            {
                Debug.LogWarning("[CustomerNPC] Cart is empty, trying to re-roll...");
                return;
            }

            // [UniTask] ส่งสินค้าเข้าสู่ระบบ ScanManager
            ScanManager.Instance.StartScanSession(_randomizedCart).Forget();
        }
    }
}