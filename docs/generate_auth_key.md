# Token Based Authentication and HTTP/2 Example with APNS (reference)

Note (2025): The article linked below remains a useful reference for APNs token-based auth. Screens in Apple Developer may have evolved; always consult the latest Apple docs alongside this guide.

[Original article](http://gobiko.com/blog/token-based-authentication-http2-example-apns/)

## Generate an auth key
Now, head over to the Apple member center and generate an APNS auth key. In the Certificates, Identifiers & Profiles section of the Member Center, under Certificates there is a new section APNs Auth Key. There click the add button to create a new key.

![image](http://gobiko.com/media/images/Screen_Shot_2016-09-30_at_11.40.09_PM.original.png)


Under Production select Apple Push Notification Authentication Key (Sandbox & Production) and click continue and a key will be created for you.

![image](http://gobiko.com/media/images/Screen_Shot_2016-09-24_at_5.01.44_AM.original.png)

Download the .p8 file and note the key ID and the .p8 filename, as we'll need those in a moment.

```
APNS_KEY_ID = 'ABC123DEFG'
APNS_AUTH_KEY = '/PATH_TO/APNSAuthKey_ABC123DEFG.p8'
```

![image](http://gobiko.com/media/images/Screen_Shot_2016-09-24_at_5.02.01_AM.original.png)


While you're in the member center, grab your Team ID as well in the membership area.

```
TEAM_ID = 'DEF123GHIJ'
```

