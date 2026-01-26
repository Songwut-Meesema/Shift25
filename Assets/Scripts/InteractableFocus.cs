using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift25.Gameplay
{
    public class InteractableFocus : MonoBehaviour
    {
        private Outline _outline;
        [SerializeField] private float focusDistance = 150f;

        private void Awake()
        {
            if (TryGetComponent<Outline>(out var outline))
            {
                _outline = outline;
                _outline.enabled = false;
            }
        }

        private void Update()
        {
            if (_outline == null || Camera.main == null) return;

            //check distance from screen center
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            float distance = Vector2.Distance(screenPos, screenCenter);
            
            _outline.enabled = (distance < focusDistance); //enable outline if within focus distance
        }
    }
}