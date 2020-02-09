using System;
using TechTalk.SpecFlow;
using RestSharp;
using FluentAssertions;
using tundraDemo.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using TechTalk.SpecFlow.Assist;

namespace tundraDemo.StepDefinitions
{
    public class ScenarioData
    {
        public int UserId;
        public string Email;
        public int ResponseStatusCode;
        public List<Post> Posts;
    }

    [Binding]
    public class UserSteps
    {

        private readonly ScenarioData scenarioData;
        public UserSteps(ScenarioData scenarioData)
        {
            this.scenarioData = scenarioData;
        }

        [Given(@"Generate random number between (.*) and (.*) as userID")]
        public void GivenGenerateRandomNumberBetweenAnd(int lowerLimit, int upperLimit)
        {
            NUnit.Framework.TestContext.Progress.WriteLine(ScenarioStepContext.Current.StepInfo.Text);
            Random rnd = new Random();
            scenarioData.UserId = rnd.Next(lowerLimit, upperLimit);
            
        }

        [When(@"Get user details")]
        public void WhenGetUserDetails()
        {
            NUnit.Framework.TestContext.Progress.WriteLine(ScenarioStepContext.Current.StepInfo.Text);
            var client = new RestClient("https://jsonplaceholder.typicode.com/");
            var request = new RestRequest($"users/{scenarioData.UserId}");
            var response = client.Get<GetUserReponse>(request);
            response.Should().NotBeNull();

            scenarioData.ResponseStatusCode = (int)response.StatusCode;
            scenarioData.Email = response.Data.Email;
        }

        [Then(@"Response status is (.*)")]
        public void ThenResponseStatusIs(int expectedStatusCode)
        {
            NUnit.Framework.TestContext.Progress.WriteLine(ScenarioStepContext.Current.StepInfo.Text);
            scenarioData.ResponseStatusCode.Should().Be(expectedStatusCode);
        }

        [Then(@"Email format is valid")]
        public void ThenEmailFormatIsValid()
        {
            NUnit.Framework.TestContext.Progress.WriteLine(ScenarioStepContext.Current.StepInfo.Text);
            IsValidEmail(scenarioData.Email).Should().BeTrue();
        }

        [When(@"Get user associated posts")]
        public void WhenGetUserAssociatedPosts()
        {
            NUnit.Framework.TestContext.Progress.WriteLine(ScenarioStepContext.Current.StepInfo.Text);
            var client = new RestClient("https://jsonplaceholder.typicode.com/");
            var request = new RestRequest("posts");
            request.AddQueryParameter("userId", scenarioData.UserId.ToString());

            var response = client.Get<List<Post>>(request);
            response.Should().NotBeNull();

            scenarioData.ResponseStatusCode = (int)response.StatusCode;
            scenarioData.Posts = response.Data;
        }

        [Then(@"Posts has correct structure")]
        public void ThenPostsHasCorrectStructure()
        {
            NUnit.Framework.TestContext.Progress.WriteLine(ScenarioStepContext.Current.StepInfo.Text);
            foreach (Post post in scenarioData.Posts)
            {
                post.UserId.Should().Be(scenarioData.UserId);
                post.Id.Should().BeOfType(typeof(int));
                post.Title.Should().NotBeNullOrEmpty();
                post.Title.Should().BeOfType(typeof(string));
                post.Body.Should().NotBeNullOrEmpty();
                post.Body.Should().BeOfType(typeof(string));
            }
        }

        [When(@"User do a new post with title and body:")]
        public void WhenUserDoANewPostWithTitleAndBody(Table table)
        {
            NUnit.Framework.TestContext.Progress.WriteLine(ScenarioStepContext.Current.StepInfo.Text);
            var content = table.CreateInstance<(string title, string body)>();

            var requestBody = new Post
            {
                UserId = scenarioData.UserId,
                Title = content.title,
                Body = content.body
            };

            var client = new RestClient("https://jsonplaceholder.typicode.com/");
            var request = new RestRequest("posts", Method.POST);
            request.AddJsonBody(requestBody);
            IRestResponse response = client.Execute(request);
            scenarioData.ResponseStatusCode = (int)response.StatusCode;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

    }
}
