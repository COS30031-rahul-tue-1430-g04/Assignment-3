using UnityEngine;

public class InteractableAsset : Interactable
{
	public Asset asset;
	private GameObject marker;

	private void Awake()
	{
		Transform markerTransform = transform.Find("MapMarker");
		if (markerTransform != null)
		{
			marker = markerTransform.gameObject;
			marker.SetActive(true);
		}
	}

	public override void Interact(GameObject interactor)
	{
		CanInteract = false;
		if (marker != null)
			marker.SetActive(false);
		asset.Find();
	}

	public void ResetInteraction()
	{
		CanInteract = true;
		if (marker != null)
			marker.SetActive(true);
	}

	private void OnEnable() =>
		Asset.RegisterAsset(asset);
	private void OnDisable() =>
		Asset.UnregisterAsset(asset);

}