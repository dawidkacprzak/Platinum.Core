namespace Platinum.Core.ElasticIntegration
{
    public static class ElasticConfiguration
    {
#if DEBUG
        /// <summary>
        /// user MUST be added in firewall on server.
        /// </summary>
        public static string ELASTIC_HOST = "http://oyacode.pl:9200";
#endif

#if RELEASE
        public static string ELASTIC_HOST = "http://192.168.10.2:9200";
#endif
    }
}