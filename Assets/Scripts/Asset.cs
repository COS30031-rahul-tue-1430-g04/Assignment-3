using System;
using UnityEngine;

[Serializable]
public class Asset
{
	public string name;
	[TextArea] public string explanation;
	[SerializeField] private bool councilOwned;

	public event Action<Asset> OnAssetFound;
	public event Action<bool> OnAssetAnswered;

	[Header("Asset Identification")]
	public string category = "General";
	[TextArea(2, 4)] public string description;

	public string locationDescription;
	public Sprite icon;

	[Header("Question")]
	[TextArea(2, 4)]
	public string question = "Who is responsible for maintaining this asset?";

	[Header("Educational Information")]
	[TextArea(2, 4)] public string realWorldPurpose;

	[TextArea(2, 4)] public string responsibilityDetails;

	[TextArea(2, 4)] public string interestingFact;

	[Header("Interaction Settings")]
	public string interactionHint = "Press the Interact key to inspect.";

	public void Find() =>
		OnAssetFound?.Invoke(this);
	public bool Answer(bool selectedCouncil)
	{
		bool correct = councilOwned == selectedCouncil;
		OnAssetAnswered?.Invoke(correct);
		return correct;
	}
}