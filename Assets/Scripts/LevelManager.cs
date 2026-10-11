using System;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	[SerializeField] private int assetsAnswered = 0;
	[SerializeField] private int totalAssets;

	public event Action OnAllAssetsAnswered;

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

	void OnAssetAnswered(Asset _)
	{
		assetsAnswered++;
		if (assetsAnswered >= totalAssets)
			OnAllAssetsAnswered?.Invoke();
	}
}