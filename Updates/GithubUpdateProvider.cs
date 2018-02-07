using System.Text.RegularExpressions;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Updates
{
    public class GithubUpdateProvider : IUpdateProvider
    {
        private const string VersionRegex = @"releases\/tag\/v?(?<ver>[^""]*?)"">.*?<\/a>";
        private const string BinaryRegex = @"<strong class=""pl-1"">(.*?\.dll)<\/strong>";

        private static WebClient Client = new WebClient();

        private Update? LastUpdate;

        public string ModName { get; }
        public string GithubRepoUrl { get; }

        public GithubUpdateProvider(string githubUrl, string modName)
        {
            this.GithubRepoUrl = githubUrl;
            this.ModName = modName;
        }

        public Update GetUpdate()
        {
            if (LastUpdate == null)
            {
                string url = GithubRepoUrl + (GithubRepoUrl.EndsWith("/") ? "" : "/") + "releases/latest";
                string page = Client.DownloadString(url);

                var verMatch = Regex.Match(page, VersionRegex);

                if (verMatch.Groups.Count != 1)
                {
                    throw new Exception("An error occurred while getting the latest version name from GitHub");
                }

                var version = new Version(verMatch.Groups["ver"].Value);

                var binMatch = Regex.Match(page, BinaryRegex);

                if (binMatch.Groups.Count == 0)
                {
                    throw new Exception("An error occurred while getting the latest binary name from GitHub");
                }

                string binName = binMatch.Groups[0].Value;

                LastUpdate = new Update(ModName, version, binName);
            }

            return LastUpdate.Value;
        }

        public bool IsUpdateAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
