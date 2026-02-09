using System.Collections.Generic;
using UnityEngine;
using Shift25.Managers;
using Shift25.Gameplay;
using Cysharp.Threading.Tasks;
using TMPro;
using Cinemachine;

namespace Shift25.Managers
{
    public class MicrowaveManager : MonoBehaviour
    {
        public static MicrowaveManager Instance { get; private set; }

        public enum MicrowaveState { Idle, SettingTime, Cooking, Done }
        public MicrowaveState CurrentState { get; private set; } = MicrowaveState.Idle;

        [Header("Camera & Visuals")]
        [SerializeField] private CinemachineVirtualCamera microwaveCam;

        [Header("UI References")]
        [SerializeField] private GameObject microwaveUIPanel;
        [SerializeField] private TextMeshProUGUI instructionText;

        private MicrowaveRequestData _activeRequest; 
        private float _userSelectedTime;

        public bool HasActiveRequest => _activeRequest != null;

        private void Awake() => Instance = this;

        public void AssignPendingRequest(MicrowaveRequestData request)
        {
            _activeRequest = request;
            if (instructionText != null)
                instructionText.text = $"Order: \"{_activeRequest.instructionPhrase}\"";
        }

        public void ActivateCamera() => microwaveCam.Priority = 20;
        public void DeactivateCamera() => microwaveCam.Priority = 0;

        public async UniTask StartSettingTime()
        {
            if (!HasActiveRequest) return;

            CurrentState = MicrowaveState.SettingTime;
            microwaveUIPanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            await UniTask.WaitUntil(() => CurrentState == MicrowaveState.Cooking);
            
            microwaveUIPanel.SetActive(false);
            DeactivateCamera(); 
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SubmitCookingTime(float sliderValue)
        {
            _userSelectedTime = sliderValue * 5.0f; 
            CurrentState = MicrowaveState.Cooking; 
        }

        public async UniTask RunCookingTimer(Animator doorAnimator)
        {
            if (doorAnimator != null) doorAnimator.SetBool("IsOpen", false);
            
            Debug.Log($"[Microwave] Cooking for {_userSelectedTime} seconds...");
            
            await UniTask.Delay((int)(_userSelectedTime * 1000));

            CurrentState = MicrowaveState.Done;
            Debug.Log("[Microwave] BEEP BEEP! Finished.");
        }
        //public API 
        public float GetResultAndReset()
        {
            float pGain = 1f; 
            
            if (_userSelectedTime < _activeRequest.minAcceptableTime) pGain = 3f; 
            else if (_userSelectedTime > _activeRequest.maxAcceptableTime) pGain = 2f; 
            _activeRequest = null;
            CurrentState = MicrowaveState.Idle;
            
            return pGain;
        }
    }
}