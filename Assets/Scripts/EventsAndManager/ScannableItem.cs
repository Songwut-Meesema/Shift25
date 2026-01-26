using UnityEngine;
using Shift25.Managers;

namespace Shift25.Gameplay
{
    public class ScannableItem : MonoBehaviour
    {
        private ScanItemData _data;
        private float _targetScanTime; 
        private float _hoverTimer = 0f;
        private bool _isScanned = false;
        private bool _isReadyToClick = false; // [State] พร้อมให้กดหรือยัง
        private Outline _outline;

        [Header("Feel Settings")]
        [SerializeField] private float focusThreshold = 100f;
        [SerializeField] private Color readyColor = Color.white; // สีเมื่อ Beep!

        // [Initialize Pattern] แก้ไขให้รับ ScanItemData เพื่อดึงข้อมูลพื้นฐาน
        public void Initialize(ScanItemData data)
        {
            _data = data;
            // [Logic] สุ่มเวลาสแกนเล็กน้อยเพื่อให้แต่ละชิ้นไม่เท่ากัน (Procedural Feel)
            _targetScanTime = _data.baseScanTime * Random.Range(0.8f, 1.2f);

            if (TryGetComponent<Outline>(out var outline))
            {
                _outline = outline;
                _outline.enabled = false;
                _outline.OutlineColor = Color.red; // สีตอนเริ่ม
            }
        }

        private void Update()
        {
            if (_isScanned) return;
            CheckFocusAndProcess();
        }

        private void CheckFocusAndProcess()
        {
            if (Camera.main == null || _outline == null) return;

            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            // [Syntax Decoding] เลือกจุดอ้างอิงตาม Cursor Mode (Ternary Operator)
            Vector2 refPoint = (Cursor.lockState == CursorLockMode.Locked) ? 
                                new Vector2(Screen.width / 2f, Screen.height / 2f) : (Vector2)Input.mousePosition;

            float distance = Vector2.Distance(screenPos, refPoint);

            if (distance < focusThreshold)
            {
                _outline.enabled = true;
                if (!_isReadyToClick)
                {
                    _hoverTimer += Time.deltaTime;
                    // [Logic] ถ้า Hover นานพอจนเครื่อง Beep
                    if (_hoverTimer >= _targetScanTime)
                    {
                        _isReadyToClick = true;
                        _outline.OutlineColor = readyColor; // เปลี่ยนสีบอกผู้เล่น
                    }
                }
            }
            else
            {
                _outline.enabled = false;
                _hoverTimer = 0f;
                _isReadyToClick = false;
                _outline.OutlineColor = Color.red;
            }
        }

        // [Public API] ฟังก์ชันนี้จะถูกเรียกจาก Player เมื่อกดคลิกซ้าย
        public void OnClickAction()
        {
            if (_isScanned) return;

            if (_isReadyToClick)
            {
                // [Social Oppression] คำนวณว่ากดช้าไปไหม (Human Error)
                float delay = _hoverTimer - _targetScanTime;
                CompleteScan(delay > 1.0f ? 2f : 1f); // ช้าเกินไปโดน Pressure 2
            }
            else
            {
                // [Penalty] กดก่อนเครื่องจะ Beep
                PressureManager.Instance.AddPressure(3f);
                Debug.Log("TOO FAST! System Error.");
                // ตรงนี้เพิ่ม Screen Shake หรือเสียงด่าได้
            }
        }

        private void CompleteScan(float pressureToAdd)
        {
            _isScanned = true;
            _outline.enabled = false;
            PressureManager.Instance.AddPressure(pressureToAdd);
            GameEvents.RaiseActionPerformed(1);
            ScanManager.Instance.ReportItemScanned();
            Destroy(gameObject); // สแกนเสร็จของหายไป
        }
    }
}