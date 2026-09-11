namespace Sanet.MagicalYatzy.Models.Game.Magical
{
    public class Artifact
    {
        public Artifact(Artifacts type)
        {
            Type = type;
        }

        public Artifacts Type { get; }

        public bool IsUsed { get; private set; }

        public void Use()
        {
            IsUsed = true;
        }
    }
}