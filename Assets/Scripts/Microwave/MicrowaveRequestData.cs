using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift25.Gameplay
{
    [CreateAssetMenu(fileName = "MicrowaveRequest", menuName = "Shift25/Microwave Request")]
    public class MicrowaveRequestData : ScriptableObject
    {
        public string instructionPhrase;  
        public float minAcceptableTime;  
        public float maxAcceptableTime;  
        public string failTooColdText;   
        public string failTooHotText;    
    }
}
