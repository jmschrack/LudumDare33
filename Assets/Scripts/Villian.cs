using UnityEngine;
using System.Collections;

public class Villian : MonoBehaviour {

	public bool isSpotted=false;
	public GameObject cursor;
	public bool isValidCursor=true;
	public int HP=20;
	private Renderer rend;
	private Renderer cursorRend;
	
	// Use this for initialization
	void Start () {
	 this.rend = GetComponent<Renderer>();
        //rend.material.shader = Shader.Find("Standard");
        this.cursorRend=cursor.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if ((isSpotted))
		{
			rend.material.SetColor("_Color", Color.white);
		}else{
			rend.material.SetColor("_Color", Color.black);
		}
		if(isValidCursor){
			cursorRend.material.SetColor("_EmissionColor",Color.magenta);
		}else{
			cursorRend.material.SetColor("_EmissionColor",Color.white);
		}
	}
}
