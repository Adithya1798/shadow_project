using Login_Application.Repo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Login_Application.Service
{
    public class UserCustomRoleOnBoardingService
    {
        private readonly PxWhiteClient pxWhiteClient;

        private readonly StringBuilder log;

        // Read the adopter id from app config.
        private readonly string adopterId;

        // Read the organization role id from app config.
        private readonly string organizationAdminRoleId;

        // Read the organization role id from app config.
        private readonly string siteAdminRoleId;

        // Read the adopter admin role id from app config.
        private readonly string adopterAdminRoleId;

        // Read the organization role id from app config.
        private readonly string readerRoleId;
        private const string organizationKey = "organization";
        private const string scaEntityTypeKey = "WAS_Entity_Type";

        public UserCustomRoleOnBoardingService(StringBuilder _log)
        {
            log = _log;
            pxWhiteClient = new PxWhiteClient(log);
            adopterId = ConfigurationManager.AppSettings.Get("AdopterId");
            adopterAdminRoleId = ConfigurationManager.AppSettings.Get("AdopterAdminRoleId");
            organizationAdminRoleId = ConfigurationManager.AppSettings.Get("OrganizationAdminRoleId");
            siteAdminRoleId = ConfigurationManager.AppSettings.Get("SiteAdminRoleId");
            readerRoleId = ConfigurationManager.AppSettings.Get("ReaderRoleId");
        }

        public async Task InitiateUserCustomRoleOnboardingAsync()
        {
            try
            {
                //Get "Source" section information from app config.
                // It contains required details about the source.
                Dictionary<string, string> sourceInfo = GetSectionInfo("LoginCredentialDetail");

                log.Append("Information : Obtaining security token\n");
                AuthorizationToken securityToken = await pxWhiteClient.GetSecurityTokenAsync(sourceInfo);

                // Gets registered adopter users list.
                List<UserInformation> usersInformation = await GetAdopterUsersAsync(sourceInfo, securityToken);

                // Check custom roles for users and add role to user.
                await UpdateUsersWithCustomRoles(usersInformation, securityToken, sourceInfo);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the list of user for an adopter.
        /// </summary>
        /// <param name="sourceInfo">Base url details.</param>
        /// <param name="securityToken">Security token details.</param>
        /// <returns>List of user information.</returns>
        private async Task<List<UserInformation>> GetAdopterUsersAsync(Dictionary<string, string> sourceInfo, AuthorizationToken securityToken)
        {
            try
            {
                // Read the adopter id from app config.
                string adopterId = ConfigurationManager.AppSettings.Get("AdopterId");

                log.Append("Information : Obtaining registered adopter users \n");

                //Get users assoicated with adopter
                var usersList = await pxWhiteClient.GetAdopterUsersAsync(adopterId, true, securityToken.Token, sourceInfo["Url"]);

                return usersList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This method is responsible for updating custom role.
        /// </summary>
        /// <param name="usersInformation">List of user information.</param>
        /// <param name="securityToken">Security token details.</param>
        /// <param name="sourceInfo">Base url details.</param>
        /// <returns></returns>
        private async Task UpdateUsersWithCustomRoles(List<UserInformation> usersInformation, AuthorizationToken securityToken, Dictionary<string, string> sourceInfo)
        {
            log.Append("Information : Update users with RBAC custom roles\n");

            List<Task> createUserTask = new List<Task>();
            try
            {
                // Get washington role heirarchy with flat list.
                var wasRolesHierarchy = Utility.GetRolesHierarchy().ToList();

                foreach (var user in usersInformation)
                {
                    // Get the user profile for the user.
                    var userProfile = await pxWhiteClient.GetUserProfiles(adopterId, user.UserId, securityToken.Token, sourceInfo["Url"]);

                    if (userProfile != null && userProfile.Rules != null)
                    {
                        var existingUserProfileForLogging = JsonConvert.SerializeObject(userProfile.Rules);
                        bool needToUpdateProfile = false;
                        // Ignore, If user is having adopter admin role.
                        if (!userProfile.Rules.Any(rule => rule.RoleId == adopterAdminRoleId))
                        {
                            // Get the rules group by site id.
                            var userRulesBySite = userProfile.Rules.GroupBy(r => r.SiteId);

                            foreach (var keyValue in userRulesBySite)
                            {
                                var siteRules = keyValue.ToList();

                                // Checks whether user has permission for custom role.
                                if (siteRules.Any(rule => wasRolesHierarchy.Any(x => x.RoleId == rule.RoleId)))
                                {
                                    log.Append($"Information : User {user.UserId} already has a permission to custom role operations for Site: {keyValue.Key}.\n");
                                    continue;
                                }
                                else if (siteRules.Any(rule => rule.RoleId == siteAdminRoleId) && siteRules.Any(rule => rule.RoleId == readerRoleId))
                                {
                                    if (await IsOrganizationSite(keyValue.Key, securityToken, sourceInfo))
                                    {
                                        userProfile.Rules.Add(
                                        new Rule
                                        {
                                            RoleId = organizationAdminRoleId,
                                            SiteId = keyValue.Key
                                        });
                                        needToUpdateProfile = true;
                                    }
                                    else
                                    {
                                        log.Append($"Information : User {user.UserId}, Site Id: {keyValue.Key} is not a organization Id.\n");
                                        continue;
                                    }
                                }
                                else
                                {
                                    log.Append($"Information : User {user.UserId} does not have permission to site admin and reader operation for Site Id: {keyValue.Key}.\n");
                                    continue;
                                }
                            }
                        }

                        if (needToUpdateProfile)
                        {
                            log.Append($"Information : User {user.UserId}, Existing roles details before adding custom role {existingUserProfileForLogging}.\n");
                            createUserTask.Add(UpdateUserProfile(userProfile, securityToken, sourceInfo));
                        }
                    }
                }

                await Task.WhenAll(createUserTask);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This method is responsible for Updating user profile.
        /// </summary>
        /// <param name="userProfileDetails">User profile details.</param>
        /// <param name="sourceToken">Security token details.</param>
        /// <param name="sourceInfo">Base url details.</param>
        /// <returns></returns>
        private async Task UpdateUserProfile(UserProfileResponse userProfileDetails, AuthorizationToken sourceToken, Dictionary<string, string> sourceInfo)
        {
            try
            {
                // Update user profile with custom role.
                await pxWhiteClient.UpdateUserProfile(userProfileDetails, adopterId, sourceToken.Token, sourceInfo["Url"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Checks whether given site id is organization id.
        /// </summary>
        /// <param name="siteId">Site identity.</param>
        /// <param name="sourceToken">Security token details.</param>
        /// <param name="sourceInfo">Base url details.</param>
        /// <returns>Returns true or false.</returns>
        private async Task<bool> IsOrganizationSite(string siteId, AuthorizationToken sourceToken, Dictionary<string, string> sourceInfo)
        {
            try
            {
                var siteInfo = await pxWhiteClient.GetSiteInfo(siteId, sourceToken.Token, sourceInfo["Url"], false);

                bool isvalidOrg = false;

                if (siteInfo != null && siteInfo.CustomAttributes?.Count > 0 && siteInfo.CustomAttributes.ContainsKey(scaEntityTypeKey) &&
                organizationKey.Equals(siteInfo.CustomAttributes?[scaEntityTypeKey], StringComparison.CurrentCultureIgnoreCase))
                {
                    isvalidOrg = true;
                }

                return isvalidOrg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get section info from app config file.
        /// </summary>
        /// <param name="type">Type of section.</param>
        /// <returns>Key value pair of config values.</returns>
        private Dictionary<string, string> GetSectionInfo(string type)
        {
            NameValueCollection sectionInfo = (NameValueCollection)ConfigurationManager.GetSection($"{type}/Info");
            Dictionary<string, string> sectionDictionary = sectionInfo.AllKeys.ToDictionary(key => key, key => sectionInfo[key]);
            return sectionDictionary;
        }
    }
}