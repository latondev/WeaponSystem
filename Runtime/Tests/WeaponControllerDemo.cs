using UnityEngine;

namespace PullGame.Demo
{
    /// <summary>
    /// Demo controller showing how to use Weapon with dependency injection
    /// This example demonstrates loose coupling through interfaces
    /// </summary>
    public class WeaponControllerDemo : MonoBehaviour
    {
        [Header("Weapon Setup")]
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private Weapon weapon;
        
        [Header("Debug Info")]
        [SerializeField] private bool showDebugInfo = true;
        
        // References to systems via interfaces (for external access)
        private IAmmoSystem _ammoSystem;
        private IRecoilSystem _recoilSystem;
        
        private void Start()
        {
            // Get ammo system reference for UI or other systems
            _ammoSystem = weapon.Ammo;
            
            if (showDebugInfo)
            {
                Debug.Log("=== Weapon Controller Demo ===");
                Debug.Log($"Weapon: {weaponConfig?.weaponName ?? "No config"}");
                Debug.Log("Controls: Mouse0 = Fire, R = Reload");
            }
            
            // Subscribe to events
            weapon.onShoot += OnWeaponShoot;
            weapon.onReloadStart += OnReloadStart;
            weapon.onReloadEnd += OnReloadEnd;
        }
        
        private void Update()
        {
            // Using SetFireInput for programmatic control
            // In real usage, you might use input system instead
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddAmmo(30);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ShowAmmoInfo();
            }
        }
        
        private void OnWeaponShoot()
        {
            if (showDebugInfo)
            {
                Debug.Log($"Bang! Ammo: {_ammoSystem.CurrentMagazineAmmo}/{_ammoSystem.CurrentTotalAmmo}");
            }
        }
        
        private void OnReloadStart()
        {
            if (showDebugInfo)
            {
                Debug.Log("Reloading...");
            }
        }
        
        private void OnReloadEnd()
        {
            if (showDebugInfo)
            {
                Debug.Log($"Reload Complete! Ammo: {_ammoSystem.CurrentMagazineAmmo}/{_ammoSystem.CurrentTotalAmmo}");
            }
        }
        
        /// <summary>
        /// Example: Adding ammo through the weapon
        /// </summary>
        public void AddAmmo(int amount)
        {
            weapon.AddAmmo(amount);
            Debug.Log($"Added {amount} ammo. Total: {_ammoSystem.CurrentTotalAmmo}");
        }
        
        /// <summary>
        /// Example: Show current ammo status
        /// </summary>
        public void ShowAmmoInfo()
        {
            Debug.Log($"=== Ammo Info ===" +
                     $"\nMagazine: {_ammoSystem.CurrentMagazineAmmo}/{_ammoSystem.MagazineSize}" +
                     $"\nReserve: {_ammoSystem.CurrentTotalAmmo}" +
                     $"\nReloading: {_ammoSystem.IsReloading}");
        }
        
        /// <summary>
        /// Example: Check if can fire
        /// </summary>
        public bool CanFire()
        {
            return !_ammoSystem.IsReloading && _ammoSystem.CurrentMagazineAmmo > 0;
        }
    }
}
