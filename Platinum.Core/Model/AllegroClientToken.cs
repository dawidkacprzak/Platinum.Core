using System.Diagnostics.CodeAnalysis;

namespace Platinum.Core.Model
{
    [ExcludeFromCodeCoverage]
    public class AllegroClientToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string allegro_api { get; set; }
        public string jti { get; set; }
    }
}