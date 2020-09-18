using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using HtmlAgilityPack;
using Platinum.Core.ApiIntegration;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.OfferDetailsParser
{
    public class HttpAllegroOfferDetailsParser : HttpClientInstance, IOfferDetailsParser
    {
        private string pageId;

        public OfferDetails GetPageDetails(string pageUrl, Offer offer)
        {
            pageUrl = pageUrl.Trim();
            try
            {
                if (pageUrl.Contains("allegrolokalnie"))
                {
                    throw new OfferDetailsFailException("Allegro lokalnie is not supported");
                }

                Dictionary<string, string> offerArguments = new Dictionary<string, string>();
                string description;
                string title;
                if (string.IsNullOrEmpty(pageUrl))
                {
                    throw new RequestException("Cannot get page details. pageUrl is empty? : " + pageUrl);
                }

                Thread.Sleep(200);
                OpenUrl(pageUrl);

                string content = LastResponse;


                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(content);
                offerArguments = GetAttributes(document);
                //description = GetOfferDescription(document);
                description = ""; //removed cuz of waste of memory
                title = HttpUtility.HtmlDecode(GetOfferTitle(document));
                decimal price = GetPrice(document);
                if (string.IsNullOrEmpty(title))
                {
                    throw new OfferDetailsFailException("Offer title cannot be empty: " + pageUrl);
                }

                if (offerArguments.Count == 0 && string.IsNullOrEmpty(description))
                {
                    ;
                    throw new OfferDetailsFailException(
                        "Arguments and offer description is empty, cannot fetch offer details. : " +
                        pageUrl);
                }

                using (Dal db = new Dal())
                {
                    return new OfferDetails(offer)
                    {
                        Attributes = offerArguments,
                        Description = description,
                        Price = price,
                        Title = title
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Dictionary<string, string> GetAttributes(HtmlDocument document)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            HtmlNodeCollection paginationContainer =
                document.DocumentNode.SelectNodes("//*[@data-prototype-id=\"allegro.showoffer.parameters\"]");

            if (paginationContainer.Count > 0)
            {
                HtmlDocument parameterDocument = new HtmlDocument();
                parameterDocument.LoadHtml(paginationContainer.First().OuterHtml);
                var nodes = parameterDocument.DocumentNode.SelectNodes("//li");
                foreach (var listElement in nodes.Skip(1))
                {
                    HtmlDocument parameterInnerDocument = new HtmlDocument();
                    parameterInnerDocument.LoadHtml(listElement.InnerHtml);
                    var divs = parameterInnerDocument.DocumentNode.SelectNodes("//div");

                    if (divs[1].InnerText.Where(x => x.Equals(':')).Count() > 1)
                        continue;
                    string keyArg = divs[1].InnerText;
                    string valArg = divs[2].InnerText;

                    if (keyArg[keyArg.Length - 1].Equals(':'))
                        keyArg = keyArg.Substring(0, keyArg.Length - 1);
                    parameters.TryAdd(keyArg, valArg);
                }
            }

            return parameters;
        }

        private string GetOfferDescription(HtmlDocument document)
        {
            var descriptionContainer =
                document.DocumentNode.SelectNodes("//*[@data-prototype-id=\"allegro.showoffer.description\"]");

            if (descriptionContainer.Count >= 1)
            {
                string parsedDescription = descriptionContainer.First().InnerText
                    .Replace("&nbsp;", " ")
                    .Replace("\\n", " ")
                    .Replace("\\t", " ");
                return System.Text.RegularExpressions.Regex.Replace(parsedDescription, @"\s+", " ");
            }
            else return String.Empty;
        }

        private string GetOfferTitle(HtmlDocument document)
        {
            return GetTitleFromLastWebsite();
        }

        private decimal GetPrice(HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes("//div[contains(@aria-label,'cena')]");
            if (nodes.Count > 0)
            {
                try
                {
                    string priceText = nodes.First().InnerText;
                    priceText = priceText.Replace("zł", "").Trim().Replace(" ", string.Empty);
                    var numberFormatInfo = new NumberFormatInfo {NumberDecimalSeparator = ","};
                    decimal newPrice = decimal.Parse(priceText, numberFormatInfo);
                    return newPrice;
                }
                catch (Exception ex)
                {
                    throw new OfferDetailsFailException("Cannot parse price");
                }
            }

            return 0;
        }
    }
}