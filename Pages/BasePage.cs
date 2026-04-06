using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reports;

namespace ParabankParasoftAutomation.Pages;

public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver, int timeoutSec = 10)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSec));
    }

    protected IWebElement WaitForElement(By locator)
    {
        return Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
    }

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

    protected void WaitForTitleContaining(string fragment)
    {
        Wait.Until(d => d.Title.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    // Report helpers
    protected void LogMethod(string name)
    {
        ReportManager.StartMethod(name);
    }

    protected void LogStep(string msg)
    {
        ReportManager.TestStep(msg);
    }
}
