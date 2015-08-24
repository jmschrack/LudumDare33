using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DetectorScript : MonoBehaviour {

	public List<GameObject> targets;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public GameObject getTarget(){
		if (targets.Count == 0)
			return null;
		return targets [0];
	}
	void OnTriggerEnter(Collider other){
		//Debug.Log ("Trigger Enter");
		if(other.gameObject.GetComponentInChildren<Victim>()!=null)
			targets.Add (other.gameObject);
	}
	void OnTriggerStay(Collider other) {
		//Debug.Log ("Triggered!");
		       
	}
	void OnTriggerExit(Collider other){
		//Debug.Log ("Trigger Exit");
		targets.Remove (other.gameObject);
	}

}
