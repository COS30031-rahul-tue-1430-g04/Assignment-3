using UnityEngine;

public class InteractableAsset : Interactable
{

	public Asset asset;

	private bool hasBeenInteractedWith = false;

	protected override void Interact(GameObject interactor)
	{
		hasBeenInteractedWith = true;
		asset.Find();
	}

	protected override bool CanInteract(GameObject interactor) => !hasBeenInteractedWith;
}
