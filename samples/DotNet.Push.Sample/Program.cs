using System;
using System.Threading.Tasks;
using System.IO;

namespace DotNet.Push.Sample
{
    //[SupportedOSPlatform("windows")]

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var _test_phone = "AF";

            // IPHONE
            if (_test_phone == "IA1")
            {
                var _apns = new IosPushNotifyAPNs("<team-id>", "<bundle-app-id>", "<private-key-id>", @"<key-file-path>");
                var content = new
                {
                    title = "Json Web Token(JWT)",
                    body = $"Apple Push Notification Service(APNs: {_apns.BundleAppId})"
                };

                var _result = await _apns.JwtAPNsPushAsync("<device-token>", content, Guid.NewGuid().ToString(), 1, "ping.aiff");

                Console.WriteLine($"result: {_result.success}, '{_result.message}'");
            }
            else if (_test_phone == "IA2")
            {
                var json = File.ReadAllText(@"path/to/serviceAccountKey.json");
                var _fcm = new PushNotifyFCMV1(json);

                var _result = await _fcm.SendNotificationAsync("<device-token>", "<priority>", "<title>", "<click-action>", "<message>", 1, "<icon-name>", "<color>");

                Console.WriteLine($"result: {_result.success}, '{_result.message}'");
            }

            // ANDROID
            if (_test_phone == "AF1")
            {
                var _fcm = new AosPushNotifyFCM("<server-key>", "<server-id>", "<alarm-tag>");
                
                var _result = await _fcm.SendNotificationAsync("<device-token>", "<priority>", "<title>", "<click-action>", "<message>", 1, "<icon-name>", "<color>");

                Console.WriteLine($"result: {_result.success}, '{_result.message}'");
            }
            else if (_test_phone == "AF2")
            {
                var json = File.ReadAllText(@"path/to/serviceAccountKey.json");
                var _fcm = new PushNotifyFCMV1(json);

                var _result = await _fcm.SendNotificationAsync("<device-token>", "<priority>", "<title>", "<click-action>", "<message>", 1, "<icon-name>", "<color>");

                Console.WriteLine($"result: {_result.success}, '{_result.message}'");
            }

            Console.ReadLine();
        }
    }
}