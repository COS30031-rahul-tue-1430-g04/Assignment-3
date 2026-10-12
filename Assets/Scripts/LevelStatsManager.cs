using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelStats
{
	public LevelStats(int totalAssets) => TotalAssets = totalAssets;
	public int TotalAssets { get; }
	public int AssetsAnswered { get; private set; } = 0;
	public int AssetsAnsweredCorrectly { get; private set; } = 0;
	public int Score { get; private set; } = 0;

	public void UpdateStats(bool correct)
	{
		AssetsAnswered++;
		if (correct)
			AssetsAnsweredCorrectly++;
		Score += correct ? 100 : -50;
	}
}


[CreateAssetMenu(fileName = "LevelStatsManager", menuName = "ScriptableObjects/LevelStatsManager", order = 1)]
public class LevelStatsManager : ScriptableObject
{
	public LevelStats CurrentLevelStats { get; private set; } = new(0);
	public List<LevelStats> PastLevelStats { get; private set; } = new();
	public int TotalAssets => CurrentLevelStats.TotalAssets + PastLevelStats.Sum(s => s.TotalAssets);
	public int TotalAssetsAnswered => CurrentLevelStats.AssetsAnswered + PastLevelStats.Sum(s => s.AssetsAnswered);
	public int TotalAssetsAnsweredCorrectly => CurrentLevelStats.AssetsAnsweredCorrectly + PastLevelStats.Sum(s => s.AssetsAnsweredCorrectly);
	public int TotalScore => CurrentLevelStats.Score + PastLevelStats.Sum(s => s.Score);

	public static event Action<LevelStats> OnUpdated;

	public void StoreAndNew(int totalAssets)
	{
		PastLevelStats.Add(CurrentLevelStats);
		CurrentLevelStats = new LevelStats(totalAssets);
		OnUpdated?.Invoke(CurrentLevelStats);
	}
	public void Reset()
	{
		PastLevelStats = new List<LevelStats>();
		CurrentLevelStats = new LevelStats(0);
	}
	public void UpdateStats(bool correct)
	{
		CurrentLevelStats.UpdateStats(correct);
		OnUpdated?.Invoke(CurrentLevelStats);
	}
}