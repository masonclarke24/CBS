using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Text;
using Xunit;

namespace CBSAutomatedTests
{
    public class UITests: IDisposable
    {
        private readonly IWebDriver driver;
        private readonly Random random;

        public UITests()
        {
            FirefoxOptions options = new FirefoxOptions
            {
                AcceptInsecureCertificates = true
            };
            driver = new FirefoxDriver(options);
            random = new Random();
        }

        [Fact]
        public void Record_WhenSubmitted_SuccessMessage()
        {
            driver.Navigate().GoToUrl("https://localhost:5001/Memberships/RecordMembershipApplication");

            if (driver.Url != "https://localhost:5001/Memberships/RecordMembershipApplication")
            {
                driver.FindElement(By.Id("Input_Email")).SendKeys("memberships@cbg.ca");
                driver.FindElement(By.Id("Input_Password")).SendKeys("Baist123$");
                driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            }
            string firstName = RandomName();
            string lastName = RandomName();

            driver.FindElement(By.Id("LastName")).SendKeys(firstName);
            driver.FindElement(By.Id("FirstName")).SendKeys(lastName);
            driver.FindElement(By.Id("ApplicantAddress")).SendKeys("12345 67 St");
            driver.FindElement(By.Id("ApplicantPostalCode")).SendKeys("T1T 1T1");
            driver.FindElement(By.Id("ApplicantCity")).SendKeys("Edmonton");
            driver.FindElement(By.Id("ApplicantPhone")).SendKeys("4983069840");
            driver.FindElement(By.Id("ApplicantAlternatePhone")).SendKeys("4983386910");
            driver.FindElement(By.Id("Email")).SendKeys($"{firstName}.{lastName}@test.com".ToLower());
            driver.FindElement(By.Id("DateOfBirth")).Clear();
            driver.FindElement(By.Id("DateOfBirth")).SendKeys("05-Jan-1998");
            driver.FindElement(By.Id("Occupation")).SendKeys("Business Analyst");
            driver.FindElement(By.Id("CompanyName")).SendKeys("CGI");
            driver.FindElement(By.Id("EmployerAddress")).SendKeys("12345 Jasper Ave");
            driver.FindElement(By.Id("EmployerCity")).SendKeys("Edmonton");
            driver.FindElement(By.Id("EmployerPostalCode")).SendKeys("T2T 2T2");
            driver.FindElement(By.Id("EmployerPhone")).SendKeys("7809912176");
            driver.FindElement(By.Id("Shareholder1")).SendKeys(RandomName());
            driver.FindElement(By.Id("Shareholder2")).SendKeys(RandomName());
            driver.FindElement(By.Id("Shareholder1SigningDate")).Clear();
            driver.FindElement(By.Id("Shareholder1SigningDate")).SendKeys("30-Jan-2020");
            driver.FindElement(By.Id("Shareholder2SigningDate")).Clear();
            driver.FindElement(By.Id("Shareholder2SigningDate")).SendKeys("30-Jan-2020");
            driver.FindElement(By.Id("ProspectiveMemberCertification")).Click();
            driver.FindElement(By.Id("ShareholderCertification")).Click();
            driver.FindElement(By.XPath("/html/body/div[1]/main/form/div[5]/button")).Click();

            var message = driver.FindElement(By.CssSelector("div.alert-success")).Text;

            Assert.Contains("Membership application recorded successfully", message);
        }

        public void Dispose()
        {
            driver.Close();
            driver.Dispose();
        }

        private string RandomName(int length = 5)
        {
            StringBuilder name = new StringBuilder();

            name.Append((char)random.Next('A', 'Z' + 1));

            for (int i = 0; i < length - 1; i++)
            {
                name.Append((char)random.Next('a', 'z' + 1));
            }

            return name.ToString();
        }
    }
}
