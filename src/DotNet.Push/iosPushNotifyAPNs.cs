using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Push
{
    /// <summary>
    ///
    /// </summary>
    public class IosPushNotifyAPNs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="bundleAppId"></param>
        /// <param name="apnsPrivatekeyId"></param>
        /// <param name="apnsPrivateKey"></param>
        /// <param name="algorithm"></param>
        /// <param name="production"></param>
        /// <param name="port"></param>
        public IosPushNotifyAPNs(string teamId, string bundleAppId, string apnsPrivatekeyId, string apnsPrivateKey, string algorithm = "ES256", bool production = true, int port = 443, int expireMinutes = 60)
        {
            TeamId = teamId;
            BundleAppId = bundleAppId;
            Algorithm = algorithm;

            HostServerUrl = production ? "api.push.apple.com" : "api.development.push.apple.com";
            HostPort = port;

            PrivateKeyId = apnsPrivatekeyId;
            P8PrivateKey = getP8PrivateKey(apnsPrivateKey);

            ExpireMinutes = expireMinutes;
        }

        private static DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private long getEpochTimestamp()
        {
            return (long)Math.Round((DateTime.UtcNow - UnixEpoch).TotalSeconds);
        }

        private string getP8PrivateKey(string auth_key_path)
        {
            //    var content = System.IO.File.ReadAllText(auth_key_path);
            //    return content.Split('\n')[1];

            var content = System.IO.File.ReadAllLines(auth_key_path);
            var privateKeyLines = content.Skip(1).Take(content.Length - 2); // 첫 줄과 마지막 줄을 제외
            return string.Join("", privateKeyLines); // 모든 줄을 하나의 문자열로 합침
        }

        private string getJwtToken()
        {
            var header = JsonSerializer.Serialize(new
            {
                alg = Algorithm,
                kid = PrivateKeyId
            });

            var timestamp = getEpochTimestamp();

            var payload = JsonSerializer.Serialize(new
            {
                iss = TeamId,
                iat = timestamp,
                exp = timestamp + ExpireMinutes * 60
            });

            var header_base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
            var payload_base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
             
            var jwt_data = $"{header_base64}.{payload_base64}";
            var jwt_bytes = Encoding.UTF8.GetBytes(jwt_data);

            var key_bytes = Convert.FromBase64String(P8PrivateKey);

            using var ecdsa = ECDsa.Create();
            ecdsa.ImportPkcs8PrivateKey(key_bytes, out _);

            var signature = ecdsa.SignData(jwt_bytes, HashAlgorithmName.SHA256);
            var sign_base64 = Convert.ToBase64String(signature);

            return $"{jwt_data}.{sign_base64}";
        }

        private bool getJwtExpired(string accessToken)
        {
            var result = true;

            if (!String.IsNullOrEmpty(accessToken))
            {
                var parts = accessToken.Split('.');
                if (parts.Length == 3)
                {
                    var payload = parts[1];
                    var bytes = Convert.FromBase64String(payload);

                    var json = Encoding.UTF8.GetString(bytes);
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                    if (data != null && data.TryGetValue("exp", out var expValue) && expValue is long exp)
                    {
                        var timestamp = getEpochTimestamp();
                        result = exp < timestamp;
                    }
                }
            }

            return result;
        }

        private async Task<(bool success, string message)> pushJwtAPNsAsync(Uri requestUri, string accessToken, string payload, string requestUuid, CancellationToken cancellationToken)
        {
            var result = (success: false, message: "ok");

            try
            {
                using (var http_client = new HttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = requestUri,
                        Version = new Version("2.0"),
                        Content = new StringContent(payload)
                    };

                    request.Headers.Add("authorization", String.Format("bearer {0}", accessToken));
                    request.Headers.Add("apns-id", requestUuid);
                    request.Headers.Add("apns-expiration", "0");
                    request.Headers.Add("apns-priority", "10");
                    request.Headers.Add("apns-push-type", "alert");
                    request.Headers.Add("apns-topic", BundleAppId);

                    var response = await http_client.SendAsync(request, cancellationToken);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var response_uuid = "";

                        IEnumerable<string> values;

                        if (response.Headers.TryGetValues("apns-id", out values))
                        {
                            response_uuid = values.First();

                            result.message = $"success: '{response_uuid}'";
                            result.success = true;
                        }
                        else
                        {
                            result.message = "failure";
                        }
                    }
                    else
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                        if (data.TryGetValue("reason", out var reason))
                            result.message = $"failure: '{reason}'";
                    }
                }
            }
            catch (Exception ex)
            {
                result.message = $"exception: '{ex.Message}'";
            }

            return result;
        }

        /// <summary>
        /// Token Based Authentication APNs push
        /// </summary>
        /// <param name="deviceToken">device token</param>
        /// <param name="content">push content</param>
        /// <param name="badge">badge number</param>
        /// <param name="sound">sound file name</param>
        public async Task<(bool success, string message)> JwtAPNsPushAsync(string deviceToken, object content, string apnsId, int badge, string sound, CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri($"https://{HostServerUrl}:{HostPort}/3/device/{deviceToken}");

            var accessToken = JwtToken;
            {
                if (getJwtExpired(accessToken))
                {
                    accessToken = getJwtToken();
                    JwtToken = accessToken;
                }
            }

            var payload = JsonSerializer.Serialize(new
            {
                aps = new
                {
                    alert = content,
                    badge,
                    sound
                },
                id = apnsId
            });

            return await pushJwtAPNsAsync(requestUri, accessToken, payload, apnsId, cancellationToken);
        }

        /// <summary>
        ///
        /// </summary>
        public string Algorithm
        {
            get;
        }

        /// <summary>
        ///
        /// </summary>
        public string HostServerUrl
        {
            get;
        }

        /// <summary>
        ///
        /// </summary>
        public int HostPort
        {
            get;
        }

        /// <summary>
        ///
        /// </summary>
        public string TeamId
        {
            get;
        }

        /// <summary>
        ///
        /// </summary>
        public string BundleAppId
        {
            get;
        }

        /// <summary>
        ///
        /// </summary>
        public string PrivateKeyId
        {
            get;
        }

        /// <summary>
        ///
        /// </summary>
        public string P8PrivateKey
        {
            get;
        }

        public int ExpireMinutes
        {
            get;
            set;
        }

        public string JwtToken
        {
            get;
            set;
        }
    }
}