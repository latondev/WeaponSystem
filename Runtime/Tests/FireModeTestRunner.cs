using System;
using System.Collections.Generic;
using UnityEngine;
using PullGame.WeaponSystem.Tests;

namespace PullGame.WeaponSystem.Tests
{
    /// <summary>
    /// Test runner for FireMode systems
    /// Run in Unity Editor - attach to a GameObject
    /// </summary>
    public class FireModeTestRunner : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== FireMode Test Runner ===");
            
            TestSingleFireMode();
            TestAutoFireMode();
            TestBurstFireMode();
            
            Debug.Log("=== All FireMode Tests Complete ===");
        }
        
        private void TestSingleFireMode()
        {
            Debug.Log("--- Testing SingleFireMode ---");
            
            // Create config
            var config = new FireModeConfig
            {
                mode = FireMode.Single,
                fireRate = 0.1f,
                autoShooting = false
            };
            
            var fireMode = new SingleFireMode(config);
            var firePoints = CreateTestFirePoints(1);
            
            // Test 1: Initial state - can fire
            bool canFireInitial = fireMode.CanFire();
            Debug.Log($"Test 1 - Initial CanFire: {canFireInitial}");
            Assert(canFireInitial == true, "Should be able to fire initially");
            
            // Test 2: Fire and check cooldown
            int fired = fireMode.Fire(CreateContext(firePoints), (pos, rot) => { });
            fireMode.OnFireStarted();
            Debug.Log($"Test 2 - Fired count: {fired}");
            Assert(fired == 1, "Should fire 1 projectile");
            
            // Test 3: Cannot fire during cooldown
            bool canFireDuringCooldown = fireMode.CanFire();
            Debug.Log($"Test 3 - CanFire during cooldown: {canFireDuringCooldown}");
            Assert(canFireDuringCooldown == false, "Should NOT be able to fire during cooldown");
            
            // Test 4: Can fire after cooldown
            fireMode.Update(0.2f); // Pass fire rate time
            bool canFireAfterCooldown = fireMode.CanFire();
            Debug.Log($"Test 4 - CanFire after cooldown: {canFireAfterCooldown}");
            Assert(canFireAfterCooldown == true, "Should be able to fire after cooldown");
            
            Debug.Log("--- SingleFireMode Tests Passed ---");
        }
        
        private void TestAutoFireMode()
        {
            Debug.Log("--- Testing AutoFireMode ---");
            
            var config = new FireModeConfig
            {
                mode = FireMode.Auto,
                fireRate = 0.05f,
                autoShooting = true
            };
            
            var fireMode = new AutoFireMode(config);
            var firePoints = CreateTestFirePoints(1);
            
            // Test 1: Can fire initially
            bool canFire = fireMode.CanFire();
            Debug.Log($"Test 1 - Initial CanFire: {canFire}");
            Assert(canFire == true, "Auto mode should allow initial fire");
            
            // Test 2: Fire multiple times rapidly
            int totalFired = 0;
            for (int i = 0; i < 5; i++)
            {
                if (fireMode.CanFire())
                {
                    int fired = fireMode.Fire(CreateContext(firePoints), (pos, rot) => { });
                    fireMode.OnFireStarted();
                    totalFired += fired;
                }
                fireMode.Update(0.01f);
            }
            Debug.Log($"Test 2 - Total fired in rapid succession: {totalFired}");
            Assert(totalFired <= 2, "Should limit fire rate based on fireRate");
            
            Debug.Log("--- AutoFireMode Tests Passed ---");
        }
        
        private void TestBurstFireMode()
        {
            Debug.Log("--- Testing BurstFireMode ---");
            
            var config = new FireModeConfig
            {
                mode = FireMode.Burst,
                fireRate = 0.5f,
                burstCount = 3,
                timeBetweenShots = 0.1f,
                timeBetweenBursts = 1f,
                minSpreadAngle = -5f,
                maxSpreadAngle = 5f
            };
            
            // Note: BurstFireMode requires a MonoBehaviour for coroutines
            // We'll test the basic logic here
            var fireMode = new BurstFireMode(config, this);
            var firePoints = CreateTestFirePoints(1);
            
            // Test 1: Can fire initially
            bool canFireInitial = fireMode.CanFire();
            Debug.Log($"Test 1 - Initial CanFire: {canFireInitial}");
            Assert(canFireInitial == true, "Should be able to fire initially");
            
            // Test 2: Fire starts burst
            int fired = fireMode.Fire(CreateContext(firePoints), (pos, rot) => { });
            fireMode.OnFireStarted();
            Debug.Log($"Test 2 - Initial fire: {fired}");
            Assert(fired == 1, "Initial fire should spawn 1 projectile");
            
            // Test 3: Cannot fire during burst
            bool canFireDuringBurst = fireMode.CanFire();
            Debug.Log($"Test 3 - CanFire during burst: {canFireDuringBurst}");
            Assert(canFireDuringBurst == false, "Should NOT be able to fire during burst");
            
            Debug.Log("--- BurstFireMode Tests Passed ---");
        }
        
        // Helper methods
        private List<FirePoint> CreateTestFirePoints(int count)
        {
            var points = new List<FirePoint>();
            var mockTransform = new GameObject("TestMuzzle").transform;
            
            for (int i = 0; i < count; i++)
            {
                var point = new FirePoint(Vector3.zero, Vector3.right, mockTransform);
                points.Add(point);
            }
            
            return points;
        }
        
        private ShootContext CreateContext(List<FirePoint> firePoints)
        {
            return new ShootContext
            {
                firePoints = firePoints.ToArray(),
                projectilesNeeded = firePoints.Count,
                recoilAngle = 0f,
                spreadAngles = null
            };
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
