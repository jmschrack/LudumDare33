using UnityEngine;
using System.Collections;

public class AttackControl : MonoBehaviour
{
	private Animator m_Animator;
	private DetectorScript detection;
	private Villian v;
	private Transform target;
	public GameObject blood;
	private bool animLock = false;
	// Use this for initialization
	void Start ()
	{
		m_Animator = GetComponent<Animator> ();
		detection = this.GetComponentsInChildren<DetectorScript> () [0];
		v = GetComponentInChildren<Villian> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetButtonDown ("Fire1") && detection.getTarget () != null) {
			target = detection.getTarget ().transform;
			if (v.isSpotted) {
				StartCoroutine (slash ());
			} else {
				StartCoroutine (backstab ());
			}
			
		}
		if (animLock) {
			this.transform.LookAt (target.position);
		}
	}

	IEnumerator slash ()
	{
		Debug.Log ("Slashing");
		m_Animator.SetFloat ("Slash", 1.0f);
		yield return new WaitForSeconds (0.4f);
		target.gameObject.GetComponentInChildren<Victim> ().takeDamage (1);
		m_Animator.SetFloat ("Slash", 0.0f);
	}
	
	IEnumerator backstab ()
	{
		//Debug.Log ("Stabbing");
		target = detection.getTarget ().transform;
		m_Animator.SetBool ("Attack", true);
		animLock = true;
		StartCoroutine (animLockTimer ());
		target.gameObject.GetComponent<Victim> ().pauseAI ();
		yield return new WaitForSeconds (0.6f);
		GameObject temp = (GameObject)Instantiate (blood, target.position + new Vector3 (0, 0.6f, 0), Quaternion.identity);
		m_Animator.SetBool ("Attack", false);
		yield return new WaitForSeconds (0.5f);
		GameObject.Destroy (temp);

		//this.transform.rotation = Quaternion.identity;
		yield return new WaitForSeconds (1.7f);

		target.gameObject.GetComponentInChildren<Victim> ().takeDamage (5);

		//target.GetComponent<Rigidbody> ().AddForce (this.transform.forward*100);

	}
	//don't judge me
	IEnumerator animLockTimer ()
	{
		yield return new WaitForSeconds (2.0f);
		animLock = false;
	}
}
