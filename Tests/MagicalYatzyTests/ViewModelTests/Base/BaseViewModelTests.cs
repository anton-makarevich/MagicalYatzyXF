using NSubstitute;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.ViewModels.Base;
using Xunit;
using System;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services.Navigation;

namespace MagicalYatzyTests.ViewModelTests.Base
{
    public class BaseViewModelTests
    {
        protected IPlayerService playerServiceMock;
        protected INavigationService navigationServiceMock;

        private SimpleTestViewModel _sut;

        public BaseViewModelTests()
        {
            playerServiceMock = Substitute.For<IPlayerService>();
            navigationServiceMock = Substitute.For<INavigationService>();

            _sut = new SimpleTestViewModel();
        }

        [Fact]
        public void IsBusyShouldBeFiredWhenValueIsChanged()
        {
            // Arrange
            var isBusyNewValue = true;
            var isPropertyFired = false;
            var isPropertyNameCorrect = false;
            var isPropertyValueCorrect = false;
            _sut.PropertyChanged += (s, e) =>
            {
                isPropertyFired = true;
                isPropertyNameCorrect = e.PropertyName == "IsBusy";
                isPropertyValueCorrect = _sut.IsBusy == isBusyNewValue;
            };

            // Act
            _sut.IsBusy = isBusyNewValue;

            // Assert
            Assert.True(isPropertyFired);
            Assert.True(isPropertyNameCorrect);
            Assert.True(isPropertyValueCorrect);
        }

        [Fact]
        public void PageWidthShouldBeFiredWhenValueIsChanged()
        {
            // Arrange
            var pageWidthNewValue = 200;
            var isPropertyFired = false;
            var isPropertyNameCorrect = false;
            var isPropertyValueCorrect = false;
            _sut.PropertyChanged += (s, e) =>
            {
                isPropertyFired = true;
                isPropertyNameCorrect = e.PropertyName == "PageWidth";
                isPropertyValueCorrect = _sut.PageWidth == pageWidthNewValue;
            };

            // Act
            _sut.PageWidth = pageWidthNewValue;

            // Assert
            Assert.True(isPropertyFired);
            Assert.True(isPropertyNameCorrect);
            Assert.True(isPropertyValueCorrect);
        }


        [Fact]
        public void NavgationServiceShouldBeSet()
        {
            _sut.SetNavigationService(navigationServiceMock);
            Assert.NotNull(_sut.NavigationService);
        }

        [Fact]
        public async Task GoBackTriggersNavigationServiceNavigateBack()
        {
            _sut.SetNavigationService(navigationServiceMock);
            _sut.BackCommand.Execute(null);
            await navigationServiceMock.Received().NavigateBackAsync();
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionIfNavigationServiceIsNotSet()
        {
            Assert.Throws<ArgumentNullException>(() => { var t = _sut.NavigationService; });
        }

        [Fact]
        public async Task InvokesOnResultIfResultIsExpectedAndResetsResultExpectation()
        {
            var result = new ResultStub();
            var getResultCount = 0;
            _sut.OnResult += (sender, o) =>
            {
                getResultCount++;
                Assert.Equal(result, o);
            };
            _sut.SetNavigationService(navigationServiceMock);
            _sut.ExpectsResult = true;

            await _sut.CloseAsync(result);
            Assert.Equal(1,getResultCount);
            Assert.False(_sut.ExpectsResult);
        }
        
        [Fact]
        public async Task DoesNotInvokeOnResultIfResultIsNotExpected()
        {
            var result = new ResultStub();
            var getResultCount = 0;
            _sut.OnResult += (sender, o) =>
            {
                getResultCount++;
                Assert.Equal(result, o);
            };
            _sut.SetNavigationService(navigationServiceMock);

            await _sut.CloseAsync(result);
            Assert.Equal(0,getResultCount);
        }

        private class SimpleTestViewModel : BaseViewModel { }
        
        private class ResultStub
        {
            public int Id => new Random().Next(100);
        }
    }
}
