using System;
using UnityEngine;

namespace PullGame
{
	public interface IWeaponFireMode
	{
	    void Update(float deltaTime);
	    bool CanFire();
	    void OnFireStarted();
	    int Fire(ShootContext context, Action<Vector3, Quaternion> spawnProjectile);
	}
	
	public struct ShootContext
	{
	    public FirePoint[] firePoints;
	    public int projectilesNeeded;
	    public float recoilAngle;
	    public float[] spreadAngles;
	}
}