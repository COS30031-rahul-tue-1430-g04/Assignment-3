using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AddEnvironmentColliders
{
	[MenuItem("Tools/Suburban Level/Add Environment Colliders")]
	public static void AddColliders()
	{
		GameObject root = Selection.activeGameObject;

		if (root == null)
		{
			Debug.LogWarning("Select Street Props or Community Greens parent.");
			return;
		}

		int processed = 0;
		int skipped = 0;

		foreach (Transform item in root.transform)
		{
			Renderer[] renderers =
				item.GetComponentsInChildren<Renderer>(true);

			if (renderers.Length == 0)
			{
				skipped++;
				continue;
			}

			// Calculate combined renderer bounds.
			Bounds bounds = renderers[0].bounds;

			foreach (Renderer r in renderers)
				bounds.Encapsulate(r.bounds);

			if (bounds.size.sqrMagnitude < 0.01f)
			{
				skipped++;
				continue;
			}

			// Remove existing Box Colliders.
			BoxCollider[] existing =
				item.GetComponentsInChildren<BoxCollider>(true);

			foreach (BoxCollider box in existing)
				Undo.DestroyObjectImmediate(box);

			// Remove previous generated collider.
			Transform old = item.Find("EnvironmentCollider");

			if (old != null)
				Undo.DestroyObjectImmediate(old.gameObject);

			// Create collider object.
			GameObject colliderObject =
				new GameObject("EnvironmentCollider");

			Undo.RegisterCreatedObjectUndo(
				colliderObject, "Add Environment Collider");

			colliderObject.transform.SetParent(item, false);

			// Align with world axes and compensate for parent scale.
			Vector3 parentScale = item.lossyScale;

			colliderObject.transform.rotation = Quaternion.identity;

			colliderObject.transform.localScale = new Vector3(
				1f / Mathf.Max(Mathf.Abs(parentScale.x), 0.001f),
				1f / Mathf.Max(Mathf.Abs(parentScale.y), 0.001f),
				1f / Mathf.Max(Mathf.Abs(parentScale.z), 0.001f)
			);

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
			root.name + ": Colliders added: " +
			processed + ", Skipped: " + skipped
		);
	}
}