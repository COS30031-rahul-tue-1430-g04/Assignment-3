using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class ReplaceTreeColliders
{
	[MenuItem("Tools/Suburban Level/Replace Tree Colliders")]
	public static void ReplaceColliders()
	{
		GameObject root = Selection.activeGameObject;

		if (root == null)
		{
			Debug.LogWarning("Select the Trees parent first.");
			return;
		}

		int processed = 0;

		foreach (Transform tree in root.transform)
		{
			Renderer[] renderers =
				tree.GetComponentsInChildren<Renderer>(true);

			if (renderers.Length == 0)
				continue;

			Bounds bounds = renderers[0].bounds;

			foreach (Renderer r in renderers)
				bounds.Encapsulate(r.bounds);

			float height = bounds.size.y;

			if (height <= 0.1f)
				continue;

			// Remove existing Box Colliders.
			foreach (BoxCollider box in
				tree.GetComponentsInChildren<BoxCollider>(true))
			{
				Undo.DestroyObjectImmediate(box);
			}

			// Remove previous generated collider.
			Transform old = tree.Find("TrunkCollider");

			if (old != null)
				Undo.DestroyObjectImmediate(old.gameObject);

			// Create collider child.
			GameObject obj = new GameObject("TrunkCollider");

			Undo.RegisterCreatedObjectUndo(obj, "Add Tree Collider");

			obj.transform.SetParent(tree, false);

			// Keep collider vertical in world space.
			obj.transform.rotation = Quaternion.identity;

			// Compensate for parent scaling.
			Vector3 parentScale = tree.lossyScale;

			obj.transform.localScale = new Vector3(
				1f / Mathf.Max(Mathf.Abs(parentScale.x), 0.001f),
				1f / Mathf.Max(Mathf.Abs(parentScale.y), 0.001f),
				1f / Mathf.Max(Mathf.Abs(parentScale.z), 0.001f)
			);

			// Root position is treated as the planting position.
			obj.transform.position =
				tree.position + Vector3.up * (height / 2f);

			CapsuleCollider capsule =
				Undo.AddComponent<CapsuleCollider>(obj);

			float radius = Mathf.Clamp(height * 0.035f, 0.15f, 0.6f);

			capsule.direction = 1;
			capsule.height = height;
			capsule.radius = radius;
			capsule.center = Vector3.zero;
			capsule.isTrigger = false;

			processed++;
		}

		EditorSceneManager.MarkSceneDirty(root.scene);

		Debug.Log("Created ground-based tree colliders: " + processed);
	}
}