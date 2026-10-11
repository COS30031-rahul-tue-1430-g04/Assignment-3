using UnityEngine;

public class TestRecieveEvent : MonoBehaviour
{
	public GameManager gameManager;
	public int assetsFound = 0;
	private void Start()
	{
		gameManager.AssetFound += (asset) =>
		{
			Debug.Log($"Asset found: {asset.name}");
			assetsFound++;
		};
	}
}