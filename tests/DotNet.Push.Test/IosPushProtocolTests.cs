using DotNet.Push;

namespace DotNet.Push.Test;

public class IosPushProtocolTests
{
    [Fact]
    public void IosPushProtocol_CanSerialize_WithNotificationAndData()
    {
        var model = new IosPushProtocol
        {
            to = "device-token",
            priority = "high",
            notification = new IosNotification
            {
                title = "Hello",
                body = "World",
                sound = "default",
                badge = "1",
                click_action = "OPEN"
            },
            data = new IosNotifyData
            {
                title = "Hello",
                message = "World",
                badge = 1
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(model);

        Assert.Contains("\"to\":\"device-token\"", json);
        Assert.Contains("\"priority\":\"high\"", json);
        Assert.Contains("\"notification\"", json);
        Assert.Contains("\"data\"", json);
    }
}
