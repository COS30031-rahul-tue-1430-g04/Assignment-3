using UnityEngine;

public class InteractableAsset : Interactable
{
	public Asset asset;

	[Header("References")]
	public GameManager gameManager;


	private bool hasBeenInteractedWith = false;

	protected override void Interact(GameObject interactor)
	{
		hasBeenInteractedWith = true;
		gameManager.InvokeAssetFound(asset);
	}

	protected override bool CanInteract(GameObject interactor) => !hasBeenInteractedWith;
}
