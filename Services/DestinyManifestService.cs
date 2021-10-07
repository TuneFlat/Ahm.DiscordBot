using Ahm.DiscordBot.Models;
using System.Data.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Ahm.DiscordBot.Services
{
    public class DestinyManifestService : IDestinyManifestService
    {
        private readonly IConfiguration _config;
        private readonly string BUNGIE_URL = "https://www.bungie.net";
        private ILogger _logger;


        public DestinyManifestService(IConfiguration configuration, ILogger<DestinyManifestService> logger)
        {
            _config = configuration;
            _logger = logger;
        }
        public JToken DoRequest(string path)
        {
            var client = new RestClient(BUNGIE_URL + path);
            var request = new RestRequest(Method.GET);

            request.AddHeader("X-API-Key", _config["Bungie:ApiToken"]);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");


            IRestResponse response = client.Execute(request);

            return JsonConvert.DeserializeObject<JToken>(response.Content);
        }

        public void CheckManifest()
        {
            var dataVersionsPath = string.Format("{0}\\DataVersions.json", Environment.CurrentDirectory);

            IFileIOService fileIOService = new FileIOService();
            var destinyManifestVersion = fileIOService.ReadFile<DestinyManifestVersion>(dataVersionsPath);

            // If the Manifest has not been checked today, update it if the version is different.
            if (destinyManifestVersion?.LastChecked.Day != null && destinyManifestVersion?.LastChecked.Day != DateTime.Now.Day)
            {
                dynamic manifest = DoRequest("/Platform/Destiny2/Manifest/");
                string versionFromManifest = manifest.Response.version;
                var enPath = manifest.Response.mobileWorldContentPaths.en;
                destinyManifestVersion.LastChecked = DateTime.Now;

                if (destinyManifestVersion?.LastModified == null || destinyManifestVersion.Version != versionFromManifest)
                {
                    destinyManifestVersion.Version = versionFromManifest;
                    destinyManifestVersion.LastModified = DateTime.Now;
                    UpdateManifest(BUNGIE_URL + enPath);
                }
            }

            try
            {
                fileIOService.WriteFile(dataVersionsPath, destinyManifestVersion); 
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }
        }

        public void UpdateManifest(string url)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string fileName = "mobileWorldContent";
            string zipManifestPath = string.Format("{0}/{1}.zip", workingDirectory, fileName);
            string unzippedManifestPath = string.Format("{0}/{1}.sqlite3", workingDirectory, fileName);

            // Deleting manifest if it already exists.

            if (File.Exists(unzippedManifestPath))
            {
                File.Delete(unzippedManifestPath);
            }


            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-API-Key", _config["Bungie:ApiToken"]);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            var response = client.DownloadData(request);

            // TODO: Test to see if property 'Copy to Output Directory' needs to be set to always on server
            try
            {
                File.WriteAllBytes(zipManifestPath, response);
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }
            // Renaming database in zipped manifest and unzipping the manifest
            RenameZipEntries(zipManifestPath);


            string extractPath = workingDirectory;
            try
            {
                ZipFile.ExtractToDirectory(zipManifestPath, extractPath);
                //File.Delete(zipManifestPath);
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }
        }

        // TODO: Refactor this to work with only one zip entry....
        private static void RenameZipEntries(string file)
        {
            using (var archive = new ZipArchive(File.Open(file, FileMode.Open, FileAccess.ReadWrite), ZipArchiveMode.Update))
            {
                var entries = archive.Entries.ToArray();
                foreach (var entry in entries)
                {
                    var newEntry = archive.CreateEntry("mobileWorldContent" + ".sqlite3");
                    using (var a = entry.Open())
                    using (var b = newEntry.Open())
                        a.CopyTo(b);
                    entry.Delete();
                }
            }
        }

        public IList<BaseManifestDefinitionModel> QueryManifest(string query)
        {
            CheckManifest();
            SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=mobileWorldContent.sqlite3");
            var result = new List<BaseManifestDefinitionModel>();

            try
            {
                sqlite_conn.Open();
                SQLiteCommand command = sqlite_conn.CreateCommand();
                command.CommandText = query;

                var data = command.ExecuteReader(System.Data.CommandBehavior.KeyInfo);

                while (data.Read())
                {
                    var id = data.GetInt32(0).ToString();
                    var json = JsonConvert.DeserializeObject(data.GetString(1));
                    result.Add(new BaseManifestDefinitionModel(id, json.ToString()));
                }
                sqlite_conn.Close();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            return result;
        }

        public IList<BaseManifestDefinitionModel> GetDefinitions(string tableName)
        {
            return QueryManifest("SELECT * FROM " + tableName);
        }

        public BaseManifestDefinitionModel GetDefinition(string tableName, string id)
        {
            return QueryManifest(string.Format("SELECT * FROM {0} WHERE id = {1}", tableName, id)).FirstOrDefault();
        }

        public void TestManifestConnection()
        {
            SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=mobileWorldContent.sqlite3");

            try
            {
                sqlite_conn.Open();
                SQLiteCommand command = sqlite_conn.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type ='table'";

                var data = command.ExecuteReader();

                while (data.Read())
                {
                    Console.WriteLine(data.GetString(0));
                }
                sqlite_conn.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

    }
}
