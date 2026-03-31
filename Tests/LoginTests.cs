using FluentAssertions;
using NUnit.Framework;
using ParabankParasoftAutomation.Pages;

namespace ParabankParasoftAutomation.Tests
{
    public class LoginTests : TestBase
    {
        [Test]
        public void Login_Positive_ShouldAuthenticateAndShowHome()
        {
            var login = new LoginPage(Driver).Navigate(BaseUrl);
            login.IsLoginFormVisible().Should().BeTrue();

            // Known demo credentials from the site
            login.EnterUsername("john").EnterPassword("demo");
            var home = login.ClickLoginAndExpectSuccess();

            home.IsUserLoggedIn().Should().BeTrue();
            home.GetPageTitle().Should().Contain("Accounts Overview");

            // logout to restore state
            var back = home.Logout();
            back.IsLoginFormVisible().Should().BeTrue();
        }

        [Test]
        public void Login_Negative_WrongPassword_ShowsError()
        {
            var login = new LoginPage(Driver).Navigate(BaseUrl);
            login.EnterUsername("john").EnterPassword("wrongpass");
            login.ClickLoginAndExpectFailure();

            login.IsErrorDisplayed().Should().BeTrue();
            login.GetErrorMessage().Should().Contain("The username and password could not be verified");
        }

        [Test]
        public void Login_Negative_EmptyCredentials_ShowsValidation()
        {
            var login = new LoginPage(Driver).Navigate(BaseUrl);
            login.EnterUsername("").EnterPassword("");
            login.ClickLoginAndExpectFailure();

            // The app does not show fancy client-side messages, but login will fail
            login.IsErrorDisplayed().Should().BeTrue();
        }
    }
}
