using Sanet.MagicalYatzy.Models.Game.Magical;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game.Magical
{
    public class ArtifactTests
    {
        Artifact _sut = new Artifact(Artifacts.ManualSet);
        
        [Fact]
        public void HasTypePassedToConstructor()
        {
            const Artifacts artifactType = Artifacts.ManualSet;
            var sut = new Artifact(artifactType);
            Assert.Equal(artifactType,sut.Type);
        }

        [Fact]
        public void NewArtifactIsNotUsed()
        {
            Assert.False(_sut.IsUsed);
        }

        [Fact]
        public void ArtifactIsUsedWhenUseIsCalled()
        {
            _sut.Use();
            Assert.True(_sut.IsUsed);
        }
    }
}