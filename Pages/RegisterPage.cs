using OpenQA.Selenium;

namespace ParabankParasoftAutomation.Pages;

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

    // Saved credentials for fallback login after registration
    private string? _lastUsername;
    private string? _lastPassword;

    public RegisterPage(IWebDriver driver) : base(driver)
    {
        // No extra setup needed — page element waits happen in Navigate()
    }

    public RegisterPage Navigate(string baseUrl)
    {
        LogStep("Opening registration page");
        Driver.Navigate().GoToUrl($"{baseUrl}/parabank/register.htm");
        WaitForElement(_firstName);
        return this;
    }

                    public RegisterPage EnterFirstName(string v)
                    {
                        WaitForElement(_firstName).Clear();
                        WaitForElement(_firstName).SendKeys(v);
                        return this;
                    }

                    public RegisterPage EnterLastName(string v)
                    {
                        WaitForElement(_lastName).Clear();
                        WaitForElement(_lastName).SendKeys(v);
                        return this;
                    }

                    public RegisterPage EnterAddress(string v)
                    {
                        WaitForElement(_address).Clear();
                        WaitForElement(_address).SendKeys(v);
                        return this;
                    }

                    public RegisterPage EnterCity(string v)
                    {
                        WaitForElement(_city).Clear();
                        WaitForElement(_city).SendKeys(v);
                        return this;
                    }

                    public RegisterPage EnterState(string v)
                    {
                        WaitForElement(_state).Clear();
                        WaitForElement(_state).SendKeys(v);
                        return this;
                    }

                    public RegisterPage EnterZip(string v)
                    {
                        WaitForElement(_zip).Clear();
                        WaitForElement(_zip).SendKeys(v);
                        return this;
                    }

                    public RegisterPage EnterPhone(string v)
                    {
                        WaitForElement(_phone).Clear();
                        WaitForElement(_phone).SendKeys(v);
                        return this;
                    }

                    public RegisterPage EnterSsn(string v)
                    {
                        WaitForElement(_ssn).Clear();
                        WaitForElement(_ssn).SendKeys(v);
                        return this;
                    }

                    public RegisterPage EnterUsername(string v)
                    {
                        WaitForElement(_username).Clear();
                        WaitForElement(_username).SendKeys(v);
                        _lastUsername = v;
                        return this;
                    }

                    public RegisterPage EnterPassword(string v)
                    {
                        WaitForElement(_password).Clear();
                        WaitForElement(_password).SendKeys(v);
                        _lastPassword = v;
                        return this;
                    }

                    public RegisterPage EnterConfirm(string v)
                    {
                        WaitForElement(_confirm).Clear();
                        WaitForElement(_confirm).SendKeys(v);
                        return this;
                    }

                    public HomePage SubmitAndExpectSuccess()
                    {
                        LogStep("Submitting registration (expect success)");
                        WaitForElement(_registerButton).Click();

                        // Wait for the logged-in state to appear
                        try
                        {
                            var waitLong = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(20));
                            waitLong.Until(d => d.FindElements(By.LinkText("Log Out")).Count > 0);
                            return new HomePage(Driver);
                        }
                        catch
                        {
                            // Site didn't auto-login — try logging in with the credentials we just registered
                            if (!string.IsNullOrEmpty(_lastUsername) && !string.IsNullOrEmpty(_lastPassword))
                            {
                                var baseUrl = Driver.Url.Contains("parabank")
                                    ? Driver.Url.Replace("/parabank/register.htm", "").TrimEnd('/')
                                    : "https://parabank.parasoft.com";
                                var login = new LoginPage(Driver).Navigate(baseUrl);
                                login.EnterUsername(_lastUsername).EnterPassword(_lastPassword);
                                return login.ClickLoginAndExpectSuccess();
                            }

                            return new HomePage(Driver);
                        }
                    }

                    public RegisterPage SubmitExpectFailure()
                    {
                        LogStep("Submitting registration (expect validation error)");
                        WaitForElement(_registerButton).Click();
                        return this;
                    }

                    public string GetSuccessMessage()
                    {
                        return WaitForElement(_successMessage).Text.Trim();
                    }

                    public bool IsFirstNamePresent()
                    {
                        return IsElementPresent(_firstName);
                    }
                }
