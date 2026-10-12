using UnityEngine;

public class MiniMapPlayerMarker : MonoBehaviour
{
	[SerializeField] private Transform player;

	private void LateUpdate()
	{
		if (player == null)
			return;

		float heading = player.eulerAngles.y;

		transform.localRotation =
			Quaternion.Euler(0f, 0f, -heading);
	}
}