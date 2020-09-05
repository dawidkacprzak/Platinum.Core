using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
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

        public OponeoController()
        {
            client = new ElasticClient(new ConnectionSettings(new Uri(ElasticConfiguration.ELASTIC_HOST)));
        }

        /// <summary>
        /// Zwraca oferty pasujące do parametrów
        /// </summary>
        /// <returns></returns>
        [HttpGet("{producent}/{srednica}/{model}/{indekspredkosci}/{szerokoscOpony}/{profilOpony}")]
        public string GetTires(string producent, string srednica, string model, string indekspredkosci,
            string szerokoscOpony, string profilOpony)
        {
            List<QueryContainer> titleQueries = new List<QueryContainer>();
            if (!string.IsNullOrEmpty(producent))
            {
                string decodedString = Uri.UnescapeDataString(producent);
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
                string decodedString = Uri.UnescapeDataString(srednica);
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
                string decodedString = Uri.UnescapeDataString(model);
                QueryContainer modelQuery = new QueryContainer();
                if (decodedString.Contains(' '))
                {
                    modelQuery = (new NestedQuery()
                    {
                        Path = "attributes",
                        Query = new MatchQuery()
                        {
                            Field = "attributes.Model",
                            Query = decodedString,
                        }
                    });
                }
                else
                {
                    modelQuery = (new NestedQuery()
                                  {
                                      Path = "attributes",
                                      Query = new WildcardQuery()
                                      {
                                          Field = "attributes.Model",
                                          Value = decodedString,
                                      },
                                  }
                                  || new WildcardQuery()
                                  {
                                      Field = "tile",
                                      Value = decodedString,
                                  }
                                  || new WildcardQuery()
                                  {
                                      Field = "tile",
                                      Value = "*" + decodedString + "*"
                                  });
                }

                titleQueries.Add(modelQuery);
            }

            if (!string.IsNullOrEmpty(profilOpony))
            {
                string decodedString = Uri.UnescapeDataString(profilOpony);
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
                string decodedString = Uri.UnescapeDataString(indekspredkosci);
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
                string decodedString = Uri.UnescapeDataString(szerokoscOpony);
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
            

            SearchRequest request = new SearchRequest<OfferDetails>("offer_details")
            {
                Size = 100,
                Query = new BoolQuery()
                {
                    Filter = titleQueries
                }
            };

            var k = client.Search<OfferDetails>(request);
            for (int x = 0; x < k.Documents.Count; x++)
            {
                k.Documents.ElementAt(x).Id = k.Hits.ElementAt(x).Source.Id;
            }

            var json = client.RequestResponseSerializer.SerializeToString(request);

            for (int i = 0; i < k.Documents.Count; i++)
            {
                foreach (var attr in  k.Documents.ElementAt(i).Attributes.Where(x=>x.Key.ToLower().Contains("liczba opon")))
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
            return JsonConvert.SerializeObject(k.Documents);
        }
    }
}