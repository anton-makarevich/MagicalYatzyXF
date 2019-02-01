using NSubstitute;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Xunit;
using System;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services.Navigation;

namespace MagicalYatzyTests.ViewModelTests.Base
{
    public class BaseViewModelTests
    {
        private readonly INavigationService _navigationServiceMock;

        private readonly SimpleTestViewModel _sut;

        public BaseViewModelTests()
        {
            Substitute.For<IPlayerService>();
            _navigationServiceMock = Substitute.For<INavigationService>();

            _sut = new SimpleTestViewModel();
        }

        [Fact]
        public void IsBusyShouldBeFiredWhenValueIsChanged()
        {
            // Arrange
            const bool isBusyNewValue = true;
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
        public void NavigationServiceIsSet()
        {
            _sut.SetNavigationService(_navigationServiceMock);
            Assert.NotNull(_sut.NavigationService);
        }

        [Fact]
        public async Task GoBackTriggersNavigationServiceNavigateBack()
        {
            _sut.SetNavigationService(_navigationServiceMock);
            _sut.BackCommand.Execute(null);
            await _navigationServiceMock.Received().NavigateBackAsync();
        }
        
        [Fact]
        public async Task CloseTriggersNavigationServiceNavigateBack()
        {
            _sut.SetNavigationService(_navigationServiceMock);
            await _sut.CloseAsync();
            await _navigationServiceMock.Received().CloseAsync();
        }

        [Fact]
        public void ThrowsArgumentNullExceptionIfNavigationServiceIsNotSet()
        {
            Assert.Throws<ArgumentNullException>(() => { var _ = _sut.NavigationService; });
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
            _sut.SetNavigationService(_navigationServiceMock);
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
            _sut.SetNavigationService(_navigationServiceMock);

            await _sut.CloseAsync(result);
            Assert.Equal(0,getResultCount);
        }

        [Fact]
        public void HasNavigationServiceIfItIsPassedInConstructor()
        {
            var sut = new SimpleTestViewModel(_navigationServiceMock);
            
            Assert.NotNull(sut.NavigationService);
        }

        private class SimpleTestViewModel : BaseViewModel
        {
            public SimpleTestViewModel()
            {
            }

            public SimpleTestViewModel(INavigationService navigationService) : base(navigationService)
            {
            }
        }
        
        private class ResultStub
        {
        }
    }
}
