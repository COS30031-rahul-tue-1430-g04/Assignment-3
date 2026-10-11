using System.Linq;
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


	private int totalAssets = 0;
	private int answeredAssets = 0;
	private int score = 0;

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

	void Awake()
	{
		if (!CheckSerializedFields()) return;

		InteractableAsset[] assets = FindObjectsByType<InteractableAsset>(
			FindObjectsInactive.Include,
			FindObjectsSortMode.None
		);
		totalAssets = assets.Length;
		foreach (Asset asset in assets.Select(a => a.asset))
			asset.OnAssetAnswered += OnAssetAnswered;

		levelText.text = $"Level: {GameManager.Instance.CurrentLevel}";
		scoreText.text = $"Score: {score}";
		progressText.text = $"Assets Found: {answeredAssets}/{totalAssets}";
	}

	void OnAssetAnswered(bool correct)
	{
		if (!CheckSerializedFields()) return;

		scoreText.text = $"Score: {(score += correct ? 100 : -50)}";
		progressText.text = $"Assets Found: {++answeredAssets}/{totalAssets}";
		progressBar.value = (float)answeredAssets / totalAssets;
	}
}