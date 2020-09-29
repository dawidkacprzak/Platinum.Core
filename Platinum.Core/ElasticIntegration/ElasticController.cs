﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using Platinum.Core.Model;
using Platinum.Core.Model.Elastic;
using PuppeteerSharp.Input;

namespace Platinum.Core.ElasticIntegration
{
    public class ElasticController
    {
        private static ElasticClient client;
        private static ElasticController instance;
        private static object padlock = new object();
        Logger _logger = LogManager.GetCurrentClassLogger();

        public static ElasticController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ElasticController();
                }

                return instance;
            }
        }

        private ElasticController()
        {
            ConnectionSettings settings = new ConnectionSettings(new Uri(ElasticConfiguration.ELASTIC_HOST));
            settings.GlobalHeaders(new System.Collections.Specialized.NameValueCollection()
            {
                {"authorization", "opnsdgsd353sapgqejpg"}
            });
            settings.DisableDirectStreaming(true);
            client = new ElasticClient(settings);
        }

        public void InsertOffer(Offer offer)
        {
            client.Index(new ELBufforedOffers(offer), i => i.Index("buffered_offers"));
        }

        public bool InsertOffer(string offer)
        {
            var response = client.Index(new ELBufforedOffers(offer), i => i.Index("buffered_offers"));
            return response.IsValid;
        }

        public bool InsertOfferDetails(OfferDetails offerDetails, int userId, int categoryId)
        {
            lock (padlock)
            {
                string index = userId + "_cat" + categoryId;
                if (userId == 1)
                {
                    IndexResponse status = client.Index(offerDetails, i => i.Index("offer_details"));
                    return status.IsValid;
                }
                else
                {
                    var ex = client.Indices.Exists(index);
                    if (ex.Exists == false)
                    {
                        offerDetails.CreatedDate = DateTime.Now;
                        var res = client.Indices.Create(index, c => c
                            .Map<OfferDetails>(x => x.AutoMap()).Settings(s =>
                                s.NumberOfReplicas(2).NumberOfShards(2)));
                        if (!res.IsValid || !res.Acknowledged)
                        {
                            throw new Exception("Cannot create index: " + index);
                        }
                    }

                    IndexResponse status = client.Index<OfferDetails>(offerDetails, i => i.Index(index));
                    return status.IsValid;
                }
            }
        }

        public bool OfferDetailsExists(int offerId)
        {
            var searchResponse = client.Search<OfferDetails>(s => s.Index("offer_details")
                .From(0)
                .Size(10)
                .Query(q =>
                    q
                        .MatchPhrase(c => c
                            .Field(p => p.Id)
                            .Query(offerId.ToString())
                        )
                )
            );

            return searchResponse.Documents.Count > 0;
        }

        public bool OfferExistsInBuffor(string uri)
        {
            var searchResponse = client.Search<ELBufforedOffers>(s => s.Index("buffered_offers")
                .From(0)
                .Size(10)
                .Query(q =>
                    q
                        .MatchPhrase(c => c
                            .Field(p => p.uri)
                            .Query(uri)
                        )
                )
            );
            return searchResponse.Documents.Count > 0;
        }

        public long GetIndexDocumentCountThisMonth(int categoryId, int userId)
        {
            DateTime thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime nextMonth = thisMonth.AddMonths(1);
            return GetIndexDocumentCountByDateRange(categoryId, userId, thisMonth, nextMonth);
        }

        public long GetIndexDocumentCount(int categoryId, int userId)
        {
            try
            {
                var indicies = client.Indices.Get(Indices.All);
                string indexName = userId + "_cat" + categoryId;
                if (userId == 1)
                {
                    indexName = "offer_details";
                }

                List<string> filteredIndicies = indicies.Indices.Where(x => x.Key.Name.ToLower().Contains(indexName))
                    .Select(x => x.Key.Name).ToList();
                if (filteredIndicies.Count > 0)
                {
                    var k = client.Count<OfferDetails>(c => c.Index(indexName));

                    return k.Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return 0;
            }
        }


        public long GetIndexDocumentCountByDateRange(int categoryId, int userId, DateTime minDate, DateTime maxDate)
        {
            try
            {
                var indicies = client.Indices.Get(Indices.All);
                string indexName = userId + "_cat" + categoryId;
                if (userId == 1)
                {
                    indexName = "offer_details";
                }

                List<string> filteredIndicies = indicies.Indices.Where(x => x.Key.Name.ToLower().Contains(indexName))
                    .Select(x => x.Key.Name).ToList();
                if (filteredIndicies.Count > 0)
                {
                    List<QueryContainer> queries = new List<QueryContainer>();
                    QueryContainer dateQuery = new QueryContainer();
                    dateQuery = (new DateRangeQuery()
                    {
                        Field = "createdDate",
                        GreaterThanOrEqualTo = minDate,
                        LessThan = maxDate
                    });

                    queries.Add(dateQuery);
                    CountRequest request = new CountRequest<OfferDetails>(indexName)
                    {
                        Query = new BoolQuery()
                        {
                            Filter = queries
                        }
                    };
                    var k = client.Count(request);

                    return k.Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return 0;
            }
        }

        public List<SimpleMapping> GetIndexMappings(int categoryId, int userId)
        {
            IRequestConfiguration conf = new RequestConfiguration();
            conf.Headers = new System.Collections.Specialized.NameValueCollection();
            conf.Headers.Add(new System.Collections.Specialized.NameValueCollection()
            {
                {"authorization", "opnsdgsd353sapgqejpg"},
                {"http_authorization", "opnsdgsd353sapgqejpg"}
            });
            List<SimpleMapping> ret = new List<SimpleMapping>();
            var response = client.LowLevel.Indices.GetMapping<GetMappingResponse>($"{userId}_cat{categoryId}");
            if (response.Indices.Values.Any())
            {
                for (int i = 0; i < response.Indices.Values.Count(); i++)
                {
                    var typeMapping = response.Indices.Values.ElementAt(i).Mappings;
                    foreach (var v in typeMapping.Properties)
                    {
                        try
                        {
                            ObjectProperty propValue = ((ObjectProperty) v.Value);
                            if (propValue.Properties.Count > 0)
                            {
                                string baseName = propValue.Name.Name.ToLower();
                                if (!(baseName.Equals("attributes") || baseName.Equals("comparer") ||
                                      baseName.Equals("description") || baseName.Equals("processed") ||
                                      baseName.Equals("uriHash") || baseName.Equals("websiteId") ||
                                      baseName.Equals("websiteCategoryId") || baseName.Equals("value") ||
                                      baseName.Equals("keys") || baseName.Equals("key") || baseName.Equals("item") ||
                                      baseName.Equals("items") ||
                                      baseName.Equals("count") ||
                                      baseName.Equals("values")))
                                {
                                    ret.Add(new SimpleMapping(propValue.Name.Name, "object"));
                                }

                                foreach (IProperty property in propValue.Properties.Values)
                                {
                                    string propName = property.Name.Name.ToLower();
                                    if (propName.Equals("attributes") || propName.Equals("comparer") ||
                                        propName.Equals("description") || propName.Equals("processed") ||
                                        propName.Equals("uriHash") || propName.Equals("websiteId") ||
                                        propName.Equals("websiteCategoryId") || propName.Equals("value") ||
                                        propName.Equals("keys") || propName.Equals("key") ||
                                        propName.Equals("item") || propName.Equals("items") ||
                                        propName.Equals("count") || propName.Equals("description") ||
                                        propName.Equals("id") ||
                                        propName.Equals("values"))
                                    {
                                        continue;
                                    }

                                    ret.Add(new SimpleMapping(property.Name.Name, baseName, property.Type));
                                }
                            }
                        }
                        catch (InvalidCastException)
                        {
                            IProperty propValue = ((IProperty) v.Value);

                            ret.Add(new SimpleMapping(propValue.Name.Name, propValue.Type));
                        }
                    }
                }
            }

            return ret;
        }


        public List<OfferDetails> GetPaginatedOfferDetails(int categoryId, int userId, int maxPerPage, int page)
        {
            IRequestConfiguration conf = new RequestConfiguration();
            conf.Headers = new System.Collections.Specialized.NameValueCollection();
            conf.Headers.Add(new System.Collections.Specialized.NameValueCollection()
            {
                {"authorization", "opnsdgsd353sapgqejpg"},
                {"http_authorization", "opnsdgsd353sapgqejpg"}
            });

            SearchRequest request = new SearchRequest<OfferDetails>($"{userId}_cat{categoryId}")
            {
                Size = maxPerPage,
                From = page * maxPerPage
            };

            request.RequestConfiguration = conf;
            var k = client.Search<OfferDetails>(request);
            for (int x = 0; x < k.Documents.Count; x++)
            {
                k.Documents.ElementAt(x).Id = k.Hits.ElementAt(x).Source.Id;
            }


            return k.Documents.ToList();
        }

        public List<OfferDetails> GetByAnyFieldKeywords(string keywords, int categoryId, int userId)
        {
            string[] args = keywords.Split(',');

            QueryContainer qc = new QueryContainer();
            BoolQuery bq = new BoolQuery();

            foreach (var VARIABLE in args)
            {
                qc &= new MultiMatchQuery()
                {
                    Fields = "*",
                    Query = VARIABLE,
                    Operator = Operator.Or
                };
            }

            bq = new BoolQuery()
            {
                Must = new[] {qc}
            };

            string indexName = $"{userId}_cat{categoryId}";
            if (userId == 1)
            {
                indexName = "offer_details";
            }

            SearchRequest request = new SearchRequest<OfferDetails>(indexName)
            {
                Size = 100,
                Query = bq
            };
            IRequestConfiguration conf = new RequestConfiguration();
            conf.Headers = new System.Collections.Specialized.NameValueCollection();
            conf.Headers.Add(new System.Collections.Specialized.NameValueCollection()
            {
                {
                    "authorization", "opnsdgsd353sapgqejpg"
                },
                {
                    "http_authorization", "opnsdgsd353sapgqejpg"
                }
            });
            request.RequestConfiguration = conf;
            var k = client.Search<OfferDetails>(request);
            for (int x = 0;
                x < k.Documents.Count;
                x++)
            {
                k.Documents.ElementAt(x).Id = k.Hits.ElementAt(x).Source.Id;
            }

            var json = client.RequestResponseSerializer.SerializeToString(request);
            return k.Documents.ToList();
        }
    }
}