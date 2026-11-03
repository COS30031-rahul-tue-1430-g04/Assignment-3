using System;
using UnityEngine;
[RequireComponent(typeof(KeepAlive))]
public class WebGLRouter : MonoBehaviour
{
	public static WebGLRouter Instance { get; private set; }

	void Awake() => Instance = this;

	public event Action OnPointerLock;
	public event Action OnPointerUnlock;
	public event Action<float, float> OnMouseMove;
	public bool IsPointerLocked { get; private set; } = false;

	public void PointerLocked()
	{
		if (IsPointerLocked) return;
		IsPointerLocked = true;
		OnPointerLock?.Invoke();
	}

	public void PointerUnlocked()
	{
		if (!IsPointerLocked) return;
		IsPointerLocked = false;
		OnPointerUnlock?.Invoke();
	}

	public void MouseMove(string coords)
	{
		string[] parts = coords.Split(',');
		float x = float.Parse(parts[0]);
		float y = float.Parse(parts[1]);
		OnMouseMove?.Invoke(x, y);
	}
}