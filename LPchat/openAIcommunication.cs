namespace LPchat
{

    using RestSharp;
    using Newtonsoft.Json;
    using System.Transactions;

    public class ChatGPTClient
    {
        private readonly string _apiKey;
        private readonly RestClient _client;

        

        // Constructor that takes the API key as a parameter
        public ChatGPTClient(string apiKey)
        {
            _apiKey = "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";
            _apiKey = "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";
            _apiKey = "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";
            _apiKey = "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";
            _apiKey = "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";
            _apiKey = "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";
            // Initialize the RestClient with the ChatGPT API endpoint
            _client = new RestClient("https://api.openai.com/v1/engines/text-davinci-003/completions");
        }

        // We'll add methods here to interact with the API.
        public string SendMessage(string message)
        {

                // Create a new POST request
                var request = new RestRequest("", Method.Post);
                // Set the Content-Type header
                request.AddHeader("Content-Type", "application/json");
                // Set the Authorization header with the API key
                request.AddHeader("Authorization", $"Bearer {_apiKey}");

                // Create the request body with the message and other parameters
                var requestBody = new
                {
                    prompt = message,
                    max_tokens = 100,
                    n = 1,
                    stop = (string?)null,
                    temperature = 0.7,
                };

                // Add the JSON body to the request
                request.AddJsonBody(JsonConvert.SerializeObject(requestBody));

                // Execute the request and receive the response
                var response = _client.Execute(request);

                // Deserialize the response JSON content
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content ?? string.Empty);
                return jsonResponse?.choices[0]?.text?.ToString()?.Trim() ?? string.Empty;
        }
    }
    public class openAIcommunication
    {
        int secondChance = 0;
        List<string> apiKeys = new List<string>();
        int keyIndex = 0;
        public string communicate(string input)
        {
            string res = "";
            // Replace with your ChatGPT API key

            apiKeys.Add("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");
            apiKeys.Add("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");
            apiKeys.Add("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");
            apiKeys.Add("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");
            apiKeys.Add("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");
            apiKeys.Add("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");
            // Create a ChatGPTClient instance with the API key

            

            // Display a welcome message
            //     Console.WriteLine("Welcome to the ChatGPT chatbot! Type 'exit' to quit.");

            // Enter a loop to take user input and display chatbot responses            
            // Prompt the user for input
            //   Console.ForegroundColor = ConsoleColor.Green; // Set text color to green
            //   Console.Write("You: ");
            //   Console.ResetColor(); // Reset text color to default
            string response = "";
            int startIndex = keyIndex;
            int keyIndexExport = keyIndex;
            while (response=="")
            try
            {

                    var chatGPTClient = new ChatGPTClient(apiKeys[keyIndex]);
                    response = chatGPTClient.SendMessage(input);
            }
            catch
                {
                    keyIndex++;
                    if (keyIndex == startIndex) { keyIndex = 0; }
                    if (keyIndex== startIndex) { break; }
                }
            return startIndex.ToString()+"%#"+response;
        }
    }        
}
