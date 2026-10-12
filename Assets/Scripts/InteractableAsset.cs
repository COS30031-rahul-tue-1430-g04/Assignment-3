using UnityEngine;

public class InteractableAsset : Interactable
{
	[Header("Core Asset Data")]
	public Asset asset;

	[Header("Asset Identification")]
	public string assetCategory = "General";

	[TextArea(2, 4)]
	public string assetDescription;

	public string locationDescription;

	[Header("Question")]
	[TextArea(2, 4)]
	public string question =
		"Who is responsible for maintaining this asset?";

	[Header("Educational Information")]
	[TextArea(2, 4)]
	public string realWorldPurpose;

	[TextArea(2, 4)]
	public string responsibilityDetails;

	[TextArea(2, 4)]
	public string interestingFact;

	[Header("Interaction Settings")]
	public string interactionHint = "Press the Interact key to inspect.";

	public Sprite assetIcon;

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

		if (string.IsNullOrWhiteSpace(assetCategory))
			assetCategory = "General";
	}
}