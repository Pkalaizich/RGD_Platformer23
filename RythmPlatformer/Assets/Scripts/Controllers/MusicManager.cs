using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    #region Singleton
    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MusicManager>();
                //DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    #endregion
    [SerializeField] private AK.Wwise.Event musicEvent;
    [SerializeField] private AK.Wwise.Event testSfx;
    [SerializeField] private List<AK.Wwise.Event> soundEffects;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            musicEvent.Post(gameObject);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            testSfx.Post(gameObject);
        }
    }

    public void PlayMusic()
    {
        musicEvent.Post(gameObject);
    }

    public void PlaySound(int index)
    {
        //testSfx.Post(gameObject);
        soundEffects[index].Post(gameObject);
    }

    public enum AvailableSFX
    {
        Damage,
        Footstep,
        Jump,
        JumpLand,
        KickSwing,
        KickHit,
        WallGrab
    }
}
