using Firebase.CloudMessaging;
using Foundation;
using UIKit;
using UserNotifications;

namespace DotNet.Push.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Initialize Firebase
        Firebase.Core.App.Configure();

        // APNs 등록
        RegisterForRemoteNotifications();
        return base.FinishedLaunching(application, launchOptions);
    }

    private void RegisterForRemoteNotifications()
    {
        // 푸시 알림 권한 요청
        if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
        {
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                (granted, error) =>
                {
                    if (granted)
                    {
                        InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
                    }
                });
        }
        else
        {
            var settings = UIUserNotificationSettings.GetSettingsForTypes(
                UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
        }
    }

    [Foundation.Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        // Pass device token to Firebase
        Messaging.SharedInstance.ApnsToken = deviceToken;

        var tokenBytes = deviceToken.ToArray();
        var token = BitConverter.ToString(tokenBytes).Replace("-", "").ToLowerInvariant();

        // 디바이스 토큰 저장
        Preferences.Set("DeviceToken", token);
        Console.WriteLine($"Device Token: {token}");
    }

    [Export("messaging:didReceiveRegistrationToken:")]
    public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
    {
        Console.WriteLine($"FCM token: {fcmToken}");
        // Send the token to your server or save it for later use
    }

    [Foundation.Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        System.Diagnostics.Debug.WriteLine($"Failed to register for remote notifications: {error.LocalizedDescription}");
    }
}