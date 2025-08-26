namespace DotNet.Push
{
    /// <summary>
    /// Target, options, and payload of a downstream HTTP message (JSON)
    /// </summary>
    public class AosPushProtocol
    {
        /// <summary>
        /// Specifies the recipient of the message.
        /// The value must be a registration token, a notification key, or a topic.
        /// Do not set this field when sending to multiple topics; use the 'condition' field instead.
        /// </summary>
        public string to
        {
            get; set;
        }

        /// <summary>
        /// Specifies a list of device registration tokens or IDs that will receive a multicast message.
        /// Must contain between 1 and 1,000 registration tokens.
        /// Use this parameter only for multicast messaging, not a single recipient.
        /// When sending to two or more registration tokens, only the HTTP JSON format is supported.
        /// </summary>
        public string registration_ids
        {
            get; set;
        }

        /// <summary>
        /// Specifies a logical expression of conditions that determine the message target.
        /// Supported conditions are topics specified in the form: "'yourTopic' in topics" (case-sensitive).
        /// Supported operators are && and ||. Up to two operators are supported per topic message.
        /// </summary>
        public string condition
        {
            get; set;
        }

        /// <summary>
        /// Identifies a group of collapsible messages where only the last message is delivered
        /// when delivery can be resumed (e.g., collapse_key: "Updates Available").
        /// This helps avoid sending too many of the same message when the device comes online or becomes active again.
        ///
        /// Note: Delivery order is not guaranteed. At most four different collapse keys are allowed during a given period.
        /// That is, the FCM connection server can store up to four different sync messages per client app at a time.
        /// If you exceed this limit, the server does not guarantee which four collapse keys will be kept.
        /// </summary>
        public string collapse_key
        {
            get; set;
        }

        /// <summary>
        /// Sets the message priority. Valid values are 'normal' and 'high'. On iOS these map to APNs priorities 5 and 10.
        /// By default, notification messages are sent with high priority and data messages with normal priority.
        /// Normal priority optimizes battery usage and may introduce unspecified delivery delays.
        /// High priority attempts immediate delivery and may wake the device and open a network connection to your server.
        /// </summary>
        public string priority
        {
            get; set;
        }

        /// <summary>
        /// Custom key/value pairs to include in the message payload.
        ///
        /// For example, data:{"score":"3x1"}.
        /// On iOS via APNs this represents custom data fields; via the FCM connection server it is delivered
        /// as a dictionary of keys and values to AppDelegate application:didReceiveRemoteNotification:.
        ///
        /// On Android this creates an intent extra named 'score' with the string value '3x1'.
        /// Keys must not use reserved words (any key starting with 'google' or 'gcm', or 'from').
        /// Do not reuse keys defined elsewhere (e.g., collapse_key).
        /// Prefer string values. Objects or non-string types (e.g., integers or booleans) should be converted to strings.
        /// </summary>
        public AosNotifyData data
        {
            get; set;
        }

        /// <summary>
        /// Predefined notification payload key/value pairs that are displayed to the user.
        /// For more details, see the notification payload support and payload options documentation.
        /// </summary>
        public AosNotification notification
        {
            get; set;
        }
    }

    /// <summary>
    /// Android — notification message keys
    /// </summary>
    public class AosNotification
    {
        /// <summary>
        /// The notification title.
        /// </summary>
        public string title
        {
            get; set;
        }

        /// <summary>
        /// The notification body text.
        /// </summary>
        public string body
        {
            get; set;
        }

        /// <summary>
        /// The notification icon. For a drawable resource named 'myicon', set the value to 'myicon'.
        /// If omitted, FCM displays the launcher icon specified in the app manifest.
        /// </summary>
        public string icon
        {
            get; set;
        }

        /// <summary>
        /// The sound to play when the device receives the notification.
        /// Supports 'default' or the filename of a bundled sound resource located under /res/raw/.
        /// </summary>
        public string sound
        {
            get; set;
        }

        /// <summary>
        /// Whether each notification appears as a new entry in the Android notification drawer.
        /// If unset, the request creates a new notification. If set and a notification with the same tag
        /// is already displayed, the new notification replaces the existing one.
        /// </summary>
        public string tag
        {
            get; set;
        }

        /// <summary>
        /// The icon color, expressed in the #rrggbb format.
        /// </summary>
        public string color
        {
            get; set;
        }

        /// <summary>
        /// The action associated with the user's click. When set, an activity with a matching intent filter is launched.
        /// </summary>
        public string click_action
        {
            get; set;
        }

        /// <summary>
        /// The key for a localized body string. Use a key from your app's string resources.
        /// </summary>
        public string body_loc_key
        {
            get; set;
        }

        /// <summary>
        /// String values to substitute format specifiers in the localized body string.
        /// See: https://developer.android.com/guide/topics/resources/string-resource#FormattingAndStyling
        /// </summary>
        public string body_loc_args
        {
            get; set;
        }

        /// <summary>
        /// The key for a localized title string. Use a key from your app's string resources.
        /// </summary>
        public string title_loc_key
        {
            get; set;
        }

        /// <summary>
        /// String values to substitute format specifiers in the localized title string.
        /// See: https://developer.android.com/guide/topics/resources/string-resource#FormattingAndStyling
        /// </summary>
        public string title_loc_args
        {
            get; set;
        }
    }

    /// <summary>
    /// Custom data payload for Android
    /// </summary>
    public class AosNotifyData
    {
        /// <summary>
        /// The number to display as the badge of the app icon.
        /// </summary>
        public int badge
        {
            get; set;
        }

        /// <summary>
        /// A short title for the content.
        /// </summary>
        public string title
        {
            get; set;
        }

        /// <summary>
        /// The message content.
        /// </summary>
        public string message
        {
            get; set;
        }
    }
}