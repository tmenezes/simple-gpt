# SimpleGPT.Core
Simplistic library for problem/context evaluation using OpenAI's GPT LLM

## Usage
```csharp
using SimpleGPT.Core;
// ...
const string GPT_API_KEY = "< api-key-here >";
var gptClient = new GptClient(GPT_API_KEY);

var knowledgeBase = "<the specific knowledge base for your problem>";
var context = "<current context or state of the scenario you want to make question onto>";
var question = "<the question based on konwledge/context>";

var evaluator = new GptEvaluator(gptClient, knowledgeBase, context);
var result = await evaluator.Evaluate(question);
```

## Sample
![Sample](res/simple-gpt-sample.jpg)