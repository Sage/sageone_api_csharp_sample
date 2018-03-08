Sample application that integrates with Sage Business Cloud Accounting via the Sage API.

Update the Authorisation Client solution's [SageOneOAuth.cs](Sage%20One%20Authorisation%20Client/SageOneOAuth.cs) file with your application's `callback_url`, `client_id`, `client_secret` and `signing_secret`.

Authentication with Sage is handled in [SageOneOAuth.cs](Sage%20One%20Authorisation%20Client/SageOneOAuth.cs).

An example application that consumes the client can be seen in the sample Website Solution.

##### Note: Request signing and noncing (the X-Signature and X-Nonce headers) is no longer checked in v3. The related code will soon be removed from this repo.
