using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;      //1Tells Random to use the Unity Engine random number generator.

public class SumoAI : MonoBehaviour
{
    // public settings here
    public int HP = 10;
    public float timeBeforeChase = 3f;
    public int damage = 4;
    public float speed = 4f;
    public Text saySomething;
    public Image speechBubble;
    //Audio
    public AudioClip[] painSounds;
    public AudioSource audioE;
    // Blood effect
    public GameObject bloodPrefab;

    // private variables starts here
    private bool isChasing = false;
    private bool isTired = false;
    private bool isFirstTimeMeet = true;
    private bool isSaying = false;
    private GameObject playerCollider;
    private Vector2 playerPosition;
    private int walkState = Animator.StringToHash("Base Layer.walk");
    private int roarState = Animator.StringToHash("Base Layer.roar");
    private int tiredState = Animator.StringToHash("Base Layer.tired");
    private int punchState = Animator.StringToHash("Base Layer.punch");
    private int idleState = Animator.StringToHash("Base Layer.flex");
    private int beenHitState = Animator.StringToHash("Base Layer.beenHit");
    private int entryState = Animator.StringToHash("Base Layer.entry");
    private bool hasAttacked;
    private Animator animator;
    private NavMeshAgent2D enemy;
    private Transform player;
    private AnimatorStateInfo currentBaseState;
    private string[] tiredList = new string[] {
        "Stay still so I can kill you",
        "You so skinny your mom enjoyed your birth",
        "I break you, you skinny twig",
        
        "you can't escape me",
        
        "this is to much exercise"
        
    };
    private enum AnimationParams
    {
        isWalk, isPunch, isHit, isIdle, isTired, isRoar
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<NavMeshAgent2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemy.speed = speed;
        if (audioE == null)
        {
            audioE = GetComponentInChildren<AudioSource>();
        }
    }

    void Start()
    {
        saySomething.GetComponent<Text>().enabled = false;
        speechBubble.GetComponent<Image>().enabled = false;
    }

    void Update()
    {

        currentBaseState = animator.GetCurrentAnimatorStateInfo(0);
        float distance = Vector2.Distance(transform.position, player.position);

        /**************************************************/
        /* A Simple State Machine Management starts here */
        /************************************************/
        if (currentBaseState.fullPathHash.Equals(walkState))
        {
            hasAttacked = false;
            if (isChasing)
            {
                enemy.Resume();
                setEnemyDirection();
                isChasing = false;
            }

            // I have reached the previous player position or I have catched the player
            if (enemy.remainingDistance.Equals(0))
            {
                setToThisAnimation(AnimationParams.isPunch);
            }
        }
        else if (currentBaseState.fullPathHash.Equals(punchState))
        {
            if (!hasAttacked)
            {
                hasAttacked = true;
                enemy.Stop();
                float pDistance = Vector2.Distance(transform.position, player.position);



                int attack = 2;
                audioE.clip = painSounds[attack];
                audioE.Play();

                if (pDistance <= 1.5f)
                {
                    setEnemyDirection();
                    PlayerHealth.doDamage(damage, this.transform.position);
                    playerCollider = null;
                }
            }
            setToThisAnimation(AnimationParams.isTired);
        }
        else if (currentBaseState.fullPathHash.Equals(tiredState))
        {
            isTired = true;
            int quoteIndex = Random.Range(0, tiredList.Length);
            if (!isSaying)
            {
                isSaying = true;
                StartCoroutine(SaySomethingWhenTired(tiredList[quoteIndex]));
            }
            StartCoroutine(TimePause(timeBeforeChase));
        }
        else if (currentBaseState.fullPathHash.Equals(beenHitState))
        {
            // anything related to the beenHit state should locates here.
            animator.SetBool("isHit", false);
            isTired = true;
        }
        else if (currentBaseState.fullPathHash.Equals(roarState))
        {
            if (isFirstTimeMeet)
            {
                isTired = false;
                isFirstTimeMeet = false;
            }
            else
            {
                isTired = true;
            }
            StartCoroutine(TimePause(timeBeforeChase));
            StartToChase();
        }
        else if (currentBaseState.fullPathHash.Equals(idleState))
        {
            StartCoroutine(TimePause(timeBeforeChase));
            StartToChase();
        }
        else if (currentBaseState.fullPathHash.Equals(entryState))
        {
            if (distance <= 4f)
            {
                //say something
                StartCoroutine(SaySomethingFirstMeet());
                isFirstTimeMeet = false;
                enemy.Stop();
                setToThisAnimation(AnimationParams.isRoar);
                StartCoroutine(TimePause(timeBeforeChase * 1.5f));
            }
        }
    }

    IEnumerator SaySomethingFirstMeet()
    {
        saySomething.text = "At last you have come";
        saySomething.GetComponent<Text>().enabled = true;
        speechBubble.GetComponent<Image>().enabled = true;
        yield return new WaitForSeconds(1.5f);
        saySomething.text = "Prepare to die!!";
        yield return new WaitForSeconds(1.5f);
        saySomething.GetComponent<Text>().enabled = false;
        speechBubble.GetComponent<Image>().enabled = false;
    }

    IEnumerator SaySomethingWhenTired(string quote)
    {
        saySomething.GetComponent<Text>().enabled = false;
        speechBubble.GetComponent<Image>().enabled = false;
        saySomething.text = quote;
        saySomething.GetComponent<Text>().enabled = true;
        speechBubble.GetComponent<Image>().enabled = true;
        yield return new WaitForSeconds(1.5f);
        saySomething.GetComponent<Text>().enabled = false;
        speechBubble.GetComponent<Image>().enabled = false;
    }

    IEnumerator TimePause(float time1)
    {
        yield return new WaitForSeconds(time1);
    }
    void StartToChase()
    {
        isChasing = true;
        isTired = false;
        isSaying = false;
        playerPosition = player.position;
        setToThisAnimation(AnimationParams.isWalk);
        setEnemyDirection();
        enemy.destination = playerPosition;
    }

    public void EnemyBeenHit(int incomingDamage)
    {
        //if (currentBaseState.Equals(tiredState))
        if (isTired)
        {
            HP -= incomingDamage;
            showSomeBlood(incomingDamage);
            if (HP <= 0)
            {
                int death = 1;
                audioE.clip = painSounds[death];
                audioE.Play();

                Loading.loadLevel("bossQTE");
            }

            //play audio
            int general = 0;
            audioE.clip = painSounds[general];
            audioE.Play();



            setToThisAnimation(AnimationParams.isHit);
            setEnemyDirection();
        }
    }

    void setEnemyDirection()
    {
        // set the direction of the animationClips
        Vector2 pos = GetPlayerDirection(player, transform);
        animator.SetFloat("moveX", pos.x);
        animator.SetFloat("moveY", pos.y);
    }

    void setToThisAnimation(AnimationParams type)
    {
        Array values = Enum.GetValues(typeof(AnimationParams));
        foreach (AnimationParams val in values)
        {
            string name = Enum.GetName(typeof(AnimationParams), val);
            if (val.Equals(type)) { animator.SetBool(name, true); }
            else { animator.SetBool(name, false); }
        }
    }

    private Vector2 GetPlayerDirection(Transform player, Transform enemy)
    {
        Transform transform = enemy;
        float horizontal = player.position.x - transform.position.x;
        float vertical = player.position.y - transform.position.y;

        Vector2 pos = new Vector2(0, 0);
        float offset = 0.7f; //use to make the enemy not that sensetive to direction

        if (horizontal > offset)
        {
            pos.x = 1;
        }
        else if (horizontal < offset * -1)
        {
            pos.x = -1;
        }
        else if (horizontal >= offset * -1 && horizontal <= offset)
        {
            pos.x = 0;
        }

        if (vertical > offset)
        {
            pos.y = 1;
        }
        else if (vertical < offset * -1)
        {
            pos.y = -1;
        }
        else if (vertical >= offset * -1 && vertical <= offset)
        {
            pos.y = 0;
        }

        // if (enemyHP <= runAwayHP)
        // {
        //     pos.x *= -1;
        //     pos.y *= -1;
        // }

        return pos;
    }

    void showSomeBlood(int incomingdamage)
    {
        GameObject blood = Instantiate(bloodPrefab);
        // set blood position
        Vector3 bloodPos = this.transform.position;
        blood.transform.position = bloodPos;
        // set blood direction
        float playerAngle = player.gameObject.GetComponent<CharacterController>().getPlayerAngle();
        blood.GetComponent<BloodScript>().setBlood(playerAngle, (float)incomingdamage / 2f);
        // set blood damage text
        int incomingdam = ((incomingdamage * 100) + Random.Range(0, 100));
        blood.GetComponentInChildren<damageTextScr>().setDamage(incomingdam);
        Score.setDamage(incomingdam);
        Score.calcScore(CharacterController.getAttack());
        GameMaster.setScoretimer();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Equals("Player"))
        {
            playerCollider = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag.Equals("Player"))
        {
            playerCollider = null;
        }
    }
}
