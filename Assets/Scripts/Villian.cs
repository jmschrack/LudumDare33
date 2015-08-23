﻿using UnityEngine;
using System.Collections;

public class Villian : MonoBehaviour {

	public bool isSpotted=false;
	public GameObject cursor;
	public bool isValidCursor=true;
	public int HP=20;
	private Renderer rend; 
	private Renderer cursorRend;
	private bool isAggro=false;
	private DetectorScript ds;
	
	// Use this for initialization
	void Start () {
	 this.rend = GetComponent<Renderer>();
        //rend.material.shader = Shader.Find("Standard");
        this.cursorRend=cursor.GetComponent<Renderer>();
		ds = this.transform.parent.GetComponentInChildren<DetectorScript> ();
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
			rend.material.SetColor(tag,currentColor);
			yield return new WaitForSeconds(smoothness);
		}
	}
}
