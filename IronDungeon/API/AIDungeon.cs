using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IronDungeon.API
{
    public class AIDungeon
    {
        // TODO: Add the option to specify a custom API endpoint in the constructor.
        private const string ApiEndpoint = "https://api.aidungeon.io";
        public const uint AllScenarios = 362833;
        private static readonly HttpClient AidClient = new HttpClient { BaseAddress = new Uri(ApiEndpoint) };

        public string Token { get; }

        private AIDungeon()
        {
        }

        public AIDungeon(string Token)
        {
            this.Token = Token;
        }

        public AIDungeon(string Email, string Password)
        {
            var task = Task.Run(() => LoginAsync(Email, Password));
            task.Wait();
            LoginResponse Response = task.Result;
            if (Response == null || Response.Errors != null)
            {
                Token = null;
            }
            else
            {
                Token = Response.Data.Login.AccessToken;
            }
        }

        public AIDungeon(string Email, string Username, string Password)
        {
            var task = Task.Run(() => RegisterAsync(Email, Username, Password));
            task.Wait();
            RegisterResponse Response = task.Result;
            if (Response == null || Response.Errors != null)
            {
                Token = null;
            }
            else
            {
                Token = Response.Data.CreateAccount.AccessToken;
            }
        }

        private async Task<string> RunAPIRequestAsync(string JsonQuery, bool UseAccessToken = true)
        {
            var requestContent = new StringContent(JsonQuery, Encoding.UTF8, "application/json");
            using (var request = new HttpRequestMessage(HttpMethod.Post, "/graphql"))
            {
                if (UseAccessToken)
                {
                    request.Headers.Add("X-Access-Token", Token);
                }
                request.Content = requestContent;
                var response = await AidClient.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{ApiEndpoint}/graphql: {response.StatusCode} ({content})");
                }
                return content;
            }
        }

        public async Task<RegisterResponse> RegisterAsync(string Email, string Username, string Password)
        {
            string query = new RegisterInfo(Email, Username, Password).ToString();
            string content = await RunAPIRequestAsync(query, false);
            try
            {
                return JsonConvert.DeserializeObject<RegisterResponse>(content);
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }

        public async Task<LoginResponse> LoginAsync(string Email, string Password)
        {
            string query = new LoginInfo(Email, Password).ToString();
            string content = await RunAPIRequestAsync(query, false);
            try
            {
                return JsonConvert.DeserializeObject<LoginResponse>(content);
            }
            catch (JsonSerializationException)
            {
                //Console.WriteLine($"User login: invalid response content: {content}");
                return null;
            }
        }

        public async Task<CreationResponse> CreateAdventureAsync(uint ScenarioID, string Prompt = null)
        {
            string query = new CreationInfo(ScenarioID, Prompt).ToString();
            string content = await RunAPIRequestAsync(query);
            try
            {
                return JsonConvert.DeserializeObject<CreationResponse>(content);
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine($"Adventure creation ({ScenarioID}): invalid response content: {content}");
                return null;
            }
        }

        public async Task<ScenarioResponse> GetScenarioAsync(uint ScenarioID)
        {
            string query = new ScenarioInfo(ScenarioID).ToString();
            string content = await RunAPIRequestAsync(query);
            try
            {
                return JsonConvert.DeserializeObject<ScenarioResponse>(content);
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine($"Get Scenario ({ScenarioID}): invalid response content: {content}");
                return null;
            }
        }

        public async Task<AdventureListResponse> GetAdventureListAsync()
        {
            string query = new AdventureListInfo().ToString();
            string content = await RunAPIRequestAsync(query);
            try
            {
                return JsonConvert.DeserializeObject<AdventureListResponse>(content);
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine($"Get adventure list: invalid response content: {content}");
                return null;
            }
        }

        public async Task<RefreshResponse> RefreshAdventureListAsync()
        {
            var query = new RefreshInfo().ToString();
            string content = await RunAPIRequestAsync(query);
            try
            {
                return JsonConvert.DeserializeObject<RefreshResponse>(content);
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine($"Refresh adventure list: invalid response content: {content}");
                return null;
            }
        }

        public async Task<AdventureInfoResponse> GetAdventureAsync(uint AdventureID)
        {
            string query = new AdventureInfo(AdventureID).ToString();
            string content = await RunAPIRequestAsync(query);
            try
            {
                return JsonConvert.DeserializeObject<AdventureInfoResponse>(content);
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine($"Refresh adventure: invalid response content: {content}");
                return null;
            }
        }

        public async Task<ActionResponse> RunActionAsync(uint AdventureID, ActionType Action, InputType InputType = InputType.None, string Input = "", string Output = "")
        {
            string query = new ActionInfo(AdventureID, Action, InputType, Input, Output).ToString();
            string content = await RunAPIRequestAsync(query);
            try
            {
                return JsonConvert.DeserializeObject<ActionResponse>(content);
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine($"Run Action {Action} ({AdventureID}): invalid response content: {content}");
                return null;
            }
        }

        public async Task<DeleteResponse> DeleteAdventureAsync(uint AdventureID)
        {
            string query = new DeleteInfo(AdventureID).ToString();
            string content = await RunAPIRequestAsync(query);
            try
            {
                return JsonConvert.DeserializeObject<DeleteResponse>(content);
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine($"Delete Adventure ({AdventureID}): invalid response content: {content}");
                return null;
            }
        }
    }
}