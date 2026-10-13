using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
	private readonly List<Interactor> interactors = new();
	protected abstract void Interact(GameObject interactor);

	private bool InRange => interactors.Count > 0;
	private int interactableLayer;
	private int uninteractableLayer;
	private int inRangeLayer;
	private int TargetLayer => CanInteract ? (InRange ? inRangeLayer : interactableLayer) : uninteractableLayer;
	private bool _canInteract = true;
	protected bool CanInteract
	{
		get => _canInteract;
		set
		{
			_canInteract = value;
			gameObject.layer = TargetLayer;
			if (!value)
			{
				foreach (Interactor interactor in interactors)
					interactor.UnregisterInteractable(transform);
				interactors.Clear();
			}
		}
	}
	private void OnEnable()
	{
		interactableLayer = LayerMask.NameToLayer("Interactable");
		uninteractableLayer = LayerMask.NameToLayer("Uninteractable");
		inRangeLayer = LayerMask.NameToLayer("InRangeInteractable");
		gameObject.layer = TargetLayer;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(!CanInteract) return;
		if (other.TryGetComponent(out Interactor interactor))
		{
			interactor.RegisterInteractable(Interact, transform);
			interactors.Add(interactor);
			gameObject.layer = TargetLayer;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!CanInteract) return;
		if (other.TryGetComponent(out Interactor interactor))
		{
			interactor.UnregisterInteractable(transform);
			interactors.Remove(interactor);
			gameObject.layer = TargetLayer;
		}
	}
}
