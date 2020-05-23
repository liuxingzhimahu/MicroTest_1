using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using User.Identity.Options;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        private IHttpClient _httpClient;
        private ILogger<UserService> _logger;
        private string _userServiceUrl ;

        public UserService(IHttpClient httpClient,
            IOptions<ServiceDiscoveryOptions> serviceDiscoveryOptions, 
            IDnsQuery dnsquery,
             ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            this._logger = logger;
            var address = dnsquery.ResolveService("service.consul", serviceDiscoveryOptions.Value.UserServiceName);
            var addressList = address.First().AddressList;

            var host = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;

            _userServiceUrl = $"http://{host}:{port}";




        }

        public async Task<int> CheckOrCreate(string phone)
        {
            _logger.LogTrace($"Enter into CheckOrCreate:{phone}");

            var form = new Dictionary<string, string>
            {
                { "phone",phone}
            };

            try
            {
                var response = await _httpClient.PostAsync(_userServiceUrl + "/api/users/check-or-create", form);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var userId = await response.Content.ReadAsStringAsync();
                    int.TryParse(userId, out int intUserId);
                    return intUserId;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" CheckOrCreate 在重试之后失败", ex.Message + ex.StackTrace);
                throw ex;
            }

            return 0;

        }
    }
}
