using System;
using UnityEngine;
[RequireComponent(typeof(KeepAlive))]
public class WebGLRouter : MonoBehaviour
{
	public static WebGLRouter Instance { get; private set; }

	void Awake() => Instance = this;

	public event Action OnPointerLock;
	public event Action OnPointerUnlock;
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

}