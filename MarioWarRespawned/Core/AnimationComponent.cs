using Microsoft.Xna.Framework;

namespace MarioWarRespawned.Core
{
    public class AnimationComponent : IComponent
    {
        public Entity Owner { get; set; }
        public Dictionary<string, Animation> Animations { get; set; } = new();
        public Animation CurrentAnimation { get; set; }
        public string CurrentAnimationName { get; set; }
        public float AnimationTimer { get; set; }

        public void PlayAnimation(string name)
        {
            if (Animations.TryGetValue(name, out var animation) && CurrentAnimationName != name)
            {
                CurrentAnimation = animation;
                CurrentAnimationName = name;
                AnimationTimer = 0f;
            }
        }
    }

    public class Animation
    {
        public Rectangle[] Frames { get; set; }
        public float FrameDuration { get; set; }
        public bool IsLooping { get; set; } = true;
        public int CurrentFrame { get; set; }
    }
}
