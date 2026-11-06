using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AddHouseColliders
{
	[MenuItem("Tools/Suburban Level/Add House Colliders")]
	public static void AddColliders()
	{
		GameObject root = Selection.activeGameObject;

		if (root == null)
		{
			Debug.LogWarning("Please select the Houses parent.");
			return;
		}

		int processed = 0;
		int skipped = 0;

		foreach (Transform house in root.transform)
		{
			Renderer[] renderers =
				house.GetComponentsInChildren<Renderer>(true);

			if (renderers.Length == 0)
			{
				skipped++;
				continue;
			}

			// Calculate house bounds.
			Bounds bounds = renderers[0].bounds;

			foreach (Renderer r in renderers)
				bounds.Encapsulate(r.bounds);

			float height = bounds.size.y;

			if (height <= 0.1f)
			{
				skipped++;
				continue;
			}

			// Remove existing Box Colliders in this house.
			foreach (BoxCollider box in
				house.GetComponentsInChildren<BoxCollider>(true))
			{
				Undo.DestroyObjectImmediate(box);
			}

			// Remove previously generated house collider.
			Transform old = house.Find("HouseCollider");

			if (old != null)
				Undo.DestroyObjectImmediate(old.gameObject);

			// Create collider object.
			GameObject colliderObject =
				new GameObject("HouseCollider");

			Undo.RegisterCreatedObjectUndo(
				colliderObject, "Add House Collider");

			colliderObject.transform.SetParent(house, false);

			// Keep collider aligned with world axes.
			colliderObject.transform.rotation = Quaternion.identity;

			// Compensate for parent scaling.
			Vector3 parentScale = house.lossyScale;

			colliderObject.transform.localScale = new Vector3(
				1f / Mathf.Max(Mathf.Abs(parentScale.x), 0.001f),
				1f / Mathf.Max(Mathf.Abs(parentScale.y), 0.001f),
				1f / Mathf.Max(Mathf.Abs(parentScale.z), 0.001f)
			);

			// Position at the centre of the house bounds.
			colliderObject.transform.position = bounds.center;

			// Add Box Collider.
			BoxCollider collider =
				Undo.AddComponent<BoxCollider>(colliderObject);

			collider.center = Vector3.zero;
			collider.size = bounds.size;
			collider.isTrigger = false;

			processed++;
		}

		EditorSceneManager.MarkSceneDirty(root.scene);

		Debug.Log(
			"House colliders added. Processed: " +
			processed + ", Skipped: " + skipped
		);
	}
}