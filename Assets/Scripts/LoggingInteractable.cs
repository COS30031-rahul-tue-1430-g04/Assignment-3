using UnityEngine;

public class LoggingInteractable : Interactable
{
	public override void Interact(GameObject interactor)
	{
		Debug.Log($"{interactor.name} interacted with {gameObject.name}");
	}
}
