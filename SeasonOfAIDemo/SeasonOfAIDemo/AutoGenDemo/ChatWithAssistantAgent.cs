using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;

namespace AutoGenDemo;
internal class ChatWithAssistantAgent
{
    public static async Task Run(OpenAiOptions openAiOptions)
    {
        // create a teacher agent
        // teacher agent will create math questions
        var llmConfig = new AzureOpenAIConfig(openAiOptions.Endpoint, openAiOptions.Model, openAiOptions.Key);
        var teacher = new AssistantAgent(
                name: "teacher",
                systemMessage: """
                                You are a teacher that create primary-school math question for student and check answer.
                                If the answer is correct, you stop the conversation by saying [COMPLETE].
                                If the answer is wrong, you ask student to fix it.
                               """,
                llmConfig: new ConversableAgentConfig
                {
                    Temperature = 0.5f,
                    ConfigList = [llmConfig],
                })
            .RegisterMiddleware(async (msgs, option, agent, _) =>
            {
                var reply = await agent.GenerateReplyAsync(msgs, option, _);
                if (reply.GetContent()?.ToLower().Contains("complete") is true)
                {
                    return new TextMessage(Role.Assistant, GroupChatExtension.TERMINATE, from: reply.From);
                }

                return reply;
            })
            .RegisterPrintMessage();

        // create a student agent
        // student agent will answer the math questions
        var student = new AssistantAgent(
                name: "student",
                systemMessage: "You are a student that answer question from teacher",
                llmConfig: new ConversableAgentConfig
                {
                    Temperature = 0.5f,
                    ConfigList = [llmConfig],
                })
            .RegisterPrintMessage();

        // start the conversation
        var conversation = await student.InitiateChatAsync(
            receiver: teacher,
            message: "Hey teacher, please create math question for me.",
            maxRound: 10);

    }
}
