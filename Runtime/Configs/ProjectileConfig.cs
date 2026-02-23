using System;
using UnityEngine;

namespace PullGame
{
	[Serializable]
	public class ProjectileConfig
	{
	    public GameObject projectilePrefab;
	    public ProjectileType type;
	    public float speed = 10f;
	    
	    public bool unlimitedLifetime = false;
	    public float lifetime = 5f;
	    
	    public bool destroyOnDistanceTraveled = false;
	    public float maxDistance = 100f;
	    
	    public float gravityModifier = 1f;
	    public float followStrength = 5f;
	    
	    public int damage = 10;
	    public int damageToStructure = 5;
	    
	    public GameObject impactEffect;
	}
	
	public enum ProjectileType
	{
	    Normal,
	    Ballistic,
	    Homing
	}
}