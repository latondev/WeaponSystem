using System.Collections.Generic;

namespace PullGame
{
    public interface IMuzzleSystem
    {
        void Initialize();
        IReadOnlyList<FirePoint> GetFirePoints();
        void DrawGizmos();
    }
}
