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
	    
	    // Store context for coroutine
	    private ShootContext storedContext;
	    private Action<Vector3, Quaternion> storedSpawnProjectile;
	    
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
	        // Store for coroutine use
	        storedContext = context;
	        storedSpawnProjectile = spawnProjectile;
	        
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
	            // Fire for each burst shot
	            if (storedContext.firePoints != null)
	            {
	                foreach (var firePoint in storedContext.firePoints)
	                {
	                    Vector3 position = firePoint.GetWorldPosition();
	                    Quaternion rotation = firePoint.GetWorldRotation();
	                    
	                    // Apply spread for each burst shot
	                    float[] spreadAngles = GenerateBurstSpread(i, config.burstCount);
	                    if (spreadAngles != null && i < spreadAngles.Length)
	                    {
	                        rotation *= Quaternion.Euler(0, 0, spreadAngles[i]);
	                    }
	                    
	                    storedSpawnProjectile(position, rotation);
	                }
	            }
	            
	            yield return new WaitForSeconds(config.timeBetweenShots);
	        }
	        
	        cooldownTimer = config.timeBetweenBursts;
	        isBursting = false;
	        
	        // Clear stored references
	        storedContext = default;
	        storedSpawnProjectile = null;
	    }
	    
	    private float[] GenerateBurstSpread(int currentIndex, int totalBursts)
	    {
	        if (totalBursts <= 1) return null;
	        
	        float[] angles = new float[totalBursts];
	        float step = (config.maxSpreadAngle - config.minSpreadAngle) / Mathf.Max(totalBursts - 1, 1);
	        
	        for (int i = 0; i < totalBursts; i++)
	        {
	            angles[i] = config.minSpreadAngle + (step * i);
	        }
	        
	        return angles;
	    }
	}
}