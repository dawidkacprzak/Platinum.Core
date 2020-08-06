using Platinum.Core.DatabaseIntegration;

namespace Platinum.Core.Types
{
    public interface IBufforUrlQueueTask
    {
        void Run(IDal db);
        void SelectAndMoveOffersFromBufforToOffers(IDal db);
        void PopOffersFromBuffer(IDal db);
    }
}