using System;
using UnityEngine;

[Serializable]
public class Asset
{
	[SerializeField] private string name;
	[SerializeField] [TextArea] private string explanation;
	[SerializeField] private bool councilOwned;

	public event Action<Asset> OnAssetFound;
	public event Action<Asset> OnAssetAnswered;

	public void Find() =>
		OnAssetFound?.Invoke(this);
	public void Answer() =>
		OnAssetAnswered?.Invoke(this);
}