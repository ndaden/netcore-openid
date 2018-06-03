using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServerAuth
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("scope.readaccess","example api"),
                new ApiResource("scope.fullaccess","example api"),
                new ApiResource("whathever","example api")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "ReaderClient",
                    //AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = {new Secret("secret1".Sha256())},
                    AllowedScopes = {"scope.readaccess"}
                },
                new Client
                {
                    ClientId = "FullAccessClient",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = {new Secret("secret2".Sha256())},
                    AllowedScopes = { "scope.fullaccess" }
                }
            };
        }

        public static IEnumerable<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    Username = "bob",
                    Password ="bobpassword",
                    SubjectId = "1",
                },
                new TestUser
                {
                    Username = "alice",
                    Password ="alicepassword",
                    SubjectId = "2"
                }
            };
        }
    }
}
