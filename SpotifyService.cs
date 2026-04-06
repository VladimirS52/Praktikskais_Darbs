using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;


namespace Music_player
{

    public class SpotifyService
    {
        private string clientId = "d8dab7dca0134c1aba2dd7c9d1adf208"; 
        private string clientSecret = "d2cc18888aa84068807df547d05c09ec"; 
        private SpotifyClient spotify;

        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                var config = SpotifyClientConfig.CreateDefault();
                var request = new ClientCredentialsRequest(clientId, clientSecret);
                var response = await new OAuthClient(config).RequestToken(request);

                spotify = new SpotifyClient(config.WithToken(response.AccessToken));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Spotify authentication failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> SearchTracksAsync(string query)
        {
            var tracks = new List<string>();
            try
            {
                var searchResponse = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, query));
                foreach (var item in searchResponse.Tracks.Items)
                {
                    tracks.Add($"{item.Name} by {item.Artists[0].Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching tracks: {ex.Message}");
            }
            return tracks;
        }
    }
}
