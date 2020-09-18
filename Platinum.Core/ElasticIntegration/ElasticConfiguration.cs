using System;

namespace Platinum.Core.ElasticIntegration
{
    public static class ElasticConfiguration
    {

        /// <summary>
        /// user MUST be added in firewall on server.
        /// </summary>
        public static string ELASTIC_HOST
        {
            get
            {
                Random r = new Random();
                int l = r.Next(0, 2);
                switch (l)
                {
                    case 0:
                        return "http://oyacode.pl:9200";
                    case 1:
                        return "http://oyacode.pl:9201";
                    case 2:
                        return "http://oyacode.pl:9202";
                    default:
                        return "http://oyacode.pl:9200";
                }
            }
        }

    }
}