using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public class Interactor : MonoBehaviour
{
	public float interactionRadius = 5f;

	private SphereCollider _triggureCollider;
	private SphereCollider TriggerCollider
	{
		get
		{
			if (_triggureCollider == null && !TryGetComponent(out _triggureCollider))
				Debug.LogError("SphereCollider not found on Interactor.");
			return _triggureCollider;
		}
	}



	private readonly Dictionary<Transform, Action<GameObject>> interactables = new();
	public void Interact() => interactables
			.OrderBy(kvp => Vector3.Distance(transform.position, kvp.Key.position))
			.FirstOrDefault()
			.Value?.Invoke(gameObject);
	public void RegisterInteractable(Action<GameObject> interactAction, Transform interactableTransform) =>
		interactables[interactableTransform] = interactAction;
	public void UnregisterInteractable(Transform interactableTransform) =>
		interactables.Remove(interactableTransform);

	private void OnValidate()
	{
		TriggerCollider.radius = interactionRadius;
	}
}
