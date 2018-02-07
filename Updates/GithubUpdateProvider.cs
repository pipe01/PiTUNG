using System.Text.RegularExpressions;
using System.Net;
using System;

namespace PiTung_Bootstrap.Updates
{
    public class GithubUpdateProvider : UpdateProvider
    {
        private const string VersionRegex = @"releases\/tag\/v?(?<ver>[^""]*?)"">.*?<\/a>";
        private const string BinaryRegex = @"<strong class=""pl-1"">(.*?\.dll)<\/strong>";

        private static WebClient Client = new WebClient();
        
        public string GithubRepoUrl { get; }

        public GithubUpdateProvider(string githubUrl, string modPackage)
            : base(modPackage)
        {
            this.GithubRepoUrl = githubUrl;
        }

        internal override Update GetUpdate()
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

            return new Update(ModPackage, version, binName);
        }
    }
}
