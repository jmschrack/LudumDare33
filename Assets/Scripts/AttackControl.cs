using UnityEngine;
using System.Collections;

public class AttackControl : MonoBehaviour {
	private Animator m_Animator;
	private DetectorScript detection;
	private Villian v;
	private Transform target;
	public GameObject blood;
	// Use this for initialization
	void Start () {
		m_Animator = GetComponent<Animator>();
		detection = this.GetComponentsInChildren<DetectorScript> ()[0];
		v = GetComponentInChildren<Villian> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Fire1")&&detection.getTarget()!=null)
		{
			if(v.isSpotted){
				StartCoroutine(slash ());
			}else{
				StartCoroutine(backstab());
			}
			
		}
		if (m_Animator.GetBool ("Attack")) {
			this.transform.LookAt(target.position);
		}
	}

	IEnumerator slash(){
		Debug.Log ("Slashing");
		m_Animator.SetFloat("Slash",1.0f);
		yield return new WaitForSeconds (0.4f);
		target.gameObject.GetComponentInChildren<Victim> ().takeDamage (1);
		m_Animator.SetFloat("Slash",0.0f);
	}
	
	IEnumerator backstab(){
		//Debug.Log ("Stabbing");
		target = detection.getTarget ().transform;
		m_Animator.SetBool("Attack", true);
		yield return new WaitForSeconds (0.4f);
		GameObject temp=(GameObject)Instantiate (blood, target.position, Quaternion.identity);
		yield return new WaitForSeconds(0.5f);
		GameObject.Destroy (temp);
		yield return new WaitForSeconds(1.7f);
		target.gameObject.GetComponentInChildren<Victim> ().takeDamage (5);
		m_Animator.SetBool("Attack", false);
	}
}
