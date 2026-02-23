using System;
using UnityEngine;

namespace PullGame
{
	public class SpreadFireMode : IWeaponFireMode
	{
	    private readonly FireModeConfig config;
	    private float cooldownTimer;
	    
	    public SpreadFireMode(FireModeConfig config)
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
	
	    public int Fire(ShootContext context, Action spawnProjectile)
	    {
	        throw new NotImplementedException();
	    }
	
	    public int Fire(ShootContext context, Action<Vector3, Quaternion> spawnProjectile)
	    {
	        int projectilesFired = 0;
	        
	        foreach (var firePoint in context.firePoints)
	        {
	            Vector3 position = firePoint.GetWorldPosition();
	            Quaternion baseRotation = firePoint.GetWorldRotation();
	            
	            for (int i = 0; i < config.spreadProjectileCount; i++)
	            {
	                float spreadAngle = context.spreadAngles != null && i < context.spreadAngles.Length 
	                    ? context.spreadAngles[i] 
	                    : 0f;
	                
	                Quaternion rotation = baseRotation * Quaternion.Euler(0, 0, spreadAngle);
	                
	                spawnProjectile(position, rotation);
	                projectilesFired++;
	            }
	        }
	        
	        return projectilesFired;
	    }
	}
}