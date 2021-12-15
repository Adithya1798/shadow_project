using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Login_Application.Repo
{
    public class PxWhiteClient
    {
        private readonly HttpClientFactory httpClientFactory;
        private readonly StringBuilder log;

        public PxWhiteClient(StringBuilder _log)
        {
            httpClientFactory = new HttpClientFactory();
            log = _log;
        }

        public async Task<List<UserInformation>> GetAdopterUsersAsync(string adopterId, bool thisAdopterOnly, string token, string baseAddress)
        {
            var absoluteUrl = $"{baseAddress}/api/v1/accesscontrol/users?adopterId={adopterId}&thisAdopterOnly={thisAdopterOnly}";
            var response = httpClientFactory.GetAsync(absoluteUrl, token).Result;
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                log.Append($"Error : Unable to obtain users for {adopterId}\n {content}\n");

            return JsonConvert.DeserializeObject<List<UserInformation>>(content);

        }

        public async Task<UserProfileResponse> GetUserProfiles(string adopterId, string userId, string token, string baseAddress)
        {

            var absoluteUrl = $"{baseAddress}/api/v1/useraccessprofiles/adopters/{adopterId}/users/{userId}";
            var response = httpClientFactory.GetAsync(absoluteUrl, token).Result;
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                log.Append($"Error : Unable to obtain profile for {userId}\n {content}\n");

            return JsonConvert.DeserializeObject<UserProfileResponse>(content);
        }

        public async Task<SiteInformationResponse> GetSiteInfo(string siteId, string token, string baseAddress, bool recursive = false)
        {

            var absoluteUrl = $"{baseAddress}/api/v1/sites/{siteId}?recursive={recursive}";
            var response = httpClientFactory.GetAsync(absoluteUrl, token).Result;
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                log.Append($"Error : Unable to obtain site info for {siteId}\n {content}\n");

            return JsonConvert.DeserializeObject<SiteInformationResponse>(content);
        }

        public async Task UpdateUserProfile(UserProfileResponse userProfileDetails, string adopterId, string token, string baseAddress)
        {
            var absoluteUrl = $"{baseAddress}/api/v1/useraccessprofiles/adopters/{adopterId}/users/{userProfileDetails.Id}";

            var response = httpClientFactory.PutAsync(userProfileDetails, absoluteUrl, token).Result;
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.Append($"Error : Unable to add custom role for {userProfileDetails.Id} \n {content}\n");
            }
            log.Append($"Information : Added user {userProfileDetails.Id} with custom role{JsonConvert.SerializeObject(userProfileDetails.Rules)}.\n");
            return;
        }

        public async Task<AuthorizationToken> GetSecurityTokenAsync(Dictionary<string, string> sectionDictionary)
        {
            var absoluteUrl = sectionDictionary["Url"] + "/api/v1/security/token";
            var response = httpClientFactory.PostAsync(new Login(sectionDictionary["User"], sectionDictionary["Password"], sectionDictionary["ApplicationId"]), absoluteUrl).Result;
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                log.Append($"Error : Unable to obtain security token\n {content}\n");

            return JsonConvert.DeserializeObject<AuthorizationToken>(content);
        }
    }
}