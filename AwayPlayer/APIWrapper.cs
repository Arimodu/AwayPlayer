using AwayPlayer.Models;
using Newtonsoft.Json.Linq;
using SiraUtil.Logging;
using SiraUtil.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AwayPlayer
{
    internal class APIWrapper
    {
        private readonly IHttpService HttpService;
        private readonly SiraLog SiraLogger;

        public APIWrapper(SiraLog siraLog, IHttpService httpService)
        {
            SiraLogger = siraLog;
            HttpService = httpService;
#if DEBUG
            SiraLogger.DebugMode = true;
#endif
        }

        public async Task<List<Score>> GetReplayListAsync()
        {
            // Get the player UserId
            var userInfo = await BS_Utils.Gameplay.GetUserInfo.GetPlatformUserModel().GetUserInfo();

            // Fetch the total number of scores available
            var response = await HttpService.GetAsync($"https://api.beatleader.xyz/player/{userInfo.platformUserId}/scores?count=0");
            string jsonData = await response.ReadAsStringAsync();
            int total = JObject.Parse(jsonData)["metadata"]["total"].Value<int>();

            // Fetch all scores for the player
            response = await HttpService.GetAsync($"https://api.beatleader.xyz/player/{userInfo.platformUserId}/scores?count={total}");
            jsonData = await response.ReadAsStringAsync();

            List<Score> scores = new List<Score>();

            // Calculate the nearest number divisible by 24 and the remainder
            int closestDivisibleBy24 = total - (total % 24);
            int remainder = total - closestDivisibleBy24;

            // Deserialize JSON data only once into a JArray
            JArray scoresArray = JObject.Parse(jsonData)["data"] as JArray;

            // Process scores in parallel using up to 24 threads
            Parallel.For(0, 24, i =>
            {
                int chunkSize = closestDivisibleBy24 / 24; // Adjusted chunk size
                int start = i * chunkSize;
                int end = (i == 23) ? closestDivisibleBy24 : (i + 1) * chunkSize;

                // Distribute the remaining items among the threads
                if (i < remainder)
                {
                    start += i;
                    end += i + 1;
                }
                else
                {
                    start += remainder;
                    end += remainder;
                }

                List<Score> chunkScores = new List<Score>();

                // Process scores in the current chunk
                for (int j = start; j < end; j++)
                {
                    var scoreJObject = scoresArray[j] as JObject;
                    var score = scoreJObject.ToObject<Score>();
                    score.Song = scoreJObject["leaderboard"]["song"].ToObject<Song>();
                    score.Difficulty = scoreJObject["leaderboard"]["difficulty"].ToObject<Difficulty>();
                    chunkScores.Add(score);

                    // Debug information for the parsed score
                    //SiraLogger.Debug($"Parsed score for {score.Song} with {score}");
                }

                // Add the chunk of scores to the main list in a thread-safe manner
                lock (scores)
                {
                    scores.AddRange(chunkScores);
                }
            });

            return scores;
        }

        public async Task<byte[]> GetReplayDataAsync(string url)
        {
            var response = await HttpService.GetAsync(url);
            return await response.ReadAsByteArrayAsync();
        }

    }
}
