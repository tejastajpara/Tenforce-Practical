using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Test_Taste_Console_Application.Constants;

namespace Test_Taste_Console_Application.Domain.Services
{
    ///<summary>
    /// A service to create the HttpClient. 
    ///</summary>
    public class HttpClientService
    {
        public HttpClient Client { get; }

        public HttpClientService(HttpClient client)
        {
            Client = client;
            Client.BaseAddress = new Uri(UriPath.BaseUri);

            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(HttpClientSettings.JsonType));

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "258dc5e7-d359-4e11-87d7-73871d0a76e5");
        }
    }
}
