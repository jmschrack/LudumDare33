using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Teleport : MonoBehaviour {
	public float coolDown;
	private bool ready=true;
	private Transform targetIndicator;
	private Villian v;
	private VHSPostProcessEffect scanLines;
	// Use this for initialization
	void Start () {
		v=this.GetComponentsInChildren<Villian>()[0];
		targetIndicator=v.cursor.transform;
		scanLines = Camera.main.GetComponent<VHSPostProcessEffect> ();
	}
	
	// Update is called once per frame
	void Update () {
		Ray rayC = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit rh;
		if (Physics.Raycast (rayC, out rh)) {
			targetIndicator.position = rh.point + new Vector3 (0f, 0.1f, 0f);
		}
		// Instantiate(particle, transform.position, transform.rotation);
		if (CrossPlatformInputManager.GetButtonDown ("Jump") && ready && !v.isSpotted && v.isValidCursor) {
			//Debug.Log("Teleporting");
			//
			StartCoroutine (coolDownTimer());
			this.transform.position = targetIndicator.position;
			//Instantiate(particle, transform.position, transform.rotation);
		}
	}

		IEnumerator coolDownTimer(){
			scanLines.enabled = true;
			ready=false;
			yield return new WaitForSeconds (1f);
		scanLines.enabled = false;
			yield return new WaitForSeconds(coolDown);
			ready=true;
		}



}
