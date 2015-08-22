using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Teleport : MonoBehaviour {
	public float coolDown;
	private bool ready=true;
	public Transform targetIndicator;
	private Villian v;
	// Use this for initialization
	void Start () {
		v=this.GetComponentsInChildren<Villian>()[0];
	}
	
	// Update is called once per frame
	void Update () {
		  Ray rayC = Camera.main.ScreenPointToRay(Input.mousePosition);
		  RaycastHit rh;
            if (Physics.Raycast(rayC,out rh)){
				targetIndicator.position=rh.point+new Vector3(0f,0.1f,0f);
			}
               // Instantiate(particle, transform.position, transform.rotation);
		if (CrossPlatformInputManager.GetButtonDown("Jump")&&ready&&!v.isSpotted&&v.isValidCursor)
		{
			Debug.Log("Teleporting");
			//
			  this.transform.position=targetIndicator.position;
                //Instantiate(particle, transform.position, transform.rotation);
		}
	}
}
