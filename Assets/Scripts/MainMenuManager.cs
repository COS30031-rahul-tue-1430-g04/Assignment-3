using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
	public GameObject howToPlayPanel;

	void OnValidate()
	{
		if (howToPlayPanel == null)
			Debug.LogError("MainMenuManager: howToPlayPanel is not assigned.");
	}

	void Start() => GameManager.Instance.CurrentState = GameManager.GameState.Menu;

	public void PlayGame() => SceneManager.LoadScene(2);
	public void ShowHowToPlay() => howToPlayPanel.SetActive(true);
	public void HideHowToPlay() => howToPlayPanel.SetActive(false);
	public void QuitGame() => Application.Quit();
}
