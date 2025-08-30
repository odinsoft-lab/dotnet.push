using DotNet.Push;
using System.Text.Json;

namespace DotNet.Push.Test;

public class IosPushNotifyAPNsTests
{
    [Fact]
    public void JwtToken_Refresh_WhenExpired()
    {
        var apns = new IosPushNotifyAPNs(
            teamId: "T123",
            bundleAppId: "com.example.app",
            apnsPrivatekeyId: "KID123",
            apnsPrivateKey: CreateTempP8(),
            algorithm: "ES256",
            production: false,
            port: 443,
            expireMinutes: 0 // expire immediately
        );

        apns.JwtToken = null; // force refresh
        var token1 = GetTokenViaReflection(apns);
        Assert.False(string.IsNullOrEmpty(token1));

        // simulate expired by setting ExpireMinutes=0 and creating again
        apns.JwtToken = token1;
        var token2 = GetTokenViaReflection(apns);
        Assert.False(string.IsNullOrEmpty(token2));
    }

    private static string CreateTempP8()
    {
        // Generate ephemeral ECDSA key and write a minimal .p8 file content
        using var ecdsa = System.Security.Cryptography.ECDsa.Create();
        var privateKey = ecdsa.ExportPkcs8PrivateKey();
        var base64 = System.Convert.ToBase64String(privateKey);

        var path = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllText(path, $"-----BEGIN PRIVATE KEY-----\n{base64}\n-----END PRIVATE KEY-----\n");
        return path;
    }

    private static string GetTokenViaReflection(IosPushNotifyAPNs apns)
    {
        var mi = typeof(IosPushNotifyAPNs).GetMethod("JwtAPNsPushAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        // We cannot call private getJwtToken; so we simulate usage through public API up to the point of token creation.
        // But JwtAPNsPushAsync requires network; instead we access private method via reflection.
        var getJwtToken = typeof(IosPushNotifyAPNs).GetMethod("getJwtToken", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return (string)getJwtToken!.Invoke(apns, null)!;
    }
}
