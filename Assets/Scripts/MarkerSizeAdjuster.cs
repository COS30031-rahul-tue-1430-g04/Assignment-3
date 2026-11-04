using UnityEngine;

public class MarkerSizeAdjuster : MonoBehaviour
{
	public float scaleAtDefaultDistance = 1f;
	private readonly float defaultCameraDistance = 15f;

	private Camera miniMapCamera;
	private void Awake()
	{
		miniMapCamera = GameObject.FindGameObjectsWithTag("MiniMapCamera")[0].GetComponent<Camera>();
	}
	private void Start()
	{
		if (miniMapCamera == null)
		{
			Debug.LogError("MiniMapCamera not found in the scene. Please ensure there is a camera with the 'MiniMapCamera' tag.");
			return;
		}
		transform.localScale = Vector3.one * ((miniMapCamera.orthographicSize / defaultCameraDistance) * scaleAtDefaultDistance);
	}
}
