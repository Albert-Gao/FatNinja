using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{

    static MusicPlayer INSTANCE = null;
    private static bool destroyObj = false;


    private Scene scene;




    void Awake()
    {
        if (INSTANCE != null)
        {
            Destroy(gameObject);
            print("Duplicate music player self-destructing!");
        }
        else
        {
            INSTANCE = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
       
    }

    void Update()
    {
        if (destroyObj)
        {
            Destroy(gameObject);
            destroyObj = false;
        }

        //Debug.Log(scene.name.ToString());
        scene = SceneManager.GetActiveScene();
        if (scene.name == "LVL4-SwordMiniBoss" || scene.name == "LVL8-FinalBoss") {
            Destroy(gameObject);
        }
    }

    public static void destroyMusic()
    {
        destroyObj = true;
    }
    // Use this for initialization
    void Start()
    {
        
        Debug.Log("Music player Start " + GetInstanceID());
    }

}

