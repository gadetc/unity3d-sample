﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
[RequireComponent (typeof (CharacterController))]
public class Player : MonoBehaviour {

	private CharacterController characterController;
	private Transform cameraTransform;
	private Vector3 cameraAngles;
	private float cameraHeight = 1.4f;

	private float gravity = 9.81f;
	public float moveSpeed =  4.0f;
	private float jumpSpeed =  30.0f;

	public int hp;

	private Transform muzzlepoint;
	public LayerMask layerMask;
	public Transform fx;

	public AudioClip shootSound;
	private float shootTimer;

	// Use this for initialization
	void Start () {
		characterController = GetComponent<CharacterController>();
		cameraTransform = Camera.main.transform;

		Vector3 startPos = transform.position;
		startPos.y += cameraHeight;
		cameraTransform.position = startPos;

		cameraTransform.rotation = transform.rotation;
		cameraAngles = cameraTransform.eulerAngles;

		muzzlepoint = cameraTransform.FindChild("M16/weapon/muzzlepoint").transform;

		Cursor.lockState = CursorLockMode.Locked;
	}
	
	// Update is called once per frame
	void Update () {
		if(IsDeath()){
			return;
		}
		Move();
		Shoot();
	}

	public bool IsDeath(){
		return hp <= 0;
	}

	void Move(){
		MoveCamera();
		MovePlayer();

		Vector3 pos = transform.position;
		pos.y += cameraHeight;
		cameraTransform.position = pos;
	}

	void MoveCamera(){
		float y = Input.GetAxis("Mouse X");
		float x = Input.GetAxis("Mouse Y");

		cameraAngles.x -= x;
		cameraAngles.y += y;
		cameraTransform.eulerAngles = cameraAngles;

		Vector3 cameraDirection = cameraAngles;
		cameraDirection.x = 0;
		cameraDirection.z = 0;
		transform.eulerAngles = cameraDirection;
	}

	void MovePlayer(){
		float x = 0, y = 0, z = 0;

		if(Input.GetKey(KeyCode.D)){
			x += moveSpeed * Time.deltaTime;
		}else if(Input.GetKey(KeyCode.A)){
			x -= moveSpeed * Time.deltaTime;
		}

		if(Input.GetKey(KeyCode.Space)){
			y += jumpSpeed * Time.deltaTime;
		}else if(transform.position.y-cameraHeight > 0){
			y -= gravity * Time.deltaTime;
		}

		if(Input.GetKey(KeyCode.W)){
			z += moveSpeed * Time.deltaTime;
		}else if(Input.GetKey(KeyCode.S)){
			z -= moveSpeed * Time.deltaTime;
		}

		characterController.Move(transform.TransformDirection(new Vector3(x, y, z)));
	}

	void OnDrawGizmos(){
		Gizmos.DrawIcon(transform.position, "../Gizmos/Spawn.tif");
	}

	public void OnDamage(int damage){
		hp -= damage;

		GUIManager.instance.SetHp(hp);

		if(IsDeath()){
			Cursor.lockState = CursorLockMode.None;
		}
	}

	void Shoot(){
		shootTimer -= Time.deltaTime;
		if(Input.GetButtonUp("Fire1") && shootTimer <= 0){
			shootTimer = 0.1f;
			GetComponent<AudioSource>().PlayOneShot(shootSound);
			GUIManager.instance.SubBullet(1);

			RaycastHit hitInfo;

			bool hit = Physics.Raycast(
				muzzlepoint.position, 
				cameraTransform.TransformDirection(Vector3.forward), 
				out hitInfo, 100, layerMask);
			if(hit){
				if(hitInfo.collider.gameObject.tag == "enemy"){
					hitInfo.collider.gameObject.SendMessage ("OnDamage", 1);
				}
				Instantiate(fx, hitInfo.point, hitInfo.transform.rotation);
			}
		}
	}

}
