using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers
{
    public class ChatGptService : MonoBehaviour
    {
        [Header("API")]
        [SerializeField] private string _model = "gpt-4o-mini";
        [SerializeField] private TextAsset _apiKeyAsset;
        [SerializeField] private string _apiKeyResourcePath = "OpenAiKey";
        [SerializeField, Range(4f, 30f)] private float _timeoutSeconds = 12f;
        [SerializeField] private bool _logResponses;
        [SerializeField] private bool _logKeySource;
        [SerializeField] private bool _testConnectionOnStart = true;

        private string _cachedApiKey;
        private string _lastConnectionError;

        [Header("Prompt")]
        [SerializeField, TextArea] private string _systemPrompt =
            "You are a recycling guide inside a game. Answer concisely (<=3 short sentences). " +
            "Tell the player which bin to use and why, using the provided bin list. If uncertain, say so.";

        private readonly Dictionary<TrashSortType, string> _binNames = new()
        {
            { TrashSortType.Bio, "Bio" },
            { TrashSortType.Plastic, "Plastic" },
            { TrashSortType.Paper, "Paper" },
            { TrashSortType.Residual, "Residual" },
            { TrashSortType.SpecialWaste, "Special Waste" },
            { TrashSortType.WhiteGlass, "White Glass" },
            { TrashSortType.BrownGlass, "Brown Glass" },
            { TrashSortType.GreenGlass, "Green Glass" },
            { TrashSortType.Question, "Quiz" },
            { TrashSortType.Deposit, "Deposit" },
            { TrashSortType.Battery, "Battery" },
        };

        private void Start()
        {
            var key = ResolveAndCacheKey();
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("[ChatGptService] No API key found. Set _apiKeyPlain, OPENAI_API_KEY, a TextAsset, or Resources/OpenAiKey.txt.");
            }
            else if (_logKeySource)
            {
                Debug.Log("[ChatGptService] API key loaded successfully at startup.");
            }

            if (_testConnectionOnStart && !string.IsNullOrEmpty(key))
            {
                _ = TestConnectionAsync();
            }
        }

        public bool HasApiKey => !string.IsNullOrEmpty(_cachedApiKey ?? ResolveAndCacheKey());
        public string LastConnectionError => _lastConnectionError;

    public async Task<string> AskAsync(string playerQuestion, TrashData trashData, IReadOnlyList<TrashSortType> availableBins)
    {
        var apiKey = _cachedApiKey ?? ResolveAndCacheKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            return "AI help is unavailable because no API key is configured.";
        }

        var bins = (availableBins != null && availableBins.Count > 0)
            ? availableBins
            : Enum.GetValues(typeof(TrashSortType)).Cast<TrashSortType>().ToArray();

            string context = BuildContext(trashData, bins);
            string userText = string.IsNullOrWhiteSpace(playerQuestion)
                ? "Where should this trash go and why?"
                : playerQuestion.Trim();

            var requestBody = new ChatRequest
            {
                model = _model,
                temperature = 0.2f,
                max_tokens = 220,
                messages = new List<ChatMessage>
                {
                    new ChatMessage { role = "system", content = _systemPrompt },
                    new ChatMessage { role = "system", content = context },
                    new ChatMessage { role = "user", content = userText }
            }
        };

        string json = JsonConvert.SerializeObject(requestBody);
        const int maxAttempts = 2;
        float backoffSeconds = 2f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            using var request = BuildRequest(json, apiKey);
            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            long status = request.responseCode;
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonConvert.DeserializeObject<ChatResponse>(request.downloadHandler.text);
                    string reply = response?.choices?.FirstOrDefault()?.message?.content;
                    reply = string.IsNullOrWhiteSpace(reply) ? "AI returned an empty answer." : TrimReply(reply);
                    if (_logResponses) Debug.Log($"[ChatGptService] {reply}");
                    return reply;
                }
                catch (Exception e)
                {
                    return $"AI parsing error: {e.Message}";
                }
            }

            if (status == 429 && attempt < maxAttempts - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(backoffSeconds));
                backoffSeconds *= 1.5f;
                continue;
            }

            string bodyPreview = request.downloadHandler != null
                ? request.downloadHandler.text
                : string.Empty;
            string errorMsg = status == 429
                ? "AI is currently rate limited (429). Please try again in a few seconds."
                : $"AI request failed ({status}): {request.error} {(string.IsNullOrWhiteSpace(bodyPreview) ? string.Empty : $"Body: {bodyPreview}")}";
            return errorMsg;
        }

            return "AI request failed after retries.";
        }

        private UnityWebRequest BuildRequest(string json, string apiKey)
        {
        var request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        var body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.timeout = Mathf.CeilToInt(_timeoutSeconds);
        return request;
    }

        private string ResolveAndCacheKey()
        {
            var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(key) && _apiKeyAsset != null)
            {
                key = _apiKeyAsset.text;
            }

            if (string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(_apiKeyResourcePath))
            {
                var fromResources = Resources.Load<TextAsset>(_apiKeyResourcePath);
                if (fromResources != null)
                {
                    key = fromResources.text;
                }
            }

            key = string.IsNullOrWhiteSpace(key) ? null : key.Trim();
            _cachedApiKey = key;

            if (_logKeySource)
            {
                string source = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY")) ? "Env var"
                    : _apiKeyAsset != null ? "TextAsset field"
                    : Resources.Load<TextAsset>(_apiKeyResourcePath) != null ? "Resources"
                    : "None";
                string preview = key != null && key.Length > 8 ? key.Substring(0, 8) + "..." : "null";
                Debug.Log($"[ChatGptService] Key source: {source}, length: {key?.Length ?? 0}, preview: {preview}");
            }

            return key;
        }

        private async Task TestConnectionAsync()
        {
            var probe = new ChatRequest
            {
                model = _model,
                temperature = 0f,
                max_tokens = 10,
                messages = new List<ChatMessage>
                {
                    new ChatMessage { role = "system", content = "You are a probe. Reply with OK." },
                    new ChatMessage { role = "user", content = "OK?" }
                }
            };

            string json = JsonConvert.SerializeObject(probe);
            using var request = BuildRequest(json, _cachedApiKey);
            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                _lastConnectionError = null;
                if (_logResponses) Debug.Log("[ChatGptService] Connection test succeeded.");
            }
            else
            {
                string bodyPreview = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
                _lastConnectionError = $"Test failed ({request.responseCode}): {request.error} {bodyPreview}";
                Debug.LogWarning($"[ChatGptService] {_lastConnectionError}");
            }
        }

        private string BuildContext(TrashData data, IReadOnlyList<TrashSortType> bins)
        {
            if (data == null)
            {
                return "Item details are unavailable. Use general recycling advice.";
            }

            string binName = _binNames.TryGetValue(data.SortType, out var n) ? n : data.SortType.ToString();
            string available = string.Join(", ", bins.Select(b => _binNames.TryGetValue(b, out var name) ? name : b.ToString()));
            var sb = new StringBuilder();
            sb.Append($"Item name: {data.Name}. ");
            if (!string.IsNullOrWhiteSpace(data.Information))
            {
                sb.Append($"Item info: {data.Information}. ");
            }

            if (data.DepositValue > 0)
            {
                sb.Append($"Deposit value: {data.DepositValue}. ");
            }

            sb.Append($"Known correct bin: {binName}. ");
            sb.Append($"Available bins to choose from: {available}. ");
            sb.Append("If the player asks why, explain briefly using the item info.");
            return sb.ToString();
        }

        private string TrimReply(string reply)
        {
            const int maxChars = 400;
            reply = reply.Replace("\n\n", "\n").Trim();
            return reply.Length <= maxChars ? reply : reply.Substring(0, maxChars) + "...";
        }

        [Serializable]
        private class ChatRequest
        {
            public string model;
            public float temperature;
            public int max_tokens;
            public List<ChatMessage> messages;
        }

        [Serializable]
        private class ChatMessage
        {
            public string role;
            public string content;
        }

        [Serializable]
        private class ChatResponse
        {
            public List<Choice> choices;
        }

        [Serializable]
        private class Choice
        {
            public ChatMessage message;
        }
    }
}
