using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.ElasticIntegration;
using Platinum.Core.Model;
using Platinum.Core.OfferDetailsParser;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Service.OfferDetailsFetcher
{
    public class AllegroOfferDetailsFetcher : IOfferDetailsFetcher
    {
        Logger _logger = LogManager.GetCurrentClassLogger();
        public int CountOfParallelTasks { get; set; }
        public string LocalBrowserPort { get; set; }

        public AllegroOfferDetailsFetcher(string port, int countOfTasks)
        {
            LocalBrowserPort = port;
            CountOfParallelTasks = countOfTasks;
        }

        public void InsertToElastic(OfferDetails offer)
        {
            throw new NotImplementedException();
        }

        public void Run(IDal dal)
        {
            List<Offer> lastNotProcessedOffers = GetLastNotProcessedOffers(dal, CountOfParallelTasks).ToList();
            AllegroOfferDetailsParser tempParser =
                new AllegroOfferDetailsParser("http://localhost:" + LocalBrowserPort);
            tempParser.InitBrowser();

            Task[] tasks = new Task[lastNotProcessedOffers.Count()];
            for (int i = 0; i < lastNotProcessedOffers.Count(); i++)
            {
                tasks[i] = new Task(() =>
                {
                    using (Dal db = new Dal())
                    {
                        Task t = CreateTaskForProcessOrder(db, lastNotProcessedOffers.ElementAt(i),
                            new AllegroOfferDetailsParser("http://localhost:" + LocalBrowserPort));
                        t.RunSynchronously();
                    }
                });
                tasks[i].Start();
                Thread.Sleep(1500);
            }

            Task.WaitAll(tasks);
        }

        public IEnumerable<Offer> GetLastNotProcessedOffers(IDal dal, int count)
        {
            List<int> ids = new List<int>();
            using (DbDataReader reader =
                dal.ExecuteReader(
                    $"SELECT TOP {count} Id FROM offers WHERE processed = {(int) EOfferProcessed.NotProcessed} and id > 5000")
            )
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetInt32(0));
                }
            }

            foreach (int id in ids)
            {
                yield return new Offer(dal, id);
            }
        }

        public void SetOffersAsInProcess(IDal dal, IEnumerable<Offer> offers)
        {
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int) EOfferProcessed.InProcess} WHERE Id in {string.Join(",", offers)}");
        }

        public void SetOfferAsProcessed(IDal dal, Offer offer)
        {
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int) EOfferProcessed.InProcess} WHERE Id = {offer.Id}");
        }


        public void SetOfferAsUnprocessed(IDal dal, Offer offer)
        {
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int) EOfferProcessed.NotProcessed} WHERE Id = {offer.Id}");
        }

        public void SetOfferAsInActive(IDal dal, Offer offer)
        {
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int) EOfferProcessed.Inactive} WHERE Id = {offer.Id}");
        }

        public void SetOffersAsUnprocessed(IDal dal, IEnumerable<Offer> offer)
        {
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int) EOfferProcessed.InProcess} WHERE Id in {string.Join(",", offer)}");
        }

        public Task CreateTaskForProcessOrder(IDal dal, Offer offer, IOfferDetailsParser parser)
        {
            return new Task(() =>
            {
                try
                {
                    OfferDetails details = parser.GetPageDetails(offer.Uri, offer);
                    BufforController.Instance.InsertOfferDetails(details);
                    SetOfferAsProcessed(dal, offer);
                }
                catch (OfferDetailsFailException ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsInActive(dal, offer);
                }
                catch (Exception ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsUnprocessed(dal, offer);
                }
            });
        }
    }
}