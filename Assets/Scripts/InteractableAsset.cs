using UnityEngine;

public class InteractableAsset : Interactable
{
	public Asset asset;

	protected override void Interact(GameObject interactor)
	{
		CanInteract = false;
		asset.Find();
	}

	public void ResetInteraction() =>
		CanInteract = true;

}