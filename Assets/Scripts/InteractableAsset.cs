using UnityEngine;

public enum AssetClassification
{
	CouncilPublic,
	Private
}

public class InteractableAsset : MonoBehaviour
{
	[Header("Asset Information")]
	public string assetName = "New Asset";

	[TextArea(2, 4)]
	public string question =
		"Who generally looks after this asset?";

	[Header("Classification")]
	public AssetClassification correctClassification =
		AssetClassification.CouncilPublic;

	[TextArea(2, 4)]
	public string correctExplanation;

	[TextArea(2, 4)]
	public string incorrectExplanation;

	[Header("Scoring")]
	public int pointsForCorrectAnswer = 100;
	public int pointsForIncorrectAnswer = -50;

	public bool IsCompleted { get; private set; }

	public bool CanInteract => !IsCompleted;

	public void MarkCompleted()
	{
		IsCompleted = true;
	}
}