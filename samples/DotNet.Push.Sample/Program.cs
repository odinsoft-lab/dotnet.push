using System;
using System.Threading.Tasks;

namespace DotNet.Push.Sample
{
    //[SupportedOSPlatform("windows")]

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var _test_phone = "AF";

            // IPHONE
            if (_test_phone == "IA")
            {
                var _apns = new IosPushNotifyAPNs("<team-id>", "<bundle-app-id>", "<private-key-id>", @"<key-file-path>");
                var content = new
                {
                    title = "Json Web Token(JWT)",
                    body = "Apple Push Notification Service(APNs)",
                };

                var _result = await _apns.JwtAPNsPushAsync("<device-token>", content, Guid.NewGuid().ToString(), 1, "ping.aiff");

                Console.WriteLine($"result: {_result.success}, '{_result.message}'");
            }

            // ANDROID
            if (_test_phone == "AF")
            {
                var _fcm = new AosPushNotifyFCM("<server-api-key>", "<server-id>", "<alarm-tag>");
                
                var _result = await _fcm.SendNotificationAsync("<to>", "<priority>", "<title>", "<click-action>", "<message>", 1, "<icon-name>", "<color>");

                Console.WriteLine($"result: {_result.success}, '{_result.message}'");
            }

            Console.ReadLine();
        }
    }
}