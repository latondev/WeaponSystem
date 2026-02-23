using System;
using System.Collections;
using UnityEngine;

namespace PullGame
{
	public class BurstFireMode : IWeaponFireMode
	{
	    private readonly FireModeConfig config;
	    private readonly MonoBehaviour coroutineRunner;
	    
	    private float cooldownTimer;
	    private bool isBursting;
	    
	    public BurstFireMode(FireModeConfig config, MonoBehaviour coroutineRunner)
	    {
	        this.config = config;
	        this.coroutineRunner = coroutineRunner;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        if (!isBursting)
	        {
	            cooldownTimer -= deltaTime;
	        }
	    }
	    
	    public bool CanFire()
	    {
	        return cooldownTimer <= 0f && !isBursting;
	    }
	    
	    public void OnFireStarted()
	    {
	        isBursting = true;
	        coroutineRunner.StartCoroutine(BurstCoroutine());
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
	    
	    private IEnumerator BurstCoroutine()
	    {
	        for (int i = 0; i < config.burstCount; i++)
	        {
	            yield return new WaitForSeconds(config.timeBetweenShots);
	        }
	        
	        cooldownTimer = config.timeBetweenBursts;
	        isBursting = false;
	    }
	}
}