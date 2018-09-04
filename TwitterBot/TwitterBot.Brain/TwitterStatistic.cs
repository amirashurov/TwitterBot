using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LinqToTwitter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TwitterBot.Brain
{
    public class TwitterStatistic
    {
        private readonly TwitterContext _twitterContext;
        
        public TwitterStatistic(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore()
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    //OAuthToken = token,
                    //OAuthTokenSecret = tokenSecret
                }
            };

            _twitterContext = new TwitterContext(auth);
        }

        public string GetStatisticForTweets(string username, int tweetsCount = 5, bool ignoreCase = false)
        {
            string result = string.Empty;
            List<string> tweets = new List<string>();
            try
            {
                tweets =
                (from tweet in _twitterContext.Status
                 where tweet.Type == StatusType.User &&
                       tweet.ScreenName == username &&
                       tweet.Count == 5
                 select tweet.Text)
                .ToList();
            }
            catch (TwitterQueryException ex)
            {
                //TODO: Надо дописать обработку exceptions...
                throw new Exception(ex.Message);
            }

            return GetStatistic(tweets, ignoreCase);
        }
   
        public int PostStatistic(string username, string statistic)
        {
            if (!username.StartsWith("@"))
                username = "@" + username;

            //divide statistic to parts because maximum tweet length is 140
            var statisticDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(statistic);

            int takeCount = 7;
            int skipCount = 0;
            int iterateCount = statisticDictionary.Count / takeCount + 1;

            for (int i = 0; i < iterateCount; i++)
            {
                var statisticPortion = statisticDictionary.Skip(skipCount).Take(takeCount).ToDictionary(kv => kv.Key, kv => kv.Value);
                string json = JsonConvert.SerializeObject(statisticPortion);
                string message = $"{username}, статистика последних 5 твитов:\n {json}";

                try
                {
                    _twitterContext.TweetAsync(message);
                }
                catch (TwitterQueryException ex)
                {
                    //TODO: Надо дописать обработку exceptions...
                    switch (ex.ErrorCode)
                    {
                        case 187: throw new Exception("Такой твит уже был :(");
                        default: throw new Exception(ex.Message);
                    }
                }

                skipCount += takeCount;
            }

            return iterateCount;
        }

        private string GetStatistic(List<string> tweets, bool ignoreCase)
        {
            //remove useless symbols
            tweets = tweets.Select(t => Regex.Replace(t, @"[^a-zA-Zа-яА-Я]", "")).ToList();

            var summaryText = string.Concat(tweets);
            if (ignoreCase)
            {
                summaryText = summaryText.ToLower();
            }
            JObject jsonObj = new JObject();

            foreach (var grouping in summaryText.GroupBy(c => c).OrderBy(c => c.Key))
            {
                jsonObj[grouping.Key.ToString()] = CalcFrequency(grouping.Count(), summaryText.Length);
            }

            return jsonObj.ToString();
        }

        private double CalcFrequency(int count, int length)
        {
            return Math.Round((double)count / (double)length, 3);
        }

    }
}