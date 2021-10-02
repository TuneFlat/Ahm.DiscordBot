using Ahm.DiscordBot.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Ahm.DiscordBot.Services
{
    public class DestinyService : IDestinyService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountConnectionsService _accountConnections;
        private readonly IDestinyManifestService _destinyManifestService;
        private readonly IMapper _mapper;

        private readonly string urlRoot = "https://www.bungie.net/Platform";

        private Dictionary<string, List<string>> _titleHashes;

        // Instantiate the request url which will be built as the service is used
        public DestinyService(IAccountConnectionsService accountConnections, IConfiguration configuration,
            IDestinyManifestService destinyManifestService, IMapper mapper)
        {
            _configuration = configuration;
            _accountConnections = accountConnections;
            _destinyManifestService = destinyManifestService;
            _mapper = mapper;

            
            _titleHashes = GetTitleHashsFromManifest();
        }


        public bool HasTitle(string titleName, ulong discordId)
        {
            titleName = titleName.ToLower();

            var destinyMembership = GetDestinyMembershipWithType(discordId);
            string requestEndpoint = string.Format("/Destiny2/{0}/Profile/{1}/?components=900",
                destinyMembership.destiny_membership_type, destinyMembership.destiny_membership_id);

            var requestContent = ExecuteRequest(requestEndpoint).Content;

            var destinyProfileModel = JsonConvert.DeserializeObject<DestinyProfileModel>(requestContent);

            if (destinyProfileModel is null) return false;

            // Locate the record for the title and return the objective complete property
            var records = destinyProfileModel.Response.profileRecords.data.records;
            var isComplete = false;
            foreach (var hash in _titleHashes[titleName]) {
                var titleRecord = records[hash];
                isComplete = titleRecord.objectives.Any(x => x.complete);
            }
            return Convert.ToBoolean(isComplete);
        }

        public IRestResponse ExecuteRequest(string requestEndpoint)
        {

            var client = new RestClient(urlRoot); // titles are included in this
            var request = new RestRequest(requestEndpoint, Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("X-API-Key", _configuration["Bungie:ApiToken"]);
            IRestResponse response = client.Execute(request);

            return response;
        }

        public AccountConnectionsModel GetDestinyMembershipWithType(ulong discordId)
        {
            return _accountConnections.GetDestinyMembershipWithType(discordId);

        }
        
        public Dictionary<string, List<string>> GetTitleHashsFromManifest()
        {
            //_destinyManifestService.TestManifestConnection();
            var PresentationNodeDef = "DestinyPresentationNodeDefinition";
            var RecordDef = "DestinyRecordDefinition";

            var currentSealsData = _destinyManifestService.GetDefinition(PresentationNodeDef, "616318467");
            var legacySealsData = _destinyManifestService.GetDefinition(PresentationNodeDef, "1881970629");

            
            var currentSealsPresentation = JsonConvert.DeserializeObject<DestinyPresentationNodeModel>(currentSealsData.Json);
            var legacySealsPresentation = JsonConvert.DeserializeObject<DestinyPresentationNodeModel>(legacySealsData.Json);
            var allSealPresentationNodes = currentSealsPresentation.children.presentationNodes
                .Concat(legacySealsPresentation.children.presentationNodes);


            // TODO: See about adding type arguments to GetDefinition.
            Dictionary<string, List<string>> titleWithHashes = new Dictionary<string, List<string>>();
            foreach (var node in allSealPresentationNodes)
            {
                var sealPresentationDef = _destinyManifestService.GetDefinition(PresentationNodeDef, HashToSigned32Int(node.presentationNodeHash));
                var sealPresentationNode = JsonConvert.DeserializeObject<DestinyPresentationNodeModel>(sealPresentationDef.Json);
                var completionRecordHash = sealPresentationNode.completionRecordHash;



                var titleObj = _destinyManifestService.GetDefinition(RecordDef, HashToSigned32Int(completionRecordHash));
                if (titleObj != null)
                {
                    var testTitle = JsonConvert.DeserializeObject<DestinyRecordModel>(titleObj.Json);
                    if (testTitle.titleInfo.hasTitle)
                    {
                        var title = testTitle.titleInfo.titlesByGender.Male.ToLower();
                        if (!titleWithHashes.ContainsKey(title))
                            titleWithHashes.Add(title, new List<string>());

                        titleWithHashes[title].Add(testTitle.hash.ToString());
                    }
                }
            }
            // var titleInformationPath = string.Format("{0}\\TitleInformation.json", Environment.CurrentDirectory);
            // File.WriteAllText(titleInformationPath, JsonConvert.SerializeObject(titleWithHashes, Formatting.Indented));
            return titleWithHashes;
        }

        public string HashToSigned32Int(string unsignedhash)
        {
            return unchecked((int)UInt32.Parse(unsignedhash)).ToString();
        }

        public List<string> GetUserTitles(ulong discordId)
        {
            List<string> userTitles = new List<string>();

            var destinyMembership = GetDestinyMembershipWithType(discordId);
            string requestEndpoint = string.Format("/Destiny2/{0}/Profile/{1}/?components=900",
                destinyMembership.destiny_membership_type, destinyMembership.destiny_membership_id);

            var requestContent = ExecuteRequest(requestEndpoint).Content;

            var destinyProfileModel = JsonConvert.DeserializeObject<DestinyProfileModel>(requestContent);
            var records = destinyProfileModel.Response.profileRecords.data.records;
            var isComplete = false;
            var titles = _titleHashes.Values.SelectMany(x => x).ToList();
            foreach (var title in titles)
            {
                RecordProperties titleRecord = new RecordProperties();
                if (records.TryGetValue(title, out titleRecord))
                {
                    isComplete = titleRecord.objectives.Any(x => x.complete);
                    if (isComplete)
                        userTitles.Add(title);
                };
            }

            return userTitles;
        }
    }
}
