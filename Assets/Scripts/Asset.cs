using System;
using UnityEngine;

[Serializable]
public class Asset
{
	public string name;
	[TextArea] public string explanation;
	public bool councilOwned;

	public event Action<Asset> OnAssetFound;
	public event Action<Asset> OnAssetAnswered;

	public void Find() =>
		OnAssetFound?.Invoke(this);
	public void Answer() =>
		OnAssetAnswered?.Invoke(this);
}