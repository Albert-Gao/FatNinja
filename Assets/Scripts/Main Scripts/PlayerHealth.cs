using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {

    public static float MaxHP = 20;
    public static float PlayersHP = MaxHP;
    public static float Damage = 0;
    public Texture HP_Indercator;
    public Texture HP_Container;
    Vector2 pos  = new Vector2(40,30);
    Vector2 size  = new Vector2(200,20);

    // fade and die coroutine
    private IEnumerator coroutine;
    private bool startedDying = false;
    public GameObject dyingPrefabTEMP;
    private static GameObject dyingPrefab;

    public GameObject bloodPrefabTEMP;
    private static GameObject bloodPrefab;
    public Text GameOverText = null;

    public AudioClip[] deathSound;
    public AudioSource audio;


    // Use this for initialization
    void Start () {
        bloodPrefab = bloodPrefabTEMP;
        dyingPrefab = dyingPrefabTEMP;
	}

    void OnGUI()
    {

        // draw the background:
        GUI.BeginGroup(new Rect(pos.x, pos.y, size.x, size.y));
        GUI.DrawTexture(new Rect(0, 0, size.x, size.y), HP_Container);

        // Removes health from bar - bar total = 20
        GUI.BeginGroup(new Rect(0, 0, size.x - (Damage * 10), size.y));
        GUI.DrawTexture(new Rect(0, 0, size.x, size.y), HP_Indercator);

        GUI.EndGroup();
        GUI.EndGroup();

    }

    // Update is called once per frame
    void Update () {
        // restart level on 0 health
        if (PlayersHP <= 0 && !startedDying)
        {
           /* GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject go in enemies)
            {
                if (go.GetComponent<AudioSource>()!=null) {
                    go.GetComponent<AudioSource>().enabled = false;
                }
            }*/
        
           


            audio.clip = deathSound[0];
            audio.Play();
            startedDying = true;

            
            coroutine = FadeAndDie();
            StartCoroutine(coroutine);
           
        }
    }

    public static void doDamage(int damageAmount, Vector3 enemyPosition)
    {
        Damage += damageAmount;
        PlayersHP = MaxHP - Damage;

        // spawn blood
        GameObject blood = Instantiate(bloodPrefab);
        // set blood position
        Vector3 bloodPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        blood.transform.position = bloodPos;
        // set blood direction
        Vector2 enemyDirection = (GameObject.FindGameObjectWithTag("Player").transform.position - enemyPosition).normalized;
        float enemyAngle = enemyDirection.x * Vector2.Angle(Vector2.up, enemyDirection);
        //float playerAngle = player.gameObject.GetComponent<CharacterController>().getPlayerAngle();
        blood.GetComponent<BloodScript>().setBlood(enemyAngle, (float)damageAmount / 4f);
        // set blood damage text
        int incomingdam = ((damageAmount * 100) + Random.Range(0, 100));
        blood.GetComponentInChildren<damageTextScr>().setDamage(incomingdam);
    }

    IEnumerator FadeAndDie() {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        float alpha = 1f;

        // make player invisible, and stop moving
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 pos = player.transform.position;
        player.GetComponent<SpriteRenderer>().enabled = false;
		player.GetComponent<CharacterController> ().enabled = false;
		player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        //player.GetComponent<CircleCollider2D>().enabled = false;

        // spawn dying prefab
        GameObject dyingPlayer = Instantiate(dyingPrefab);
        dyingPlayer.transform.position = pos;
        dyingPlayer.transform.parent = player.transform;
        dyingPlayer.GetComponent<EnemyAI>().isDead = true;
        dyingPlayer.GetComponent<Animator>().SetBool("IsEnemyDead", true);

        // spawn dead text
        GameOverText.text = "GAME OVER";

        // slowly fade in text
        // slowly fade out all other objects
        while (alpha > -1f) {
            foreach(GameObject go in allObjects) {
                if (go == null) continue;
                if (go.gameObject.tag == "Player") continue;
                
                NavMeshAgent2D e = go.gameObject.GetComponent<NavMeshAgent2D>();
                if (e) {
                    e.Stop();
                }

                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr) {
                    Color c = sr.color;
                    c.a = Mathf.Max(0f, alpha);
                    sr.color = c;
                }
            }
            alpha -= Time.deltaTime * 0.3f;
            //player.transform.position = pos;
            yield return new WaitForEndOfFrame();
        }



        startedDying = false;
        GameOverText.text = "";
        GameMaster.SoftReset();

    }


    public static void heal(int healAmount)
    {
        Damage = Damage - healAmount < 0 ? 0 : Damage - healAmount;
        PlayersHP = MaxHP - Damage;
    }
}
