using InControl;
using UnityEngine;
using Fungus;
using UnityEngine.SceneManagement;

public class SkipMenu : MonoBehaviour {
    public Fungus.Flowchart flowChart;
    private PlayerAction characterActions;
    // Use this for initialization
    void Start () {
        characterActions = new PlayerAction();

        characterActions.Skip.AddDefaultBinding(InputControlType.Action1);
        characterActions.Skip.AddDefaultBinding(Key.Z);
        characterActions.Skip.AddDefaultBinding(InputControlType.Action2);
        characterActions.Skip.AddDefaultBinding(Key.X);
        characterActions.Skip.AddDefaultBinding(InputControlType.Command);
        characterActions.Skip.AddDefaultBinding(Key.Escape);
    }
	
	// Update is called once per frame
	void Update () {
        if (characterActions.Skip)
        {
            flowChart.StopAllCoroutines();
            flowChart.StopAllBlocks();
            var musicManager = FungusManager.Instance.MusicManager;
            musicManager.StopMusic();
            Loading.loadLevel("Start Menu");
        }
    }
}
