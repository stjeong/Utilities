using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ukubectl
{
    class GithubHelper
    {
        public static async Task<string> GetLastestTagName(string owner, string repository)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("octokit-client"));

            // https://octokitnet.readthedocs.io/en/latest/releases/
            Release latest = await github.Repository.Release.GetLatest(owner, repository);
            return latest.TagName;
        }
    }
}
