using System;
using System.Collections.Generic;
using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface IBaseOfferListController : IDisposable
    {
        /// <summary>
        /// Opens first page and fetch offers for all next pages;
        /// </summary>
        /// <param name="fetchJustFirstPage">Fetch offers from just first page</param>
        void StartFetching(bool fetchJustFirstPage, OfferCategory category, List<WebsiteCategoriesFilterSearch>urlArguments = null);
        /// <summary>
        /// Open next page in virtual browser if possible
        /// </summary>
        /// <returns>false if open next page is impossible (f.e do not exist)</returns>
        bool OpenNextPage();
        IEnumerable<string> GetAllOfferLinks();
        void UpdateDatabaseWithOffers(IEnumerable<string> offers);
        int GetCurrentPageIndex();
        int GetLastPageIndex();
        void DeInit();
    }
}