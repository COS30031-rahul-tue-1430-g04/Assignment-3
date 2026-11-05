using UnityEngine;

public class MarkerSizeAdjuster : MonoBehaviour
{
	public float scaleAtDefaultDistance = 1f;
	private readonly float defaultCameraDistance = 15f;

	private float? CameraSize
	{
		get
		{
			GameObject miniMapCameraGameobject = GameObject.FindGameObjectWithTag("MiniMapCamera");
			if (miniMapCameraGameobject == null) return null;
			if (!miniMapCameraGameobject.TryGetComponent<Camera>(out var miniMapCamera)) return null;
			return miniMapCamera.orthographicSize;
		}
	}

	private void OnValidate()
	{
		if (CameraSize == null || transform.parent == null) return;
		float desiredScale = (CameraSize.Value / defaultCameraDistance) * scaleAtDefaultDistance;
		Vector3 parentScale = transform.parent.lossyScale;
		transform.localScale = new Vector3(
			desiredScale / parentScale.x,
			desiredScale / parentScale.y,
			desiredScale / parentScale.z
		);
	}

}
