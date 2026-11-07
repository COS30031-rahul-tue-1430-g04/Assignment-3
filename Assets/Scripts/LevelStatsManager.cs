using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelStats
{
	public LevelStats(int totalAssets) => TotalAssets = totalAssets;

	[field: SerializeField]
	public int TotalAssets { get; }

	[field: SerializeField]
	public int AssetsAnswered { get; private set; } = 0;

	[field: SerializeField]
	public int AssetsAnsweredCorrectly { get; private set; } = 0;

	[field: SerializeField]
	public int Score { get; private set; } = 0;

	public void UpdateStats(bool correct)
	{
		AssetsAnswered++;

		if (correct)
			AssetsAnsweredCorrectly++;

		Score += correct ? 100 : -50;
	}
}

[CreateAssetMenu(
	fileName = "LevelStatsManager",
	menuName = "ScriptableObjects/LevelStatsManager",
	order = 1)]
public class LevelStatsManager : ScriptableObject
{
	[field: SerializeField]
	public LevelStats CurrentLevelStats { get; private set; } = new(0);

	[field: SerializeField]
	public List<LevelStats> PastLevelStats { get; private set; } = new();

	public int TotalAssets =>
		CurrentLevelStats.TotalAssets +
		PastLevelStats.Sum(s => s.TotalAssets);

	public int TotalAssetsAnswered =>
		CurrentLevelStats.AssetsAnswered +
		PastLevelStats.Sum(s => s.AssetsAnswered);

	public int TotalAssetsAnsweredCorrectly =>
		CurrentLevelStats.AssetsAnsweredCorrectly +
		PastLevelStats.Sum(s => s.AssetsAnsweredCorrectly);

	public int TotalScore =>
		CurrentLevelStats.Score +
		PastLevelStats.Sum(s => s.Score);

	public static event Action<LevelStats> OnUpdated;

	// Start a new level without losing previous scores.
	public void StoreAndNew(int totalAssets)
	{
		PastLevelStats.Add(CurrentLevelStats);

		CurrentLevelStats = new LevelStats(totalAssets);

		OnUpdated?.Invoke(CurrentLevelStats);
	}

	// Use when the entire game session should restart.
	public void Reset()
	{
		PastLevelStats = new List<LevelStats>();
		CurrentLevelStats = new LevelStats(0);

		OnUpdated?.Invoke(CurrentLevelStats);
	}

	public void UpdateStats(bool correct)
	{
		CurrentLevelStats.UpdateStats(correct);

		OnUpdated?.Invoke(CurrentLevelStats);
	}
}