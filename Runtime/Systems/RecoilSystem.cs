using UnityEngine;

namespace PullGame
{
	public class RecoilSystem
	{
	    private readonly RecoilConfig config;
	    
	    private float recoilFactor;
	    private float timeSinceLastShot;
	    
	    public RecoilSystem(RecoilConfig config)
	    {
	        this.config = config;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        timeSinceLastShot += deltaTime;
	        
	        if (timeSinceLastShot >= config.recoveryTime)
	        {
	            recoilFactor = 0f;
	        }
	    }
	    
	    public void ApplyRecoil()
	    {
	        if (!config.enabled) return;
	        
	        recoilFactor += Time.deltaTime;
	        timeSinceLastShot = 0f;
	    }
	    
	    public float GetRecoilAngle()
	    {
	        if (!config.enabled) return 0f;
	        
	        float curveValue = config.recoilCurve.Evaluate(Mathf.Clamp01(recoilFactor / config.recoveryTime));
	        return Random.Range(-config.force, config.force) * curveValue;
	    }
	}
}