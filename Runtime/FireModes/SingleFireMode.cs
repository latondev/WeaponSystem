using System;
using UnityEngine;

namespace PullGame
{
	public class SingleFireMode : IWeaponFireMode
	{
	    private readonly FireModeConfig config;
	    private float cooldownTimer;
	    
	    public SingleFireMode(FireModeConfig config)
	    {
	        this.config = config;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        cooldownTimer -= deltaTime;
	    }
	    
	    public bool CanFire()
	    {
	        return cooldownTimer <= 0f;
	    }
	    
	    public void OnFireStarted()
	    {
	        cooldownTimer = config.fireRate;
	    }
	    
	    public int Fire(ShootContext context, Action<Vector3, Quaternion> spawnProjectile)
	    {
	        int projectilesFired = 0;
	        
	        foreach (var firePoint in context.firePoints)
	        {
	            Vector3 position = firePoint.GetWorldPosition();
	            Quaternion rotation = firePoint.GetWorldRotation();
	            
	            rotation *= Quaternion.Euler(0, 0, context.recoilAngle);
	            
	            spawnProjectile(position, rotation);
	            projectilesFired++;
	        }
	        
	        return projectilesFired;
	    }
	}
}