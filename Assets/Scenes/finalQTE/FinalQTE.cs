using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using InControl;

[CommandInfo("QTE", "Add QTE to flow", "Quick Time Event System")]
public class FinalQTE : Command
{
    public string blockWhenFailed;
    public string blockWhenSuccess;

    [HideInInspector] public enum QTState { Ready, Delay, Ongoing, Done };
    [HideInInspector] public QTState qtState = QTState.Ready;
    // an enum for a possible response!
    [HideInInspector] public enum QTResponse { Null, Success, Fail };
    [HideInInspector] public QTResponse qtResponse = QTResponse.Null;
    //public KeyCode QTEButton = new KeyCode();
     public enum Action{
        punch, kick
    }
    public Action QTEButton;

    // How long should the event last?
    public float CountTimer = 2f;
    // Should we wait before the event starts?
    public float DelayTimer = 0f;

    public Flowchart flowChart;

    public GameObject ButtonDisplay;

    PlayerAction characterActions;

    // Use this for initialization
    void Start()
    {
        //qtState = QTState.Ready;
        characterActions = new PlayerAction();
        
        characterActions.Punch.AddDefaultBinding(InputControlType.Action1);
        characterActions.Punch.AddDefaultBinding(Key.Z);

        characterActions.Kick.AddDefaultBinding(InputControlType.Action2);
        characterActions.Kick.AddDefaultBinding(Key.X);
    }

    public override void OnEnter()
    {
        if (qtState == QTState.Ready)
        {
            StartCoroutine(StateChange());
        }
    }

    void Update()
    {
        if (qtState == QTState.Ongoing)
        {
            // succeed QTE (press right button)
            if (
                (QTEButton == Action.punch && characterActions.Punch) || 
                (QTEButton == Action.kick && characterActions.Kick)
            )
            {
                qtState = QTState.Done;
                qtResponse = QTResponse.Success;

                if (System.String.IsNullOrEmpty(blockWhenSuccess))
                {
                    Continue();
                }
                else
                {
                    flowChart.ExecuteBlock(blockWhenSuccess);
                }
            }

            // fail QTE (press wrong button)
            if (
                (QTEButton == Action.punch && characterActions.Kick) || 
                (QTEButton == Action.kick && characterActions.Punch)
            ) {
                qtState = QTState.Done;
                qtResponse = QTResponse.Fail;
                flowChart.ExecuteBlock(blockWhenFailed);

            }
        }
    }

    private IEnumerator StateChange()
    {
        qtState = QTState.Delay;

        // Wait for the Delay if any delay at all.
        yield return new WaitForSeconds(DelayTimer);

        // This line below is only for Debug Purposes
        //Debug.Log(QTEButton.ToString());

        // Shrink the QTE Button
        StartCoroutine(ScaleOverTime(CountTimer));

        qtState = QTState.Ongoing;
        yield return new WaitForSeconds(CountTimer);

        // If the timer is over and the event isn't over? Fix it! because most likely they failed.
        if (qtState == QTState.Ongoing)
        {
            qtResponse = QTResponse.Fail;
            qtState = QTState.Done;

            flowChart.ExecuteBlock(blockWhenFailed);
        }
    }

    IEnumerator ScaleOverTime(float time)
    {
        Vector3 originalScale = ButtonDisplay.transform.localScale;
        Vector3 destinationScale = new Vector3(0f, 0f, 0f);

        float currentTime = 0.0f;

        do
        {
            ButtonDisplay.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        GameObject.Destroy(ButtonDisplay);
    }

    // For 2D Colission
    void OnTriggerEnter2D(Collider2D other)
    {
        if (qtState == QTState.Ready)
        {
            StartCoroutine(StateChange());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (qtState == QTState.Done)
        {
            qtState = QTState.Ready;
            qtResponse = QTResponse.Null;
        }
    }
}
