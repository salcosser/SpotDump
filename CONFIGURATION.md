## Spotify credential setup

The playlist endpoints require a Spotify access token that represents a user (not just an application). Follow these steps to gather the values you need for `ClientId`, `ClientSecret`, and `RefreshToken` before running the API.

1. **Create or sign in to a Spotify developer account** at <https://developer.spotify.com/dashboard/>.
2. **Create an app** ("Create an app" → give it a name/description). After creation, open the app detail page and copy:
   - `Client ID`
   - `Client Secret` (click *View client secret*).
3. **Add a redirect URI** on the app detail page → *Settings* → *Redirect URIs* → add `http://localhost:5173/callback` (any http/https URL you control works as long as you use the same one in the next step).
4. **Authorize the app for your user** to obtain an authorization code:
   - In a browser, navigate to:
     ```
     https://accounts.spotify.com/authorize?client_id=<CLIENT_ID>&response_type=code&redirect_uri=http://localhost:5173/callback&scope=playlist-read-private%20playlist-read-collaborative
     ```
   - Sign in if prompted and approve the scopes. You will be redirected to the URI from step 3 with a `code` query parameter, e.g. `http://localhost:5173/callback?code=...`.
5. **Exchange the authorization code for a refresh token** (one-time). Replace placeholders and run:
   ```bash
   curl -X POST "https://accounts.spotify.com/api/token" \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "grant_type=authorization_code" \
     -d "code=<AUTH_CODE_FROM_STEP_4>" \
     -d "redirect_uri=http://localhost:5173/callback" \
     -d "client_id=<CLIENT_ID>" \
     -d "client_secret=<CLIENT_SECRET>"
   ```
   The JSON response contains an `access_token` and a `refresh_token`. Save the `refresh_token`; the API will use it to automatically refresh access tokens for you.
6. **Provide credentials to the app** using either of these options:
   - Update `appsettings.Development.json` (or `appsettings.json` in production) under `Spotify` → `ClientId`, `ClientSecret`, and `RefreshToken`.
   - Or create a credentials file (default path: `SpotDump.WebApi/Config/spotify-auth.txt`) with the following content:
     ```
     client_id=<CLIENT_ID>
     client_secret=<CLIENT_SECRET>
     refresh_token=<REFRESH_TOKEN>
     ```
     Then set `Spotify:CredentialFilePath` in `appsettings.Development.json` to point to that file.

Once these values are in place, start the API. The built-in Swagger UI (`/swagger`) will let you call the playlist endpoints and, if requested, export playlist data to CSV on disk.
