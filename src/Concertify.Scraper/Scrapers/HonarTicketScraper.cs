using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Concertify.Scraper.Scrapers;

public class HonarTicketScraper(IWebDriver driver) : IWebScraper
{
    private readonly IWebDriver _driver = driver;

    public List<string> ExtractLinks(string url)
    {
        string concertXPath = "/html/body/div[1]/div[2]/div[2]/div[4]//a";
        _driver.Navigate().GoToUrl(url);

        var elems = _driver.FindElements(By.XPath(concertXPath));

        List<string> links = elems.Select(e => e.GetAttribute("href")).ToList();
        links.RemoveAll(l => l.Contains("javascript") || l == null);
        return links;
    }

    public Concert Scrape(string url)
    {
        _driver.Navigate().GoToUrl(url);

        Actions actions = new(_driver);

        string titleSelector = "/html/body/div[1]/div[2]/div[2]/div[2]/div[1]/div[2]/span/h1/a";
        string dateSelector = "/html/body/div[1]/div[2]/div[2]/div[4]/div[1]";
        string timeSelector = "//*[@id=\"page\"]/div[1]/span[3]";
        string locationSelector = "//*[@id=\"page\"]/div[1]/span[6]";
        string addressSelector = "//*[@id=\"page\"]/div[1]/span[6]/span";
        string showPriceSelector = "/html/body/div[1]/div[2]/div[2]/div[4]/div[5]/div/a[1]";
        string mapSelector = "/html/body/div[1]/div[2]/div[2]/div[4]/div[2]/small/a";

        try
        {
            var titleElem = _driver.FindElement(by: By.XPath(titleSelector));
            var dateElem = _driver.FindElement(by: By.XPath(dateSelector));
            var timeElem = _driver.FindElement(by: By.XPath(timeSelector));
            var locationElem = _driver.FindElement(by: By.XPath(locationSelector));
            var addressElem = _driver.FindElement(by: By.XPath(addressSelector));
            var showPriceElem = _driver.FindElement(by: By.XPath(showPriceSelector));
            var mapElem = _driver.FindElement(By.XPath(mapSelector));
            var mapUrl = new Uri(mapElem.GetAttribute("href"));
            string[] query = mapUrl.Query.Split(',');


            actions.Click(showPriceElem);
            actions.Perform();
            var priceSelector = "/html/body/div[1]/div[2]/div[2]/div[4]/div[5]/div/div/div[2]/div[2]/span";
            //var priceSelector2 = "/html/body/div[1]/div[2]/div[2]/div[4]/div[5]/div/div/div[2]/div[3]/span";

            IWebElement priceElem = _driver.FindElement(By.XPath(priceSelector));

            string script = @"
                var element = arguments[0];
                var text = '';
                for (var i = 0; i < element.childNodes.length; i++) {
                    if (element.childNodes[i].nodeType === Node.TEXT_NODE) {
                        text += element.childNodes[i].nodeValue;
                    }
                }
                return text;
            ";

            var jsExecutor = (IJavaScriptExecutor)_driver;

            string? location = jsExecutor.ExecuteScript(script, locationElem).ToString()?.Trim();
            string? date = jsExecutor.ExecuteScript(script, dateElem).ToString()?.Trim();
            string? title = jsExecutor.ExecuteScript(script, titleElem).ToString()?.Trim();
            string time = timeElem.Text.Trim();
            string address = addressElem.Text.Trim();
            string prices = priceElem.Text.Trim();

            Concert concert = new Concert
            {
                Title = title,
                Description = title,
                Address = address,
                StartDate = date,
                Category = "کنسرت",
                TicketPrice = prices,
                Latitude = float.Parse(query[0][3..]),
                Longitude = float.Parse(query[1])
            };

            return concert;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }

    }
}
