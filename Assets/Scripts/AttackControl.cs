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
			}else{
			StartCoroutine(backstab());
			}
			
		}
	}
	
	IEnumerator backstab(){
		m_Animator.SetBool("InstaKill", true);
		yield return new WaitForSeconds(2.0f);
		m_Animator.SetBool("InstaKill", false);
	}
}
