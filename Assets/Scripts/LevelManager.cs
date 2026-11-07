using System;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public LevelStatsManager levelStats;

	public event Action OnLevelFinished;

	private bool levelInitialized = false;

	private bool CheckSerializedFields()
	{
		if (levelStats == null)
		{
			Debug.LogError(
				"LevelManager: LevelStatsManager is not assigned."
			);
			return false;
		}

		return true;
	}

	void Awake()
	{
		if (!CheckSerializedFields())
			return;

		if (levelInitialized)
			return;

		InteractableAsset[] assets =
			FindObjectsByType<InteractableAsset>(
				FindObjectsInactive.Include,
				FindObjectsSortMode.None
			);

		// Start current level statistics.
		// Previous level statistics remain preserved.
		levelStats.StoreAndNew(assets.Length);

		foreach (Asset asset in assets.Select(a => a.asset))
		{
			if (asset != null)
				asset.OnAssetAnswered += OnAssetAnswered;
		}

		levelInitialized = true;

		Debug.Log(
			"Level started. Current score: " +
			levelStats.CurrentLevelStats.Score +
			" | Total score: " + levelStats.TotalScore
		);
	}

	void Start()
	{
		GameManager.Instance.CurrentState =
			GameManager.GameState.Gameplay;
	}

	void OnAssetAnswered(bool correct)
	{
		levelStats.UpdateStats(correct);

		Debug.Log("Current level score: " +
			levelStats.CurrentLevelStats.Score);

		Debug.Log("Overall score: " +
			levelStats.TotalScore);

		if (levelStats.CurrentLevelStats.AssetsAnswered >=
			levelStats.CurrentLevelStats.TotalAssets)
		{
			OnLevelFinished?.Invoke();
		}
	}

	private void OnDestroy()
	{
		if (levelStats == null)
			return;

		InteractableAsset[] assets =
			FindObjectsByType<InteractableAsset>(
				FindObjectsInactive.Include,
				FindObjectsSortMode.None
			);

		foreach (Asset asset in assets.Select(a => a.asset))
		{
			if (asset != null)
				asset.OnAssetAnswered -= OnAssetAnswered;
		}
	}
}