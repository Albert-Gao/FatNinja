using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class comboHitText : MonoBehaviour {

	private int prevChain = 0;
	private Vector3 initialScale;

	// Use this for initialization
	void Start () {
		initialScale = transform.localScale;
	}

	// Update is called once per frame
	void Update () {
		int chain = Score.getMultipler() - 1;

		if (chain != prevChain && chain > 1) {
			GetComponent<TextMesh>().text = chain + " hits!";
			transform.localScale = initialScale * 2;
		}

		transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime);

		if (chain <= 1) {
			GetComponent<TextMesh>().text = "";
		}
		prevChain = chain;
	}

	void LateUpdate()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player)
			transform.position = player.transform.position + new Vector3(0, 1.5f, -16f);		
	}
}
