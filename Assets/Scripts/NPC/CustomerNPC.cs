using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shift25.Managers;

namespace Shift25.NPC
{
    public class CustomerNPC : MonoBehaviour, IInteractable
    {
        private List<ScanItemData> _randomizedCart = new List<ScanItemData>();
        [SerializeField] private string prompt = "Press E to Scan Items";

        public string InteractionPrompt => prompt;

        private void Start()
        {
            // [Logic] เมื่อลูกค้าถูกสร้างขึ้น ให้ทำการสุ่มของในรถเข็นทันที
            GenerateRandomCart();
        }

        private void GenerateRandomCart()
        {
            // [Null Safety] เช็คก่อนว่า Phase Manager พร้อมทำงานไหม
            var settings = GamePhaseManager.Instance?.CurrentPhase;
            if (settings == null || settings.availableItems.Count == 0) return;

            // [Logic] สุ่มจำนวนชิ้นตามช่วงที่ Phase กำหนด
            int itemCount = Random.Range(settings.minItemsPerCustomer, settings.maxItemsPerCustomer + 1);

            for (int i = 0; i < itemCount; i++)
            {
                // [Logic] สุ่มเลือกสินค้าจากรายการที่อนุญาตใน Phase นี้
                int randomIndex = Random.Range(0, settings.availableItems.Count);
                _randomizedCart.Add(settings.availableItems[randomIndex]);
            }
        }

        public void Interact()
        {
            if (_randomizedCart.Count == 0) return;

            // [Singleton Pattern] ส่งสินค้าที่สุ่มได้ไปให้ ScanManager
            ScanManager.Instance.StartScanSession(_randomizedCart).Forget();
            
            // เมื่อสแกนเสร็จ (ในอนาคต) ลูกค้าจะเดินออกจากร้านและสุ่มของใหม่ถ้าใช้ Object Pooling
        }
    }
}