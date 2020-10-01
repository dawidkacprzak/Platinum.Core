using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platinum.Core.Model
{
    public class ScrollOfferDetails
    {
        public string ScrollId { get; set; }
        public bool AnyDocuments { get => OfferDetails.Any(); }
        public List<OfferDetails> OfferDetails { get; set; }
    }
}
