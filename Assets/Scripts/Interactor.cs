using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Interactor : MonoBehaviour
{
	public InputActionReference interactAction;
	public float interactionRadius = 5f;
	private int inRangeLayer;
	private int interactableLayer;
	private int uninteractableLayer;
	private int InteractableLayerMask => 1 << interactableLayer | 1 << inRangeLayer;
	private Interactable _currentInteractable = null;
	private Interactable CurrentInteractable
	{
		get => _currentInteractable;
		set
		{
			if (_currentInteractable != value)
			{
				if (_currentInteractable != null)
					_currentInteractable.gameObject.layer =
						_currentInteractable.CanInteract ? interactableLayer : uninteractableLayer;
				if (value != null)
					value.gameObject.layer = inRangeLayer;
				_currentInteractable = value;
			}
		}
	}

	private void Interact(CallbackContext _) => Interact();
	private void Interact()
	{
		if (CurrentInteractable != null)
			CurrentInteractable.Interact(gameObject);
	}

	private void OnEnable()
	{
		interactAction.action.performed += Interact;
		interactableLayer = LayerMask.NameToLayer("Interactable");
		inRangeLayer = LayerMask.NameToLayer("InRangeInteractable");
		uninteractableLayer = LayerMask.NameToLayer("Uninteractable");
	}
	private void OnDisable() => interactAction.action.performed -= Interact;

	private void Update()
	{
		CurrentInteractable = null;
		if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactionRadius, InteractableLayerMask))
		{
			Interactable hitInteractable = hit.collider.GetComponent<Interactable>();
			if (hitInteractable != null && hitInteractable.CanInteract)
				CurrentInteractable = hitInteractable;
		}
	}
}
