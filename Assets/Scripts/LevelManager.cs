using System;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	[SerializeField] private int assetsAnswered = 0;
	[SerializeField] private int assetsAnsweredCorrectly = 0;
	[SerializeField] private int totalAssets;
	[SerializeField] private int score;

	public event Action OnAllAssetsAnswered;
	public event Action<int> OnScoreChanged;
	public event Action<int, int> OnAssetsAnsweredChanged; // total, correct

	void Awake()
	{
		InteractableAsset[] assets = FindObjectsByType<InteractableAsset>(
			FindObjectsInactive.Include,
			FindObjectsSortMode.None
		);
		totalAssets = assets.Length;
		foreach (Asset asset in assets.Select(a => a.asset))
			asset.OnAssetAnswered += OnAssetAnswered;
	}

	void OnAssetAnswered(bool correct)
	{
		assetsAnswered++;
		if (correct)
		{
			assetsAnsweredCorrectly++;
			score += 100;
		}
		else
		{
			score -= 50;
		}
		OnScoreChanged?.Invoke(score);
		OnAssetsAnsweredChanged?.Invoke(assetsAnswered, assetsAnsweredCorrectly);
		if (assetsAnswered >= totalAssets)
			OnAllAssetsAnswered?.Invoke();
	}
}