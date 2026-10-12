using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTransitionManager : MonoBehaviour
{
	public LevelManager levelManager;

	[Header("Level Complete UI")]
	public GameObject levelCompletePanel;
	public CanvasGroup backgroundOverlay;
	public RectTransform panelTransform;

	[Header("Sound")]
	public AudioSource audioSource;
	public AudioClip levelCompleteClip;

	[Header("Fade Settings")]
	public float fadeDuration = 0.5f;

	bool readyToTransition = false;

	private bool CheckSerializedFields()
	{
		if (levelCompletePanel == null || backgroundOverlay == null || panelTransform == null ||
			audioSource == null || levelCompleteClip == null || levelManager == null)
		{
			Debug.LogError("LevelTransitionManager: One or more serialized fields are not assigned.");
			return false;
		}
		return true;
	}

	private void Awake()
	{
		if (!CheckSerializedFields()) return;

		levelManager.OnAllAssetsAnswered += () =>
		{
			readyToTransition = true;
			if (GameManager.Instance.CurrentState == GameManager.GameState.Gameplay)
			{
				GameManager.Instance.CurrentState = GameManager.GameState.Menu;
				ShowLevelComplete();
			}
		};
		GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
	}

	private void OnGameStateChanged(GameManager.GameState state)
	{
		if (!readyToTransition) return;
		if (state != GameManager.GameState.Gameplay) return;
		IEnumerator DeferedTask()
		{
			yield return null;
			GameManager.Instance.CurrentState = GameManager.GameState.Menu;
			ShowLevelComplete();
		}
		StartCoroutine(DeferedTask());

	}

	private void Start()
	{
		if (!CheckSerializedFields()) return;

		levelCompletePanel.SetActive(false);
		backgroundOverlay.alpha = 1f;
		StartCoroutine(FadeInFromBlack());
	}
	private IEnumerator FadeInFromBlack()
	{
		float time = 0f;
		while (time < fadeDuration)
		{
			time += Time.deltaTime;
			float progress = time / fadeDuration;
			backgroundOverlay.alpha = Mathf.Lerp(1f, 0f, progress);
			yield return null;
		}

		backgroundOverlay.alpha = 0f;
	}
	public void ShowLevelComplete()
	{
		if (!CheckSerializedFields()) return;
		
		levelCompletePanel.SetActive(true);
		panelTransform.localScale = Vector3.zero;
		audioSource.PlayOneShot(levelCompleteClip);

		StartCoroutine(AnimateLevelComplete());
	}
	private IEnumerator AnimateLevelComplete()
	{		
		if (!CheckSerializedFields()) yield break;

		float duration = 1.5f;

		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			float progress = time / duration;
			backgroundOverlay.alpha = Mathf.Lerp(0f, 0.75f, progress);
			float scale = Mathf.Lerp(0.8f, 1f, progress);
			panelTransform.localScale = new Vector3(scale, scale, 1f);
			yield return null;
		}

		backgroundOverlay.alpha = 0.75f;
		panelTransform.localScale = Vector3.one;
	}

	private int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;
	public void RestartLevel() =>
		StartCoroutine(FadeAndLoad(CurrentSceneIndex));
	public void LoadNextLevel() =>
		StartCoroutine(FadeAndLoad(CurrentSceneIndex + 1));

	private IEnumerator FadeAndLoad(int sceneIndex)
	{
		if (!CheckSerializedFields()) yield break;
		levelCompletePanel.SetActive(false);

		float startAlpha = backgroundOverlay.alpha;

		float time = 0f;
		while (time < fadeDuration)
		{
			time += Time.deltaTime;
			float progress = time / fadeDuration;
			backgroundOverlay.alpha = Mathf.Lerp(startAlpha, 1f, progress);
			yield return null;
		}

		backgroundOverlay.alpha = 1f;

		SceneManager.LoadScene(sceneIndex);
	}
}