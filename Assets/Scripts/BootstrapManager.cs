using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene(1);
	}
}
