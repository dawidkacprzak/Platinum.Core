using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
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

        public AllegroOfferDetailsFetcher(int countOfTasks)
        {
            CountOfParallelTasks = countOfTasks;
        }

        [ExcludeFromCodeCoverage]
        public void Run(IDal dal)
        {
            Console.WriteLine($"Application started and tak count {CountOfParallelTasks}");
            try
            {
                WebClient c = new WebClient();
                string exIp = c.DownloadString("https://ifconfig.me");
                AddErrorLogsToDb("App (V2) started with ip: " + exIp);
            }catch(Exception)
            {

            }

            _logger.Info($"Application started and task count {CountOfParallelTasks}");
            List<Offer> lastNotProcessedOffers = GetLastNotProcessedOffers(dal, CountOfParallelTasks * 60).ToList();
            _logger.Info($"Fetched: " + lastNotProcessedOffers.Count + " offers");
            HttpAllegroOfferDetailsParser tempParser =
                new HttpAllegroOfferDetailsParser();
            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(CountOfParallelTasks))
            {
                List<Task> tasks = new List<Task>();
                foreach (var offer in lastNotProcessedOffers)
                {
                    _logger.Info("last not processed offers: " + lastNotProcessedOffers.Count);
                    concurrencySemaphore.Wait();
                    var t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            using (Dal db = new Dal())
                            {
                                Task tx = CreateTaskForProcessOrder(db, offer,
                                    new HttpAllegroOfferDetailsParser());
                                tx.RunSynchronously();
                            }
                        }
                        finally
                        {
                            concurrencySemaphore.Release();
                        }
                    });

                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        public IEnumerable<Offer> GetLastNotProcessedOffers(IDal dal, int count)
        {
            List<int> ids = new List<int>();
            using (DbDataReader reader =
                dal.ExecuteReader(
                    $"update offers set Processed = {(int)EOfferProcessed.InProcess} Output inserted.Id where Id in (select top {count} Id from offers with(nolock) where Processed = {(int)EOfferProcessed.NotProcessed})")
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
                $"UPDATE Offers set Processed = {(int)EOfferProcessed.InProcess} WHERE Id in {string.Join(",", offers)}");
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int)EOfferProcessed.InProcess} WHERE Id in {string.Join(",", offers)}");
        }

        public void SetOfferAsProcessed(IDal dal, Offer offer)
        {
            _logger.Info($"UPDATE Offers set Processed = {(int)EOfferProcessed.Processed} WHERE Id = {offer.Id}");

            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int)EOfferProcessed.Processed} WHERE Id = {offer.Id}");
        }


        public void SetOfferAsUnprocessed(IDal dal, Offer offer)
        {
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int)EOfferProcessed.NotProcessed} WHERE Id = {offer.Id}");
        }

        public void SetOfferAsInActive(IDal dal, Offer offer)
        {
            dal.ExecuteNonQuery(
                $"UPDATE Offers set Processed = {(int)EOfferProcessed.Inactive} WHERE Id = {offer.Id}");
        }

        [ExcludeFromCodeCoverage]
        public Task CreateTaskForProcessOrder(IDal dal, Offer offer, IOfferDetailsParser parser)
        {
            return new Task(() =>
            {
                System.Diagnostics.Debug.WriteLine("start");
                try
                {
                    OfferDetails details = parser.GetPageDetails(offer.Uri, offer);
                    ElasticController.Instance.InsertOfferDetails(details);
                }
                catch (OfferDetailsFailException ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    AddErrorLogsToDb(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsInActive(dal, offer);
                    _logger.Info(offer.Uri + ": fail - " + ex.Message + ex.StackTrace);
                    System.Diagnostics.Debug.WriteLine("end");

                    return;
                }
                catch (Exception ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    AddErrorLogsToDb(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsInActive(dal, offer);
                    _logger.Info(offer.Uri + ": fail - " + ex.Message + ex.StackTrace);
                    System.Diagnostics.Debug.WriteLine("end");

                    return;
                }

                try
                {
                    System.Diagnostics.Debug.WriteLine("PROCESSED");
                    SetOfferAsProcessed(dal, offer);
                    _logger.Info(offer.Uri + ": processed");
                }
                catch (OfferDetailsFailException ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    AddErrorLogsToDb(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    SetOfferAsInActive(dal, offer);
                    _logger.Info(offer.Uri + ": fail - " + ex.Message + ex.StackTrace);
                }
                catch (Exception ex)
                {
                    _logger.Info(ex.Message + " " + ex.StackTrace + " " + offer.Uri);
                    AddErrorLogsToDb(ex.Message + " " + ex.StackTrace + " " + offer.Uri);

                    SetOfferAsInActive(dal, offer);
                    _logger.Info(offer.Uri + ": fail - " + ex.Message + ex.StackTrace);
                }
            });
        }

        private void AddErrorLogsToDb(string message)
        {
            string machineName;
            try
            {
                machineName = Environment.MachineName;
            }
            catch (Exception)
            {
                machineName = "unknown";
            }
            using (Dal db = new Dal())
            {
                message = message.Replace('\'', ' ');
                message = message.Replace('\"', ' ');
                db.ExecuteNonQuery(@"
                    INSERT INTO dockerOfferDetailsFetcherLogs VALUES('" + message + "',getdate(),'" + machineName + "');"
                );
            }
        }
    }
}