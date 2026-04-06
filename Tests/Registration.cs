using FluentAssertions;
using NUnit.Framework;
using ParabankParasoftAutomation.Pages;
using System;

namespace ParabankParasoftAutomation.Tests
{
    public class Registration : TestBase
    {
        [Test]
        public void Registration_Positive_ShouldCreateAccount()
        {
            var reg = new RegisterPage(Driver).Navigate(BaseUrl);
            var unique = DateTime.UtcNow.ToString("yyyyMMddHH");
            var username = $"testuser{unique}";

            reg.EnterFirstName("Test")
               .EnterLastName("User")
               .EnterAddress("123 Test St")
               .EnterCity("Testville")
               .EnterState("TS")
               .EnterZip("12345")
               .EnterPhone("1234567890")
               .EnterSsn("123-45-6789")
               .EnterUsername(username)
               .EnterPassword("Password123")
               .EnterConfirm("Password123");

            var home = reg.SubmitAndExpectSuccess();

            home.IsUserLoggedIn().Should().BeTrue();
        }

        [Test]
        public void Registration_Negative_PasswordMismatch_ShowsError()
        {
            var reg = new RegisterPage(Driver).Navigate(BaseUrl);

            reg.EnterFirstName("Test")
               .EnterLastName("User")
               .EnterAddress("123 Test St")
               .EnterCity("Testville")
               .EnterState("TS")
               .EnterZip("12345")
               .EnterPhone("1234567890")
               .EnterSsn("123-45-6789")
               .EnterUsername("someuser")
               .EnterPassword("Password123")
               .EnterConfirm("DifferentPass");

            reg.SubmitExpectFailure();

            reg.IsFirstNamePresent().Should().BeTrue();
        }
    }
}
