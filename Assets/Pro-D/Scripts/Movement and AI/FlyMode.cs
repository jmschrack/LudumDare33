using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterMotor))]
public class FlyMode : MonoBehaviour 
{
	private CharacterMotor characterMotor;
	private CharacterController characterController;

	private Transform myTransform;

	private Vector3 velocity;

	public bool isFlying = false;

	public float flySpeed = 1.0f;

	private float gravityBefore;

	// Use this for initialization
	private void OnEnable () 
	{
		myTransform = transform;
		characterMotor = GetComponent<CharacterMotor>();
		characterController = GetComponent<CharacterController>();
		isFlying = false;
	}
	
	// Update is called once per frame
	private void Update () 
	{
		if (Input.GetKeyUp(KeyCode.F))
		{
			isFlying = !isFlying;
		}

		if (isFlying)
		{
			if (characterController.enabled)
			{
				characterController.enabled = false;
				characterMotor.enabled = false;
			}


			velocity = Vector3.zero;

			velocity.x = Input.GetAxis("Horizontal") * flySpeed;
			velocity.y = Input.GetKey(KeyCode.Space) ? flySpeed : Input.GetKey(KeyCode.LeftShift) ? -flySpeed : 0;
			velocity.z = Input.GetAxis("Vertical") * flySpeed;

			myTransform.Translate(velocity);
		}
		else
		{
			if (!characterController.enabled)
			{
				characterController.enabled = true;
				characterMotor.enabled = true;
				characterMotor.movement.velocity = velocity;
			}
		}
	}
}
