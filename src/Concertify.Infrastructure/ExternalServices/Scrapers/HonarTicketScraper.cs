using System.Globalization;

using Concertify.Domain.Models;
using Concertify.Infrastructure.Dtos;
using Concertify.Infrastructure.Interfaces;

using HtmlAgilityPack;

using Microsoft.Extensions.Configuration;

namespace Concertify.Infrastructure.ExternalServices.Scrapers;

public class HonarTicketScraper(IConfiguration configuration) : IWebScraper
{
    private readonly IConfiguration _configuration = configuration;
    readonly string baseUrl = "https://www.honarticket.com";
    public async Task<List<Concert>> ExtractLinks(string url)
    {
        HttpClient client = new();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string html = await response.Content.ReadAsStringAsync();

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        string urlPath = "//div[starts-with(@id, \"concerts-\")]";
        var nodes = doc.DocumentNode.SelectNodes(urlPath);
        List<ScraperContext> urls = [];
        List<Concert> scrapedConcerts = [];

        foreach (var node in nodes)
        {
            var inners = node.SelectNodes(urlPath + "//a[starts-with(@id, \"item-\")]");
            foreach (var i in inners)
            {
                if (i.Elements("div")
                    .First(e => e.GetAttributeValue("class", "") == "info")
                    .Element("span")
                    .GetAttributeValue("class", "")
                    .Trim()
                    .Equals("btn disabled"))
                    continue;

                string href = i.GetAttributeValue("href", "");

                if (href == null || href.Contains("javascript"))
                    continue;

                string city;

                if (node.GetAttributeValue("id", "").Equals("concerts-tehran-section"))
                    city = "تهران";
                else if (node.GetAttributeValue("id", "").Equals("concerts-kish-section"))
                    city = "کیش";
                else
                    city = i.Descendants("div").First().InnerText.Trim();

                if (city.Trim() == "")
                    city = "تهران";

                string image = i.Element("img").GetAttributeValue("src", "");

                string saveDir = _configuration["ScrapedImagesPath"]
                    ?? throw new NullReferenceException("The path for saving the scraped images was not provided.");

                var imageBytes = await client.GetByteArrayAsync(image);
                string fileName = Path.GetFileName(new Uri(image).LocalPath);
                string filePath = Path.Combine(saveDir, fileName);
                await File.WriteAllBytesAsync(filePath, imageBytes);

                ScraperContext context = new()
                {

                    Url = baseUrl + href,
                    City = city,
                    CardImage = filePath
                };

                Concert concert = await Scrape(context);
                scrapedConcerts.Add(concert);
                urls.Add(context);
            }
        }

        return scrapedConcerts;
    }

    public async Task<Concert> Scrape(ScraperContext context)
    {
        PersianCalendar pc = new();

        string url = context.Url;

        HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string html = await response.Content.ReadAsStringAsync();
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        string datePath = "//div[@id = \"showTimesMenu\"]/a";
        string titlePath = "//div[@id = \"header\"]/div[@class = \"c\"]/div[@class = \"title attached\"]/span/h1/a/text()";
        string locPath = "//span[@class = \"location-value\"]/text()";
        string addressPath = "//span[@class = \"location-value\"]/span";
        var timePath = "//div[@id = \"showTimesMenu\"]/a/div/span[@class = \"instance-time\"]/text()";
        string photoPath = "//img[contains(@class, \"cover-image\")]";
        string pricePath = "//div[@class = \"price-info\"]";
        string mapPath = "//div[@id = \"location\"]/small/a";

        var date = doc.DocumentNode.SelectNodes(datePath)?.First().GetAttributeValue("data-date", "");
        var title = doc.DocumentNode.SelectNodes(titlePath)?.First().InnerText.Trim();
        var loc = doc.DocumentNode.SelectNodes(locPath)?.First().InnerText.Trim();
        var address = doc.DocumentNode.SelectNodes(@addressPath)?.First().InnerText.Trim().Replace("\t", "");
        var time = doc.DocumentNode.SelectNodes(timePath)?.First().InnerText.Trim().Replace("\t", "");
        var image = doc.DocumentNode.SelectNodes(photoPath)?.First().GetAttributeValue("src", "");
        var priceRange = doc.DocumentNode.SelectNodes(pricePath)?.First().InnerText.Trim();

        var mapNode = doc.DocumentNode.SelectNodes(mapPath)?.First();

        string latitude, longtitude;
        if (mapNode != null)
        {
            var mapUri = mapNode.GetAttributeValue("href", "").Trim();
            latitude = mapUri;
            longtitude = mapUri;
        }
        else
        {
            latitude = "";
            longtitude = "";
        }

        int[] vals = Array.ConvertAll(date.Split("-"), int.Parse);
        var timeVals = Array.ConvertAll(PersianDigitsToEnglish(time).Split(":"), int.Parse);
        var gregDate = pc.ToDateTime(vals[0], vals[1], vals[2], timeVals[0], timeVals[1], 0, 0);

        priceRange = priceRange.Replace(",", "");

        string saveDir = _configuration["ScrapedImagesPath"]
            ?? throw new NullReferenceException("The path for saving the scraped images was not provided.");

        var imageBytes = await client.GetByteArrayAsync(image);
        string fileName = Path.GetFileName(new Uri(image).LocalPath);
        string filePath = Path.Combine(saveDir, fileName);
        await File.WriteAllBytesAsync(filePath, imageBytes);

        Concert concert = new()
        {
            Title = title,
            Description = title,
            StartDateTime = gregDate,
            City = context.City,
            Location = loc,
            Address = address,
            Category = "Concert",
            TicketPrice = priceRange,
            Latitude = float.Parse(latitude),
            Longitude = float.Parse(longtitude),
            CoverImage = filePath,
            CardImage = context.CardImage,
            Url = url
        };

        return concert;
    }

    private static string PersianDigitsToEnglish(string text)
    {
        string[] persian = ["۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹"];

        for (int j = 0; j < persian.Length; j++)
            text = text.Replace(persian[j], j.ToString());

        return text;
    }

}
