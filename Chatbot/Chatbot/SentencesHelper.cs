using Azure.AI.OpenAI;
using System.Text;

namespace Chatbot;
internal class SentencesHelper
{
    private static readonly ISet<char> EndOfSentenceChars = new HashSet<char> { '.', '!', '?', ';', '。', '！', '？', '；', '\n', '\r' };

    public static async IAsyncEnumerable<string> GetSentences(IAsyncEnumerable<ChatMessage> chatMessages)
    {
        var ongoingSentence = new StringBuilder();

        await foreach (var chatMessage in chatMessages)
        {
            var message = chatMessage.Content;

            if (!string.IsNullOrEmpty(message))
            {
                ongoingSentence.Append(message);

                int endOfSentenceIndex = FindEndOfSentenceIndex(message);

                if (endOfSentenceIndex >= 0)
                {
                    yield return ongoingSentence.ToString();
                    ongoingSentence.Clear();
                }
            }
        }

        if (ongoingSentence.Length > 0)
        {
            yield return ongoingSentence.ToString();
        }
    }

    private static int FindEndOfSentenceIndex(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (EndOfSentenceChars.Contains(text[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
