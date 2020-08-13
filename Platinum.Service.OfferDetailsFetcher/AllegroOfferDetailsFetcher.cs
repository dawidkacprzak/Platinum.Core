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
            Console.WriteLine($"Application started on port {LocalBrowserPort} and tak count {CountOfParallelTasks}");
            _logger.Info($"Application started on port {LocalBrowserPort} and tak count {CountOfParallelTasks}");
            List<Offer> lastNotProcessedOffers = GetLastNotProcessedOffers(dal, CountOfParallelTasks).ToList();
            _logger.Info($"Fetched: " + lastNotProcessedOffers.Count + " offers");
            AllegroOfferDetailsParser tempParser =
                new AllegroOfferDetailsParser("http://localhost:" + LocalBrowserPort);
            tempParser.InitBrowser();

            Task[] tasks = new Task[lastNotProcessedOffers.Count()];
            for (int i = 0; i < lastNotProcessedOffers.Count(); i++)
            {
                int i1 = i;
                tasks[i] = new Task(() =>
                {
                    using (Dal db = new Dal())
                    {
                        Task t = CreateTaskForProcessOrder(db, lastNotProcessedOffers.ElementAt(i1),
                            new AllegroOfferDetailsParser("http://localhost:" + LocalBrowserPort));
                        t.RunSynchronously();
                    }
                });
                tasks[i].Start();
                Thread.Sleep(2000);
            }

            Task.WaitAll(tasks);
        }

        public IEnumerable<Offer> GetLastNotProcessedOffers(IDal dal, int count)
        {
            List<int> ids = new List<int>();
            using (DbDataReader reader =
                dal.ExecuteReader(
                    $"update offers set Processed = {(int) EOfferProcessed.InProcess} Output inserted.Id where Id in (select top {count} Id from offers with(nolock) where Processed = {(int) EOfferProcessed.NotProcessed})")
            )
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetInt32(0));
                }

                _logger.Info("Downloaded offers with id: " + string.Join(",", ids));
            }

            foreach (int id in ids)
            {
                yield return new Offer(dal, id);
            }
        }

        public void SetOffersAsInProcess(IDal dal, IEnumerable<Offer> offers)
        {
            _logger.Info(
                $"UPDATE Offers set Processed = {(int) EOfferProcessed.InProcess} WHERE Id in {string.Join(",", offers)}");
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int) EOfferProcessed.InProcess} WHERE Id in {string.Join(",", offers)}");
        }

        public void SetOfferAsProcessed(IDal dal, Offer offer)
        {
            _logger.Info($"UPDATE Offers set Processed = {(int) EOfferProcessed.Processed} WHERE Id = {offer.Id}");

            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int) EOfferProcessed.Processed} WHERE Id = {offer.Id}");
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

        public Task CreateTaskForProcessOrder(IDal dal, Offer offer, IOfferDetailsParser parser)
        {
            return new Task(() =>
            {
                try
                {
                    OfferDetails details = parser.GetPageDetails(offer.Uri, offer);
                    BufforController.Instance.InsertOfferDetails(details);
                }
                catch (OfferDetailsFailException ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsInActive(dal, offer);
                    _logger.Info(offer.Uri + ": fail - " + ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsUnprocessed(dal, offer);
                    _logger.Info(offer.Uri + ": fail - " + ex.Message);
                    return;
                }

                try
                {
                    SetOfferAsProcessed(dal, offer);
                    _logger.Info(offer.Uri + ": processed");
                }
                catch (OfferDetailsFailException ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsInActive(dal, offer);
                    _logger.Info(offer.Uri + ": fail - " + ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsUnprocessed(dal, offer);
                    _logger.Info(offer.Uri + ": fail - " + ex.Message);
                }
            });
        }
    }
}