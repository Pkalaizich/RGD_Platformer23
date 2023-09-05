using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
                //DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    #endregion
    public bool gameIsActive { get; private set; }

    void Start()
    {
        GameplayEvents.OnGameEnded.AddListener(EndGame);
    }
    public void StartGame()
    {
        gameIsActive= true;
        GameplayEvents.OnGameStarted?.Invoke();
    }

    public void EndGame()
    {
        gameIsActive= false;
        StartCoroutine(RestartGame());
    }
    

   
    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2); 
        SceneManager.LoadScene(0);
    }

}
