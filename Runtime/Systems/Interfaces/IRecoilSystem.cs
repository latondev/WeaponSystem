using System;

namespace PullGame
{
    public interface IRecoilSystem
    {
        void Update(float deltaTime);
        void ApplyRecoil();
        float GetRecoilAngle();
    }
}
