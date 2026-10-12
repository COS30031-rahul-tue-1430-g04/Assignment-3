using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : MonoBehaviour
{
	[Header("Panels")]
	public GameObject questionPanel;
	public GameObject feedbackPanel;
	public GameObject promptObject;

	[Header("Question UI")]
	public TMP_Text questionText;
	public Button councilButton;
	public Button privateButton;

	[Header("Feedback UI")]
	public TMP_Text feedbackText;
	public Button continueButton;

	[Header("HUD")]
	public TMP_Text scoreText;
	public TMP_Text exploredText;
	public TMP_Text answeredText;

	[Header("Progress")]
	public GameProgressManager progressManager;

	public bool IsQuestionOpen { get; private set; }

	private InteractableAsset currentAsset;

	private void Start()
	{
		// Find the progress manager if one was not assigned.
		if (progressManager == null)
		{
			progressManager =
				FindFirstObjectByType<GameProgressManager>();
		}

		// Hide panels at the start.
		if (questionPanel != null)
			questionPanel.SetActive(false);

		if (feedbackPanel != null)
			feedbackPanel.SetActive(false);

		ShowPrompt(false);

		// Register button listeners.
		if (councilButton != null)
			councilButton.onClick.AddListener(ChooseCouncilPublic);

		if (privateButton != null)
			privateButton.onClick.AddListener(ChoosePrivate);

		if (continueButton != null)
			continueButton.onClick.AddListener(ContinueAfterFeedback);

		UpdateHUD();
	}

	public void ShowPrompt(bool show)
	{
		if (promptObject != null)
			promptObject.SetActive(show);
	}

	public void OpenQuestion(InteractableAsset asset)
	{
		if (asset == null)
			return;

		currentAsset = asset;
		IsQuestionOpen = true;

		ShowPrompt(false);

		if (questionText != null)
		{
			questionText.text =
				asset.assetName + "\n\n" + asset.question;
		}

		if (questionPanel != null)
			questionPanel.SetActive(true);

		if (feedbackPanel != null)
			feedbackPanel.SetActive(false);

		if (progressManager != null)
			progressManager.RecordExplored();

		UpdateHUD();
	}

	private void ChooseCouncilPublic()
	{
		SubmitAnswer(AssetClassification.CouncilPublic);
	}

	private void ChoosePrivate()
	{
		SubmitAnswer(AssetClassification.Private);
	}

	private void SubmitAnswer(
		AssetClassification chosenClassification)
	{
		if (currentAsset == null)
			return;

		bool isCorrect =
			chosenClassification ==
			currentAsset.correctClassification;

		int points = isCorrect
			? currentAsset.pointsForCorrectAnswer
			: currentAsset.pointsForIncorrectAnswer;

		if (progressManager != null)
			progressManager.RecordAnswer(isCorrect, points);

		if (questionPanel != null)
			questionPanel.SetActive(false);

		if (feedbackPanel != null)
			feedbackPanel.SetActive(true);

		if (feedbackText != null)
		{
			string result = isCorrect
				? "CORRECT!\n\n"
				: "NOT QUITE!\n\n";

			string explanation = isCorrect
				? currentAsset.correctExplanation
				: currentAsset.incorrectExplanation;

			feedbackText.text = result + explanation;
		}

		UpdateHUD();
	}

	public void ContinueAfterFeedback()
	{
		if (feedbackPanel != null)
			feedbackPanel.SetActive(false);

		currentAsset = null;
		IsQuestionOpen = false;

		UpdateHUD();
	}

	private void UpdateHUD()
	{
		if (progressManager == null)
			return;

		if (scoreText != null)
			scoreText.text = "Score: " + progressManager.Score;

		if (exploredText != null)
			exploredText.text =
				"Explored: " + progressManager.AssetsExplored;

		if (answeredText != null)
			answeredText.text =
				"Correct answers: " + progressManager.AssetsAnswered;
	}

	private void OnDestroy()
	{
		if (councilButton != null)
			councilButton.onClick.RemoveListener(ChooseCouncilPublic);

		if (privateButton != null)
			privateButton.onClick.RemoveListener(ChoosePrivate);

		if (continueButton != null)
			continueButton.onClick.RemoveListener(ContinueAfterFeedback);
	}
}