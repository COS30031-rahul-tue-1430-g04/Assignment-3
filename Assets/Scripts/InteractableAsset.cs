using UnityEngine;

public class InteractableAsset : Interactable
{
	[Header("Core Asset Data")]
	public Asset asset;

	[SerializeField]
	private bool interactionEnabled = true;

	private bool hasBeenInteractedWith;

	public bool HasBeenInteractedWith =>
		hasBeenInteractedWith;

	public bool CanBeInteractedWith =>
		interactionEnabled &&
		!hasBeenInteractedWith &&
		asset != null;

	protected override void Interact(GameObject interactor)
	{
		if (!CanBeInteractedWith)
			return;

		hasBeenInteractedWith = true;
		asset.Find();
	}

	protected override bool CanInteract(GameObject interactor)
	{
		return CanBeInteractedWith;
	}

	public void ResetInteraction()
	{
		hasBeenInteractedWith = false;
	}

	public void SetInteractionEnabled(bool enabled)
	{
		interactionEnabled = enabled;
	}

	private void OnValidate()
	{
		if (asset == null)
		{
			Debug.LogWarning(
				$"No Asset data assigned to '{gameObject.name}'.",
				this
			);
		}
	}
}