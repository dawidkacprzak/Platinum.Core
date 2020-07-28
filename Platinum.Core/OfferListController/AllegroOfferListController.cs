using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using Platinum.Core.ApiIntegration;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.OfferListController
{
    public class AllegroOfferListController : PlatinumBrowserRestClient, IBaseOfferListController
    {
        private const string baseUrl = "https://allegro.pl";
        private string pageId;
        private OfferCategory _initiedOfferCategory;

        public void StartFetching(bool fetchJustFirstPage, OfferCategory category)
        {
            _initiedOfferCategory = category;
            InitBrowser();
            pageId = CreatePage();
            if (fetchJustFirstPage)
            {
                Open(pageId,baseUrl + "/" + category.CategoryUrl);
                IEnumerable<string> offerLinks = GetAllOfferLinks();
                UpdateDatabaseWithOffers(offerLinks);
            }
            else
            {
                Open(pageId,baseUrl + "/" + category.CategoryUrl);
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
                System.Diagnostics.Debug.WriteLine(currentPage);
                if (currentPage == lastPage)
                {
                    return false;
                }

                Open(pageId,baseUrl + "/" + _initiedOfferCategory.CategoryUrl + "?p=" + (currentPage + 1));
                return true;
            }
            catch (Exception ex)
            {
                throw new OfferListControllerException("Cannot open next page, check browser is initied", this);
            }
        }

        public IEnumerable<string> GetAllOfferLinks()
        {
            string pageSource = CurrentSiteSource(pageId);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(pageSource);

            var offerContainer = document.DocumentNode.SelectNodes("//*[@id=\"opbox-listing--base\"]");
            if (offerContainer == null || !offerContainer.Any())
            {
                throw new OfferListControllerException("Allegro layout has been changed", this);
            }
            else
            {
                HtmlNodeCollection offerLinks = offerContainer.First().SelectNodes("//a");

                List<HtmlNode> offerLinksNode = offerLinks.Where(x => x.HasAttributes).ToList();
                offerLinksNode = offerLinksNode
                    .Where(x => x.Attributes.Any(c => c.Name.ToLower().Trim().Equals("href"))).ToList();

                foreach (var offerLink in offerLinksNode)
                {
                    HtmlAttribute link = offerLink.Attributes
                        .FirstOrDefault(x =>
                            x.Name.ToLower().Trim().Equals("href") && x.Value.Contains("/oferta/") &&
                            x.Value.Contains("http"));
                    if (link != null)
                    {
                        yield return link.Value;
                    }
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
                foreach (string offer in offers)
                {
                    try
                    {
                        db.ExecuteNonQuery($@"INSERT INTO [dbo].offers VALUES 
                        (
                        {(int) OfferWebsite.Allegro}
                        ,'{offer}'
                        ,HashBytes('MD5','{offer}')
                        , 0
                        , getdate()
                        )");
                    }
                    catch (DalException ex)
                    {
                        if (!ex.Message.Contains("Cannot insert duplicate key"))
                        {
                            throw;
                        }
                    }
                }
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
            CloseBrowser();
        }

        private bool ValidatePaginationContainer(HtmlNodeCollection collection)
        {
            return !(collection == null || !collection.Any());
        }
    }
}