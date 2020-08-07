using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using Platinum.Core.ApiIntegration;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.ElasticIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.OfferListController
{
    public class AllegroOfferListController : PlatinumBrowserRestClient, IBaseOfferListController
    {
        private const string baseUrl = "https://allegro.pl";
        private string pageId;
        private OfferCategory initiedOfferCategory;
        private string urlArgs = "";
        public AllegroOfferListController() : base()
        {
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public AllegroOfferListController(string host) : base(host)
        {
        }

        public void StartFetching(bool fetchJustFirstPage, OfferCategory category,
            List<WebsiteCategoriesFilterSearch> urlArguments = null)
        {
            initiedOfferCategory = category;
            if (urlArguments != null && urlArguments.Any(x => x.WebsiteCategoryId != category.CategoryId))
                throw new OfferListControllerException("Url argument do not fit to page", this);

            pageId = CreatePage();
            if (urlArguments != null && urlArguments.Count > 0)
            {
                int index = 0;
                foreach (var arg in urlArguments)
                {
                    if (index == 0)
                    {
                        urlArgs += "?" + arg.Argument + "=" + arg.Value;
                        index++;
                    }
                    else
                    {
                        urlArgs += "&" + arg.Argument + "=" + arg.Value;
                    }
                }
            }

            RunFetching(fetchJustFirstPage);
        }

        private void RunFetching(bool fetchJustFirstPage)
        {
            if (fetchJustFirstPage)
            {
                Open(pageId, baseUrl + "/" + initiedOfferCategory.CategoryUrl + urlArgs);
                IEnumerable<string> offerLinks = GetAllOfferLinks();
                UpdateDatabaseWithOffers(offerLinks);
            }
            else
            {
                Open(pageId, baseUrl + "/" + initiedOfferCategory.CategoryUrl + urlArgs);
                IEnumerable<string> offerLinks = GetAllOfferLinks();
                UpdateDatabaseWithOffers(offerLinks);

                while (OpenNextPage())
                {
                    offerLinks = GetAllOfferLinks();
                    UpdateDatabaseWithOffers(offerLinks);
                }
            }
        }

        public bool OpenNextPage()
        {
            try
            {
                int currentPage = GetCurrentPageIndex();
                int lastPage = GetLastPageIndex();
                if (currentPage >= lastPage)
                {
                    return false;
                }

                if (urlArgs.Length > 1)
                {
                    Open(pageId,
                        baseUrl + "/" + initiedOfferCategory.CategoryUrl + urlArgs + "&p=" + (currentPage + 1));
                }
                else
                {
                    Open(pageId, baseUrl + "/" + initiedOfferCategory.CategoryUrl + "?p=" + (currentPage + 1));
                }

                return true;
            }
            catch (Exception)
            {
                throw new OfferListControllerException("Cannot open next page, check browser is initied", this);
            }
        }

        public IEnumerable<string> GetAllOfferLinks()
        {
            string pageSource = CurrentSiteSource(pageId);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(pageSource);
            var offerContainer = document.DocumentNode.SelectNodes("//div[@id=\"opbox-listing--base\"]");
            if (offerContainer == null || !offerContainer.Any())
            {
                throw new OfferListControllerException("Allegro layout has been changed", this);
            }
            else
            {
                HtmlNodeCollection offerLinks = offerContainer.First().SelectNodes("//a");

                List<HtmlNode> offerLinksNode = offerLinks.Where(x => x.HasAttributes).ToList();

                foreach (var offerLink in offerLinksNode)
                {
                    if (offerLink.Attributes["href"] != null &&
                        offerLink.Attributes["href"].Value.Contains("/oferta/") &&
                        offerLink.Attributes["href"].Value.Contains("http"))
                        yield return offerLink.Attributes["href"].Value;
                }
            }
        }

        public void UpdateDatabaseWithOffers(IEnumerable<string> offers)
        {
#if DEBUG
            using (Dal db = new Dal(true))
#endif
#if RELEASE
            using (Dal db = new Dal(false))
#endif
            {
                string query = $@"INSERT INTO [dbo].offersBuffor VALUES ";
                int index = 0;
                List<string> enumerable = offers.ToList();
                List<string> uniqueOffers = new List<string>();
                for (int i = 0; i < enumerable.Count; i++)
                {
                    bool offerBuffored = BufforController.Instance.OfferExistsInBuffor(enumerable.ElementAt(i));
                    if (!offerBuffored)
                    {
                        BufforController.Instance.InsertOffer(enumerable.ElementAt(i));
                        uniqueOffers.Add(enumerable.ElementAt(i));
                    }
                }
                int offerCount = uniqueOffers.Count();
                if (offerCount == 0) return;
                foreach (string offer in uniqueOffers)
                {
                    if (!offer.Contains("\'"))
                    {
                        query += $@"
                        (
                            {(int) EOfferWebsite.Allegro}
                            ,'{offer}'
                            ,HashBytes('MD5','{offer}')
                            , getdate()
                            , {initiedOfferCategory.CategoryId}
                        )";
                    }

                    index++;
                    if (index < offerCount)
                    {
                        query += ",";
                    }
                }

                db.ExecuteNonQuery(query);
            }
        }

        public int GetCurrentPageIndex()
        {
            HtmlNodeCollection paginationContainer;
            int validationCounter = 0;
            do
            {
                string pageSource = CurrentSiteSource(pageId);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageSource);
                paginationContainer =
                    document.DocumentNode.SelectNodes("//*[@aria-label=\"paginacja\"]");
                validationCounter++;
                if (validationCounter > 1)
                {
                    Thread.Sleep(2500);
                    RefreshPage(pageId);
                    Thread.Sleep(2500);
                }

                if (validationCounter == 10)
                {
                    throw new OfferListControllerException(
                        "Cannot fetch max page index. Allegro layout could be changed",
                        this);
                }
            } while (!ValidatePaginationContainer(paginationContainer));


            HtmlNode maxPageInput = paginationContainer.First().ChildNodes.First(x => x.Name == "input");

            return int.Parse(maxPageInput.GetAttributeValue("value", "default"));
        }

        public int GetLastPageIndex()
        {
            HtmlNodeCollection paginationContainer;

            int validationCounter = 0;
            do

            {
                string pageSource = CurrentSiteSource(pageId);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageSource);
                paginationContainer =
                    document.DocumentNode.SelectNodes("//*[@aria-label=\"paginacja\"]");
                validationCounter++;
                if (validationCounter > 1)
                {
                    Thread.Sleep(2500);
                    RefreshPage(pageId);
                    Thread.Sleep(2500);
                }

                if (validationCounter == 10)
                {
                    throw new OfferListControllerException(
                        "Cannot fetch max page index. Allegro layout could be changed",
                        this);
                }

                Thread.Sleep(5000);
            } while (!ValidatePaginationContainer(paginationContainer));


            HtmlNode maxPageInput = paginationContainer.First().ChildNodes.First(x => x.Name == "input");

            HtmlAttribute maxPage = maxPageInput.Attributes
                .First(x => x.Name.Equals("data-maxpage"));

            return int.Parse(maxPage.Value);
        }

        public void Dispose()
        {
            ClosePage(pageId);
        }

        private bool ValidatePaginationContainer(HtmlNodeCollection collection)
        {
            return !(collection == null || !collection.Any());
        }
    }
}