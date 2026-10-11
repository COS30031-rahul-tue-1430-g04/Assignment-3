using UnityEngine;

[System.Serializable]
public class Asset
{
	public string name;
	[TextArea] public string explanation;
	public bool councilOwned;
}