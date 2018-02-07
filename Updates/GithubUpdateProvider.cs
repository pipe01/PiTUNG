using System.Text.RegularExpressions;
using System.Net;
using System;

namespace PiTung_Bootstrap.Updates
{
    public class GithubUpdateProvider : UpdateProvider
    {
        private const string VersionRegex = @"releases\/tag\/(?<ver>[^""]*?)"">.*?<\/a>";
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
            string url = GithubRepoUrl + (GithubRepoUrl.EndsWith("/") ? "" : "/");
            string releasesUrl = url + "releases/latest";

            string html = Client.DownloadString(url);

            var verMatch = Regex.Match(html, VersionRegex);

            if (verMatch.Groups.Count != 2)
            {
                throw new Exception("An error occurred while getting the latest version name from GitHub");
            }

            string versionStr = verMatch.Groups["ver"].Value;
            var version = new Version(Versionize(versionStr));

            var binMatch = Regex.Match(html, BinaryRegex);

            if (binMatch.Groups.Count == 0)
            {
                throw new Exception("An error occurred while getting the latest binary name from GitHub");
            }

            string binName = binMatch.Groups[1].Value;

            string binUrl = url + $"/releases/download/{versionStr}/BetterSaves.dll";

            return new Update(ModPackage, version, binName);
        }

        private static string Versionize(string str)
        {
            string ret = "";

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (char.IsNumber(c) || c == '.')
                {
                    ret += c;
                }
            }

            return ret;
        }
    }
}
