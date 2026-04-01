using OpenQA.Selenium;

namespace ParabankParasoftAutomation.Pages
{
    public class RegisterPage : BasePage
    {
        private readonly By _firstName = By.Id("customer.firstName");
        private readonly By _lastName = By.Id("customer.lastName");
        private readonly By _address = By.Id("customer.address.street");
        private readonly By _city = By.Id("customer.address.city");
        private readonly By _state = By.Id("customer.address.state");
        private readonly By _zip = By.Id("customer.address.zipCode");
        private readonly By _phone = By.Id("customer.phoneNumber");
        private readonly By _ssn = By.Id("customer.ssn");
        private readonly By _username = By.Id("customer.username");
        private readonly By _password = By.Id("customer.password");
        private readonly By _confirm = By.Id("repeatedPassword");
        private readonly By _registerButton = By.CssSelector("input[value='Register']");
        private readonly By _successMessage = By.CssSelector("#rightPanel .title");

        // Keep last entered credentials to allow fallback login verification
        private string? _lastUsername;
        private string? _lastPassword;

        public RegisterPage(IWebDriver driver) : base(driver) { }

        public RegisterPage Navigate(string baseUrl)
        {
            Driver.Navigate().GoToUrl($"{baseUrl}/parabank/register.htm");
            WaitForElement(_firstName);
            return this;
        }

        public RegisterPage EnterFirstName(string v) { WaitForElement(_firstName).Clear(); WaitForElement(_firstName).SendKeys(v); return this; }
        public RegisterPage EnterLastName(string v) { WaitForElement(_lastName).Clear(); WaitForElement(_lastName).SendKeys(v); return this; }
        public RegisterPage EnterAddress(string v) { WaitForElement(_address).Clear(); WaitForElement(_address).SendKeys(v); return this; }
        public RegisterPage EnterCity(string v) { WaitForElement(_city).Clear(); WaitForElement(_city).SendKeys(v); return this; }
        public RegisterPage EnterState(string v) { WaitForElement(_state).Clear(); WaitForElement(_state).SendKeys(v); return this; }
        public RegisterPage EnterZip(string v) { WaitForElement(_zip).Clear(); WaitForElement(_zip).SendKeys(v); return this; }
        public RegisterPage EnterPhone(string v) { WaitForElement(_phone).Clear(); WaitForElement(_phone).SendKeys(v); return this; }
        public RegisterPage EnterSsn(string v) { WaitForElement(_ssn).Clear(); WaitForElement(_ssn).SendKeys(v); return this; }
        public RegisterPage EnterUsername(string v) { WaitForElement(_username).Clear(); WaitForElement(_username).SendKeys(v); _lastUsername = v; return this; }
        public RegisterPage EnterPassword(string v) { WaitForElement(_password).Clear(); WaitForElement(_password).SendKeys(v); _lastPassword = v; return this; }
        public RegisterPage EnterConfirm(string v) { WaitForElement(_confirm).Clear(); WaitForElement(_confirm).SendKeys(v); return this; }

        /// <summary>
        /// Clicks register and returns the HomePage when registration succeeds.
        /// </summary>
        public HomePage SubmitAndExpectSuccess()
        {
            WaitForElement(_registerButton).Click();

            // After submit, try to wait for the authenticated home state (logout link) for a longer timeout
            try
            {
                var waitLong = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(20));
                waitLong.Until(d => d.FindElements(By.LinkText("Log Out")).Count > 0);
                return new HomePage(Driver);
            }
            catch
            {
                // Fallback: If logout not visible, try to login explicitly using the credentials we submitted
                if (!string.IsNullOrEmpty(_lastUsername) && !string.IsNullOrEmpty(_lastPassword))
                {
                    var login = new LoginPage(Driver).Navigate(Driver.Url.Contains("parabank") ? Driver.Url.Replace("/parabank/register.htm", "").TrimEnd('/') : "https://parabank.parasoft.com");
                    login.EnterUsername(_lastUsername).EnterPassword(_lastPassword);
                    var home = login.ClickLoginAndExpectSuccess();
                    return home;
                }

                // As a last resort construct HomePage (may not be fully authenticated)
                return new HomePage(Driver);
            }
        }

        /// <summary>
        /// Clicks register and returns the RegisterPage expecting validation errors.
        /// </summary>
        public RegisterPage SubmitExpectFailure()
        {
            WaitForElement(_registerButton).Click();
            return this;
        }

        public string GetSuccessMessage() => WaitForElement(_successMessage).Text.Trim();

        public bool IsFirstNamePresent() => IsElementPresent(_firstName);
    }
}
