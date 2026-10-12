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

	bool CheckSerializedFields()
	{
		if (
			scoreText == null || progressText == null || levelText == null ||
			progressBar == null
		)
		{
			Debug.LogError("HUDManager: One or more serialized fields are not assigned.");
			return false;
		}
		return true;
	}

	void Awake() =>
		LevelStatsManager.OnUpdated += OnAssetAnswered;

	void OnAssetAnswered(LevelStats stats)
	{
		if (!CheckSerializedFields()) return;

		scoreText.text = $"Score: {stats.Score}";
		progressText.text = $"Assets Found: {stats.AssetsAnswered}/{stats.TotalAssets}";
		progressBar.value = (float)stats.AssetsAnswered / stats.TotalAssets;
	}
}