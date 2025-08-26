using Android.App;
using Android.Content;
using Android.Util;
using Firebase.Messaging;

namespace DotNet.Push.Maui.Platforms.Android.Services;

[Service(Name = "com.example.yourapp.MyFirebaseMessagingService")]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class MyFirebaseMessagingService : FirebaseMessagingService
{
    const string TAG = "MyFirebaseMessagingService";

    public override void OnNewToken(string token)
    {
        Log.Debug(TAG, $"FCM token: {token}");
        SendRegistrationToServer(token);
    }

    void SendRegistrationToServer(string token)
    {
        // 서버로 토큰 전송 로직 구현
        // 예: DependencyService를 통해 토큰 저장
        //DependencyService.Get<IDeviceTokenService>().SaveDeviceToken(token);
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        Log.Debug(TAG, $"From: {message.From}");
        if (message.GetNotification() != null)
        {
            Log.Debug(TAG, $"Notification Message Body: {message.GetNotification().Body}");
        }
    }
}
