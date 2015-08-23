using UnityEngine;
using System.Collections;

public class AttackControl : MonoBehaviour {
	private Animator m_Animator;
	private DetectorScript detection;
	private Villian v;
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
	}

	IEnumerator slash(){
		Debug.Log ("Slashing");
		m_Animator.SetFloat("Slash",1.0f);
		yield return new WaitForSeconds (0.5f);
		m_Animator.SetFloat("Slash",0.0f);
	}
	
	IEnumerator backstab(){
		Debug.Log ("Stabbing");
		m_Animator.SetBool("Attack", true);
		yield return new WaitForSeconds(1.0f);
		m_Animator.SetBool("Attack", false);
	}
}
