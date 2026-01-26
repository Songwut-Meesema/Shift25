using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    string InteractionPrompt {get;} // Prompt to show when player can interact
    void Interact(); // Method to define interaction behavior
}
