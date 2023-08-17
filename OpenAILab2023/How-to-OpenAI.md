# Example 1
1. Nuget:
```
Install-Package Azure.AI.OpenAI -Version 1.0.0-beta.6
```
2. C# code
```csharp
using Azure;
using Azure.AI.OpenAI;

var openAiClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
var userMessage = new ChatMessage(ChatRole.User, "Who is Bill Gates?");
var options = new ChatCompletionsOptions();
options.Messages.Add(userMessage);
var response = await openAiClient.GetChatCompletionsAsync("gpt-35-turbo", options);
var chatCompletions = response.Value;
Console.WriteLine(chatCompletions.Choices.First().Message.Content);
```

# Example 2
```csharp
var systemMsg = new ChatMessage(ChatRole.System, "You are an English teacher that helps student to understand the roots of the English word.");
while(true)
{
    string word = Console.ReadLine();
    var options = new ChatCompletionsOptions();
    options.Messages.Add(systemMsg);
    options.Messages.Add(new ChatMessage(ChatRole.User, word));
    var response = await openAiClient.GetChatCompletionsAsync("gpt-35-turbo", options);
    var chatCompletions = response.Value;
    Console.WriteLine(chatCompletions.Choices.First().Message.Content);
}
```

# Example 3
structured output:
```
You are an intelligent English sentence parser, and you will parse the word class of each word in a sentence. The ouput is in XML format. For example, if user inputs 'I am a boy', the output is '<items><item><word>I</word><type>pronoun</type></item><item><word>am</word><type>verb</type></item><item><word>a</word><type>article</type></item><item><word>boy</word><type>noun</type></item></items>'. The output is XML only.
```
# Example 4
```csharp
var options = new ChatCompletionsOptions();
while (true)
{
    Console.Write("User:");
    string userMsg = Console.ReadLine();
    options.Messages.Add(new ChatMessage(ChatRole.User,userMsg));
    var response = await openAiClient.GetChatCompletionsAsync("gpt-35-turbo", options);
    var chatCompletions = response.Value;
    Console.WriteLine("AI:"+chatCompletions.Choices.First().Message.Content);
}
```

# Example 5
```csharp
var response = await openAiClient.GetChatCompletionsStreamingAsync("gpt-35-turbo", options);
using var streamingChatCompletions = response.Value;
await foreach (var choice in streamingChatCompletions.GetChoicesStreaming())
{
	await foreach (var item in choice.GetMessageStreaming())
	{
		Console.Write(item.Content);
	}
}
```
