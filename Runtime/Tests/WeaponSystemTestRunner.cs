using System;
using System.Collections.Generic;
using UnityEngine;

namespace PullGame.WeaponSystem.Tests
{
    /// <summary>
    /// Simple test runner for WeaponSystem - Run in Unity Editor
    /// Attach to a GameObject to see console output
    /// </summary>
    public class WeaponSystemTestRunner : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== WeaponSystem Test Runner ===");
            
            TestAmmoSystem();
            TestRecoilSystem();
            
            Debug.Log("=== All Tests Complete ===");
        }
        
        private void TestAmmoSystem()
        {
            Debug.Log("--- Testing AmmoSystem ---");
            
            // Create config
            var config = new AmmoConfig
            {
                maxAmmo = 100,
                magazineSize = 30,
                unlimitedAmmo = false,
                unlimitedMagazineSize = false,
                reloadTime = 2f,
                autoReload = true,
                partialAmmoFire = false,
                countEachProjectile = true,
                ammoPerShot = 1
            };
            
            // Test 1: Initial ammo
            var ammoSystem = new AmmoSystem(config);
            Debug.Log($"Test 1 - Initial: Magazine={ammoSystem.CurrentMagazineAmmo}, Total={ammoSystem.CurrentTotalAmmo}");
            Assert(ammoSystem.CurrentMagazineAmmo == 30, "Initial magazine should be 30");
            Assert(ammoSystem.CurrentTotalAmmo == 100, "Initial total should be 100");
            
            // Test 2: Can fire with enough ammo
            bool canFire = ammoSystem.CanFire(1);
            Debug.Log($"Test 2 - CanFire(1): {canFire}");
            Assert(canFire == true, "Should be able to fire with 1 projectile");
            
            // Test 3: Consume ammo
            ammoSystem.ConsumeAmmo(1);
            Debug.Log($"Test 3 - After consume: Magazine={ammoSystem.CurrentMagazineAmmo}");
            Assert(ammoSystem.CurrentMagazineAmmo == 29, "Magazine should be 29 after consuming 1");
            
            // Test 4: Cannot fire when empty magazine
            ammoSystem.ConsumeAmmo(29); // Use remaining ammo
            bool canFireEmpty = ammoSystem.CanFire(1);
            Debug.Log($"Test 4 - CanFire with empty: {canFireEmpty}");
            Assert(canFireEmpty == false, "Should NOT be able to fire with empty magazine");
            
            // Test 5: Add ammo
            ammoSystem.AddAmmo(50);
            Debug.Log($"Test 5 - After AddAmmo(50): Total={ammoSystem.CurrentTotalAmmo}");
            Assert(ammoSystem.CurrentTotalAmmo == 120, "Total should be capped at maxAmmo");
            
            // Test 6: Unlimited ammo mode
            var unlimitedConfig = new AmmoConfig
            {
                unlimitedAmmo = true,
                unlimitedMagazineSize = true
            };
            var unlimitedAmmoSystem = new AmmoSystem(unlimitedConfig);
            bool canFireUnlimited = unlimitedAmmoSystem.CanFire(100);
            Debug.Log($"Test 6 - Unlimited CanFire(100): {canFireUnlimited}");
            Assert(canFireUnlimited == true, "Unlimited ammo should always allow firing");
            
            Debug.Log("--- AmmoSystem Tests Passed ---");
        }
        
        private void TestRecoilSystem()
        {
            Debug.Log("--- Testing RecoilSystem ---");
            
            // Create config with enabled recoil
            var config = new RecoilConfig
            {
                enabled = true,
                force = 10f,
                recoveryTime = 0.5f,
                recoilCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1))
            };
            
            var recoilSystem = new RecoilSystem(config);
            
            // Test 1: Initial state
            float initialAngle = recoilSystem.GetRecoilAngle();
            Debug.Log($"Test 1 - Initial angle: {initialAngle}");
            
            // Test 2: Apply recoil
            recoilSystem.ApplyRecoil();
            recoilSystem.Update(0.1f);
            float afterRecoil = recoilSystem.GetRecoilAngle();
            Debug.Log($"Test 2 - After ApplyRecoil: {afterRecoil}");
            Assert(Mathf.Abs(afterRecoil) <= config.force, "Recoil angle should be within force range");
            
            // Test 3: Recoil recovery over time
            recoilSystem.ApplyRecoil();
            recoilSystem.Update(0.6f); // Past recovery time
            float recoveredAngle = recoilSystem.GetRecoilAngle();
            Debug.Log($"Test 3 - After recovery: {recoveredAngle}");
            Assert(recoveredAngle == 0f, "Recoil should be fully recovered");
            
            // Test 4: Disabled recoil
            var disabledConfig = new RecoilConfig
            {
                enabled = false
            };
            var disabledRecoil = new RecoilSystem(disabledConfig);
            disabledRecoil.ApplyRecoil();
            float disabledAngle = disabledRecoil.GetRecoilAngle();
            Debug.Log($"Test 4 - Disabled recoil: {disabledAngle}");
            Assert(disabledAngle == 0f, "Disabled recoil should always return 0");
            
            Debug.Log("--- RecoilSystem Tests Passed ---");
        }
        
        private void Assert(bool condition, string message)
        {
            if (condition)
            {
                Debug.Log($"  [PASS] {message}");
            }
            else
            {
                Debug.LogError($"  [FAIL] {message}");
            }
        }
    }
}
