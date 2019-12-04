using UnityEngine;
using UnityEngine.SceneManagement;

public class Transitioner : MonoBehaviour
{
    private void Start()
    {
        SceneLoader.Instance.Show(SceneManager.LoadSceneAsync("Main"));
    }

}