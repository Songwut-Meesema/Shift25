using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; // [UniTask]

namespace Shift25.Managers
{
    public class GamePhaseManager : MonoBehaviour
    {
        public static GamePhaseManager Instance { get; private set; }

        [SerializeField] private List<PhaseSettings> allPhases;
        private PhaseSettings _currentPhaseSettings;
        private int _currentPhaseIndex = 0;

        public PhaseSettings CurrentPhase => _currentPhaseSettings;

        private void Awake() => Instance = this;

        private void Start()
        {
            // [UniTask] เริ่มต้นกะการทำงาน (Phase 1)
            StartShift().Forget();
        }

        private async UniTaskVoid StartShift()
        {
            while (_currentPhaseIndex < allPhases.Count)
            {
                _currentPhaseSettings = allPhases[_currentPhaseIndex];
                GameEvents.RaisePhaseChanged(_currentPhaseSettings.phaseNumber);
                
                Debug.Log($"[Phase Manager] Switched to Phase {_currentPhaseSettings.phaseNumber}");

                // [UniTask] รอจนจบเวลาของ Phase ปัจจุบัน
                await UniTask.Delay((int)(_currentPhaseSettings.durationInSeconds * 1000));
                
                _currentPhaseIndex++;
            }
            
            Debug.Log("End of Shift. Final Phase triggered.");
        }
    }
}