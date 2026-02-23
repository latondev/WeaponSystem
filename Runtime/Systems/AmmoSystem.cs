using System;
using System.Collections;
using UnityEngine;

namespace PullGame
{
	public class AmmoSystem
	{
	    private readonly AmmoConfig config;
	    
	    private int currentMagazineAmmo;
	    private int currentTotalAmmo;
	    private bool isReloading;
	    
	    public event Action OnReloadStart;
	    public event Action OnReloadEnd;
	    public event Action<int, int> OnAmmoChanged;
	    
	    public int CurrentMagazineAmmo => currentMagazineAmmo;
	    public int CurrentTotalAmmo => currentTotalAmmo;
	    public int MagazineSize => config.magazineSize;
	    public bool IsReloading => isReloading;
	    
	    public AmmoSystem(AmmoConfig config)
	    {
	        this.config = config;
	        
	        currentMagazineAmmo = config.unlimitedMagazineSize ? int.MaxValue : config.magazineSize;
	        currentTotalAmmo = config.unlimitedAmmo ? int.MaxValue : config.maxAmmo;
	    }
	    
	    public bool CanFire(int projectilesNeeded)
	    {
	        if (config.unlimitedAmmo && config.unlimitedMagazineSize)
	            return true;
	            
	        int ammoRequired = CalculateAmmoRequired(projectilesNeeded);
	        
	        if (!config.unlimitedMagazineSize)
	        {
	            if (config.partialAmmoFire)
	                return currentMagazineAmmo > 0;
	            else
	                return currentMagazineAmmo >= ammoRequired;
	        }
	        else if (!config.unlimitedAmmo)
	        {
	            if (config.partialAmmoFire)
	                return currentTotalAmmo > 0;
	            else
	                return currentTotalAmmo >= ammoRequired;
	        }
	        
	        return true;
	    }
	    
	    public void ConsumeAmmo(int projectilesFired)
	    {
	        if (config.unlimitedAmmo && config.unlimitedMagazineSize)
	            return;
	            
	        int ammoToConsume = config.countEachProjectile ? projectilesFired : config.ammoPerShot;
	        
	        if (!config.unlimitedMagazineSize)
	        {
	            currentMagazineAmmo = Mathf.Max(0, currentMagazineAmmo - ammoToConsume);
	        }
	        else if (!config.unlimitedAmmo)
	        {
	            currentTotalAmmo = Mathf.Max(0, currentTotalAmmo - ammoToConsume);
	        }
	        
	        OnAmmoChanged?.Invoke(currentMagazineAmmo, currentTotalAmmo);
	    }
	    
	    private int CalculateAmmoRequired(int projectilesNeeded)
	    {
	        return config.countEachProjectile ? projectilesNeeded : config.ammoPerShot;
	    }
	    
	    public bool TryReload()
	    {
	        if (isReloading) return false;
	        if (config.unlimitedMagazineSize) return false;
	        if (currentMagazineAmmo >= config.magazineSize) return false;
	        if (!config.unlimitedAmmo && currentTotalAmmo <= 0) return false;
	        
	        OnReloadStart?.Invoke();
	        return true;
	    }
	    
	    public IEnumerator PerformReload(MonoBehaviour coroutineRunner)
	    {
	        isReloading = true;
	        
	        yield return new WaitForSeconds(config.reloadTime);
	        
	        if (config.unlimitedAmmo)
	        {
	            currentMagazineAmmo = config.magazineSize;
	        }
	        else
	        {
	            int neededAmmo = config.magazineSize - currentMagazineAmmo;
	            int ammoToLoad = Mathf.Min(neededAmmo, currentTotalAmmo);
	            
	            currentMagazineAmmo += ammoToLoad;
	            currentTotalAmmo -= ammoToLoad;
	        }
	        
	        isReloading = false;
	        OnReloadEnd?.Invoke();
	        OnAmmoChanged?.Invoke(currentMagazineAmmo, currentTotalAmmo);
	    }
	    
	    public void AddAmmo(int amount)
	    {
	        if (config.unlimitedAmmo) return;
	        
	        currentTotalAmmo = Mathf.Min(currentTotalAmmo + amount, config.maxAmmo);
	        OnAmmoChanged?.Invoke(currentMagazineAmmo, currentTotalAmmo);
	    }
	}
}