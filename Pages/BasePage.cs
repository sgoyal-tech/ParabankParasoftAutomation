using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reports;

namespace ParabankParasoftAutomation.Pages;

public abstract class BasePage
{
    
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver, int timeoutSeconds = 10)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
    }

    /// <summary>
    /// Waits for an element to be visible in the DOM before returning it.
    /// Throws if the element is not found within the configured timeout.
    /// </summary>
    protected IWebElement WaitForElement(By locator)
    {
        return Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
    }

    /// <summary>
    /// Safely checks whether an element exists in the DOM without throwing.
    /// </summary>
    protected bool IsElementPresent(By locator)
    {
        try
        {
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Waits until the page title contains the expected substring.
    /// </summary>
    protected void WaitForTitleContaining(string titleFragment)
    {
        Wait.Until(d => d.Title.Contains(titleFragment, StringComparison.OrdinalIgnoreCase));
    }

    protected void LogMethodStart(string methodName)
    {
        ReportManager.StartMethod(methodName);
    }

    protected void LogStep(string message)
    {
        ReportManager.TestStep(message);
    }

}
