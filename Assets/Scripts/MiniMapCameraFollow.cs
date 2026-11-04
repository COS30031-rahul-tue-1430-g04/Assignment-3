using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MiniMapCameraFollow : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Transform followTransform;

	[Header("Map Settings")]
	[Min(5f)]
	[SerializeField] private float height = 30f;

	[Min(5f)]
	[SerializeField] private float viewingDistance = 100f;

	[Min(0f)]
	[SerializeField] private float followSmoothness = 10f;

	private Camera mapCamera;

	private void Awake()
	{
		mapCamera = GetComponent<Camera>();
		mapCamera.orthographic = true;
		mapCamera.orthographicSize = viewingDistance;

		transform.rotation = Quaternion.Euler(90f, 0f, 0f);
	}

	private void LateUpdate()
	{
		if (followTransform == null)
			return;

		Vector3 targetPosition = new(
			followTransform.position.x,
			followTransform.position.y + height,
			followTransform.position.z
		);

		if (followSmoothness <= 0f)
		{
			transform.position = targetPosition;
		}
		else
		{
			float blend = 1f -
				Mathf.Exp(-followSmoothness * Time.deltaTime);

			transform.position = Vector3.Lerp(
				transform.position,
				targetPosition,
				blend
			);
		}
	}

	public void SetViewingDistance(float distance)
	{
		viewingDistance = Mathf.Max(5f, distance);

		if (mapCamera != null)
			mapCamera.orthographicSize = viewingDistance;
	}
}