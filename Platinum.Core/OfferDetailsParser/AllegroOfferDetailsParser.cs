using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Platinum.Core.ApiIntegration;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.OfferDetailsParser
{
    public class AllegroOfferDetailsParser : SharpBrowserClient, IOfferDetailsParser
    {
        private string pageId;

        public OfferDetails GetPageDetails(string pageUrl, Offer offer)
        {
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

                if (string.IsNullOrEmpty(pageId))
                {
                    this.pageId = CreatePage();
                }

                Open(pageId, pageUrl);

                string content = CurrentSiteSource(pageId);
                string headerSource = CurrentSiteHeader(pageId);

                HtmlDocument document = new HtmlDocument();
                HtmlDocument header = new HtmlDocument();
                header.LoadHtml(headerSource);
                document.LoadHtml(content);
                offerArguments = GetAttributes(document);
                description = GetOfferDescription(document);
                title = GetOfferTitle(header);
                decimal price = GetPrice(document);
                if (string.IsNullOrEmpty(title))
                {
                    throw new OfferDetailsFailException("Offer title cannot be empty: " + pageUrl);
                }

                if (offerArguments.Count == 0 && string.IsNullOrEmpty(description))
                {
                    ClosePage(pageId);
                    throw new OfferDetailsFailException(
                        "Arguments and offer description is empty, cannot fetch offer details. : " +
                        pageUrl);
                }

                ClosePage(pageId);
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
                ClosePage(pageId);
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
                return descriptionContainer.First().InnerText;
            else return String.Empty;
        }

        private string GetOfferTitle(HtmlDocument document)
        {
            var titleContainer =
                document.DocumentNode.SelectNodes("//meta[@property=\"og:title\"]");

            if (titleContainer.Count >= 1)
            {
                return titleContainer.First().Attributes["content"].Value;
            }
            else return String.Empty;
        }

        private decimal GetPrice(HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes("//div[contains(@aria-label,'cena')]");
            if (nodes.Count > 0)
            {
                try
                {
                    string priceText = nodes.First().InnerText;
                    priceText = priceText.Replace("zł", "").Trim().Replace(" ",string.Empty);
                    var numberFormatInfo = new NumberFormatInfo { NumberDecimalSeparator = "," };
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