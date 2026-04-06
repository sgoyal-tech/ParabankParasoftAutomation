using OpenQA.Selenium;

namespace ParabankParasoftAutomation.Pages;

/// <summary>
/// Accounts Overview / Home page shown after login.
/// </summary>
public class HomePage : BasePage
{
    private readonly By _pageTitle = By.CssSelector(".title");
    private readonly By _logoutLink = By.LinkText("Log Out");
    private readonly By _leftPanel = By.Id("leftPanel");
    private readonly By _userFullName = By.CssSelector("#leftPanel p.smallText");

    public HomePage(IWebDriver driver) : base(driver)
    {
        WaitForElement(_logoutLink);
    }

    public bool IsUserLoggedIn()
    {
        return IsElementPresent(_logoutLink) && IsElementPresent(_leftPanel);
    }

    public bool IsLogoutLinkVisible()
    {
        return IsElementPresent(_logoutLink);
    }

    public bool IsAccountServicesPanelVisible()
    {
        return IsElementPresent(_leftPanel);
    }

    public string GetPageTitle()
    {
        return WaitForElement(_pageTitle).Text.Trim();
    }

    public string GetWelcomeText()
    {
        try
        {
            return WaitForElement(_userFullName).Text.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    public string CurrentUrl
    {
        get
        {
            return Driver.Url;
        }
    }

    public LoginPage Logout()
    {
        LogMethod(nameof(Logout));
        LogStep("Clicking Log Out");
        WaitForElement(_logoutLink).Click();
        return new LoginPage(Driver);
    }
}

