using UnityEngine;
using Shift25.Managers;
using Cysharp.Threading.Tasks;
using static Shift25.Managers.MicrowaveManager;

namespace Shift25.Gameplay
{
    public class MicrowaveStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private Animator doorAnimator;

        public string InteractionPrompt
        {
            get 
            {
                bool hasWork = MicrowaveManager.Instance.HasActiveRequest;
                var mState = MicrowaveManager.Instance.CurrentState;

                if (!hasWork) return ""; 

                if (mState == MicrowaveState.Idle) return "Press E to Start Cooking";
                if (mState == MicrowaveState.Done) return "Press E to Pick Up Food";
                
                return "";
            }
        }

        public void Interact()
        {
            if (!MicrowaveManager.Instance.HasActiveRequest)
            {
                Debug.Log("[GameFeel] Machine is locked. No active orders.");
                return;
            }

            var state = MicrowaveManager.Instance.CurrentState;
            if (state == MicrowaveState.Idle && PlayerStateManager.Instance.CurrentState == PlayerStateManager.PlayerState.Roaming)
            {
                StartSequence().Forget();
            }
            else if (state == MicrowaveState.Done)
            {
                PickUpSequence().Forget();
            }
        }

        private async UniTaskVoid StartSequence()
        {
            PlayerStateManager.Instance.SwitchState(PlayerStateManager.PlayerState.Interacting);
            MicrowaveManager.Instance.ActivateCamera();
            await UniTask.Delay(600);

            doorAnimator.SetBool("IsOpen", true); 
            await MicrowaveManager.Instance.StartSettingTime();
            
            await MicrowaveManager.Instance.RunCookingTimer(doorAnimator);
            
            PlayerStateManager.Instance.SwitchState(PlayerStateManager.PlayerState.Roaming);
        }

        private async UniTaskVoid PickUpSequence()
        {
            doorAnimator.SetBool("IsOpen", true);
            await UniTask.Delay(500);
            Debug.Log("[Microwave] You picked up the food.");
            
        }
    }
}