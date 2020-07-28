using System;

namespace Platinum.Core.Types.Exceptions
{
    public class OfferListControllerException : Exception
    {
        public IBaseOfferListController InvalidController;
        public OfferListControllerException(string message, IBaseOfferListController controller) : base(message)
        {
            InvalidController = controller;
        }
    }
}