Sample application that integrates with Sage One Accounting via the Sage One API.

Update the Sage One Authorisation Client solution's [SageOneOAuth.cs](Sage%20One%20Authorisation%20Client/SageOneOAuth.cs) file with your application's `callback_url`, `client_id`, `client_secret`, `signing_secret` and `subscription_key`.

`callback_url`, `client_id`, `client_secret` and `signing_secret` are created by registering an application on https://developers.sageone.com

Create a profile on https://developer.columbus.sage.com to generate your `subscription_key`

Authentication with Sage One is handled in [SageOneOAuth.cs](Sage%20One%20Authorisation%20Client/SageOneOAuth.cs).

An example application that consumes the client can be seen in the Sage One API sample Website Solution.
