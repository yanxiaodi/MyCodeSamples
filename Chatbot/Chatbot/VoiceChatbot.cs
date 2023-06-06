using Azure.AI.OpenAI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Text;

namespace Chatbot;

internal class VoiceChatbot
{
    public static async Task Chat(OpenAIClient openAiClient)
    {
        Console.WriteLine("Hello, World! Ask a question. Say quit to exit.");

        var speechConfig = SpeechConfig.FromSubscription("YOUR_SPEECH_SERVICE_KEY", "eastus");
        // To find more supported voices, see https://learn.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support?WT.mc_id=DT-MVP-5001643
        //speechConfig.SpeechSynthesisVoiceName = "zh-CN-XiaomengNeural";
        speechConfig.SpeechSynthesisVoiceName = "en-US-JaneNeural";

        speechConfig.SpeechRecognitionLanguage = "en-US";
        speechConfig.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "3000");
        speechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "1000");
        using var speechSynthesizer = new SpeechSynthesizer(speechConfig);
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        Queue<ChatMessage> chatMessages = new();
        var systemMessage = new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information.");

        var taskExecutor = new TaskExecutor();
        // Start the task execution loop in a separate thread
        _ = Task.Run(taskExecutor.StartTaskExecution);
        while (true)
        {
            Console.WriteLine("User: ");
            string userMessageString = string.Empty;
            var userInput = string.Empty;
            while (string.IsNullOrWhiteSpace(userMessageString))
            {
                userInput = Console.ReadLine();

                if (userInput?.Trim().ToLower() == "r")
                {
                    userMessageString = await RecognizeSpeechAsync(speechRecognizer);
                }
                else
                {
                    userMessageString = userInput;
                }
            }

            if (userInput?.Trim().ToLower() == "quit" || userInput?.Trim().ToLower() == "q")
            {
                Console.WriteLine("Goodbye!");
                taskExecutor.StopTaskExecution();
                break;
            }

            var userMessage = new ChatMessage(ChatRole.User, userMessageString);
            chatMessages.Enqueue(userMessage);
            while (chatMessages.Count > 20)
            {
                chatMessages.Dequeue();
            }
            var options = new ChatCompletionsOptions()
            {
                Temperature = (float)0.7,
                MaxTokens = 300,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            };
            options.Messages.Add(systemMessage);
            foreach (var message in chatMessages)
            {
                options.Messages.Add(message);
            }


            var response = await openAiClient.GetChatCompletionsStreamingAsync("gpt-35-turbo", options);
            using var streamingChatCompletions = response.Value;
            var assistantMessageStringBuilder = new StringBuilder();
            Console.WriteLine("Assistant: ");



            await foreach (var streamingChatChoice in streamingChatCompletions.GetChoicesStreaming())
            {
                var streamingMessages = streamingChatChoice.GetMessageStreaming();
                await foreach (var sentence in SentencesHelper.GetSentences(streamingMessages))
                {
                    assistantMessageStringBuilder.Append(sentence);
                    Console.Write(sentence);
                    // Add the task to the task executor
                    taskExecutor.AddTask(async () => await speechSynthesizer.SpeakTextAsync(sentence));
                    //_ = Task.Run(async () =>
                    //{
                    //    _ = await speechSynthesizer.SpeakTextAsync(sentence);
                    //});
                    //await speechSynthesizer.SpeakTextAsync(sentence);
                }
            }

            Console.WriteLine();
            var assistantMessage = new ChatMessage(ChatRole.Assistant, assistantMessageStringBuilder.ToString());
            chatMessages.Enqueue(assistantMessage);

        }
    }

    private static async Task<string> RecognizeSpeechAsync(SpeechRecognizer recognizer)
    {
        Console.WriteLine("Say something...");
        var message = string.Empty;
        while (message.Length == 0)
        {
            var result = await recognizer.RecognizeOnceAsync();
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                message = result.Text;
            }
            OutputSpeechRecognitionResult(result);
        }
        return message;
    }

    private static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
    {
        switch (speechRecognitionResult.Reason)
        {
            case ResultReason.RecognizedSpeech:
                Console.WriteLine($"{speechRecognitionResult.Text}");
                break;
            case ResultReason.NoMatch:
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                break;
            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                }
                break;
        }
    }
}
