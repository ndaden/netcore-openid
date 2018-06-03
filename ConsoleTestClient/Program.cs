using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using IdentityModel.Client;

namespace ConsoleTestClient
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var discoveryClient = DiscoveryClient.GetAsync(Constant._AuthorityUrl).GetAwaiter().GetResult();
            if (discoveryClient.IsError)
            {
                Console.WriteLine(discoveryClient.Error);
                return;
            }

            //request token
            var tokenClient = new TokenClient(discoveryClient.TokenEndpoint, Constant.clientId, Constant.clientSecret);
            var tokenResponse = tokenClient.RequestResourceOwnerPasswordAsync(Constant.username, Constant.password, Constant.scope).GetAwaiter().GetResult();

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            AuthorizationServerAnswer authorizationServerToken1;
            authorizationServerToken1 = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthorizationServerAnswer>(tokenResponse.Json.ToString());

            //secured web api request
            string resp = RequestValuesToSecuredWebApi(authorizationServerToken1)
                .GetAwaiter()
                .GetResult();

            Console.WriteLine("Response received from WebAPI:");
            Console.WriteLine(resp);

            //authorization server parameters owned from the client
            //this values are issued from the authorization server to the client through a separate process (registration, etc...)
            Uri authorizationServerTokenIssuerUri = new Uri("http://localhost:57696/connect/token");
            


            //resource owner password 
            string password = RequestResourceOwnerPassword(authorizationServerTokenIssuerUri, Constant.username, Constant.password, Constant.scope)
                .GetAwaiter()
                .GetResult();

            //access token request
            string rawJwtToken = RequestTokenToAuthorizationServer(
                 authorizationServerTokenIssuerUri,
                 Constant.clientId,
                 Constant.scope,
                 Constant.clientSecret)
                .GetAwaiter()
                .GetResult();

            AuthorizationServerAnswer authorizationServerToken;
            authorizationServerToken = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthorizationServerAnswer>(rawJwtToken);

            Console.WriteLine("Token acquired from Authorization Server:");
            Console.WriteLine(authorizationServerToken.access_token);

            //secured web api request
            string response = RequestValuesToSecuredWebApi(authorizationServerToken)
                .GetAwaiter()
                .GetResult();

            Console.WriteLine("Response received from WebAPI:");
            Console.WriteLine(response);
            Console.ReadKey();
        }
        
        //request a token from authorization server using client credentials
        private static async Task<string> RequestTokenToAuthorizationServer(Uri uriAuthorizationServer, string clientId, string scope, string clientSecret)
        {
            HttpResponseMessage responseMessage;
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, uriAuthorizationServer);
                HttpContent httpContent = new FormUrlEncodedContent(
                    new[]
                    {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("scope", scope),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                    });
                tokenRequest.Content = httpContent;
                responseMessage = await client.SendAsync(tokenRequest);
            }
            return await responseMessage.Content.ReadAsStringAsync();
        }

        //request resource owner password using user credentials
        private static async Task<string> RequestResourceOwnerPassword(Uri uriAuthorizationServer,string username, string password, string scope)
        {
            HttpResponseMessage responseMessage;
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, uriAuthorizationServer);
                HttpContent httpContent = new FormUrlEncodedContent(
                    new[]
                    {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("scope", scope)
                    });
                tokenRequest.Content = httpContent;
                responseMessage = await client.SendAsync(tokenRequest);
            }
            return await responseMessage.Content.ReadAsStringAsync();
        }

        private static async Task<string> RequestValuesToSecuredWebApi(AuthorizationServerAnswer authorizationServerToken)
        {
            HttpResponseMessage responseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationServerToken.access_token);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:51420/api/values");
                responseMessage = await httpClient.SendAsync(request);
            }

            return await responseMessage.Content.ReadAsStringAsync();
        }

        private class AuthorizationServerAnswer
        {
            public string access_token { get; set; }
            public string expires_in { get; set; }
            public string token_type { get; set; }

        }
    }

    
}


