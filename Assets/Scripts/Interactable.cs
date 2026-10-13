using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
	public abstract void Interact(GameObject interactor);
	public bool CanInteract { get; protected set; } = true;
	private void OnEnable() =>
		gameObject.layer = LayerMask.NameToLayer("Interactable");
}
