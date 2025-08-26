# Communicating with APNs (refreshed)

This document consolidates guidance from Apple's Remote Notifications programming guide. Links and examples remain valid as of 2025; verify against the latest Apple docs for changes.

[Official reference](https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CommunicatingwithAPNs.html)

The Apple Push Notification (APNs) Provider API lets you send remote notification requests to APNs. 
APNs then delivers notifications to apps on iOS, tvOS, macOS devices, and to Apple Watch via iOS.

The provider API is based on the HTTP/2 network protocol. 
Each interaction starts with a POST request from the provider that includes a JSON payload and a device token. 
APNs delivers the notification payload to the app on a specific user device that is identified by the device token in the request.

A provider is the server that you configure, deploy, and manage to use APNs.


## Provider Authentication Tokens

To connect securely to APNs you can use either a provider authentication token or a provider certificate. 
This section describes connections that use tokens.

The provider API supports the JSON Web Token (JWT) specification so you can pass a set of claims and metadata along with each push notification. 
See the specification at [https://tools.ietf.org/html/rfc7519](https://tools.ietf.org/html/rfc7519). 
For additional information about JWT and a list of libraries you can use to generate signed JWTs, see [https://jwt.io](https://jwt.io/).


The provider authentication token is a JSON object that must include the following in the header:

 - The cryptographic algorithm used to encrypt the token (`alg`)
 - The 10-character key identifier (`kid`) obtained from your [Apple Developer account](https://developer.apple.com/account/)


The token’s claims payload must include:

 - The issuer (`iss`) registered claim key, whose value is your 10-character Team ID from your [Apple Developer account](https://developer.apple.com/account/)
 - The issued at (`iat`) registered claim key, the time the token was generated as a UTC value in seconds since the epoch


After creating the token, sign it with your private key. 
Then encrypt the token using ECDSA (Elliptic Curve Digital Signature Algorithm) with the P-256 curve and the SHA-256 hash algorithm. 
Specify `ES256` in the algorithm header key (`alg`). 

For more details on composing the token, see [Configure push notifications](http://help.apple.com/xcode/mac/current/#/dev11b059073) in Xcode Help.

The decoded JWT provider authentication token for APNs has the following format:


```json
{
    "alg": "ES256",
    "kid": "ABC123DEFG"
}
{
    "iss": "DEF123GHIJ",
    "iat": 1437179036
 }
 ```

```
(NOTE)
APNs supports only provider authentication tokens signed with the `ES256` algorithm. 
Unsigned JWTs or JWTs signed with other algorithms are rejected and the provider receives an `InvalidProviderToken (403)` response.
```

For security, you must periodically generate a new token. 

The new token includes an updated issued-at entry indicating when the token was created. 
If the timestamp is not within the last hour, APNs rejects subsequent push messages and returns an `ExpiredProviderToken (403)` error.

If you suspect your provider token signing key has been compromised, revoke it in your [Apple Developer account](https://developer.apple.com/account/). 
You can issue a new key pair and use the new private key to generate new tokens. 
To maximize security, close all existing connections to APNs that used the now-revoked key and reconnect before using tokens signed with the new key.


## APNs Provider Certificates

Using APNs provider certificates as described in Xcode Help's "Configure push notifications" lets you connect to both the APNs production and development environments.

With an APNs certificate, you can send notifications to the primary app identified by its bundle ID, and to any Apple Watch complications or background VoIP services associated with that app. 
Use the certificate extension (1.2.840.113635.100.6.3.6) to identify the topics for push notifications. 
For example, if you deliver an app whose bundle ID is com.yourcompany.yourexampleapp, the certificate can specify the following entries:

```
1. Extension ( 1.2.840.113635.100.6.3.6 )
2. Critical NO
3. Data com.yourcompany.yourexampleapp
4. Data app
5. Data com.yourcompany.yourexampleapp.voip
6. Data voip
7. Data com.yourcompany.yourexampleapp.complication
8. Data complication
```


## APNs Connections

The first step in sending remote notifications is to establish a connection with the appropriate APNs server:

 - Development server: api.development.push.apple.com:443
 - Production server: api.push.apple.com:443

 ```
 NOTE
 You can also use port 2197 when communicating with APNs, for example, if you allow APNs traffic through your firewall but block other HTTPS traffic.
 ```

When connecting to APNs, your provider must support TLS 1.2 or later. 
As described in [Create a universal push notification client SSL certificate](https://developer.apple.com/library/content/documentation/IDEs/Conceptual/AppDistributionGuide/AddingCapabilities/AddingCapabilities.html#//apple_ref/doc/uid/TP40012582-CH26-SW11), you can use a provider client certificate obtained from your [developer account](https://developer.apple.com/account/).

To connect without an APNs provider certificate, create a provider authentication token signed with a key from your developer account (see Xcode Help, ["Configure push notifications"](http://help.apple.com/xcode/mac/current/#/dev11b059073)). 
Once you have this token, you can send push messages. You must then refresh the token periodically. 
Each APNs provider authentication token is valid for one hour.

APNs allows multiple concurrent streams on each connection. The exact number depends on whether you use a provider certificate or a provider token, and on server load. 
Do not rely on a fixed stream count.

When you establish a connection using a token rather than a certificate, only a single stream is permitted until you send a push message with a valid provider authentication token. 
APNs ignores HTTP/2 PRIORITY frames; do not send them.

## Best Practices for Managing Connections

Keep your APNs connections open across multiple notifications. Don’t repeatedly open and close connections.
APNs treats rapid connection and disconnection as a denial-of-service attack. 
Keep connections open unless you know they’ll be idle for a long period. 
For example, if you send notifications only once per day, it’s reasonable to use a new connection each day.

Don’t generate a new provider authentication token for every push request. 
After obtaining a token, continue using it for all push requests for the token’s validity window (1 hour).

To improve performance, you can establish multiple connections to APNs. 
When sending large volumes of remote notifications, distribute them across connections to multiple server endpoints.
This sends notifications faster compared to using a single connection, and lets APNs deliver them more quickly as well.

If your provider certificate is revoked, or the key used to sign your provider tokens is revoked, close all existing connections to APNs and open new ones.

Use HTTP/2 PING frames to check connection health.


## Terminating an APNs Connection

When APNs decides to terminate an established HTTP/2 connection, it sends a GOAWAY frame. 
The GOAWAY frame can include JSON data in the payload with a reason value that indicates why the connection is ending. 
For the list of possible reason values, see Table 8-6.

Normal request failures do not cause the connection to be closed.


## APNs Notification API

The APNs Provider API consists of requests that you construct and send using the HTTP/2 POST command, and the responses you receive. 
Use requests to send push notifications to the APNs server and responses to determine the result of those requests.


## HTTP/2 Request to APNs

Use a request to send a notification to a specific user device.


 <table class="graybox" border="0" cellspacing="0" cellpadding="5">
    <caption class="tablecaption"><strong class="caption-number">Table 8-1</strong>HTTP/2 request fields</caption>
    <thead>
        <tr>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Name
</p></th>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Value
</p></th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">:method</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">POST</code>
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">:path</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">/3/device/</code><em>&lt;device-token&gt;</em>
</p></td>
        </tr>
    </tbody>
  </table>


For the <device-token> parameter, specify the hexadecimal bytes of the device token for the target device.

APNs uses HPACK (HTTP/2 header compression) to avoid repeating header keys and values.
APNs maintains a small dynamic table for HPACK. If you cannot rely on APNs' HPACK table being pre-populated—or want to avoid table eviction—encode headers as follows, especially when sending a large number of streams:
- Especially when sending many streams:

 - Encode the :path value as a literal header field without indexing.
 - If the authorization request header is present, encode it as a literal header field without indexing.
 - For the apns-id, apns-expiration, and apns-collapse-id request headers, the appropriate encoding depends on whether this is the first POST or a subsequent one:

	- On the first send of these headers, encode with incremental indexing so the header name is added to the dynamic table.
	- On subsequent sends, encode as a literal header field without indexing.


Encode all other headers as literal header fields using incremental indexing. 
For details on header encoding, see [RFC 7541, Section 6.2.1](http://tools.ietf.org/html/rfc7541#section-6.2.1) 
and [Section 6.2.2](http://tools.ietf.org/html/rfc7541#section-6.2.2).


 <table class="graybox" border="0" cellspacing="0" cellpadding="5">
    <caption class="tablecaption"><strong class="caption-number">Table 8-2</strong>APNs request headers</caption>
    <thead>
        <tr>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Header
</p></th>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Description
</p></th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">authorization</code>
</p></td>
            <td><p class="para">
  The provider token that authorizes APNs to send push notifications for the specified topics. The token is in Base64URL-encoded JWT format, specified as <code class="code-voice">bearer &lt;provider token&gt;</code>. 
</p><p class="para">
  When the provider certificate is used to establish a connection, this request header is ignored.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">apns-id</code>
</p></td>
            <td><p class="para">
  A canonical UUID that identifies the notification. If there is an error sending the notification, APNs uses this value to identify the notification to your server. 
</p><p class="para">
  The canonical form is 32 lowercase hexadecimal digits, displayed in five groups separated by hyphens in the form 8-4-4-4-12. An example UUID is as follows:
</p><p class="para">
  <code class="code-voice">123e4567-e89b-12d3-a456-42665544000</code>
</p><p class="para">
  If you omit this header, a new UUID is created by APNs and returned in the response.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">apns-expiration</code>
</p></td>
            <td><p class="para">
  A UNIX epoch date expressed in seconds (UTC). This header identifies the date when the notification is no longer valid and can be discarded.
</p><p class="para">
  If this value is nonzero, APNs stores the notification and tries to deliver it at least once, repeating the attempt as needed if it is unable to deliver the notification the first time. If the value is <code class="code-voice">0</code>, APNs treats the notification as if it expires immediately and does not store the notification or attempt to redeliver it.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">apns-priority</code>
</p></td>
            <td><p class="para">
  The priority of the notification. Specify one of the following values:
</p><ul class="list-bullet">
  <li class="item"><p class="para">
  <code class="code-voice">10</code>–Send the push message immediately. Notifications with this priority must trigger an alert, sound, or badge on the target device. It is an error to use this priority for a push notification that contains only the <code class="code-voice">content-available</code> key.
</p>
</li><li class="item"><p class="para">
  <code class="code-voice">5</code>—Send the push message at a time that takes into account power considerations for the device. Notifications with this priority might be grouped and delivered in bursts. They are throttled, and in some cases are not delivered.
</p>
</li>
</ul><p class="para">
  If you omit this header, the APNs server sets the priority to <code class="code-voice">10</code>. 
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">apns-topic</code>
</p></td>
            <td><p class="para">
  The topic of the remote notification, which is typically the bundle ID for your app. The certificate you create in your developer account must include the capability for this topic.
</p><p class="para">
  If your certificate includes multiple topics, you must specify a value for this header.
</p><p class="para">
  If you omit this request header and your APNs certificate does not specify multiple topics, the APNs server uses the certificate’s Subject as the default topic.
</p><p class="para">
  If you are using a provider token instead of a certificate, you must specify a value for this request header. The topic you provide should be provisioned for the your team named in your developer account.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">apns-collapse-id</code>
</p></td>
            <td><p class="para">
  Multiple notifications with the same collapse identifier are displayed to the user as a single notification. The value of this key must not exceed 64 bytes. For more information, see <span class="x-name"><a href="APNSOverview.html#//apple_ref/doc/uid/TP40008194-CH8-SW5" data-renderer-version="2" data-id="//apple_ref/doc/uid/TP40008194-CH8-SW5">Quality of Service, Store-and-Forward, and Coalesced Notifications</a></span>.
</p></td>
        </tr>
    </tbody>
</table>



The message body is a JSON dictionary object for the notification payload. Do not compress the body; the maximum size is 4 KB (4096 bytes). 
For VoIP (Voice over Internet Protocol) notifications, the maximum body size is 5 KB (5120 bytes). 
For the keys and values to include in the payload, see the payload keys reference.


## HTTP/2 Response from APNs

The response to the request has the format listed in Table 8-3.

<table class="graybox" border="0" cellspacing="0" cellpadding="5">
    <caption class="tablecaption"><strong class="caption-number">Table 8-3</strong>APNs response headers</caption>
    <thead>
        <tr>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Header name
</p></th>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Value
</p></th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">apns-id</code>
</p></td>
            <td><p class="para">
  The <code class="code-voice">apns-id</code> value from the request. If no value was included in the request, the server creates a new UUID and returns it in this header.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">:status</code>
</p></td>
            <td><p class="para">
  The HTTP status code. For a list of possible status codes, see <span class="x-name"><a href="#//apple_ref/doc/uid/TP40008194-CH11-SW15" data-renderer-version="2" data-id="//apple_ref/doc/uid/TP40008194-CH11-SW15">Table 8-4</a></span>.
</p></td>
        </tr>
    </tbody>
  </table>


Table 8-4 lists the possible status codes for a request. This value is included in the :status header of the response.



  <table class="graybox" border="0" cellspacing="0" cellpadding="5">
    <caption class="tablecaption"><strong class="caption-number">Table 8-4</strong>Status codes for an APNs response</caption>
    <thead>
        <tr>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Status code
</p></th>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Description
</p></th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td scope="row"><p class="para">
  200
</p></td>
            <td><p class="para">
  Success
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  400
</p></td>
            <td><p class="para">
  Bad request
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  403
</p></td>
            <td><p class="para">
  There was an error with the certificate or with the provider authentication token
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  405
</p></td>
            <td><p class="para">
  The request used a bad <code class="code-voice">:method</code> value. Only <code class="code-voice">POST</code> requests are supported.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  410
</p></td>
            <td><p class="para">
  The device token is no longer active for the topic.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  413
</p></td>
            <td><p class="para">
  The notification payload was too large.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  429
</p></td>
            <td><p class="para">
  The server received too many requests for the same device token.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  500
</p></td>
            <td><p class="para">
  Internal server error
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  503
</p></td>
            <td><p class="para">
  The server is shutting down and unavailable.
</p></td>
        </tr>
    </tbody>
  </table>




If the request succeeds, the response body is empty. If it fails, the response body contains a JSON dictionary with the keys listed in Table 8-5.
This JSON data can also be included in a GOAWAY frame if the connection is terminated.


 <table class="graybox" border="0" cellspacing="0" cellpadding="5">
    <caption class="tablecaption"><strong class="caption-number">Table 8-5</strong>APNs JSON data keys</caption>
    <thead>
        <tr>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Key
</p></th>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Description
</p></th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">reason</code>
</p></td>
            <td><p class="para">
  The error indicating the reason for the failure. The error code is specified as a string. For a list of possible values, see <span class="x-name"><a href="#//apple_ref/doc/uid/TP40008194-CH11-SW17" data-renderer-version="2" data-id="//apple_ref/doc/uid/TP40008194-CH11-SW17">Table 8-6</a></span>.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">timestamp</code>
</p></td>
            <td><p class="para">
  If the value in the <code class="code-voice">:status</code> header is <code class="code-voice">410</code>, the value of this key is the last time at which APNs confirmed that the device token was no longer valid for the topic.
</p><p class="para">
  Stop pushing notifications until the device registers a token with a later timestamp with your provider.
</p></td>
        </tr>
    </tbody>
  </table>




Table 8-6 lists the possible error codes included in the reason key of the JSON payload of the response.



<table class="graybox" border="0" cellspacing="0" cellpadding="5">
    <caption class="tablecaption"><strong class="caption-number">Table 8-6</strong>Values for the APNs JSON <code class="code-voice">reason</code> key</caption>
    <thead>
        <tr>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Status code
</p></th>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Error string
</p></th>
            <th scope="col" class="TableHeading_TableRow_TableCell"><p class="para">
  Description
</p></th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadCollapseId</code>
</p></td>
            <td><p class="para">
  The collapse identifier exceeds the maximum allowed size
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadDeviceToken</code>
</p></td>
            <td><p class="para">
  The specified device token was bad. Verify that the request contains a valid token and that the token matches the environment.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadExpirationDate</code>
</p></td>
            <td><p class="para">
  The <code class="code-voice">apns-expiration</code> value is bad.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadMessageId</code>
</p></td>
            <td><p class="para">
  The <code class="code-voice">apns-id</code> value is bad.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadPriority</code>
</p></td>
            <td><p class="para">
  The <code class="code-voice">apns-priority</code> value is bad.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadTopic</code>
</p></td>
            <td><p class="para">
  The <code class="code-voice">apns-topic</code> was invalid.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">DeviceTokenNotForTopic</code>
</p></td>
            <td><p class="para">
  The device token does not match the specified topic.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">DuplicateHeaders</code>
</p></td>
            <td><p class="para">
  One or more headers were repeated.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">IdleTimeout</code>
</p></td>
            <td><p class="para">
  Idle time out.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">MissingDeviceToken</code>
</p></td>
            <td><p class="para">
  The device token is not specified in the request <code class="code-voice">:path</code>. Verify that the <code class="code-voice">:path</code> header contains the device token.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">MissingTopic</code>
</p></td>
            <td><p class="para">
  The <code class="code-voice">apns-topic</code> header of the request was not specified and was required. The apns-topic header is mandatory when the client is connected using a certificate that supports multiple topics.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">PayloadEmpty</code>
</p></td>
            <td><p class="para">
  The message payload was empty.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">400</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">TopicDisallowed</code>
</p></td>
            <td><p class="para">
  Pushing to this topic is not allowed.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">403</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadCertificate</code>
</p></td>
            <td><p class="para">
  The certificate was bad.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">403</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadCertificateEnvironment</code>
</p></td>
            <td><p class="para">
  The client certificate was for the wrong environment.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">403</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">ExpiredProviderToken</code>
</p></td>
            <td><p class="para">
  The provider token is stale and a new token should be generated.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">403</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">Forbidden</code>
</p></td>
            <td><p class="para">
  The specified action is not allowed.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">403</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">InvalidProviderToken</code>
</p></td>
            <td><p class="para">
  The provider token is not valid or the token signature could not be verified.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">403</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">MissingProviderToken</code>
</p></td>
            <td><p class="para">
  No provider certificate was used to connect to APNs and Authorization header was missing or no provider token was specified.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">404</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">BadPath</code>
</p></td>
            <td><p class="para">
  The request contained a bad <code class="code-voice">:path</code> value.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">405</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">MethodNotAllowed</code>
</p></td>
            <td><p class="para">
  The specified <code class="code-voice">:method</code> was not <code class="code-voice">POST</code>.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">410</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">Unregistered</code>
</p></td>
            <td><p class="para">
  The device token is inactive for the specified topic.
</p><p class="para">
  Expected HTTP/2 status code is <code class="code-voice">410</code>; see <span class="x-name"><a href="#//apple_ref/doc/uid/TP40008194-CH11-SW15" data-renderer-version="2" data-id="//apple_ref/doc/uid/TP40008194-CH11-SW15">Table 8-4</a></span>. 
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">413</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">PayloadTooLarge</code>
</p></td>
            <td><p class="para">
  The message payload was too large. See <span class="x-name"><a href="CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1" data-renderer-version="2" data-id="//apple_ref/doc/uid/TP40008194-CH10-SW1">Creating the Remote Notification Payload</a></span> for details on maximum payload size. 
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">429</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">TooManyProviderTokenUpdates</code>
</p></td>
            <td><p class="para">
  The provider token is being updated too often.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">429</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">TooManyRequests</code>
</p></td>
            <td><p class="para">
  Too many requests were made consecutively to the same device token.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">500</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">InternalServerError</code>
</p></td>
            <td><p class="para">
  An internal server error occurred.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">503</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">ServiceUnavailable</code>
</p></td>
            <td><p class="para">
  The service is unavailable.
</p></td>
        </tr>
        <tr>
            <td scope="row"><p class="para">
  <code class="code-voice">503</code>
</p></td>
            <td><p class="para">
  <code class="code-voice">Shutdown</code>
</p></td>
            <td><p class="para">
  The server is shutting down.
</p></td>
        </tr>
    </tbody>
  </table>





## HTTP/2 Request/Response Examples for APNs


Listing 8-1 shows a sample request constructed for a provider certificate.

Listing 8-1Sample request for a certificate with a single topic

```
HEADERS
  - END_STREAM
  + END_HEADERS
  :method = POST
  :scheme = https
  :path = /3/device/00fc13adff785122b4ad28809a3420982341241421348097878e577c991de8f0
  host = api.development.push.apple.com
  apns-id = eabeae54-14a8-11e5-b60b-1697f925ec7b
  apns-expiration = 0
  apns-priority = 10
DATA
  + END_STREAM
    { "aps" : { "alert" : "Hello" } }
```

Listing 8-2 shows a sample request constructed for a provider authentication token.

Listing 8-2Sample request for a provider authentication token

```
HEADERS
  - END_STREAM
  + END_HEADERS
  :method = POST
  :scheme = https
  :path = /3/device/00fc13adff785122b4ad28809a3420982341241421348097878e577c991de8f0
  host = api.development.push.apple.com
  authorization = bearer eyAia2lkIjogIjhZTDNHM1JSWDciIH0.eyAiaXNzIjogIkM4Nk5WOUpYM0QiLCAiaWF0I
 jogIjE0NTkxNDM1ODA2NTAiIH0.MEYCIQDzqyahmH1rz1s-LFNkylXEa2lZ_aOCX4daxxTZkVEGzwIhALvkClnx5m5eAT6
 Lxw7LZtEQcH6JENhJTMArwLf3sXwi
  apns-id = eabeae54-14a8-11e5-b60b-1697f925ec7b
  apns-expiration = 0
  apns-priority = 10
  apns-topic = <MyAppTopic>
DATA
  + END_STREAM
    { "aps" : { "alert" : "Hello" } }
```

Listing 8-3 shows a sample request constructed for a certificate that contains multiple topics.

Listing 8-3Sample request for a certificate with multiple topics

```
HEADERS
  - END_STREAM
  + END_HEADERS
  :method = POST
  :scheme = https
  :path = /3/device/00fc13adff785122b4ad28809a3420982341241421348097878e577c991de8f0
  host = api.development.push.apple.com
  apns-id = eabeae54-14a8-11e5-b60b-1697f925ec7b
  apns-expiration = 0
  apns-priority = 10
  apns-topic = <MyAppTopic> 
DATA
  + END_STREAM
    { "aps" : { "alert" : "Hello" } }
```

Listing 8-4 shows a sample response for a successful push request.

Listing 8-4Sample response for a successful request

```
HEADERS
  + END_STREAM
  + END_HEADERS
  apns-id = eabeae54-14a8-11e5-b60b-1697f925ec7b
  :status = 200
```

Listing 8-5 shows a sample response when an error occurs.

Listing 8-5Sample response for a request that encountered an error

```
HEADERS
  - END_STREAM
  + END_HEADERS
  :status = 400
  content-type = application/json
    apns-id: <a_UUID>
DATA
  + END_STREAM
  { "reason" : "BadDeviceToken" }
```
