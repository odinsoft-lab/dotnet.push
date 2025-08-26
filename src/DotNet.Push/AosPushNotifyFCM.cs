using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNet.Push
{
    public class AosPushNotifyFCM
    {
        /// <summary>
        /// Initialize the FCM pusher with server credentials and a default notification tag.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="serverId"></param>
        /// <param name="alarmTag">Whether each notification appears as a new entry in Android's notification drawer.</param>
        public AosPushNotifyFCM(string serverKey, string serverId, string alarmTag)
        {
            ServerKey = serverKey;
            ServerId = serverId;
            AlarmTag = alarmTag;
        }

        /// <summary>
        /// Send a notification to a specific device token.
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <param name="priority">high,normal</param>
        /// <param name="title"></param>
        /// <param name="clickAction"></param>
        /// <param name="message"></param>
        /// <param name="badge"></param>
        /// <param name="iconName"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public async Task<(bool success, string message)> SendNotificationAsync(string deviceToken, string priority, string title, string clickAction, string message, int badge, string iconName, string color)
        {
            var result = (success: false, message: "ok");

            try
            {
                var pusher = new AosPushProtocol
                {
                    to = deviceToken,
                    priority = priority,

                    notification = new AosNotification
                    {
                        title = title,
                        click_action = clickAction,

                        body = message,
                        icon = iconName,
                        color = color,
                        tag = AlarmTag
                    },
                    data = new AosNotifyData
                    {
                        title = title,
                        badge = badge,
                        message = message
                    }
                };

                var content = JsonSerializer.Serialize(pusher);

                using (var http_client = new HttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri("https://fcm.googleapis.com/fcm/send"),
                        Method = HttpMethod.Post,
                        Content = new StringContent(content, Encoding.UTF8, "application/json")
                    };

                    request.Headers.Authorization = new AuthenticationHeaderValue("key", "=" + ServerKey);
                    request.Headers.Add("Sender", "id=" + ServerId);

                    var response = await http_client.SendAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result.message = $"success";
                        result.success = true;
                    }
                    else
                    {
                        result.message = $"{response.StatusCode}";
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
        /// FCM server key
        /// </summary>
        public string ServerKey
        {
            get;
            set;
        }

        /// <summary>
        /// FCM sender ID
        /// </summary>
        public string ServerId
        {
            get;
            set;
        }

        /// <summary>
        /// Default notification tag for Android
        /// </summary>
        public string AlarmTag
        {
            get;
            set;
        }
    }
}