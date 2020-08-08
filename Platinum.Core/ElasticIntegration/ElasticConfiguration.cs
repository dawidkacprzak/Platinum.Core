namespace Platinum.Core.ElasticIntegration
{
    public static class ElasticConfiguration
    {
        #if DEBUG
        public static string ELASTIC_HOST = "http://5.196.143.190:9200";
        #endif
        #if RELEASE
        public static string ELASTIC_HOST = "http://192.168.10.2:9200";
#endif
    }
}