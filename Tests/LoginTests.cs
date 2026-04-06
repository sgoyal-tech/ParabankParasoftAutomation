using FluentAssertions;
using NUnit.Framework;
using ParabankParasoftAutomation.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace ParabankParasoftAutomation.Tests
{
    public class LoginTests : TestBase
    {
        [Test]
        public void Login_Positive_ShouldAuthenticateAndShowHome()
        {
            var login = new LoginPage(Driver).Navigate(BaseUrl);
            login.IsLoginFormVisible().Should().BeTrue();

            login.EnterUsername("john").EnterPassword("demo");
            var home = login.ClickLoginAndExpectSuccess();

            home.IsUserLoggedIn().Should().BeTrue();
            home.GetPageTitle().Should().Contain("Accounts Overview");

            var back = home.Logout();
            back.IsLoginFormVisible().Should().BeTrue();
        }

        [Test]
        public void Login_Negative_WrongPassword_ShowsError()
        {
            var login = new LoginPage(Driver).Navigate(BaseUrl);
            login.IsLoginFormVisible().Should().BeTrue();

            login.EnterUsername("john").EnterPassword("Admin");
            login.ClickLoginAndExpectFailure();

            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => login.IsErrorDisplayed());

            login.IsErrorDisplayed().Should().BeTrue();
            var msg = login.GetErrorMessage();
            msg.Should().NotBeNullOrWhiteSpace();

            // Check the error text matches expected patterns
            var containsKnown = msg.Contains("could not be verified", StringComparison.OrdinalIgnoreCase)
                                || msg.Contains("The username and password", StringComparison.OrdinalIgnoreCase)
                                || msg.Contains("invalid", StringComparison.OrdinalIgnoreCase);

            containsKnown.Should().BeTrue("an authentication error message should be shown for invalid credentials. Actual: '{0}'", msg);
        }

        [Test]
        public void Login_Negative_EmptyCredentials_ShowsValidation()
        {
            var login = new LoginPage(Driver).Navigate(BaseUrl);
            login.EnterUsername("").EnterPassword("");
            login.ClickLoginAndExpectFailure();

            login.IsErrorDisplayed().Should().BeTrue();
        }
    }
}
