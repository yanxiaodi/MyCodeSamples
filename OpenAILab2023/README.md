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
* Lab 1
  * [Completions](https://learn.microsoft.com/en-us/azure/ai-services/openai/quickstart?tabs=command-line&pivots=programming-language-csharp&WT.mc_id=DT-MVP-5001643)
  * [Chat](https://learn.microsoft.com/en-us/azure/ai-services/openai/chatgpt-quickstart?tabs=command-line&pivots=programming-language-csharp&WT.mc_id=DT-MVP-5001643)
  * [Chatbot with voice output](https://github.com/yanxiaodi/MyCodeSamples/tree/main/Chatbot)
* Lab 2 - [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/chat-copilot/&WT.mc_id=DT-MVP-5001643)
  * [Getting Started](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/loading-the-kernel?WT.mc_id=DT-MVP-5001643)
  * [Loading the kernel from files](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/running-prompts-from-files?WT.mc_id=DT-MVP-5001643)
  * [Loading the kernel from inline code](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/semantic-function-inline?WT.mc_id=DT-MVP-5001643)
  * [Setting context variables](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/context-variables-chat?WT.mc_id=DT-MVP-5001643)
  * [Chaining functions together](https://learn.microsoft.com/en-us/semantic-kernel/ai-orchestration/chaining-functions?WT.mc_id=DT-MVP-5001643)
  * [Using Planner](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/using-the-planner?WT.mc_id=DT-MVP-5001643)

## Pricing

* [Azure OpenAI Service Pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/&WT.mc_id=DT-MVP-5001643)

## References

* [What is Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview&WT.mc_id=DT-MVP-5001643)
* [Completions & chat completions](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt?pivots=programming-language-chat-completions&WT.mc_id=DT-MVP-5001643)
* [What is Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/&WT.mc_id=DT-MVP-5001643)
* [Start learning how to use Semantic Kernel](https://learn.microsoft.com/semantic-kernel/get-started/quick-start-guide/?toc=%2Fsemantic-kernel%2Fget-started%2Fquick-start-guide%2Ftoc.json&WT.mc_id=DT-MVP-5001643)
* [More Semantic Kernel sample apps](https://learn.microsoft.com/en-us/semantic-kernel/samples-and-solutions/&WT.mc_id=DT-MVP-5001643)
* [Azure ChatGPT](https://github.com/microsoft/azurechatgpt)
* [Awesome ChatGPT prompts](https://github.com/f/awesome-chatgpt-prompts)
* [The Art of ChatGPT Prompting: A Guide to Crafting Clear and Effective Prompts](https://fka.gumroad.com/l/art-of-chatgpt-prompting)
* [GPT Fundamentals, Use Cases and Sample Solutions](https://github.com/Azure/azure-openai-samples)
* [OpenAI Cookbook](https://github.com/openai/openai-cookbook)
* [semantic-kernel-starters](https://github.com/microsoft/semantic-kernel-starters)
