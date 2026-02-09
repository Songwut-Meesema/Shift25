using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using Shift25.Managers;

namespace Shift25.Gameplay
{
    public class MicrowaveUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Button startButton;

        private void Start()
        {
            startButton.onClick.AddListener(() => OnStartClicked());
        }

        private void OnStartClicked()
        {
            MicrowaveManager.Instance.SubmitCookingTime(timeSlider.value);
            
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleSliderShaking();
        }

        private void HandleSliderShaking()
        {
        }
    }
}
