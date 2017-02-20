using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;
using UnityEngine.SceneManagement;


public class CharacterController : MonoBehaviour {

    public float speed;
    public float comboTimeMin;
    public float comboTimeMax;
    public int punchDamage = 1;
    public int kickDamage = 1;
    public int upperCutDamage = 1;
    public int hurricaneKickDamage = 1;
    public int surasshuSlashDamage = 1;
    public Sprite A;
    public Sprite B;
    public Sprite Y;
    public Sprite X;
    public Sprite comboButtonFlashing;
    public SpriteRenderer[] combo = new SpriteRenderer[5];

    private Animator anim;
    private PlayerAction characterActions;
    private GameObject enemy;
    private Rigidbody2D myRigidbody;
    private Vector2 lastMove;
    private static float lastHitTime;
    private static float timeSinceLastHit;
    private static bool playerMoving;
    private static bool playerAttacking;
    private static bool comboTiming;
    private static bool hasAlreadyAttacked;
    private static bool pressed;
    private static bool comboSuccess;
    private static bool hasAttackAnimationPlayed;
    private static string attackString;
    private static int attackDamage;

    public static List<string> comboTracker;
    private static List<string[][]> refindComboList;
    private static string[][][] comboList;

    //audio
    public AudioClip[] attackSounds;	 
    public AudioSource audio;

    public AudioSource SurasshuSlash;
    public AudioSource SpinKick;
    public AudioSource UpperCut;




    void Start() {
        characterActions = new PlayerAction();

        characterActions.Punch.AddDefaultBinding(InputControlType.Action1);
        characterActions.Punch.AddDefaultBinding(Key.Z);

        characterActions.Kick.AddDefaultBinding(InputControlType.Action2);
        characterActions.Kick.AddDefaultBinding(Key.X);

        characterActions.Blank.AddDefaultBinding(InputControlType.Action3);
        characterActions.Blank.AddDefaultBinding(Key.C);

        characterActions.Roll.AddDefaultBinding(InputControlType.Action4);
        characterActions.Roll.AddDefaultBinding(Key.V);

        anim = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody2D>();


        enemy = null;
        lastHitTime = 1f;
        attackDamage = 0;
        attackString = "";
        playerMoving = false;
        playerAttacking = false;
        pressed = false;        
        hasAlreadyAttacked = false;
        comboSuccess = false;
        hasAttackAnimationPlayed = false;


        comboTracker = new List<string>();
        refindComboList = new List<string[][]>();
        comboList = new string[][][] {
            new string[][] { new string[] { "Punch", "Punch", "Kick" }, new string[] { "UpperCut" }},
            new string[][] { new string[] {"Kick", "Kick", "Punch" }, new string[] { "HurricaneKick" }},
            new string[][] { new string[] {"Kick", "Punch", "Punch", "Kick" }, new string[] { "SurasshuSlash" }},
        };

        //audio

        int ran = UnityEngine.Random.Range (0, 1);
         audio.clip = attackSounds [ran]; 
    }

    void OnTriggerEnter2D(Collider2D other) {
        //Sets enemy gameObject to the enemy object if the attack collider hits an enemy
        if (attackString != "" && enemy == null && other.tag == "EnemyHitBox")
        {
            GameObject temp = other.gameObject;
            enemy = other.gameObject.transform.parent.gameObject;
        }

        if (attackString != "" && enemy == null && other.tag == "PunchingBagHitBox")
        {
            Training.animate(true);
            //Training.training();
        }
    }

    void Update() {
        if (!GameMaster.pause)
        {
            timeSinceLastHit = Time.time - lastHitTime;
            PerformMovement();
        }
        checkTraining();
    }

    private static void checkTraining()
    {
        Scene scene = SceneManager.GetActiveScene();
        Training.setTrainingMode(scene.name == "LVL3-Training");
    }

    private void PerformMovement()
    {
        //playerMoving = true if there is horizontal or vertical movement and the player is not attacking, otherwise false.
        playerMoving = Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0 || playerAttacking  ? false : true;

        if (attackString == "HurricaneKick")
            playerMoving = true;
        
        //playerAttacking = false once the attack antimation has been played.
        //Stops the player from moving whilst attacking.
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(attackString) && timeSinceLastHit >= comboTimeMin)
            playerAttacking = false; 
        else
            myRigidbody.velocity = new Vector2(0f, 0f);

        //Setting up the movement variables
        bool setH = Input.GetAxisRaw("Horizontal") > 0.1 || Input.GetAxisRaw("Horizontal") < -0.1;
        bool setV = Input.GetAxisRaw("Vertical") > 0.1 || Input.GetAxisRaw("Vertical") < -0.1;
        int h = 0;
        int v = 0;
        if (setH)
            h = Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;
        if (setV)
            v = Input.GetAxisRaw("Vertical") > 0 ? 1 : -1;

        //Sets the diagonal speed to 5 and the horizontal and vertical speed to 6
        float movementspeed;
        if (h != 0 && v != 0)
            movementspeed = speed * .7f;
        else
            movementspeed = speed;

        if (!playerAttacking || attackString == "HurricaneKick")
        {
            if (comboTracker != null && comboTracker.Count != 0 && Training.getTrainingMode())
            {
                combo[comboTracker.Count].GetComponent<SpriteRenderer>().sprite = comboButtonFlashing;
                combo[comboTracker.Count].GetComponent<SpriteRenderer>().enabled = true;
            }

            //--movement
            if (h == 1 || h == -1)
            { 
                myRigidbody.velocity = new Vector2(h * movementspeed, myRigidbody.velocity.y);
                lastMove = new Vector2(h, v);
            }
            if (v == 1 || v == -1)
            {
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, v * movementspeed);
                lastMove = new Vector2(h, v);
            }

            //Stops the player from sliding
            if (Input.GetAxisRaw("Horizontal") < 0.1f && Input.GetAxisRaw("Horizontal") > -0.1f)
            {
                myRigidbody.velocity = new Vector2(0f, myRigidbody.velocity.y);
            }
            if (Input.GetAxisRaw("Vertical") < 0.1f && Input.GetAxisRaw("Vertical") > -0.1f)
            {
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, 0f);
            }

            //Reset the comboUI
            if (comboSuccess == true)
            {
                comboSuccess = false;
                comboReset();
                displayComboUIReset();
            }

            //Calls the Action method which deals with attacks
            if(attackString == "")
                Action();
        }
        if(comboTracker != null || comboTracker.Count > 0)
            comboCheck();

        anim.SetFloat("MoveX", h);
        anim.SetFloat("MoveY", v);

        anim.SetFloat("LastMoveX", lastMove.x);
        anim.SetFloat("LastMoveY", lastMove.y);
        //Plays movement animation is playerMoving is true
        anim.SetBool("PlayerMoving", playerMoving);

        //Plays attacking animation and checks if the attack is hitting an enemy
        if (playerAttacking && attackString != "")
        {
            //Plays the attack animation only once
            if (!hasAttackAnimationPlayed)
            {

                //attacking sounds
                int rand = UnityEngine.Random.Range(0, attackSounds.Length);
                audio.clip = attackSounds[rand];
                audio.Play();
                //attacking animation
                anim.SetBool(attackString, playerAttacking);
                hasAttackAnimationPlayed = true;
            }

            //If an attack collies with an enemy calls a damage method on that enemy
            if (enemy != null)
            {
                enemy.SendMessage("EnemyBeenHit",attackDamage);
                enemy = null;
            }
        }
        else if (attackString != "")
        {
            if (Training.getTrainingMode())
                Training.animate(false);
            //Attack animation is set to false and attacking variables are reset
            anim.SetBool(attackString, playerAttacking);
            attackString = "";
            TextCanvas.setText("");
            attackDamage = 0;
            hasAttackAnimationPlayed = false;
        }
    }

    private void Action() {
        //If condtions are ture means you still have time to complete a combo
        comboTiming =  timeSinceLastHit <= comboTimeMax;

        //Resest combo if combo has run out of time or you have successfuly completed a combo
        if (comboTracker != null && comboTracker.Count >= 5 || !comboTiming)
        {
            comboReset();
            displayComboUIReset();
        }

        //Need to add || characterActions.Roll || characterActions.Blank once other buttons are added
        if (characterActions.Kick || characterActions.Punch)
        {
            //Can only attack if its your first attack or its been longer than .3 sec after you last attack
            //Only does this once every button push
            if (!pressed && timeSinceLastHit >= comboTimeMin || !hasAlreadyAttacked)
            {
                pressed = true;
                playerAttacking = true;
                hasAlreadyAttacked = true;


                if (characterActions.Punch)
                {
                    attackString = "PlayerPunching";
                    attackDamage = punchDamage;
                    lastHitTime = Time.time;
                    comboTracker.Add("Punch");
                } 
                else if (characterActions.Kick)
                {
                    attackString = "PlayerKicking";
                    attackDamage = kickDamage;
                    lastHitTime = Time.time;
                    comboTracker.Add("Kick");
                } 
                //Other buttons to enable DO NOT REMOVE!
                /*else if (characterActions.Blank)
                {
                    lastHitTime = Time.time;
                    comboTracker.Add("Blank");
                } 
                else if (characterActions.Roll)
                {
                    lastHitTime = Time.time;
                    comboTracker.Add("Blank");
                }*/
            }
        } 
        else
            pressed = false;
    }

    private void comboCheck()
    {
        List<string[][]> tempComboList = new List<string[][]>();
        
        //Adds the first move in a combo to the refindComboList
        if (comboTracker.Count == 1)
        {
            foreach (string[][] moveSet in comboList)
            {
                if (moveSet[0][0] == comboTracker[0])
                {
                    refindComboList.Add(moveSet);
                }
            }
        }
        //Adds all other moves in a combo to the refindComboList
        else if (comboTracker.Count > 1)
        {
            foreach (string[][] moveSet in refindComboList)
            {
                bool addMoveSet = true;
                for (int i = 0; i < comboTracker.Count; i++)
                {
                    if (moveSet[0][i] != comboTracker[i])
                        addMoveSet = false;
                }
                if (addMoveSet)
                {
                    if (moveSet[0].Length == comboTracker.Count)
                    {
                        comboSuccess = true;
                        attackString = moveSet[1][0];
                        if (moveSet[1][0] == "HurricaneKick")
                        {
                           SpinKick.GetComponent<AudioSource>().Play();

                            attackDamage = hurricaneKickDamage;
                            TextCanvas.setText("Hurricane Kick");
                            if (Training.getTrainingMode())
                                Training.training("Hurricane Kick");
                        }
                        else if (moveSet[1][0] == "UpperCut")
                        {
                            UpperCut.GetComponent<AudioSource>().Play();
                            attackDamage = upperCutDamage;
                            TextCanvas.setText("Upper Cut");
                            if (Training.getTrainingMode())
                                Training.training("Upper Cut");
                        }
                        else if (moveSet[1][0] == "SurasshuSlash")
                        {

                          
                            SurasshuSlash.GetComponent<AudioSource>().Play();

                            attackDamage = surasshuSlashDamage;
                            TextCanvas.setText("Surasshu Slash");
                            
                           

                            if (Training.getTrainingMode())
                                Training.training("Surasshu Slash");
                        }
                    } else
                        tempComboList.Add(moveSet);
                }
            }
            if (tempComboList == null || tempComboList.Count == 0 && !comboSuccess)
            {
                comboTracker.RemoveAt(0);
                foreach (string[][] moveSet in comboList)
                {
                    bool addMove = true;
                    for (int i = 0; i < comboTracker.Count; i++)
                    {
                        if (moveSet[0][i] != comboTracker[i])
                            addMove = false;
                    }
                    if (addMove)
                        tempComboList.Add(moveSet);
                }
            }
            displayComboUIReset();
            refindComboList = tempComboList;
        }
    
        //Fail combo, reset combo variables
        if ((refindComboList == null || refindComboList.Count == 0) && !comboSuccess)
        {
            comboReset();
            displayComboUIReset();
        }
        displayComboUI();
    }

    private void comboReset()
    {
        //Resets all the combo variables
        comboTiming = false;
        comboTracker = new List<string>();
    }

    private void displayComboUI()
    {
        //Displays as sprites all keys pressed in the current combo
        if (comboTracker.Count > 0 && comboTracker.Count <= 5)
        {
            combo[comboTracker.Count - 1].GetComponent<SpriteRenderer>().enabled = true;

            if (comboTracker[comboTracker.Count - 1] == "Punch")
                combo[comboTracker.Count - 1].GetComponent<SpriteRenderer>().sprite = A;
            else if (comboTracker[comboTracker.Count - 1] == "Kick")
                combo[comboTracker.Count - 1].GetComponent<SpriteRenderer>().sprite = B;
            //Other buttons to enable DO NOT REMOVE!
            /*else if (comboTracker[comboTracker.Count - 1] == "Roll")
                combo[comboTracker.Count - 1].GetComponent<SpriteRenderer>().sprite = Y;
            else if (comboTracker[comboTracker.Count - 1] == "Blank")
                combo[comboTracker.Count - 1].GetComponent<SpriteRenderer>().sprite = X;
            */
        }
    }

    public float getPlayerAngle() {
        Vector2 v = lastMove.normalized;
        // multiply by lastMove.x for negative angles
        float angle = lastMove.x * Vector2.Angle(Vector2.up, v);
        return angle;
    }

    private void displayComboUIReset()
    {
        //Resets all the combo UI variables 
        for (int i = 0; i < 5; i++)
        {
            if (i >= comboTracker.Count)
            {
                if (Training.getTrainingMode())
                {
                    if (i > comboTracker.Count || i == 0)
                        combo[i].GetComponent<SpriteRenderer>().enabled = false;
                }
                else
                    combo[i].GetComponent<SpriteRenderer>().enabled = false;


            }
        }
    }

    /*
     * @Return - returns the name of the current attack
     */
    public static string getAttack()
    {
        return attackString;
    }

    public static bool isAttacking()
    {
        return playerAttacking;
    }

    public void DoDamage()
    {
        //Needed for ai.cs
    }

    public void KickPlayer()
    {
        //Needed for ai.cs
    }
}
