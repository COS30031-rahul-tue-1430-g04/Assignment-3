using System.Linq;
using TMPro;
using UnityEngine;
public class InteractionPanelManager : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private string questionString;

	[Header("References")]
	[SerializeField] private GameObject panel;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI questionText;

	bool CheckSerializedFields()
	{
		if (panel == null || nameText == null || questionText == null)
		{
			Debug.LogError("InteractionPanelManager: One or more serialized fields are not assigned.");
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
		foreach (Asset asset in assets.Select(a => a.asset))
			asset.OnAssetFound += OnAssetFound;
	}

	void OnAssetFound(Asset asset)
	{
		if (!CheckSerializedFields()) return;

		panel.SetActive(true);
		nameText.text = asset.name;
		questionText.text = questionString;
	}
}