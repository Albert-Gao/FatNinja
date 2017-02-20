using UnityEngine;

public class DoorScript : MonoBehaviour {

    public string level;
	public BoxCollider2D blockExitCollider = null;
    private bool loadLevel;

    // Use this for initialization
    void Start () {
        loadLevel = false;
    }
	
	// Update is called once per frame
	void Update () {
		GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
		foreach	(GameObject go in spawners) {
			if (go.GetComponent<SpawnWaves>().numberOfWaves != 0)
			return;
		}

		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
		if (enemies.Length == 0) {
			this.GetComponent<Animator>().SetBool("OpenDoor", true);
           // GetComponentInChildren<BoxCollider2D>().enabled = false;//doesnt work wtf
			if (blockExitCollider != null)
				blockExitCollider.enabled = false;
        }
        if (loadLevel)
        {
            loadLevel = false;
            Loading.loadLevel(level);
        }
    }

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player"){
            loadLevel = true;
		}
	}
}
