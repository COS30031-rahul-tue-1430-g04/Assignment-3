using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
	[Header("References")]
	public Camera playerCamera;
	public InteractionUI interactionUI;

	[Header("Interaction Settings")]
	public float interactionDistance = 3f;
	public LayerMask interactableLayers = ~0;

	private InteractableAsset currentTarget;

	private void Awake()
	{
		if (playerCamera == null)
			playerCamera = Camera.main;
	}

	private void Update()
	{
		if (interactionUI == null)
			return;

		if (interactionUI.IsQuestionOpen)
			return;

		FindTarget();

		if (currentTarget != null &&
			Keyboard.current != null &&
			Keyboard.current.eKey.wasPressedThisFrame)
		{
			InteractWithTarget();
		}
	}

	private void FindTarget()
	{
		currentTarget = null;

		if (playerCamera == null)
		{
			interactionUI.ShowPrompt(false);
			return;
		}

		Ray ray = playerCamera.ViewportPointToRay(
			new Vector3(0.5f, 0.5f, 0f)
		);

		if (Physics.Raycast(
			ray,
			out RaycastHit hit,
			interactionDistance,
			interactableLayers,
			QueryTriggerInteraction.Ignore))
		{
			InteractableAsset asset =
				hit.collider.GetComponentInParent<InteractableAsset>();

			if (asset != null && asset.CanInteract)
			{
				currentTarget = asset;
			}
		}

		interactionUI.ShowPrompt(currentTarget != null);
	}

	private void InteractWithTarget()
	{
		if (currentTarget == null ||
			!currentTarget.CanInteract)
			return;

		InteractableAsset selectedAsset = currentTarget;

		// Prevent repeated interaction with the same asset.
		selectedAsset.MarkCompleted();

		currentTarget = null;

		// Pass the object directly to the UI.
		interactionUI.OpenQuestion(selectedAsset);
	}
}