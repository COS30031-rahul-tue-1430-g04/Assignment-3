using System;
using UnityEngine;

[Serializable]
public class Asset
{
	public string name;
	[TextArea] public string explanation;
	[SerializeField] private bool councilOwned;

	public event Action<Asset> OnAssetFound;
	public event Action<bool> OnAssetAnswered;

	public void Find() =>
		OnAssetFound?.Invoke(this);
	public bool Answer(bool selectedCouncil)
	{
		bool correct = councilOwned == selectedCouncil;
		OnAssetAnswered?.Invoke(correct);
		return correct;
	}
}