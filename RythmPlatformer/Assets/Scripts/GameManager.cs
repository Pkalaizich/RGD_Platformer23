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
        GameplayEvents.OnGameWon.AddListener(WonGame);
    }
    public void StartGame()
    {
        gameIsActive= true;
        GameplayEvents.OnGameStarted?.Invoke();
    }

    public void EndGame()
    {
        gameIsActive= false;
        StartCoroutine(RestartGame(2f));
    }
    
    public void WonGame()
    {
        gameIsActive = false;
        StartCoroutine(RestartGameAfterWin(4f));
    }

   
    private IEnumerator RestartGame(float waitTime)
    {
        yield return new WaitForSeconds(waitTime); 
        SceneManager.LoadScene(0);
    }
    private IEnumerator RestartGameAfterWin(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        FindObjectOfType<MenusUI>().EndAnimation();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(0);
    }

}
