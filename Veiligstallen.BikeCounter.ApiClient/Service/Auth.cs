using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using SQLitePCL;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        private string GetAuthorizationHeaderValue()
            => GetAuthorizationHeaderValue(_user, _pass, _authHeader);

        /// <summary>
        /// Gets roles 
        /// </summary>
        /// <returns></returns>
        public async Task<AuthOutput> AuthenticateAsync()
        {
            var authHdr = GetAuthorizationHeaderValue();

            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<AuthOutput>(_cfg.Endpoint,
                Configuration.Routes.AUTH, Method.GET, authToken: authHdr);

            EnsureValidResponse(apiOut.Response);

            return apiOut.Output;
        }

        /// <summary>
        /// Gets authorization header value
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="authHeader"></param>
        /// <returns></returns>
        private static string GetAuthorizationHeaderValue(string user, string pass, string authHeader)
            => string.IsNullOrWhiteSpace(authHeader)
                ? GetAuthorizationHeaderValue(user, pass)
                : GetAuthorizationHeaderValue(authHeader);

        /// <summary>
        /// Gets auth hdr value
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        private static string GetAuthorizationHeaderValue(string user, string pass)
        {
            var cfg = Configuration.Read();

            switch (cfg?.AuthorizationScheme?.ToLower())
            {
                case "basic":
                    return GetAuthorizationHeaderValue(
                        System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{user}:{pass}"))
                    );
            }

            return null;
        }

        /// <summary>
        /// Gets auth hdr value
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        private static string GetAuthorizationHeaderValue(string authToken)
        {
            var cfg = Configuration.Read();
            return $"{cfg?.AuthorizationScheme} {authToken}";
        }
    }
}
