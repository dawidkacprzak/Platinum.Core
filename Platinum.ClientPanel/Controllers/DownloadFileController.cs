using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Platinum.Core.Model;

namespace Platinum.ClientPanel.Controllers
{
    public class DownloadFileController
    {
        public static string CreateCsvOffer(List<OfferDetails> offers)
        {
            if (!Directory.Exists("wwwroot"))
            {
                Directory.CreateDirectory("wwwroot/ufiles");
            }

            if (!Directory.Exists("wwwroot/ufiles"))
            {
                Directory.CreateDirectory("wwwroot/ufiles");
            }

            string path;
            string guid = Guid.NewGuid().ToString();
            path = Path.Combine("wwwroot/ufiles", guid + ".csv");

            List<string> csvLines = new List<string>();
            csvLines.Add("Tytuł;Link;Data Zaindeksowania;Cena zł;Atrybuty");
            foreach (var o in offers)
            {
                string line = string.Empty;
                line += o.Title.Replace(";", "");
                line += ";";
                line += o.Uri.ToString(CultureInfo.InvariantCulture).Replace(";", "");
                line += ";";
                line += o.CreatedDate.ToString(CultureInfo.InvariantCulture).Replace(";", "");
                line += ";";
                line += o.Price.ToString().Replace(";", "");
                foreach(var attr in o.Attributes)
                {
                    line += ";";
                    line += attr.Key.Replace(";", ""); ;
                    line += ";";
                    line +=attr.Value.Replace(";", "");
                }
                csvLines.Add(line);
            }

            System.IO.File.WriteAllLines(path, csvLines,Encoding.UTF8);
            return Path.Combine("ufiles", guid + ".csv");
            ;
        }
    }
}