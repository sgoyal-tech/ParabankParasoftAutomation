using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParabankParasoftAutomation.Pages;

/// <summary>
/// Page Object Model for the ParaBank Home / Accounts Overview page,
/// shown to users after a successful login.
/// </summary>
public class HomePage : BasePage
{
    // ─── Locators ─────────────────────────────────────────────────────────────
    private readonly By _pageTitle = By.CssSelector(".title");
    private readonly By _logoutLink = By.LinkText("Log Out");
    private readonly By _leftPanel = By.Id("leftPanel");
    private readonly By _userFullName = By.CssSelector("#leftPanel p.smallText");
    private readonly By _accountsOverview = By.LinkText("Accounts Overview");

    public HomePage(IWebDriver driver) : base(driver)
    {
        // Wait for the authenticated page to be ready
        WaitForElement(_logoutLink);
    }

    // ─── State ────────────────────────────────────────────────────────────────

    public bool IsUserLoggedIn() =>
        IsElementPresent(_logoutLink) && IsElementPresent(_leftPanel);

    public bool IsLogoutLinkVisible() => IsElementPresent(_logoutLink);
    public bool IsAccountServicesPanelVisible() => IsElementPresent(_leftPanel);

    public string GetPageTitle() => WaitForElement(_pageTitle).Text.Trim();

    public string GetWelcomeText()
    {
        try { return WaitForElement(_userFullName).Text.Trim(); }
        catch { return string.Empty; }
    }

    public string CurrentUrl => Driver.Url;

    // ─── Actions ──────────────────────────────────────────────────────────────

    public LoginPage Logout()
    {
        WaitForElement(_logoutLink).Click();
        return new LoginPage(Driver);
    }
}

