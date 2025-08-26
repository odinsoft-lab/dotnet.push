using Android.App;
using Android.Content.PM;
using Android.Gms.Extensions;
using Android.OS;
using Firebase;
using Firebase.Messaging;

namespace DotNet.Push.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Firebase 초기화
            FirebaseApp.InitializeApp(this);

            // Firebase 메시징 인스턴스 얻기
            //FirebaseMessaging.Instance.SubscribeToTopic("all");



            var token = await FirebaseMessaging.Instance.GetToken();

            // 디바이스 토큰 저장
            Preferences.Set("DeviceToken", token.ToString());
        }
    }
}
