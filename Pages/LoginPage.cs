using OpenQA.Selenium;

namespace ParabankParasoftAutomation.Pages;

public class LoginPage : BasePage
{
    private readonly By _usernameInput = By.Name("username");
    private readonly By _passwordInput = By.Name("password");
    private readonly By _loginButton = By.XPath("//input[@value='Log In']");
    private readonly By _errorMessage = By.CssSelector(".error");
    private readonly By _registerLink = By.LinkText("Register");
    private readonly By _forgotLink = By.LinkText("Forgot login info?");
    private readonly By _loginPanel = By.Id("loginPanel");

    public LoginPage(IWebDriver driver) : base(driver)
    {
    }

    public LoginPage Navigate(string baseUrl)
    {
        LogStep("Opening login page");
        Driver.Navigate().GoToUrl($"{baseUrl}/parabank/index.htm");
        WaitForElement(_loginPanel);
        return this;
    }

    public LoginPage EnterUsername(string username)
    {
        LogStep($"Typing username: {username}");
        var el = WaitForElement(_usernameInput);
        el.Clear();
        el.SendKeys(username);
        return this;
    }

    public LoginPage EnterPassword(string password)
    {
        LogStep("Typing password");
        var el = WaitForElement(_passwordInput);
        el.Clear();
        el.SendKeys(password);
        return this;
    }

    public HomePage ClickLoginAndExpectSuccess()
    {
        LogStep("Clicking Log In (expect success)");
        WaitForElement(_loginButton).Click();
        return new HomePage(Driver);
    }

    public LoginPage ClickLoginAndExpectFailure()
    {
        LogStep("Clicking Log In (expect failure)");
        WaitForElement(_loginButton).Click();
        return this;
    }

    public bool IsLoginFormVisible()
    {
        return IsElementPresent(_usernameInput)
            && IsElementPresent(_passwordInput)
            && IsElementPresent(_loginButton);
    }

    public bool IsErrorDisplayed()
    {
        return IsElementPresent(_errorMessage);
    }

    public string GetErrorMessage()
    {
        return WaitForElement(_errorMessage).Text.Trim();
    }

    public string GetPasswordInputType()
    {
        return WaitForElement(_passwordInput).GetAttribute("type") ?? "";
    }

    public string GetUsernameInputType()
    {
        return WaitForElement(_usernameInput).GetAttribute("type") ?? "";
    }

    public bool IsRegisterLinkVisible()
    {
        return IsElementPresent(_registerLink);
    }

    public bool IsForgotLinkVisible()
    {
        return IsElementPresent(_forgotLink);
    }

    public string CurrentUrl
    {
        get
        {
            return Driver.Url;
        }
    }
}

