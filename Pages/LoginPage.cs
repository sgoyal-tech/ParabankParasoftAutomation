using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParabankParasoftAutomation.Pages;

/// <summary>
/// Page Object Model for the ParaBank Login page.
/// Exposes fluent-style methods for test readability.
/// URL: https://parabank.parasoft.com/parabank/index.htm
/// </summary>
public class LoginPage : BasePage
{
    // ─── Locators ─────────────────────────────────────────────────────────────
    private readonly By _usernameInput = By.Name("username");
    private readonly By _passwordInput = By.Name("password");
    private readonly By _loginButton = By.XPath("//input[@value='Log In']");
    private readonly By _errorMessage = By.CssSelector(".error");
    private readonly By _registerLink = By.LinkText("Register");
    private readonly By _forgotLink = By.LinkText("Forgot login info?");
    private readonly By _loginPanel = By.Id("loginPanel");

    public LoginPage(IWebDriver driver) : base(driver) { }

    // ─── Navigation ───────────────────────────────────────────────────────────

    public LoginPage Navigate(string baseUrl)
    {
        Driver.Navigate().GoToUrl($"{baseUrl}/parabank/index.htm");
        WaitForElement(_loginPanel);
        return this;
    }

    // ─── Actions ──────────────────────────────────────────────────────────────

    public LoginPage EnterUsername(string username)
    {
        var field = WaitForElement(_usernameInput);
        field.Clear();
        field.SendKeys(username);
        return this;
    }

    public LoginPage EnterPassword(string password)
    {
        var field = WaitForElement(_passwordInput);
        field.Clear();
        field.SendKeys(password);
        return this;
    }

    /// <summary>
    /// Clicks Login and returns a HomePage, expecting navigation to succeed.
    /// </summary>
    public HomePage ClickLoginAndExpectSuccess()
    {
        WaitForElement(_loginButton).Click();
        return new HomePage(Driver);
    }

    /// <summary>
    /// Clicks Login and stays on LoginPage, expecting an authentication failure.
    /// </summary>
    public LoginPage ClickLoginAndExpectFailure()
    {
        WaitForElement(_loginButton).Click();
        return this;
    }

    // ─── Assertions / State ───────────────────────────────────────────────────

    public bool IsLoginFormVisible() =>
        IsElementPresent(_usernameInput) &&
        IsElementPresent(_passwordInput) &&
        IsElementPresent(_loginButton);

    public bool IsErrorDisplayed() => IsElementPresent(_errorMessage);

    public string GetErrorMessage() => WaitForElement(_errorMessage).Text.Trim();

    public string GetPasswordInputType() =>
        WaitForElement(_passwordInput).GetAttribute("type") ?? string.Empty;

    public string GetUsernameInputType() =>
        WaitForElement(_usernameInput).GetAttribute("type") ?? string.Empty;

    public bool IsRegisterLinkVisible() => IsElementPresent(_registerLink);
    public bool IsForgotLinkVisible() => IsElementPresent(_forgotLink);

    public string CurrentUrl => Driver.Url;
}

