using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private TMP_Text scoreText;
	[SerializeField] private TMP_Text progressText;
	[SerializeField] private TMP_Text levelText;
	[SerializeField] private Slider progressBar;

	[Header("Statistics")]
	[SerializeField] private LevelStatsManager levelStats;

	private bool CheckSerializedFields()
	{
		if (scoreText == null ||
			progressText == null ||
			levelText == null ||
			progressBar == null ||
			levelStats == null)
		{
			Debug.LogError(
				"HUDManager: One or more references are not assigned."
			);
			return false;
		}

		return true;
	}

	private void OnEnable()
	{
		LevelStatsManager.OnUpdated += OnAssetAnswered;
	}

	private void Start()
	{
		UpdateHUD();
	}

	private void OnDisable()
	{
		LevelStatsManager.OnUpdated -= OnAssetAnswered;
	}

	private void OnAssetAnswered(LevelStats stats)
	{
		UpdateHUD();
	}

	private void UpdateHUD()
	{
		if (!CheckSerializedFields())
			return;

		LevelStats current = levelStats.CurrentLevelStats;

		// Display cumulative score across all levels.
		scoreText.text = $"Score: {levelStats.TotalScore}";

		// Display current level progress.
		progressText.text =
			$"Assets Found: {current.AssetsAnswered}/{current.TotalAssets}";

		progressBar.value = current.TotalAssets > 0
			? (float)current.AssetsAnswered / current.TotalAssets
			: 0f;
	}
}