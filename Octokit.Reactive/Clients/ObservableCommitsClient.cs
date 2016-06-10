﻿using System;
using System.Reactive.Threading.Tasks;

namespace Octokit.Reactive
{
    /// <summary>
    /// A client for GitHub's Git Commits API.
    /// </summary>
    /// <remarks>
    /// See the <a href="http://developer.github.com/v3/git/commits/">Git Commits API documentation</a> for more information.
    /// </remarks>
    public class ObservableCommitsClient : IObservableCommitsClient
    {
        readonly ICommitsClient _client;

        public ObservableCommitsClient(IGitHubClient client)
        {
            Ensure.ArgumentNotNull(client, "client");

            _client = client.Git.Commit;
        }

        /// <summary>
        /// Gets a commit for a given repository by sha reference
        /// </summary>
        /// <remarks>
        /// http://developer.github.com/v3/git/commits/#get-a-commit
        /// </remarks>
        /// <param name="owner">The owner of the repository</param>
        /// <param name="name">The name of the repository</param>
        /// <param name="reference">Tha sha reference of the commit</param>
        /// <returns>A <see cref="Commit"/> representing commit for specified repository and reference</returns>
        public IObservable<Commit> Get(string owner, string name, string reference)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            Ensure.ArgumentNotNullOrEmptyString(reference, "reference");

            return _client.Get(owner, name, reference).ToObservable();
        }

        /// <summary>
        /// Create a commit for a given repository
        /// </summary>
        /// <remarks>
        /// http://developer.github.com/v3/git/commits/#create-a-commit
        /// </remarks>
        /// <param name="owner">The owner of the repository</param>
        /// <param name="name">The name of the repository</param>
        /// <param name="commit">The commit to create</param>
        /// <returns>A <see cref="IObservable{Commit}"/> of <see cref="Commit"/> representing created commit for specified repository</returns>
        public IObservable<Commit> Create(string owner, string name, NewCommit commit)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            Ensure.ArgumentNotNull(commit, "commit");

            return _client.Create(owner, name, commit).ToObservable();
        }
    }
}