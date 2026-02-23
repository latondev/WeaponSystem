using UnityEngine;

namespace PullGame
{
	public class Projectile : MonoBehaviour
	{
	    private ProjectileConfig config;
	    private Transform target;
	    
	    private Vector3 velocity;
	    private Vector3 startPosition;
	    private float lifetime;
	    private float traveledDistance;
	    
	    public void Initialize(ProjectileConfig config, Transform target = null)
	    {
	        this.config = config;
	        this.target = target;
	        
	        velocity = transform.right * config.speed;
	        startPosition = transform.position;
	        lifetime = config.lifetime;
	    }
	    
	    private void Update()
	    {
	        float deltaTime = Time.deltaTime;
	        
	        UpdateMovement(deltaTime);
	        UpdateLifetime(deltaTime);
	        CheckDistance();
	    }
	    
	    private void UpdateMovement(float deltaTime)
	    {
	        switch (config.type)
	        {
	            case ProjectileType.Normal:
	                transform.position += velocity * deltaTime;
	                break;
	                
	            case ProjectileType.Ballistic:
	                velocity += ((Vector3)Physics2D.gravity * (config.gravityModifier * deltaTime));
	                transform.position += velocity * deltaTime;
	                
	                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
	                transform.rotation = Quaternion.Euler(0, 0, angle);
	                break;
	                
	            case ProjectileType.Homing:
	                if (target != null)
	                {
	                    Vector3 directionToTarget = (target.position - transform.position).normalized;
	                    velocity = Vector3.Lerp(velocity, directionToTarget * config.speed, config.followStrength * deltaTime);
	                    
	                    float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
	                    transform.rotation = Quaternion.Euler(0, 0, targetAngle);
	                }
	                
	                transform.position += velocity * deltaTime;
	                break;
	        }
	        
	        traveledDistance += velocity.magnitude * deltaTime;
	    }
	    
	    private void UpdateLifetime(float deltaTime)
	    {
	        if (config.unlimitedLifetime) return;
	        
	        lifetime -= deltaTime;
	        if (lifetime <= 0f)
	        {
	            DestroyProjectile();
	        }
	    }
	    
	    private void CheckDistance()
	    {
	        if (!config.destroyOnDistanceTraveled) return;
	        
	        if (traveledDistance >= config.maxDistance)
	        {
	            DestroyProjectile();
	        }
	    }
	    
	    private void OnTriggerEnter2D(Collider2D collision)
	    {
	        var damageable = collision.GetComponent<IDamageable>();
	        if (damageable != null)
	        {
	            damageable.TakeDamage(config.damage);
	        }
	        
	        var structure = collision.GetComponent<IStructure>();
	        if (structure != null)
	        {
	            structure.TakeDamage(config.damageToStructure);
	        }

	        Debug.Log($"<b><color=green>_Log Bullet Collision</color></b> :");

	        DestroyProjectile();
	    }
	    
	    private void DestroyProjectile()
	    {
	        if (config.impactEffect != null)
	        {
	            Instantiate(config.impactEffect, transform.position, Quaternion.identity);
	        }
	        
	        Destroy(gameObject);
	    }
	}
}