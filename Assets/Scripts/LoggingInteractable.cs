using UnityEngine;

public class LoggingInteractable : Interactable
{
	protected override void Interact(GameObject interactor)
	{
		Debug.Log($"{interactor.name} interacted with {gameObject.name}");
	}
}
