using UnityEngine;

public class MarkerSizeAdjuster : MonoBehaviour
{
	public float scaleAtDefaultDistance = 1f;
	private readonly float defaultCameraDistance = 15f;

	private MiniMapCameraFollow MiniMapCameraFollow
	{
		get
		{
			GameObject miniMapCameraGameobject = GameObject.FindGameObjectWithTag("MiniMapCamera");
			if (miniMapCameraGameobject == null) return null;
			if (!miniMapCameraGameobject.TryGetComponent<MiniMapCameraFollow>(out var miniMapCamera)) return null;
			return miniMapCamera;
		}
	}

	private void OnValidate()
	{
		if (MiniMapCameraFollow == null || transform.parent == null) return;
		float scaleFactor = MiniMapCameraFollow.viewingDistance / defaultCameraDistance;
		float desiredScale = scaleFactor * scaleAtDefaultDistance;
		Vector3 parentScale = transform.parent.lossyScale;
		transform.localScale = new Vector3(
			desiredScale / parentScale.x,
			desiredScale / parentScale.y,
			desiredScale / parentScale.z
		);
		transform.position = new Vector3(
			transform.parent.position.x,
			MiniMapCameraFollow.height - 1f,
			transform.parent.position.z
		);
		transform.localRotation = Quaternion.Inverse(transform.parent.rotation) * Quaternion.Euler(90, 0, 0);
	}

}
