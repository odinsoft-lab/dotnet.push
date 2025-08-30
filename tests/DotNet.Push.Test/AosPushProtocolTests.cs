using DotNet.Push;

namespace DotNet.Push.Test;

public class AosPushProtocolTests
{
    [Fact]
    public void AosPushProtocol_CanSerialize_WithNotificationAndData()
    {
        var model = new AosPushProtocol
        {
            to = "device-token",
            priority = "high",
            notification = new AosNotification
            {
                title = "Hello",
                body = "World",
                icon = "ic",
                color = "#ffffff",
                click_action = "OPEN",
                tag = "alarm"
            },
            data = new AosNotifyData
            {
                title = "Hello",
                message = "World",
                badge = 3
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(model);

        Assert.Contains("\"to\":\"device-token\"", json);
        Assert.Contains("\"priority\":\"high\"", json);
        Assert.Contains("\"notification\"", json);
        Assert.Contains("\"data\"", json);
    }
}
