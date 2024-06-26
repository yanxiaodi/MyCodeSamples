# Wellington .NET User Group - Microsoft Azure OpenAI Workshop - 2023-08-17

## Introduction

This workshop is designed to introduce you to the Microsoft Azure OpenAI platform.

## Prerequisites

* A Microsoft Azure OpenAI account - see the **Resources** section
* VS Code - Install it from [here](https://code.visualstudio.com/)
* If you prefer to use Visual Studio, you can install it from [here](https://visualstudio.microsoft.com/downloads/)

## Azure OpenAI playground

### Chat

* Use the default system message and the prompt `What is SOLID?`
* Use the system message `You are an expert in software development. You always explain complicated concepts in simple terms. You can also show some metaphor and code examples.`

### Completions

* Summarize the following text
* Generate an email response:
  `I got an email from a customer. The customer is complaining about our website because it is not working. The customer is saying that he wants to check the details of one product but he is not able to find it. Write an email response to the customer and explain to them that we are working on the issue and we will get back to them as soon as possible. Use professional language and an approachable tone.`

### DALL·E 3

* `engineer working`
* `one engineer working`
* `one software engineer working at midnight to fix bugs`
* `one software engineer working at midnight to fix bugs. Cartoon style.`
* `one software engineer with glasses working at midnight to fix bugs. Cartoon style, humorous`
* `one software engineer with glasses working at midnight to fix bugs. photo style, serious`

### Embeddings

Use different system messages:

`You are an expert of house insurance. You know the details of Kiwibank house insurance policies. You need to answer customer's questions in a professional manner and friendly tone. If they ask anything about Kiwibank's house insurance policies, you need to provide a detailed response. When you first time respond to a customer, say greetings then answer questions. If you don't know the answer, guide them to contact us.`

* `I have a house and I'm looking for advice about house insurance. Do you have any recommendations about kiwibank house insurance?`
* `What if my house is damaged by fire?`
* `What about earthquake?`

## Resources

* Resource group: `rg-welly-openai`
* Key: 45ca6eac4fa247c4a0d2472357db2457
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
  * [Lab doc](How-to-OpenAI.md)
  * [Completions](https://learn.microsoft.com/en-us/azure/ai-services/openai/quickstart?tabs=command-line&pivots=programming-language-csharp&WT.mc_id=DT-MVP-5001643)
  * [Chat](https://learn.microsoft.com/en-us/azure/ai-services/openai/chatgpt-quickstart?tabs=command-line&pivots=programming-language-csharp&WT.mc_id=DT-MVP-5001643)
  * [Chatbot with voice output](https://github.com/yanxiaodi/MyCodeSamples/tree/main/Chatbot). [A Step-by-Step Guide to Developing Intelligent Chatbots with Azure Speech Recognition Capabilities](https://medium.com/gitconnected/a-step-by-step-guide-to-developing-intelligent-chatbots-with-azure-speech-recognition-capabilities-9c6f8c494b9)
* Lab 2 - [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/chat-copilot/&WT.mc_id=DT-MVP-5001643)
  * [Sample code](/sk-demo/)
  * [Getting Started](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/loading-the-kernel?WT.mc_id=DT-MVP-5001643)
  * [Loading the kernel from files](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/running-prompts-from-files?WT.mc_id=DT-MVP-5001643)
  * [Loading the kernel from inline code](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/semantic-function-inline?WT.mc_id=DT-MVP-5001643)
  * [Setting context variables](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/context-variables-chat?WT.mc_id=DT-MVP-5001643)
  * [Using Planner](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide/using-the-planner?WT.mc_id=DT-MVP-5001643)

## Pricing

* [Azure OpenAI Service Pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/&WT.mc_id=DT-MVP-5001643)

## References

* [What is Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview&WT.mc_id=DT-MVP-5001643)
* [Completions & chat completions](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt?pivots=programming-language-chat-completions&WT.mc_id=DT-MVP-5001643)
* [What is Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/&WT.mc_id=DT-MVP-5001643)
* [Start learning how to use Semantic Kernel](https://learn.microsoft.com/semantic-kernel/get-started/quick-start-guide/?toc=%2Fsemantic-kernel%2Fget-started%2Fquick-start-guide%2Ftoc.json&WT.mc_id=DT-MVP-5001643)
* [More Semantic Kernel sample apps](https://learn.microsoft.com/en-us/semantic-kernel/samples-and-solutions/&WT.mc_id=DT-MVP-5001643)
* [Azure OpenAI samples](https://github.com/Azure/azure-openai-samples)
* [Awesome ChatGPT prompts](https://github.com/f/awesome-chatgpt-prompts)
* [The Art of ChatGPT Prompting: A Guide to Crafting Clear and Effective Prompts](https://fka.gumroad.com/l/art-of-chatgpt-prompting)
* [GPT Fundamentals, Use Cases and Sample Solutions](https://github.com/Azure/azure-openai-samples)
* [OpenAI Cookbook](https://github.com/openai/openai-cookbook)
* [semantic-kernel-starters](https://github.com/microsoft/semantic-kernel-starters)
* [Build an OpenAI-powered VS Code extension in no time](https://medium.com/gitconnected/build-an-openai-powered-vs-code-extension-in-no-time-ebaf23cea224)
