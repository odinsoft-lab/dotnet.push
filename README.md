# DotNet.Push

[![Build status](https://ci.appveyor.com/api/projects/status/dnp9i3t6sexv9tpa?svg=true)](https://ci.appveyor.com/project/lisa3907/dotnet-push)
[![NuGet Downloads](https://img.shields.io/nuget/dt/dotnet.push.svg)](https://www.nuget.org/packages/dotnet.push)
[![License](https://img.shields.io/github/license/lisa3907/dotnet.push.svg)](LICENSE.md)

A lightweight library for sending push notifications to iOS APNs (JWT-based) and Android FCM from .NET.

## Supported frameworks

- .NET 9.0
- .NET 8.0

Note: netstandard targets were removed in recent versions.

## Install

Install from NuGet:

```powershell
dotnet add package DotNet.Push
```

## Quick start

### iOS: APNs (JWT)

```csharp
var apns = new IosPushNotifyAPNs(
	teamId: "<team-id>",
	bundleAppId: "<bundle-app-id>",
	apnsPrivatekeyId: "<key-id>",
	apnsPrivateKey: @"<path to AuthKey_XXXXXX.p8>");

var content = new { title = "Json Web Token(JWT)", body = "Apple Push Notification Service(APNs)" };
var result = await apns.JwtAPNsPushAsync(
	device_token: "<device-token>",
	content: content,
	apnsId: Guid.NewGuid().ToString(),
	badge: 1,
	sound: "ping.aiff");

Console.WriteLine($"APNs: {result.success}, {result.message}");
```

### Android: FCM

```csharp
var fcm = new AosPushNotifyFCM(
	serverKey: "<server-api-key>",
	serverId: "<server-id>",
	alarmTag: "<alarm-tag>");

var result = await fcm.SendNotificationAsync(
	deviceToken: "<to>",
	priority: "high",
	title: "<title>",
	clickAction: "<click-action>",
	message: "<message>",
	badge: 1,
	iconName: "<icon-name>",
	color: "#ffffff");

Console.WriteLine($"FCM: {result.success}, {result.message}");
```

See the full example in `samples/DotNet.Push.Sample/Program.cs`.

## API overview

### IosPushNotifyAPNs

- Ctor: `IosPushNotifyAPNs(string teamId, string bundleAppId, string apnsPrivatekeyId, string apnsPrivateKey, string algorithm = "ES256", bool production = true, int port = 443, int expireMinutes = 60)`
- Send: `Task<(bool success, string message)> JwtAPNsPushAsync(string device_token, object content, string apnsId, int badge, string sound, CancellationToken cancellationToken = default)`

Required: Apple Team ID, Bundle ID, Key ID, and path to the .p8 private key. Defaults use ES256 and the production server (`api.push.apple.com:443`).

### AosPushNotifyFCM

- Ctor: `AosPushNotifyFCM(string serverKey, string serverId, string alarmTag)`
- Send: `Task<(bool success, string message)> SendNotificationAsync(string deviceToken, string priority, string title, string clickAction, string message, int badge, string iconName, string color)`

Required: FCM Server Key and Sender ID. Default endpoint is `https://fcm.googleapis.com/fcm/send`.

## Documentation

- [Communicating with APNs](docs/communicate_apns.md)
- [Token Based Authentication and HTTP/2 Example with APNS](docs/generate_auth_key.md)
- [How to make the .NET HttpClient use HTTP/2](docs/http2handler.md)
- [Project Roadmap](docs/ROADMAP.md)
- [Tasks Board](docs/TASK.md)

## Development

- Visual Studio 2022
- .NET 8.0 / .NET 9.0 SDK

Local build: run `dotnet build` at the solution root.

## License

See [LICENSE.md](LICENSE.md).

## Contributing

See [CONTRIBUTING.md](docs/CONTRIBUTING.md).

## Changelog

```
2025-08-27: TargetFrameworks updated to .NET 8.0, 9.0 (removed netstandard)
2023-11-15: update to .NET 7.0
2020-12-30: update to .NET 5.0
2019-02-17: upgrade .NET Core 2.2 & net462
2018-07-10: upgrade .NET Core 2.1 & downgrade net462
2018-03-29: upgrade .NET Core 2.0 & net47
```

## Contact

- Homepage: http://www.odinsoft.co.kr
- Email: help@odinsoft.co.kr

## License

See LICENSE.md.

## 👥 Team

### **Core Development Team**
- **SEONGAHN** - Lead Developer & Project Architect ([lisa@odinsoft.co.kr](mailto:lisa@odinsoft.co.kr))
- **YUJIN** - Senior Developer & Exchange Integration Specialist ([yoojin@odinsoft.co.kr](mailto:yoojin@odinsoft.co.kr))
- **SEJIN** - Software Developer & API Implementation ([saejin@odinsoft.co.kr](mailto:saejin@odinsoft.co.kr))

---

**Built with ❤️ by the ODINSOFT Team** | [⭐ Star us on GitHub](https://github.com/odinsoft-lab/dotnet.push)
