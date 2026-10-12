/*using UnityEngine;

public enum AssetClassification
{
	CouncilPublic,
	Private
}

[CreateAssetMenu(
	fileName = "New Asset Data",
	menuName = "Who Looks After This/Asset Data"
)]
public class AssetData : ScriptableObject
{
	[Header("Asset Information")]
	public string assetName;

	[TextArea(2, 5)]
	public string question =
		"Who generally looks after this asset?";

	[Header("Answer")]
	public AssetClassification correctClassification;

	[TextArea(2, 5)]
	public string correctExplanation;

	[TextArea(2, 5)]
	public string incorrectExplanation;

	[Header("Scoring")]
	public int pointsForCorrectAnswer = 100;
	public int pointsForIncorrectAnswer = -50;
}*/