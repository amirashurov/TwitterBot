using System;
using TwitterBot.Brain;

namespace TwitterBot
{
    class Program
    {
        static void Main(string[] args)
        {
                var twitterStatistic = new TwitterStatistic(
                    "Gx2fEcBHAnp41L4Fvg0ZeB2DU",
                    "J7AvIUAbyXYfiGEwehfBvxdZcWukL1gN5agP8SbLAWhe0EwtSo",
                    "900250846331629568-wubFjIuAZIvlAJSRWgtB8IMIip2CSkk",
                    "GRNe4yEJnDQw7S1aiu6pMMusMt4G2IrcekzwwKZ7V6BrI");

                Console.Write("Enter twitter account: ");
                string username = Console.ReadLine();

                while (!String.IsNullOrEmpty(username) && username.ToLower() != "!q")
                {
                    try
                    {
                        Console.WriteLine($"Getting tweet from user: {username}...");

                        string statistic = twitterStatistic.GetStatisticForTweets(username: username);
                        Console.WriteLine(statistic + "\n");
                        Console.WriteLine("Posting statistic to twitter...");

                        var tweetsCount = twitterStatistic.PostStatistic(username, statistic);
                        Console.WriteLine($"Posted {tweetsCount} tweets\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Console.Write("Enter twitter account: ('!Q' to exit): ");
                    username = Console.ReadLine();
                }
           
        }
    }
}
