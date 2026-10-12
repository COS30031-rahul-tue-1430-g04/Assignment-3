using System.Collections.Generic;
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
		new Color(0.75f, 0.90f, 0.75f);

	public Color incorrectBackgroundColor =
		new Color(0.95f, 0.75f, 0.75f);

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
	private InteractableAsset currentInteractable;

	private readonly Dictionary<Asset, InteractableAsset>
		interactableLookup = new();

	private bool CheckSerializedFields()
	{
		if (interactionPanel == null ||
			nameText == null ||
			questionText == null ||
			feedbackPanel == null ||
			feedbackText == null ||
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

		InteractableAsset[] assets =
			FindObjectsByType<InteractableAsset>(
				FindObjectsInactive.Include,
				FindObjectsSortMode.None
			);

		foreach (InteractableAsset interactable in assets)
		{
			if (interactable.asset == null)
				continue;

			Asset data = interactable.asset;

			if (interactableLookup.ContainsKey(data))
			{
				Debug.LogWarning(
					$"Asset data '{data.name}' is assigned to multiple " +
					"InteractableAsset components. Each world object " +
					"should have its own Asset data.",
					interactable
				);
			}

			interactableLookup[data] = interactable;

			// Prevent duplicate subscriptions.
			data.OnAssetFound -= OnAssetFound;
			data.OnAssetFound += OnAssetFound;
		}
	}

	private void OnAssetFound(Asset data)
	{
		if (!CheckSerializedFields() || data == null)
			return;

		currentAsset = data;
		currentInteractable = null;

		interactableLookup.TryGetValue(
			data,
			out currentInteractable
		);

		nameText.text = string.IsNullOrWhiteSpace(data.name)
			? "Unknown Asset"
			: data.name;

		questionText.text =
			currentInteractable != null &&
			!string.IsNullOrWhiteSpace(
				currentInteractable.question)
				? currentInteractable.question
				: defaultQuestion;

		SetOptionalText(
			categoryText,
			"Category: ",
			GetValue(a => a.assetCategory)
		);

		SetOptionalText(
			descriptionText,
			"",
			GetValue(a => a.assetDescription)
		);

		SetOptionalText(
			locationText,
			"Location: ",
			GetValue(a => a.locationDescription)
		);

		SetOptionalText(
			purposeText,
			"Purpose: ",
			GetValue(a => a.realWorldPurpose)
		);

		SetOptionalText(
			responsibilityText,
			"Responsibility: ",
			GetValue(a => a.responsibilityDetails)
		);

		SetOptionalText(
			interestingFactText,
			"Did you know? ",
			GetValue(a => a.interestingFact)
		);

		SetOptionalText(
			interactionHintText,
			"",
			GetValue(a => a.interactionHint)
		);

		if (assetIconImage != null)
		{
			Sprite icon = currentInteractable != null
				? currentInteractable.assetIcon
				: null;

			assetIconImage.sprite = icon;
			assetIconImage.enabled = icon != null;
		}

		interactionPanel.SetActive(true);
		feedbackPanel.SetActive(false);

		GameManager.Instance.CurrentState =
			GameManager.GameState.AssetMenu;
	}

	private string GetValue(
		System.Func<InteractableAsset, string> selector)
	{
		if (currentInteractable == null)
			return null;

		return selector(currentInteractable);
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

		Asset answeredAsset = currentAsset;
		InteractableAsset answeredInteractable =
			currentInteractable;

		bool isCorrect = answeredAsset.Answer(selectedCouncil);

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

		string explanation = answeredAsset.explanation;

		if (answeredInteractable != null)
		{
			explanation = AppendInformation(
				explanation,
				answeredInteractable.responsibilityDetails
			);

			explanation = AppendInformation(
				explanation,
				string.IsNullOrWhiteSpace(
					answeredInteractable.interestingFact)
					? null
					: "Did you know? " +
					  answeredInteractable.interestingFact
			);
		}

		explanationText.text = explanation;

		// The object has already been found, so stop listening
		// for another found event from this asset.
		answeredAsset.OnAssetFound -= OnAssetFound;

		currentAsset = null;
		currentInteractable = null;
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

	private void OnDestroy()
	{
		foreach (Asset data in interactableLookup.Keys)
		{
			if (data != null)
				data.OnAssetFound -= OnAssetFound;
		}
	}
}