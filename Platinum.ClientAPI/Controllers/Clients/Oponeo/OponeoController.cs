using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using NLog;
using Platinum.ClientAPI.Auth;
using Platinum.Core.ElasticIntegration;
using Platinum.Core.Model;
using Platinum.Core.Model.Elastic;
using OfferDetails = Platinum.ClientAPI.Model.Oponeo.OfferDetails;

namespace Platinum.ClientAPI.Controllers.Clients.Oponeo
{
    [ApiController]
    [Route("[controller]")]
    [BasicAuth("Oponeo")]
    public class OponeoController : ControllerBase
    {
        ElasticClient client;
        Logger _logger = LogManager.GetCurrentClassLogger();

        public OponeoController()
        {
            ConnectionSettings settings = new ConnectionSettings(new Uri(ElasticConfiguration.ELASTIC_HOST));
            settings.GlobalHeaders(new System.Collections.Specialized.NameValueCollection()
            {
                {"authorization", "opnsdgsd353sapgqejpg"},
                {"http_authorization", "opnsdgsd353sapgqejpg"}
            });
            client = new ElasticClient(settings);
        }

        /// <summary>
        /// Zwraca oferty pasujące do parametrów
        /// </summary>
        /// <returns></returns>
        [HttpGet("{producent}/{srednica}/{model}/{indekspredkosci}/{szerokoscOpony}/{profilOpony}/{indeksNosnosci}")]
        public string GetTires(string producent, string srednica, string model, string indekspredkosci,
            string szerokoscOpony, string profilOpony, string indeksNosnosci)
        {
            List<QueryContainer> titleQueries = new List<QueryContainer>();
            if (!string.IsNullOrEmpty(producent))
            {
                string decodedString = HttpUtility.UrlDecode(producent);
                QueryContainer producentQuery = new QueryContainer();

                producentQuery = (new NestedQuery()
                                     {
                                         Path = "attributes",
                                         Query = new BoolQuery()
                                         {
                                             Should = new QueryContainer[]
                                             {
                                                 new WildcardQuery()
                                                 {
                                                     Field = "attributes.Producent",
                                                     Value = decodedString,
                                                     Boost = 1
                                                 }
                                             }
                                         }
                                     }
                                 )
                                 || new WildcardQuery()
                                 {
                                     Field = "title",
                                     Value = "*" + decodedString + "*",
                                     Boost = 0.9
                                 };
                titleQueries.Add(producentQuery);
            }

            if (!string.IsNullOrEmpty(srednica))
            {
                string decodedString = HttpUtility.UrlDecode(srednica);
                QueryContainer srednicaQuery = new QueryContainer();
                srednicaQuery = (new NestedQuery()
                {
                    Path = "attributes",
                    Query = new WildcardQuery()
                    {
                        Field = "attributes.Średnica",
                        Value = "*" + decodedString + "*"
                    }
                });
                titleQueries.Add(srednicaQuery);
            }

            if (!string.IsNullOrEmpty(model))
            {
                string decodedString = HttpUtility.UrlDecode(model);
                QueryContainer modelQuery = new QueryContainer();

                modelQuery = (new NestedQuery()
                {
                    Path = "attributes",
                    Query = new MatchQuery()
                    {
                        Field = "attributes.Model",
                        Query = decodedString,
                    }
                });


                titleQueries.Add(modelQuery);
            }

            if (!string.IsNullOrEmpty(profilOpony))
            {
                string decodedString = HttpUtility.UrlDecode(profilOpony);
                QueryContainer profilOponyQuery = new QueryContainer();
                profilOponyQuery = (new NestedQuery()
                {
                    Path = "attributes",
                    Query = new WildcardQuery()
                    {
                        Field = "attributes.Profil opony",
                        Value = decodedString + "*",
                    }
                });
                titleQueries.Add(profilOponyQuery);
            }

            if (!string.IsNullOrEmpty(indekspredkosci))
            {
                string decodedString = HttpUtility.UrlDecode(indekspredkosci);
                QueryContainer indekspredkosciQuery = new QueryContainer();
                indekspredkosciQuery = (new NestedQuery()
                {
                    Path = "attributes",
                    Query = new WildcardQuery()
                    {
                        Field = "attributes.Indeks prędkości",
                        Value = decodedString + "*",
                    }
                });
                titleQueries.Add(indekspredkosciQuery);
            }


            if (!string.IsNullOrEmpty(szerokoscOpony))
            {
                string decodedString = HttpUtility.UrlDecode(szerokoscOpony);
                QueryContainer szerokoscOponyQuery = new QueryContainer();
                szerokoscOponyQuery = (new NestedQuery()
                {
                    Path = "attributes",
                    Query = new MatchQuery()
                    {
                        Field = "attributes.Szerokość opony",
                        Query = decodedString + " ",
                    } || new MatchPhraseQuery()
                    {
                        Field = "attributes.Szerokość opony",
                        Query = decodedString,
                    }
                });
                titleQueries.Add(szerokoscOponyQuery);
            }

            bool indeksPredkosciEmpty = false;
            if (!string.IsNullOrEmpty(indeksNosnosci) && !indeksNosnosci.Equals("nullvalue"))
            {
                string decodedString = HttpUtility.UrlDecode(indeksNosnosci);
                if (!decodedString.Equals("nullvalue"))
                {
                    QueryContainer indeksNosnosciQuery = new QueryContainer();
                    indeksNosnosciQuery = (new NestedQuery()
                    {
                        Path = "attributes",
                        Query = new MatchQuery()
                        {
                            Field = "attributes.Indeks nośności",
                            Query = decodedString + " -",
                        }
                    });
                    titleQueries.Add(indeksNosnosciQuery);
                }
                else indeksPredkosciEmpty = true;
            }
            else
            {
                indeksPredkosciEmpty = true;
            }

            if (indeksPredkosciEmpty)
            {
                QueryContainer indeksNosnosciQuery = new QueryContainer();
                indeksNosnosciQuery = (
                    new BoolQuery()
                    {
                        MustNot = new QueryContainer[]
                        {
                            new NestedQuery()
                            {
                                Path = "attributes",
                                Query = new ExistsQuery()
                                {
                                    Field = "attributes.Indeks nośności"
                                }
                            }
                        }
                    }
                );
                titleQueries.Add(indeksNosnosciQuery);
            }

            SearchRequest request = new SearchRequest<OfferDetails>("offer_details")
            {
                Size = 100,
                Query = new BoolQuery()
                {
                    Filter = titleQueries
                }
            };
            IRequestConfiguration conf = new RequestConfiguration();
            conf.Headers = new System.Collections.Specialized.NameValueCollection();
            conf.Headers.Add(new System.Collections.Specialized.NameValueCollection()
            {
                {"authorization", "opnsdgsd353sapgqejpg"},
                {"http_authorization", "opnsdgsd353sapgqejpg"}
            });
            request.RequestConfiguration = conf;
            var k = client.Search<OfferDetails>(request);
            for (int x = 0; x < k.Documents.Count; x++)
            {
                k.Documents.ElementAt(x).Id = k.Hits.ElementAt(x).Source.Id;
            }

            var json = client.RequestResponseSerializer.SerializeToString(request);

            for (int i = 0; i < k.Documents.Count; i++)
            {
                foreach (var attr in k.Documents.ElementAt(i).Attributes
                    .Where(x => x.Key.ToLower().Contains("liczba opon")))
                {
                    string foundCount = Regex.Match(attr.Value, @"\d+",
                        RegexOptions.IgnoreCase).Value;
                    if (int.TryParse(foundCount, out _))
                    {
                        int countOfItem = int.Parse(foundCount);
                        k.Documents.ElementAt(i).PriceForOne = Math.Round(k.Documents.ElementAt(i).Price / countOfItem);
                        k.Documents.ElementAt(i).QuantityInOffer = countOfItem;
                    }
                }
            }

            List<int> idsToRemove = new List<int>();
            foreach (var offer in k.Documents.Where(x => x.Attributes.ContainsKey("Model")))
            {
                if (model != null)
                {
                    string decodedString = HttpUtility.UrlDecode(model).ToLower();

                    string attrValue = offer.Attributes["Model"].ToLower();
                    if (!decodedString.Equals(attrValue))
                    {
                        idsToRemove.Add(offer.Id);
                    }
                }
            }

            var returnOffers = k.Documents.Where(x => !idsToRemove.Contains(x.Id)).ToList();
            if (returnOffers.Count > 0)
            {
                _logger.Info("Returned: " + returnOffers.Count + " offers. Ids: " +
                             string.Join(',', k.Documents.Select(x => x.Id).ToList()));
            }
            else
            {
                _logger.Info($@"Not found for: {producent}, {srednica}, {model}, {indekspredkosci},
                {szerokoscOpony}, {profilOpony}");
            }

            return JsonConvert.SerializeObject(returnOffers);
        }
    }
}