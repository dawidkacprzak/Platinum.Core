using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;

namespace Platinum.Service.BufforUrlQueue
{
    public class Worker : BackgroundService
    {
        Logger _logger = LogManager.GetCurrentClassLogger();
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (Dal db = new Dal())
                {
                    try
                    {
                        List<Offer> cachedOffers = GetOldest50Offers().ToList();
                        db.BeginTransaction();
                        _logger.Info($"Fetched {cachedOffers.Count} offers");
                        foreach (Offer offer in cachedOffers)
                        {
                            ProceedAndPopFromBuffor(db, offer);
                        }

                        db.CommitTransaction();
                        await Task.Delay(500, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.Info(ex.Message + "\n" + ex.StackTrace);
                        db.RollbackTransaction();
                    }
                }
            }
        }

        public void ProceedAndPopFromBuffor(Dal db, Offer offer)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "hash",
                SqlDbType = SqlDbType.VarBinary,
                Value = offer.UriHash
            });

            int count = (int) db.ExecuteScalar($"SELECT COUNT(*) as Count FROM offers WITH (NOLOCK) where UriHash = @hash",
                parameters);

            _logger.Info($"Append to proceed {offer.Uri}");

            if (count == 0)
            {
                _logger.Info($"Offer inserted");
                InsertOffer(db, offer);
            }

            PopOfferFromBuffer(db, offer);
        }

        public void PopOfferFromBuffer(Dal db, Offer offer)
        {
            db.ExecuteNonQuery($"DELETE FROM offersBuffor WHERE UriHash = @hash;", new List<SqlParameter>()
            {
                new SqlParameter()
                {
                    ParameterName = "hash",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = offer.UriHash
                }
            });
        }

        public void InsertOffer(Dal db, Offer offer)
        {
            db.ExecuteNonQuery($@"INSERT INTO offers VALUES (
            @websiteId,
            @uri,
            @uriHash,
            @createdDate,
            @websiteCategory,
            0
            );", new List<SqlParameter>()
            {
                new SqlParameter()
                    {ParameterName = "websiteId", SqlDbType = SqlDbType.Int, Value = offer.WebsiteId},
                new SqlParameter()
                    {ParameterName = "uri", SqlDbType = SqlDbType.Text, Value = offer.Uri},
                new SqlParameter()
                    {ParameterName = "uriHash", SqlDbType = SqlDbType.VarBinary, Value = offer.UriHash},
                new SqlParameter()
                    {ParameterName = "createdDate", SqlDbType = SqlDbType.DateTime, Value = offer.CreatedDate},
                new SqlParameter()
                    {ParameterName = "websiteCategory", SqlDbType = SqlDbType.Int, Value = offer.WebsiteCategoryId}
            });
        }

        public IEnumerable<Offer> GetOldest50Offers()
        {
            List<Offer> offers = new List<Offer>();
            using (Dal db = new Dal())
            {
                using (DbDataReader reader = db.ExecuteReader("SELECT top 50 * FROM offersBuffor"))
                {
                    while (reader.Read())
                    {
                        offers.Add(new Offer(
                            reader.GetInt32(reader.GetOrdinal("Id")),
                            reader.GetInt32(reader.GetOrdinal("WebsiteId")),
                            reader.GetString(reader.GetOrdinal("Uri")),
                            (byte[]) reader["UriHash"],
                            reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            reader.GetInt32(reader.GetOrdinal("WebsiteCategoryId"))
                        ));
                    }
                }
            }

            return offers;
        }
    }
}