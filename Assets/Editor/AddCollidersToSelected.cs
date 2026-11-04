using UnityEngine;
using UnityEditor;

public class AddCollidersToSelected
{
	[MenuItem("Tools/Suburban Level/Add Colliders To Selected")]
	private static void AddColliders()
	{
		int added = 0;

		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj.GetComponent<Collider>() != null)
				continue;

			BoxCollider collider = obj.AddComponent<BoxCollider>();

			Renderer[] renderers =
				obj.GetComponentsInChildren<Renderer>();

			if (renderers.Length > 0)
			{
				Bounds bounds = renderers[0].bounds;

				foreach (Renderer renderer in renderers)
					bounds.Encapsulate(renderer.bounds);

				collider.center = obj.transform.InverseTransformPoint(bounds.center);

				Vector3 size = obj.transform.InverseTransformVector(bounds.size);

				collider.size = new Vector3(
					Mathf.Abs(size.x),
					Mathf.Abs(size.y),
					Mathf.Abs(size.z)
				);
			}

			added++;
		}

		Debug.Log("Added colliders to " + added + " objects.");
	}
}