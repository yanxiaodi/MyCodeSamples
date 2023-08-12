# Wellington .NET User Group - Microsoft Azure OpenAI Workshop - 2023-08-17

## Introduction

This workshop is designed to introduce you to explore the Microsoft Azure OpenAI platform.

## Prerequisites

* A Microsoft Azure OpenAI account - see the **Resources** section
* VS Code - Install it from [here](https://code.visualstudio.com/)
* If you prefer to use Visual Studio, you can install it from [here](https://visualstudio.microsoft.com/downloads/)

## Resources

* Resource group: `rg-welly-openai`
* Key: Will send to attendees during the session
* Location: `eastus`
* Endpoint: `https://welly-openai-lab.openai.azure.com/`
* Deployment name:
  * gpt-35-turbo
  * gpt-35-turbo-16k
  * gpt-4
  * gpt-4-32k
  * text-davinci-003

## Limitations of the free tier

* gpt-35-turbo
  * 120K tokens per minute quote available.
  * 720 corresponding requests per minute.
* gpt-35-turbo-16k
  * 120K tokens per minute quote available.
  * 720 corresponding requests per minute.
* gpt-4
  * 10K tokens per minute quote available.
  * 60 corresponding requests per minute.
* gpt-4-32k
  * 30K tokens per minute quote available.
  * 180 corresponding requests per minute.
* text-davinci-003
  * 60K tokens per minute quote available.
  * 360 corresponding requests per minute.

## Workshop

* Presentation (30 minutes)
* Lab 1 - [Completions using CLI](https://learn.microsoft.com/en-us/azure/ai-services/openai/quickstart?tabs=command-line&pivots=rest-api)
* Lab 2 - [Chatbot using C#](https://github.com/yanxiaodi/MyCodeSamples/tree/main/Chatbot)
* Lab 3- [Chat Copilot with Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/chat-copilot/)

## Pricing

* [Azure OpenAI Service Pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/)

## References

* [What is Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview)
* [Completions & chat completions](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt?pivots=programming-language-chat-completions)
* [What is Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/)
* [More Semantic Kernel sample apps](https://learn.microsoft.com/en-us/semantic-kernel/samples-and-solutions/)