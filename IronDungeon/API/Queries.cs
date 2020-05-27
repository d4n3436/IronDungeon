using Newtonsoft.Json;

namespace IronDungeon.API
{
    // TODO: Use GraphQL query builder

    public class RegisterInfo
    {
        public RegisterInfo(string Email, string Username, string Password)
        {
            Query = _query;
            Variables = new RegisterVariables
            {
                Email = Email,
                Username = Username,
                Password = Password
            };
        }

        private const string _query = "mutation ($email: String!, $username: String!, $password: String!) {\n  createAccount(email: $email, username: $username, password: $password) {\n    id\n    accessToken\n    __typename\n  }\n}\n";

        [JsonProperty("operationName")]
        public string OperationName { get; set; } = null;

        [JsonProperty("variables")]
        public RegisterVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class RegisterVariables
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    public class LoginInfo
    {
        public LoginInfo(string Email, string Password)
        {
            Variables = new LoginVariables
            {
                Email = Email,
                Password = Password
            };
            Query = _query;
        }

        public const string _query = "mutation ($email: String!, $password: String!) {\n  login(email: $email, password: $password) {\n    id\n    accessToken\n    __typename\n  }\n}\n";

        [JsonProperty("operationName")]
        public string OperationName { get; set; } = null;

        [JsonProperty("variables")]
        public LoginVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class LoginVariables
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    public class ScenarioInfo
    {
        public ScenarioInfo(uint ScenarioID)
        {
            Query = _query;
            Variables = new ScenarioVariables { Id = $"scenario:{ScenarioID}" };
        }

        private const string _query = "query ($id: String) {\n  content(id: $id) {\n    id\n    contentType\n    contentId\n    title\n    description\n    prompt\n    memory\n    tags\n    nsfw\n    published\n    createdAt\n    updatedAt\n    deletedAt\n    options {\n      id\n      title\n      __typename\n    }\n    __typename\n  }\n}\n";

        [JsonProperty("operationName")]
        public string OperationName { get; set; } = null;

        [JsonProperty("variables")]
        public ScenarioVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ScenarioVariables
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class CreationInfo
    {
        public CreationInfo(uint ScenarioID, string Prompt = null)
        {
            Query = _query;
            var Vars = new CreationVariables
            {
                Id = $"scenario:{ScenarioID}"
            };
            if (Prompt != null)
            {
                Vars.Prompt = Prompt;
            }

            Variables = Vars;
        }

        private const string _query = "mutation ($id: String!, $prompt: String) {\n  createAdventureFromScenarioId(id: $id, prompt: $prompt) {\n    id\n    contentType\n    contentId\n    title\n    description\n    tags\n    nsfw\n    published\n    createdAt\n    updatedAt\n    deletedAt\n    publicId\n    historyList\n    __typename\n  }\n}\n";

        [JsonProperty("operationName")]
        public string OperationName { get; set; } = null;

        [JsonProperty("variables")]
        public CreationVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class CreationVariables
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("prompt", NullValueHandling = NullValueHandling.Ignore)]
        public string Prompt { get; set; }
    }

    public class AdventureListInfo
    {
        public AdventureListInfo()
        {
            OperationName = "user";
            Query = _query;
            Variables = new AdventureListVariables
            {
                Input = new AdventureListInputInfo
                {
                    ContentType = "adventure",
                    SearchTerm = "",
                    SortOrder = "createdAt",
                    TimeRange = null
                }
            };
        }

        [JsonProperty("operationName")]
        public string OperationName { get; set; }

        [JsonProperty("variables")]
        public AdventureListVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public const string _query = "query user($input: ContentListInput) {\n  user {\n    id\n    contentList(input: $input) {\n      id\n      contentType\n      contentId\n      title\n      description\n      tags\n      nsfw\n      published\n      createdAt\n      updatedAt\n      deletedAt\n      username\n      userVote\n      totalUpvotes\n      totalDownvotes\n      totalComments\n      __typename\n    }\n    __typename\n  }\n}\n";

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class AdventureListVariables
    {
        [JsonProperty("input")]
        public AdventureListInputInfo Input { get; set; }
    }

    public partial class AdventureListInputInfo
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("searchTerm")]
        public string SearchTerm { get; set; }

        [JsonProperty("sortOrder")]
        public string SortOrder { get; set; }

        [JsonProperty("timeRange")]
        public string TimeRange { get; set; }
    }

    public class RefreshInfo
    {
        public RefreshInfo()
        {
            Variables = new RefreshVariables();
            Query = _query;
        }

        [JsonProperty("operationName")]
        public string OperationName { get; set; } = null;

        [JsonProperty("variables")]
        public RefreshVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public const string _query = "mutation {\n  refreshSearchIndex\n}\n";

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class RefreshVariables
    {
    }

    public class AdventureInfo
    {
        public AdventureInfo(uint AdventureID)
        {
            Variables = new AdventureVariables
            {
                Id = $"adventure:{AdventureID}"
            };
            Query = _query;
        }

        [JsonProperty("operationName")]
        public string OperationName { get; set; } = null;

        [JsonProperty("variables")]
        public AdventureVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public const string _query = "query ($id: String) {\n  content(id: $id) {\n    id\n    published\n    createdAt\n    historyList\n    weeklyContest\n    __typename\n  }\n}\n";

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class AdventureVariables
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class ActionInfo
    {
        public ActionInfo(uint AdventureID, ActionType Action, InputType Type = InputType.None, string Input = "", string Output = "")
        {
            Query = _query;
            Variables = new ActionVariables();
            var Inputs = new InputInfo
            {
                AdventureId = AdventureID.ToString()
            };
            if (Action == ActionType.Progress)
            {
                if (Input != "")
                {
                    Inputs.Input = Input;
                }
                else
                {
                    Action = ActionType.Continue;
                }
            }
            Inputs.ActionName = Action.ToString().ToLowerInvariant();
            if (Action == ActionType.Progress)
            {
                if (Type == InputType.None)
                    Type = InputType.Do;
                Inputs.InputType = Type.ToString();
            }
            if (Action == ActionType.Remember || Action == ActionType.Alter)
            {
                Inputs.Input = Input;
                if (Action == ActionType.Alter)
                {
                    Inputs.Output = Output;
                }
            }
            Inputs.Platform = "web";

            Variables.Input = Inputs;
        }

        public const string _query = "mutation ($input: UserActionInput!) {\n  performUserAction(input: $input) {\n    id\n    historyList\n    memoryList\n    __typename\n  }\n}\n";

        [JsonProperty("operationName")]
        public string OperationName { get; set; } = null;

        [JsonProperty("variables")]
        public ActionVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ActionVariables
    {
        [JsonProperty("input")]
        public InputInfo Input { get; set; }
    }

    public class InputInfo
    {
        [JsonProperty("actionName")]
        public string ActionName { get; set; } // progress, continue (when no text passed)

        [JsonProperty("adventureId")]
        public string AdventureId { get; set; }

        [JsonProperty("input", NullValueHandling = NullValueHandling.Ignore)]
        public string Input { get; set; }

        [JsonProperty("inputType", NullValueHandling = NullValueHandling.Ignore)]
        public string InputType { get; set; }

        [JsonProperty("output", NullValueHandling = NullValueHandling.Ignore)]
        public string Output { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }
    }

    public class DeleteInfo
    {
        public DeleteInfo(uint AdventureID)
        {
            Query = _query;
            Variables = new DeleteVariables { Id = $"adventure:{AdventureID}" };
        }

        private const string _query = "mutation ($id: String!) {\n  deleteContent(id: $id) {\n    id\n    deletedAt\n    __typename\n  }\n}\n";

        [JsonProperty("operationName")]
        public string OperationName { get; set; } = null;

        [JsonProperty("variables")]
        public DeleteVariables Variables { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class DeleteVariables
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}