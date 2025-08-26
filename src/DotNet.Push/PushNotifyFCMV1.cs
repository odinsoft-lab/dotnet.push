using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNet.Push;

public class PushNotifyFCMV1
{
    public PushNotifyFCMV1(string json)
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromJson(json),
        });
    }

    /// <summary>
    ///
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
            var request = new Message()
            {
                Token = deviceToken,
                Notification = new Notification()
                {
                    Title = title,
                    Body = message
                },
                Apns = new ApnsConfig()
                {
                    Headers = new Dictionary<string, string>()
                    {
                        { "apns-priority", priority == "high" ? "10" : "5" }
                    },
                    Aps = new Aps()
                    {
                        Alert = new ApsAlert()
                        {
                            Title = title,
                            Body = message
                        },
                        Badge = badge,
                        Sound = "default",
                        ContentAvailable = true,
                        MutableContent = true
                    }
                },
                Android = new AndroidConfig()
                {
                    Priority = priority == "high" ? Priority.High : Priority.Normal,
                    Notification = new AndroidNotification()
                    {
                        ClickAction = clickAction,
                        Icon = iconName,
                        Color = color,
                    }
                },
                Data = new Dictionary<string, string>()
                {
                    { "title", title },
                    { "badge", badge.ToString() },
                    { "message", message }
                }
            };

            var messaging = FirebaseMessaging.DefaultInstance;
            var response = await messaging.SendAsync(request);

            result.message = response;
            result.success = true;
        }
        catch (Exception ex)
        {
            result.message = $"exception: '{ex.Message}'";
        }

        return result;
    }
}