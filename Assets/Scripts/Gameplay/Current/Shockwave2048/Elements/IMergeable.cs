namespace Gameplay.Current.Shockwave2048.Elements
{
    public interface IMergeable
    {
        void Init(ElementData elementData);
        void PlayMergeEffect();
        void StopMergeEffect();
    }
}