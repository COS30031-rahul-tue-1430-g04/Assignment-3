using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PanelManager : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private string questionString;
	[Tooltip("Background colour shown when the answer is correct.")]
	public Color correctBackgroundColor = new(0.75f, 0.90f, 0.75f);

	[Tooltip("Background colour shown when the answer is incorrect.")]
	public Color incorrectBackgroundColor = new(0.95f, 0.75f, 0.75f);

	[Header("References")]
	[SerializeField] private GameObject interactionPanel;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI questionText;
	[SerializeField] private GameObject feedbackPanel;
	[SerializeField] private TextMeshProUGUI feedbackText;
	[SerializeField] private Image feedbackBackground;
	[SerializeField] private TextMeshProUGUI explanationText;
	[SerializeField] private GameManager gameManager;

	private Asset currentAsset = null;

	bool CheckSerializedFields()
	{
		if (
			interactionPanel == null || nameText == null || questionText == null ||
			feedbackPanel == null || feedbackText == null || explanationText == null ||
			gameManager == null
		)
		{
			Debug.LogError("PanelManager: One or more serialized fields are not assigned.");
			return false;
		}
		return true;
	}


	void Awake()
	{
		if (!CheckSerializedFields()) return;

		InteractableAsset[] assets = FindObjectsByType<InteractableAsset>(
			FindObjectsInactive.Include,
			FindObjectsSortMode.None
		);
		foreach (Asset asset in assets.Select(a => a.asset))
			asset.OnAssetFound += OnAssetFound;
	}

	void OnAssetFound(Asset asset)
	{
		if (!CheckSerializedFields()) return;

		interactionPanel.SetActive(true);
		nameText.text = asset.name;
		questionText.text = questionString;

		currentAsset = asset;
		asset.OnAssetFound -= OnAssetFound;

		gameManager.CurrentState = GameManager.GameState.AssetMenu;
	}

	public void OnInteractionPanelButtonClicked(bool selectedCouncil)
	{
		if (!CheckSerializedFields()) return;
		if (currentAsset == null)
		{
			Debug.LogError("PanelManager: No current asset to answer.");
			return;
		}

		interactionPanel.SetActive(false);
		feedbackPanel.SetActive(true);

		if (currentAsset!.Answer(selectedCouncil))
		{
			feedbackText.text = "CORRECT!";
			feedbackBackground.color = correctBackgroundColor;
		}
		else
		{
			feedbackText.text = "INCORRECT!";
			feedbackBackground.color = incorrectBackgroundColor;
		}

		explanationText.text = currentAsset!.explanation;


		currentAsset = null;
	}

	public void OnFeedbackContinueClicked()
	{
		if (!CheckSerializedFields()) return;

		feedbackPanel.SetActive(false);
		gameManager.CurrentState = GameManager.GameState.Gameplay;
	}
}