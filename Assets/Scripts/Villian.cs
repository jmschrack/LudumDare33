using UnityEngine;
using System.Collections;

public class Villian : MonoBehaviour {
	private Animator m_Animator;
	public bool isSpotted=false;
	public GameObject cursor;
	public bool isValidCursor=true;
	public int HP=5;
	public int lives = 3;
	private Renderer rend; 
	private Renderer cursorRend;
	private bool isAggro=false;
	private DetectorScript ds;
	public bool isAlive=true;
	
	// Use this for initialization
	void Start () {
		m_Animator = this.transform.parent.gameObject.GetComponentInChildren<Animator> ();
	 this.rend = GetComponent<Renderer>();
        //rend.material.shader = Shader.Find("Standard");
        this.cursorRend=cursor.GetComponent<Renderer>();
		ds = this.transform.parent.GetComponentInChildren<DetectorScript> ();
	}
	
	// Update is called once per frame
	void Update () {
		if ((isSpotted))
		{
			setAllColors(rend,"_Color",Color.white);
			//rend.material.SetColor("_Color", Color.white);
		}else{
			setAllColors(rend,"_Color",Color.black);
			//rend.material.SetColor("_Color", Color.black);
		}
		if(isValidCursor){
			cursorRend.material.SetColor("_EmissionColor",Color.magenta);
		}else{
			cursorRend.material.SetColor("_EmissionColor",Color.white);

		}
		colorMode (ds.getTarget () != null);
	}

	public void colorMode(bool isAggro){
		if (isAggro != this.isAggro) {
			if (isAggro) {
				 StartCoroutine(changeColor ("_EmissionColor", Color.green, Color.red, 0.25f));
			}else{
				StartCoroutine(changeColor ("_EmissionColor", Color.red, Color.green, 1.0f));
			}
			this.isAggro=isAggro;
		}
	}

	IEnumerator changeColor(string tag, Color from, Color to,float time){

		//rend.material.SetColor (tag, Color.Lerp (from, to, time));
		float progress = 0;
		float smoothness = 0.05f;
		float increment = smoothness/time;
		Color currentColor;
		while(progress < 1)
		{
			currentColor = Color.Lerp(from, to, progress);
			progress += increment;
			//rend.material.SetColor(tag,currentColor);
			setAllColors(rend,tag,currentColor);
			yield return new WaitForSeconds(smoothness);
		}
	}

	void setAllColors(Renderer renderer,string tag, Color c){
		for (int i=0; i<renderer.materials.Length; i++) {
			renderer.materials[i].SetColor(tag,c);
		}
	}

	public void takeDamage(int damage){
		m_Animator.SetTrigger ("Shot");

		HP -= damage;
		if (HP <= 0) {
			die ();
		}
		//m_Animator.SetBool ("Shot", false);
	}

	void die(){
		m_Animator.SetTrigger ("Death");
	}

	public void respawn(){
		lives -= 1;
		HP = 5;
		isAlive = true;
		m_Animator.SetTrigger ("Respawn");

	}
}
