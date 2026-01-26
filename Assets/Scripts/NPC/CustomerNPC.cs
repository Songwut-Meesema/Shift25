using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shift25.Managers; 

namespace Shift25.NPC
{
    public class CustomerNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private List<ScanItemData> shoppingCart;
        [SerializeField] private string prompt = "Press E to Scan Items";

        public string InteractionPrompt => prompt;

        public void Interact()
        {
            //Unitask for calling ScanManager to start scanning session
            ScanManager.Instance.StartScanSession(shoppingCart).Forget();
        }
    }
}