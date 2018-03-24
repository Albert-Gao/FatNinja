using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;      //1Tells Random to use the Unity Engine random number generator.

public class EnemyAI : MonoBehaviour
{
    public GameObject enemyPrefab;
    public bool isDead = false;
    public bool isSit = false;
    public float sitTime = 2f;
    public int enemyHP = 10;
    public int damage = 2;
    public float enemySpeed = 2f;
    //public AudioSource hurtMeSound;
    public int runAwayHP = 3;
    public float randomWalkRange = 1f; //when enemy do a random action selection near player, how far should he go.
    public float checkDistance = 1.5f; // The distance between enemy and player, when real distance is smaller, enemy will start to walk.
    public int chanceToAttack = 4; // min 0, max 10;

    public float lineOfSight = 10.0f;

    public float chanceToBeStunned = 0.5f;//Chance to be stunned between 0 and 1;

    //Audio
    public AudioClip[] painSounds;
    public AudioSource audioE;

    // Blood effect
    public GameObject bloodPrefab;

    private float lastHitTime;
    private float timeSinceLastHit;

    private bool caughtPlayer = false;
    private bool attacking = false;
    private Animator animator;
    private Transform player;
    private NavMeshAgent2D enemy;
    private bool BeenHit = false;

    private bool isRandomMove;
    private Transform playerCollider;
    private bool[] formerStatus; //1-move; 2-kick;
    private Vector3 playerLastPosition;
    private float timeSinceRanAway;

    //audio
    public AudioClip[] attackSounds;
    public AudioSource audio;

    

   


    private enum AnimationParams
    {
        PlayerMoving,
        EnemyWalking,
        PlayerKicking,
        hitme
    }

    void Start()
    {
        lastHitTime = 1f;
        animator = GetComponent<Animator>();
        enemy = GetComponent<NavMeshAgent2D>();
        animator.SetBool("PlayerKicking", false);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        formerStatus = new bool[3];
        playerCollider = null;

        if (isSit)
        {
            animator.SetBool("IsSit", true);
        }

        if (audioE == null)
        {
            audioE = GetComponentInChildren<AudioSource>();
        }

        if (lineOfSight == 0) {
            lineOfSight = 10.0f;
        }

        ApplyAnimationEventToKickAnimation(CreateAnimationEvent());
        //enemy.destination = player.position;
    }

    private AnimationEvent CreateAnimationEvent()
    {
        // new event created
        return new AnimationEvent()
        {
            time = 0.06f,
            functionName = "KickPlayer"
        };
    }

    private void ApplyAnimationEventToKickAnimation(AnimationEvent evt)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            string name = clip.name;
            if (name.StartsWith("Kick") || name.StartsWith("sword-slash"))
            {
                bool isAdded = false;
                foreach (AnimationEvent e in clip.events)
                {
                    if (e.functionName == evt.functionName)
                    {
                        isAdded = true;
                    }
                }
                if (!isAdded)
                {
                    clip.AddEvent(evt);
                }
            }
        }
    }

    public void EnemyRestoreFromHit()
    {
        animator.SetBool("PlayerMoving", formerStatus[0]);
        animator.SetBool("PlayerKicking", formerStatus[1]);
        animator.SetBool("EnemyWalking", formerStatus[2]);
        animator.SetBool("hitme", false);
        BeenHit = false;
    }

    public void EnemyBeenHit(int incomingdamage)
    {
        if (!isDead)
        {
            BeenHit = true;
            //hurtMeSound.Play();
            enemyHP -= incomingdamage;
            if (Random.Range(0f, 1f) < chanceToBeStunned || CharacterController.getAttack() == "UpperCut")
            {
                SetEnemyAnimation(AnimationParams.hitme);
                formerStatus[0] = animator.GetBool("PlayerMoving");
                formerStatus[1] = animator.GetBool("PlayerKicking");
                formerStatus[2] = animator.GetBool("EnemyWalking");
            }

            //stop any current action

            //audio
            int rand = UnityEngine.Random.Range(0, painSounds.Length);
            audioE.clip = painSounds[rand];
            audioE.Play();

            // spawn blood
            GameObject blood = Instantiate(bloodPrefab);
            blood.transform.position = this.transform.position;
            float playerAngle = player.gameObject.GetComponent<CharacterController>().getPlayerAngle();
            blood.GetComponent<BloodScript>().setBlood(playerAngle, (float)incomingdamage / 2f);
            int incomingdam = ((incomingdamage * 100) + Random.Range(0, 100));
            blood.GetComponentInChildren<damageTextScr>().setDamage(incomingdam);
            Score.setDamage(incomingdam);
            Score.calcScore(CharacterController.getAttack());
            GameMaster.setScoretimer();

            if (enemyHP <= 0) { EnemyDead(); }
        }
    }

    void SetEnemyAnimation(AnimationParams type)
    {
        string targetAnimation = Enum.GetName(typeof(AnimationParams), type);
        Array values = Enum.GetValues(typeof(AnimationParams));
        foreach (AnimationParams val in values)
        {
            string name = Enum.GetName(typeof(AnimationParams), val);
            if (val.Equals(type))
            {
                animator.SetBool(name, true);
            }
            else
            {
                animator.SetBool(name, false);
            }
        }

        Vector2 pos = GetPlayerDirection(player, transform);
        if (targetAnimation.Equals("PlayerKicking"))
        {
            lastHitTime = Time.time;
            attacking = true;
            animator.SetFloat("LastMoveX", pos.x);
            animator.SetFloat("LastMoveY", pos.y);
        }
        else
        {
            if (enemyHP <= runAwayHP)
            {
                animator.SetFloat("MoveX", -1 * pos.x);
                animator.SetFloat("MoveY", -1 * pos.y);
            }
            else
            {
                animator.SetFloat("MoveX", pos.x);
                animator.SetFloat("MoveY", pos.y);
            }
        }
    }

    void StartToAttack()
    {
        if (!BeenHit)
        {
            enemy.isStopped = true;
            SetEnemyAnimation(AnimationParams.PlayerKicking);

            //attacking sounds
            if (attackSounds != null && attackSounds.Length > 0 && audio != null)
            {
                int ran = UnityEngine.Random.Range(0, attackSounds.Length);
                
                audio.clip = attackSounds[ran];
                audio.Play();

               
            }
            // Debug.Log("enemy attacks");

        }
        else
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("hit"))
            {
                BeenHit = false;
                //animator.SetBool("hitme", false);
                EnemyRestoreFromHit();
            }
        }
    }

    Vector2 GetFurthestPointAfterPlayerToEnemy()
    {
        Vector2 playerPosition = GetPlayerDirection(player, transform);
        Vector2 newPosition = transform.position;

        float moveX = 5.5f; // delta value to move
        float moveY = 5.5f; // delta value to move

        if (playerPosition.x > 0) //player at the right side of enemy
        {
            if (playerPosition.y >= 0) //upper right
            {
                newPosition.x -= moveX;
                newPosition.y -= moveY;
            }
            else if (playerPosition.y < 0) //down right
            {
                newPosition.x -= moveX;
                newPosition.y += moveY;
            }
        }
        else if (playerPosition.x < 0) //player at the left side of enemy
        {
            if (playerPosition.y >= 0) //upper left
            {
                newPosition.x += moveX;
                newPosition.y -= moveY;
            }
            else if (playerPosition.y < 0) //down left
            {
                newPosition.x += moveX;
                newPosition.y += moveY;
            }
        }
        else if (playerPosition.x == 0)
        {
            if (playerPosition.y > 0)
            { //player is at vertical top
                newPosition.x += Random.Range(-1 * moveX, moveX);
                newPosition.y -= moveY;
            }
            else if (playerPosition.y < 0)
            { //player is at vertical down
                newPosition.x += Random.Range(-1 * moveX, moveX);
                newPosition.y += moveY;
            }
        }

        return newPosition;
    }

    Vector2 GetRandomNearPlayerPosition()
    {
        Vector2 playerPos = GetPlayerDirection(player, transform);
        Vector2 oldEnemyPos = transform.position;

        float offSet = randomWalkRange;

        if (playerPos.x >= 0) //player is at right
        {
            oldEnemyPos.x += -1 * Random.Range(0, offSet);
            oldEnemyPos.y += Random.Range(-1 * offSet, offSet);
        }
        else if (playerPos.x < 0) //player is at left
        {
            oldEnemyPos.x += Random.Range(0, offSet);
            oldEnemyPos.y += Random.Range(-1 * offSet, offSet);
        }

        return oldEnemyPos;
    }

    void EnemyRandomMove()
    {
        int randomNumber = Random.Range(0, 10);
        if (randomNumber >= 0 && randomNumber <= chanceToAttack)
        { //enemy will attack;
            if (playerCollider != null)
            {
                //player is at attacking range
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerKicking") && !attacking)
                {
                    animator.SetBool("PlayerKicking", false);
                }
                if (!attacking && timeSinceLastHit >= 1)
                //if (!attacking)
                {
                    StartToAttack();
                }
            }
        }
        else if (randomNumber > chanceToAttack && randomNumber <= 10)
        { //enemy will moveBack
            enemy.isStopped = false;
            SetEnemyAnimation(AnimationParams.EnemyWalking);
            enemy.destination = GetRandomNearPlayerPosition();

            isRandomMove = true;
        }
    }

    public void KickPlayer()
    {
        AnimationEvent ae = new AnimationEvent();
        ae.messageOptions = SendMessageOptions.DontRequireReceiver;
    }

    public void DoDamage()
    {
        PlayerHealth.doDamage(damage, this.transform.position);
    }

    void EnemyDead()
    {
        if (!isDead)
        {
            isDead = true;
            this.tag = "DeadEnemy";
            enemy.isStopped = true;
            animator.SetBool("IsEnemyDead", true);
            Destroy(GetComponent<NavMeshAgent2D>());

           

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Collider2D c in GetComponents<Collider2D>())
            {
                c.enabled = false;
            }
        }
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (enemyHP <= runAwayHP)
        {
            timeSinceRanAway += Time.deltaTime;
        }
        if (timeSinceRanAway > 5f)
        {
            timeSinceRanAway = 0f;
            enemyHP = (runAwayHP * 2);
        }



        if (enemyHP <= 0)
        {
            EnemyDead();
            return;
        }

        if (isSit)
        {
            Wait(sitTime, () =>
            {
                isSit = false;
                animator.SetBool("IsSit", false);
            });
            return;
        }

        if (isRandomMove)
        {
            if (player.position.x - playerLastPosition.x > 1 ||
            player.position.y - playerLastPosition.y > 1)
            {
                isRandomMove = false;
                enemy.destination = player.position;
            }
        }

        if (isRandomMove)
        {
            if (enemy.remainingDistance <= 0.3f)
            {
                isRandomMove = false;
            }
        }

        //Get's the time snice the emeny last made an attack
        timeSinceLastHit = Time.time - lastHitTime;

        //Makese the kick animation == to false after .3 secounds after it was set to true
        if (attacking && timeSinceLastHit >= .3)
        {
            attacking = false;
        }


        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("hit"))
        {
            BeenHit = false;
        }

        if (!BeenHit)
        {
            float remainingDistance = Vector2.Distance(transform.position, player.position);
            animator.SetFloat("Distance", remainingDistance);

            if (enemyHP <= runAwayHP)
            {
                //makes enemy run away
                enemy.isStopped = false;
                SetEnemyAnimation(AnimationParams.PlayerMoving);
                enemy.speed = 0.6f * enemySpeed;
                enemy.destination = GetFurthestPointAfterPlayerToEnemy();
                return;
            }

            if (caughtPlayer)
            {
                EnemyRandomMove();
                return;
            }

            if (remainingDistance > Mathf.Min(lineOfSight, 10))
            {
                enemy.isStopped = true;
                animator.SetBool("PlayerMoving", false);
                animator.SetBool("PlayerKicking", false);
                animator.SetBool("EnemyWalking", false);
                animator.SetBool("hitme", false);
                return;
            }

            if (remainingDistance > 0 && remainingDistance < lineOfSight)
            {
                enemy.isStopped = false;
                if (remainingDistance > checkDistance)
                {
                    enemy.speed = enemySpeed;
                    SetEnemyAnimation(AnimationParams.PlayerMoving);
                }
                else if (remainingDistance <= checkDistance)
                {
					enemy.isStopped = false;
                    SetEnemyAnimation(AnimationParams.EnemyWalking);
                }

                if (!isRandomMove)
                {
                    enemy.destination = player.position;
                }
            }
            else if (remainingDistance <= 0.3f)
            { // caught the player
                EnemyRandomMove();
            }
        }

        playerLastPosition = player.position;
    }

    public void Wait(float seconds, Action action)
    {
        StartCoroutine(_wait(seconds, action));
    }
    IEnumerator _wait(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }

    IEnumerator DoBlinks(float duration, float blinkTime)
    {
        Renderer renderer = enemy.GetComponent<Renderer>();
        while (duration > 0f)
        {
            duration -= Time.deltaTime;

            //toggle renderer
            renderer.enabled = !renderer.enabled;

            //wait for a bit
            yield return new WaitForSeconds(blinkTime);
        }

        //make sure renderer is enabled when we exit
        renderer.enabled = true;
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!BeenHit)
        {
            if (other.tag == "Player")
            {
                if (enemyHP > runAwayHP)
                {
                    caughtPlayer = true;
                    playerCollider = other.gameObject.transform;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            caughtPlayer = false;
            playerCollider = null;
            BeenHit = false;
			enemy.isStopped = false;
        }
    }

    private Vector2 GetPlayerDirection(Transform player, Transform enemy)
    {
        Transform transform = enemy;
        float horizontal = player.position.x - transform.position.x;
        float vertical = player.position.y - transform.position.y;

        Vector2 pos = new Vector2(0, 0);
        float offset = 0.4f; //use to make the enemy not that sensetive to direction

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
}
