using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace YouTubeToSpotify
{
    class Program
    {
        static void Main(string[] args)
        {
            // Replace with the YouTube channel ID
            string channelId = "";

            // Use the YouTube API to retrieve information about the channel's videos
            string youtubeApiKey = "";
            string youtubeApiUrl = $"https://www.googleapis.com/youtube/v3/search?key={youtubeApiKey}&channelId={channelId}&part=snippet&type=video&order=date";
            string nextPageToken = "";
            using (StreamWriter writer = new StreamWriter("output.csv",append: true))
            {
                while (true)
                {
                    string youtubeResponse = new WebClient().DownloadString(youtubeApiUrl + (nextPageToken == "" ? "" : "&pageToken=" + nextPageToken));

                    // Deserialize the JSON response into a dynamic object
                    dynamic youtubeData = JsonConvert.DeserializeObject(youtubeResponse);

                    // Loop through each video and search for it on Spotify
                    foreach (var video in youtubeData.items)
                    {
                        string videoTitle = video.snippet.title;

                        // Use the Spotify API to search for a track with the same title
                        string spotifyApiKey = "";
                        string spotifyApiUrl = $"https://api.spotify.com/v1/search?q={WebUtility.UrlEncode(videoTitle)}&type=track&limit=2";
                        WebClient webClient = new WebClient();
                        webClient.Headers.Add("Authorization", "Bearer " + spotifyApiKey);
                        string spotifyResponse = webClient.DownloadString(spotifyApiUrl);

                        // Deserialize the JSON response into a dynamic object
                        dynamic spotifyData = JsonConvert.DeserializeObject(spotifyResponse);

                        // Check if the artist and song name match between YouTube and Spotify
                        if (spotifyData.tracks.items.Count > 0)
                        {
                            string spotifyArtist = spotifyData.tracks.items[0].artists[0].name;
                            string spotifySong = spotifyData.tracks.items[0].name;
                            Console.WriteLine(videoTitle + "|||||||||" + spotifyArtist + " - " + spotifySong);


                            if (videoTitle.Contains(spotifyArtist) && videoTitle.Contains(spotifySong))
                            {
                                writer.WriteLine($"{videoTitle},{spotifyData.tracks.items[0].external_urls.spotify}");
                                writer.Flush();
                            }
                            else
                            {
                                Console.WriteLine("Artist and song name do not match");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Track not found on Spotify");
                        }
                    }

                    // Check if there are more results to retrieve
                    if (!youtubeData.ContainsKey("nextPageToken"))
                    {
                        break;
                    }
                    nextPageToken = youtubeData.nextPageToken;
                }
            }
        }
    }
}
