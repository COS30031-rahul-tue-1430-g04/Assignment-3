using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssetPanelManager : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private string defaultQuestion =
		"Who is responsible for maintaining this asset?";

	public Color correctBackgroundColor =
		new(0.75f, 0.90f, 0.75f);

	public Color incorrectBackgroundColor =
		new(0.95f, 0.75f, 0.75f);

	[Header("Interaction Panel")]
	[SerializeField] private GameObject interactionPanel;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI questionText;

	[Header("Asset Information UI")]
	[SerializeField] private TextMeshProUGUI categoryText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private TextMeshProUGUI locationText;
	[SerializeField] private TextMeshProUGUI purposeText;
	[SerializeField] private TextMeshProUGUI responsibilityText;
	[SerializeField] private TextMeshProUGUI interestingFactText;
	[SerializeField] private TextMeshProUGUI interactionHintText;
	[SerializeField] private Image assetIconImage;

	[Header("Feedback Panel")]
	[SerializeField] private GameObject feedbackPanel;
	[SerializeField] private TextMeshProUGUI feedbackText;
	[SerializeField] private Image feedbackBackground;
	[SerializeField] private TextMeshProUGUI explanationText;

	private Asset currentAsset;
	private IEnumerable<Asset> assets;

	private bool CheckSerializedFields()
	{
		if (interactionPanel == null ||
			nameText == null ||
			questionText == null ||

			feedbackPanel == null ||
			feedbackText == null ||
			feedbackBackground == null ||
			explanationText == null)
		{
			Debug.LogError(
				"AssetPanelManager: Assign all required UI references.",
				this
			);

			return false;
		}

		return true;
	}

	private void Awake()
	{
		if (!CheckSerializedFields())
			return;

		interactionPanel.SetActive(false);
		feedbackPanel.SetActive(false);

		assets = FindObjectsByType<InteractableAsset>(FindObjectsInactive.Include, FindObjectsSortMode.None)
			.Where(interactable => interactable.asset != null)
			.Select(interactable => interactable.asset);
	}
	private void OnEnable()
	{
		foreach (Asset asset in assets)
			asset.OnAssetFound += OnAssetFound;
	}

	private void OnAssetFound(Asset asset)
	{
		if (!CheckSerializedFields() || asset == null)
			return;

		currentAsset = asset;

		nameText.text = string.IsNullOrWhiteSpace(asset.name)
			? "Unknown Asset"
			: asset.name;

		questionText.text = string.IsNullOrWhiteSpace(asset.question)
			? defaultQuestion
			: asset.question;

		SetOptionalText(categoryText, "Category: ", asset.category);
		SetOptionalText(descriptionText, "", asset.description);
		SetOptionalText(locationText, "Location: ", asset.locationDescription);
		SetOptionalText(purposeText, "Purpose: ", asset.realWorldPurpose);
		SetOptionalText(responsibilityText, "Responsibility: ", asset.responsibilityDetails);
		SetOptionalText(interestingFactText, "Did you know? ", asset.interestingFact);
		SetOptionalText(interactionHintText, "", asset.interactionHint);

		assetIconImage.sprite = asset.icon;
		assetIconImage.enabled = asset.icon != null;

		interactionPanel.SetActive(true);
		feedbackPanel.SetActive(false);

		GameManager.Instance.CurrentState = GameManager.GameState.AssetMenu;
	}

	private void SetOptionalText(
		TextMeshProUGUI target,
		string prefix,
		string value)
	{
		if (target == null)
			return;

		bool hasValue = !string.IsNullOrWhiteSpace(value);

		target.gameObject.SetActive(hasValue);

		if (hasValue)
			target.text = prefix + value;
	}

	public void OnInteractionPanelButtonClicked(
		bool selectedCouncil)
	{
		if (!CheckSerializedFields())
			return;

		if (currentAsset == null)
		{
			Debug.LogError(
				"AssetPanelManager: No asset is currently selected.",
				this
			);

			return;
		}


		bool isCorrect = currentAsset.Answer(selectedCouncil);

		interactionPanel.SetActive(false);
		feedbackPanel.SetActive(true);

		feedbackText.text = isCorrect
			? "CORRECT!"
			: "INCORRECT!";

		if (feedbackBackground != null)
		{
			feedbackBackground.color = isCorrect
				? correctBackgroundColor
				: incorrectBackgroundColor;
		}

		string explanation = currentAsset.explanation;


		explanation = AppendInformation(
			explanation,
			currentAsset.responsibilityDetails
		);

		explanation = AppendInformation(
			explanation,
			string.IsNullOrWhiteSpace(
				currentAsset.interestingFact)
				? null
				: "Did you know? " +
					currentAsset.interestingFact
		);

		explanationText.text = explanation;

		currentAsset = null;
	}

	private string AppendInformation(
		string existing,
		string additional)
	{
		if (string.IsNullOrWhiteSpace(additional))
			return existing ?? "";

		if (string.IsNullOrWhiteSpace(existing))
			return additional;

		return existing + "\n\n" + additional;
	}

	public void OnFeedbackContinueClicked()
	{
		if (!CheckSerializedFields())
			return;

		feedbackPanel.SetActive(false);

		GameManager.Instance.CurrentState =
			GameManager.GameState.Gameplay;
	}

	private void OnDisable()
	{
		if (assets == null) return;
		foreach (Asset asset in assets)
			asset.OnAssetFound -= OnAssetFound;
	}
}