using System;
using UnityEngine;
[RequireComponent(typeof(KeepAlive))]
public class WebGLRouter : MonoBehaviour
{
	public static WebGLRouter Instance { get; private set; }

	void Awake() => Instance = this;

	public event Action OnPointerLock;
	public event Action OnPointerUnlock;

	public void PointerLocked() => OnPointerLock?.Invoke();
	public void PointerUnlocked() => OnPointerUnlock?.Invoke();

}