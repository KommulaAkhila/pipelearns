﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using Octokit;
using Octokit.Internal;
using Xunit;

public class OauthClientTests
{
    public class TheCtor
    {
        [Fact]
        public void EnsuresNonNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OauthClient(null));
        }
    }

    public class TheGetGitHubLoginUrlMethod
    {
        [Theory]
        [InlineData("https://api.github.com", "https://github.com/login/oauth/authorize?client_id=secret")]
        [InlineData("https://github.com", "https://github.com/login/oauth/authorize?client_id=secret")]
        [InlineData("https://example.com/api/v3", "https://example.com/login/oauth/authorize?client_id=secret")]
        [InlineData("https://api.example.com/any/path/really", "https://api.example.com/login/oauth/authorize?client_id=secret")]
        [InlineData(null, "https://github.com/login/oauth/authorize?client_id=secret")]
        public void ReturnsProperAuthorizeUrl(string baseAddress, string expectedUrl)
        {
            var connection = Substitute.For<IConnection>();
            connection.BaseAddress.Returns(baseAddress == null ? null : new Uri(baseAddress));
            var client = new OauthClient(connection);

            var result = client.GetGitHubLoginUrl(new OauthLoginRequest("secret"));

            Assert.Equal(new Uri(expectedUrl), result);
        }

        [Fact]
        public void ReturnsUrlWithAllParameters()
        {
            var request = new OauthLoginRequest("secret")
            {
                RedirectUri = new Uri("https://example.com/foo?foo=bar"),
                Login = "johnDOE",
                Scopes = { "foo", "bar" },
                State = "canARY",
                AllowSignup = false
            };
            var connection = Substitute.For<IConnection>();
            connection.BaseAddress.Returns(new Uri("https://api.github.com"));
            var client = new OauthClient(connection);

            var result = client.GetGitHubLoginUrl(request);

            Assert.Equal("/login/oauth/authorize", result.AbsolutePath);
            Assert.Equal("?client_id=secret&redirect_uri=https%3A%2F%2Fexample.com%2Ffoo%3Ffoo%3Dbar&login=johnDOE&scope=foo%2Cbar&state=canARY&allow_signup=false", result.Query);
        }
    }

    public class TheCreateAccessTokenMethod
    {
        [Fact]
        public async Task PostsWithCorrectBodyAndContentType()
        {
            var responseToken = new OauthToken(null, null, null, null, null, null);
            var response = Substitute.For<IApiResponse<OauthToken>>();
            response.Body.Returns(responseToken);
            var connection = Substitute.For<IConnection>();
            connection.BaseAddress.Returns(new Uri("https://api.github.com/"));
            Uri calledUri = null;
            FormUrlEncodedContent calledBody = null;
            Uri calledHostAddress = null;
            connection.Post<OauthToken>(
                Arg.Do<Uri>(uri => calledUri = uri),
                Arg.Do<object>(body => calledBody = body as FormUrlEncodedContent),
                "application/json",
                null,
                Arg.Do<Uri>(uri => calledHostAddress = uri))
                .Returns(_ => Task.FromResult(response));
            var client = new OauthClient(connection);

            var token = await client.CreateAccessToken(new OauthTokenRequest("secretid", "secretsecret", "code")
            {
                RedirectUri = new Uri("https://example.com/foo")
            });

            Assert.Same(responseToken, token);
            Assert.Equal("login/oauth/access_token", calledUri.ToString());
            Assert.NotNull(calledBody);
            Assert.Equal("https://github.com/", calledHostAddress.ToString());
            Assert.Equal(
                "client_id=secretid&client_secret=secretsecret&code=code&redirect_uri=https%3A%2F%2Fexample.com%2Ffoo",
                await calledBody.ReadAsStringAsync());
        }

        [Fact]
        public async Task PostsWithCorrectBodyAndContentTypeForGHE()
        {
            var responseToken = new OauthToken(null, null, null, null, null, null);
            var response = Substitute.For<IApiResponse<OauthToken>>();
            response.Body.Returns(responseToken);
            var connection = Substitute.For<IConnection>();
            connection.BaseAddress.Returns(new Uri("https://example.com/api/v3"));
            Uri calledUri = null;
            FormUrlEncodedContent calledBody = null;
            Uri calledHostAddress = null;
            connection.Post<OauthToken>(
                Arg.Do<Uri>(uri => calledUri = uri),
                Arg.Do<object>(body => calledBody = body as FormUrlEncodedContent),
                "application/json",
                null,
                Arg.Do<Uri>(uri => calledHostAddress = uri))
                .Returns(_ => Task.FromResult(response));
            var client = new OauthClient(connection);

            var token = await client.CreateAccessToken(new OauthTokenRequest("secretid", "secretsecret", "code")
            {
                RedirectUri = new Uri("https://example.com/foo")
            });

            Assert.Same(responseToken, token);
            Assert.Equal("login/oauth/access_token", calledUri.ToString());
            Assert.NotNull(calledBody);
            Assert.Equal("https://example.com/", calledHostAddress.ToString());
            Assert.Equal(
                "client_id=secretid&client_secret=secretsecret&code=code&redirect_uri=https%3A%2F%2Fexample.com%2Ffoo",
                await calledBody.ReadAsStringAsync());
        }

        [Fact]
        public async Task InitiateDeviceFlowPostsWithCorrectBodyAndContentType()
        {
            var responseToken = new OauthDeviceFlowResponse("devicecode", "usercode", "uri", 10, 5);
            var response = Substitute.For<IApiResponse<OauthDeviceFlowResponse>>();
            response.Body.Returns(responseToken);
            var connection = Substitute.For<IConnection>();
            connection.BaseAddress.Returns(new Uri("https://api.github.com/"));
            Uri calledUri = null;
            FormUrlEncodedContent calledBody = null;
            Uri calledHostAddress = null;
            connection.Post<OauthDeviceFlowResponse>(
                Arg.Do<Uri>(uri => calledUri = uri),
                Arg.Do<object>(body => calledBody = body as FormUrlEncodedContent),
                "application/json",
                null,
                Arg.Do<Uri>(uri => calledHostAddress = uri))
                .Returns(_ => Task.FromResult(response));
            var client = new OauthClient(connection);

            var token = await client.InitiateDeviceFlow(new OauthDeviceFlowRequest("clientid"));

            Assert.Same(responseToken, token);
            Assert.Equal("login/device/code", calledUri.ToString());
            Assert.NotNull(calledBody);
            Assert.Equal("https://github.com/", calledHostAddress.ToString());
            Assert.Equal(
                "client_id=clientid",
                await calledBody.ReadAsStringAsync());
        }

        [Fact]
        public async Task CreateAccessTokenForDeviceFlowPostsWithCorrectBodyAndContentType()
        {
            var responseToken = new OauthToken(null, null, null, null, null, null);
            var response = Substitute.For<IApiResponse<OauthToken>>();
            response.Body.Returns(responseToken);
            var connection = Substitute.For<IConnection>();
            connection.BaseAddress.Returns(new Uri("https://api.github.com/"));
            Uri calledUri = null;
            FormUrlEncodedContent calledBody = null;
            Uri calledHostAddress = null;
            connection.Post<OauthToken>(
                Arg.Do<Uri>(uri => calledUri = uri),
                Arg.Do<object>(body => calledBody = body as FormUrlEncodedContent),
                "application/json",
                null,
                Arg.Do<Uri>(uri => calledHostAddress = uri))
                .Returns(_ => Task.FromResult(response));
            var client = new OauthClient(connection);

            var token = await client.CreateAccessTokenForDeviceFlow("clientid", new OauthDeviceFlowResponse("devicecode", "usercode", "uri", 10, 5));

            Assert.Same(responseToken, token);
            Assert.Equal("login/oauth/access_token", calledUri.ToString());
            Assert.NotNull(calledBody);
            Assert.Equal("https://github.com/", calledHostAddress.ToString());
            Assert.Equal(
                "client_id=clientid&device_code=devicecode&grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Adevice_code",
                await calledBody.ReadAsStringAsync());
        }

        [Fact]
        public async Task DeserializesOAuthScopeFormat()
        {
            var responseText =
            "{\"access_token\":\"token-goes-here\",\"token_type\":\"bearer\",\"scope\":\"notifications,user,user:email\"}";

            var strategy = new SimpleJsonSerializer();

            var token = strategy.Deserialize<OauthToken>(responseText);

            Assert.Equal("token-goes-here", token.AccessToken);
            Assert.Equal("bearer", token.TokenType);
            Assert.Contains("notifications", token.Scope);
            Assert.Contains("user:email", token.Scope);
        }
    }

    public class TheCreateAccessTokenFromRenewalTokenMethod
    {
        [Fact]
        public async Task PostsWithCorrectBodyAndContentType()
        {
            var responseToken = new OauthToken("bearer", "someaccesstoken", 3000, "refreshtoken", 10000, Array.Empty<string>(), null, null, null);
            var response = Substitute.For<IApiResponse<OauthToken>>();
            response.Body.Returns(responseToken);

            var connection = Substitute.For<IConnection>();
            connection.BaseAddress.Returns(new Uri("https://api.github.com/"));

            Uri calledUri = null;
            FormUrlEncodedContent calledBody = null;
            Uri calledHostAddress = null;
            connection.Post<OauthToken>(
                Arg.Do<Uri>(uri => calledUri = uri),
                Arg.Do<object>(body => calledBody = body as FormUrlEncodedContent),
                "application/json",
                null,
                Arg.Do<Uri>(uri => calledHostAddress = uri))
                .Returns(_ => Task.FromResult(response));
            var client = new OauthClient(connection);

            var token = await client.CreateAccessTokenFromRenewalToken(
                new OauthTokenRenewalRequest("secretid", "secretsecret", "refreshToken"));

            Assert.Same(responseToken, token);
            Assert.Equal("login/oauth/access_token", calledUri.ToString());
            Assert.NotNull(calledBody);
            Assert.Equal("https://github.com/", calledHostAddress.ToString());
            Assert.Equal(
                "client_id=secretid&client_secret=secretsecret&grant_type=refresh_token&refresh_token=refreshToken",
                await calledBody.ReadAsStringAsync());
        }

        [Fact]
        public async Task PostsWithCorrectBodyAndContentTypeForGHE()
        {
            var responseToken = new OauthToken("bearer", "someaccesstoken", 3000, "refreshtoken", 10000, Array.Empty<string>(), null, null, null);
            var response = Substitute.For<IApiResponse<OauthToken>>();
            response.Body.Returns(responseToken);

            var connection = Substitute.For<IConnection>();
            connection.BaseAddress.Returns(new Uri("https://example.com/api/v3"));

            Uri calledUri = null;
            FormUrlEncodedContent calledBody = null;
            Uri calledHostAddress = null;
            connection.Post<OauthToken>(
                Arg.Do<Uri>(uri => calledUri = uri),
                Arg.Do<object>(body => calledBody = body as FormUrlEncodedContent),
                "application/json",
                null,
                Arg.Do<Uri>(uri => calledHostAddress = uri))
                .Returns(_ => Task.FromResult(response));
            var client = new OauthClient(connection);

            var token = await client.CreateAccessTokenFromRenewalToken(
                new OauthTokenRenewalRequest("secretid", "secretsecret", "refreshToken"));

            Assert.Same(responseToken, token);
            Assert.Equal("login/oauth/access_token", calledUri.ToString());
            Assert.NotNull(calledBody);
            Assert.Equal("https://example.com/", calledHostAddress.ToString());
            Assert.Equal(
                "client_id=secretid&client_secret=secretsecret&grant_type=refresh_token&refresh_token=refreshToken",
                await calledBody.ReadAsStringAsync());
        }
    }
}
