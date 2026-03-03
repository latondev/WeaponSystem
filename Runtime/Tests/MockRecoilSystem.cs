using System;

namespace PullGame.WeaponSystem.Tests
{
    /// <summary>
    /// Mock implementation of IRecoilSystem for testing
    /// </summary>
    public class MockRecoilSystem : IRecoilSystem
    {
        public int UpdateCallCount { get; private set; }
        public int ApplyRecoilCallCount { get; private set; }
        public float LastRecoilAngle { get; private set; }
        
        // Configurable behavior
        public float RecoilAngleToReturn { get; set; } = 5f;
        
        public void Update(float deltaTime)
        {
            UpdateCallCount++;
        }
        
        public void ApplyRecoil()
        {
            ApplyRecoilCallCount++;
        }
        
        public float GetRecoilAngle()
        {
            LastRecoilAngle = RecoilAngleToReturn;
            return RecoilAngleToReturn;
        }
        
        public void Reset()
        {
            UpdateCallCount = 0;
            ApplyRecoilCallCount = 0;
            LastRecoilAngle = 0;
        }
    }
}
