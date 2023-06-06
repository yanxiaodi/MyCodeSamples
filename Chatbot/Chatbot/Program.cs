// See https://aka.ms/new-console-template for more information

using Azure;
using Azure.AI.OpenAI;
using Chatbot;
using System.Text;

Console.InputEncoding = Encoding.Unicode;
Console.OutputEncoding = Encoding.Unicode;


var openAiClient = new OpenAIClient(new Uri("YOUR_API_ENDPOINT"), new AzureKeyCredential("YOUR_API_KEY"));

Console.Write(@"Do you want to enable voice input/output? y for yes, n for no.");
var enableVoice = false;
var enableVoiceInputOutput = Console.ReadLine();
if (enableVoiceInputOutput?.Trim().ToLower() == "y")
{
    Console.WriteLine("Voice input/output enabled. Press r to talk or type your message.");
    enableVoice = true;
}
else
{
    Console.WriteLine("Voice input/output disabled.");
}

if (enableVoice)
{
    await VoiceChatbot.Chat(openAiClient);
}
else
{
    await TextChatbot.Chat(openAiClient);
}