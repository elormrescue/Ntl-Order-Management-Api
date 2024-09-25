using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class IdServerClient
    {
        private readonly HttpClient _client;
        private readonly IdServerClientConfig _idServerClientConfig;

        public IdServerClient(HttpClient client, IdServerClientConfig idServerClientConfig)
        {
            _client = client;
            _idServerClientConfig = idServerClientConfig;
        }

        public async Task<string> GetToken()
        {
            var disco = await _client.GetDiscoveryDocumentAsync();
            if (disco.IsError)
            {
                throw new Exception("Discovery document of identity server not found");
            }
            var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = _idServerClientConfig.ClientId,
                ClientSecret = _idServerClientConfig.Secret
            });

            if (tokenResponse.IsError)
                throw new Exception(tokenResponse.Exception.Message);

            return tokenResponse.AccessToken;

        }
    }
}
