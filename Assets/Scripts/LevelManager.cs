using System;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public LevelStatsManager levelStats;

	public event Action OnLevelFinished;

	private bool CheckSerializedFields()
	{
		if (levelStats == null)
		{
			Debug.LogError("LevelManager: One or more serialized fields are not assigned.");
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
		levelStats.StoreAndNew(assets.Length);
		foreach (Asset asset in assets.Select(a => a.asset))
			asset.OnAssetAnswered += OnAssetAnswered;
	}

	void Start() =>
		GameManager.Instance.CurrentState = GameManager.GameState.Gameplay;

	void OnAssetAnswered(bool correct)
	{
		levelStats.UpdateStats(correct);
		if (levelStats.CurrentLevelStats.AssetsAnswered >= levelStats.CurrentLevelStats.TotalAssets)
			OnLevelFinished?.Invoke();
	}
}