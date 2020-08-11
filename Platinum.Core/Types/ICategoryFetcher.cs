using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface ICategoryFetcher
    {
        bool CategoryIdExistInDb(IDal db, int categoryId);
        void UpdateCategoryInDb(IDal db, OfferCategory category);
        OfferCategory GetCategoryById(int id);
        void Run(IDal db);
        void SetIndexCategoryFetchLimit(int limit);
    }
}