﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

	[SerializeField] private float _speed = 3.0f;
	[SerializeField] private float _rotationSpeed = 130f;
	[SerializeField] private double _damage = 0.2;
	[SerializeField] private double _damageScaling = 10f;
	[SerializeField] private double _totalHp = 100;
	[SerializeField] private float _waterStrength = 3;
	
	
	private Rigidbody _myBody;
	private double _hitPoints;
	private DamageNumberController _damageNumberController;
	private List<Collider> _enemiesNearBy;

	// Use this for initialization
	void Start ()
	{
		_hitPoints = _totalHp;
		
		LineRenderer outline = gameObject.AddComponent<LineRenderer>();

		outline.startWidth = 0.1f;
		outline.endWidth = 0.1f;
		outline.positionCount = 129;
		outline.useWorldSpace = false;

		float deltaTheta = (float) (2.0 * Mathf.PI) / 128;
		float theta = 0f;

		for (int i = 0; i < 129; i++)
		{
			float x = 4.2f * Mathf.Cos(theta);
			float z = 4.2f * Mathf.Sin(theta);
			Vector3 pos = new Vector3(x, 1, z);

			outline.SetPosition(i, pos);
			theta += deltaTheta;
		}
		
		_enemiesNearBy = new List<Collider>();
		_myBody = GetComponent<Rigidbody>();
		_damageNumberController = GameMaster.GetDamageNumberController();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		if (Input.GetKey(KeyCode.Space) && _hitPoints > 0)
		{
			if (_hitPoints > 0)
			{
				_hitPoints -= _damage;
				_damageNumberController.SpawnDamageNumber( _damage, transform );
			}
		};

		if (Input.GetKeyUp(KeyCode.R)) _hitPoints = _totalHp;

		LookAtMouse();
		TakeDamage();
	}

	private void OnTriggerEnter(Collider other)
	{
		EnemyController enemy = other.GetComponentInParent<EnemyController>();
		if (!_enemiesNearBy.Contains(other) && enemy)
		{
//			enemy.setActive();
//			Debug.Log("Adding new enemy: " + enemiesNearBy.Count+1);
			_enemiesNearBy.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
//		Debug.Log("Removing " + enemiesNearBy.IndexOf(other)+1);
		if (_enemiesNearBy.Remove(other))
		{
//			other.GetComponentInParent<EnemyController>().setInactive();
		}
	}
	
	private void TakeDamage()
	{
		double totalDmg = 0;
		foreach (Collider enemyCollider in _enemiesNearBy)
		{
			FlameController enemy = enemyCollider.GetComponentInParent< FlameController >();
			totalDmg += enemy.GetIntensity()
			                  /
			                  Vector3.Distance(enemy.transform.position, transform.position)
			                  *
			                  Time.deltaTime;
		}
		if ( totalDmg > 0 )
		{
			totalDmg = Math.Round( totalDmg * _damageScaling );
			_damageNumberController.SpawnDamageNumber( totalDmg, transform );
			_hitPoints -= totalDmg;
//			Debug.Log( totalDmg );
		}
	}

	private void FixedUpdate()
	{
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");
		
		Vector3 movement = new Vector3( x, 0f, z);

		if (_myBody != null)
		{
			_myBody.velocity = movement * _speed;
		}
	}

	private void LookAtMouse( )
	{
		Plane mousePlane = new Plane( Vector3.up, transform.position);

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		float hitDist = 0f;
		
		if (mousePlane.Raycast(ray, out hitDist))
		{
			Vector3 point = ray.GetPoint(hitDist);

			Quaternion rotation = Quaternion.LookRotation(point - transform.position, Vector3.up);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
		}
	}

	public double GetHealth()
	{
		return _hitPoints / _totalHp;
	}
}
