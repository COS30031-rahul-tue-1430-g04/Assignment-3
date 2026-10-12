using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EndGameManager : MonoBehaviour
{
	[Header("References")]
	public TMP_Text finalScoreText;
	public TMP_Text finalExploredText;
	public TMP_Text finalAnsweredText;
	public LevelStatsManager levelStats;

	private bool CheckSerializeFields()
	{
		if (finalScoreText == null || finalExploredText == null || finalAnsweredText == null || levelStats == null)
		{
			Debug.LogError("EndGameManager: One or more serialized fields are not assigned.");
			return false;
		}
		return true;
	}

	private void Start()
	{
		if (!CheckSerializeFields()) return;

		finalScoreText.text = $"FINAL SCORE: {levelStats.TotalScore}";
		finalExploredText.text = $"ASSETS EXPLORED: {levelStats.TotalAssetsAnswered}";
		finalAnsweredText.text = $"ASSETS ANSWERED CORRECTLY: {levelStats.TotalAssetsAnsweredCorrectly}";
	}

	public void PlayAgain() =>
		SceneManager.LoadScene(1);

	public void MainMenu() =>
		SceneManager.LoadScene(0);

	public void QuitGame() =>
		Application.Quit();
}