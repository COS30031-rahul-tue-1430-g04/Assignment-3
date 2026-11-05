using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MiniMapCameraFollow : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Transform followTransform;

	[Header("Map Settings")]
	[SerializeField, Min(5f)] private float height = 30f;
	[SerializeField, Min(5f)] private float viewingDistance = 100f;
	[SerializeField, Min(0f)] private float catchUpTime = 0.3f;

	private float responsiveness;
	private Camera mapCamera;
	private Camera MapCamera
	{
		get
		{
			if (mapCamera == null) mapCamera = GetComponent<Camera>();
			return mapCamera;
		}
	}

	private Vector3 TargetPos => followTransform.position + Vector3.up * height;
	private Action updateAction;
	private void OnValidate()
	{
		MapCamera.orthographicSize = viewingDistance;
		MapCamera.orthographic = true;
		transform.rotation = Quaternion.Euler(90f, 0f, 0f);

		if (followTransform == null)
			updateAction = null;
		else if (catchUpTime <= 0f)
			updateAction = SnapFollow;
		else
		{
			responsiveness = 1f / catchUpTime;
			updateAction = SmoothFollow;
		}
	}
	private void SnapFollow() => transform.position = TargetPos;
	private void SmoothFollow()
	{
		float blend = 1f - Mathf.Exp(-responsiveness * Time.deltaTime);
		transform.position = Vector3.Lerp(transform.position, TargetPos, blend);
	}
	private void LateUpdate() => updateAction?.Invoke();
}