using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
	protected abstract void Interact(GameObject interactor);
	protected virtual bool CanInteract(GameObject interactor) => true;
	public bool TryInteract(GameObject interactor)
	{
		if (CanInteract(interactor))
		{
			Interact(interactor);
			return true;
		}
		return false;
	}
}
