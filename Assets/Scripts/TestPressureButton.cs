using UnityEngine;

// [Interface Integration] นำ IInteractable มาใช้เพื่อให้ PlayerInteractionController มองเห็น
public class TestPressureButton : MonoBehaviour, IInteractable
{
    // [Attributes] แสดงข้อความใน Inspector และเป็นส่วนหนึ่งของสัญญา IInteractable
    [SerializeField] private string prompt = "Press E to Stress yourself";
    [SerializeField] private float pressureToIncrease = 10f;
    [SerializeField] private Color activeColor = Color.red;

    private MeshRenderer _renderer;
    private Color _originalColor;

    // [Syntax Decoding] '=>' คือ Lambda ใช้เป็นทางลัดสำหรับ return ค่า prompt
    public string InteractionPrompt => prompt;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _originalColor = _renderer.material.color;
    }

    public void Interact()
    {
        // [Singleton Pattern] เรียกใช้งาน PressureManager โดยไม่ต้องหา Object ใน Scene
        PressureManager.Instance.AddPressure(pressureToIncrease);

        // [Observer Pattern] ส่งสัญญาณบอกโลกว่า "มีการทำ Action เกิดขึ้นนะ" 
        // โดยส่งค่า 1 (ปกติ) ไปให้ระบบอื่นๆ รับรู้
        GameEvents.RaiseActionPerformed(1);

        // [Game Feel] เปลี่ยนสีชั่วคราวเพื่อให้ผู้เล่นรู้ว่า "กดติดแล้วนะ"
        TriggerVisualFeedback();

        // [Interpolation] ใช้ '$' เพื่อแสดงข้อความพร้อมค่าตัวแปรใน Console
        Debug.Log($"[Test] Interacted! Pressure increased by {pressureToIncrease}");
    }

    private void TriggerVisualFeedback()
    {
        _renderer.material.color = activeColor;
        // [Professional Tip] ในงานจริงเราจะใช้ UniTask เพื่อรอเวลาแล้วเปลี่ยนสีกลับ 
        // แต่ตอนนี้ใช้ Invoke ง่ายๆ ไปก่อนเพื่อทดสอบ Logic หลัก
        Invoke(nameof(ResetColor), 0.2f);
    }

    private void ResetColor() => _renderer.material.color = _originalColor;
}