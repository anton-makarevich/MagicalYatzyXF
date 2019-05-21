using Sanet.MagicalYatzy.ViewModels.Base;
using Xunit;

namespace MagicalYatzyTests.ViewModels.Base
{
    public class BindableBaseTests
    {
        private readonly TestBindableBaseChild _sut = new TestBindableBaseChild();
        
        [Fact]
        public void SetPropertyShouldFirePropertyChangedEvent()
        {
            // Arrange
            var testPropertyName = "TestProperty";
            var isPropertyFired = false;
            var isPropertyNameCorrect = false;
            _sut.PropertyChanged += (s, e) =>
            {
                isPropertyFired = true;
                isPropertyNameCorrect = e.PropertyName == testPropertyName;
            };
            // Act
            _sut.SetProperty(testPropertyName);

            // Assert
            Assert.True(isPropertyFired);
            Assert.True(isPropertyNameCorrect);
        }

        private class TestBindableBaseChild: BindableBase
        {
            private string testPropetyField;
            public void SetProperty(string propertyName)
            {
                SetProperty<string>(ref testPropetyField, "some value", propertyName);
            }
        }
    }
}
