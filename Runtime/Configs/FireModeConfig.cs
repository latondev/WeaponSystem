using System;
using UnityEngine;

namespace PullGame
{
	[Serializable]
	public class FireModeConfig
	{
	    public FireMode mode;
	    public float fireRate = 0.1f;
	    
	    [Min(1)] public int burstCount = 3;
	    public float timeBetweenShots = 0.1f;
	    public float timeBetweenBursts = 0.5f;
	    
	    [Min(1)] public int spreadProjectileCount = 5;
	    public bool randomSpread = true;
	    public float minSpreadAngle = -30f;
	    public float maxSpreadAngle = 30f;
	    [Range(0, 1)] public float spreadFactor = 1f;
	    
	    public bool autoShooting = false;
	}
	
	public enum FireMode
	{
	    Single,
	    Auto,
	    Burst,
	    Spread
	}
}