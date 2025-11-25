import { ActivityTypes } from "@microsoft/agents-activity";
import {
  AgentApplicationBuilder,
  MessageFactory,
  TurnContext,
} from "@microsoft/agents-hosting";
import { SemanticKernelAgent, Message } from "./agents/semanticKernelAgent";
import { dateToolAdapter } from "./tools/adapters/dateTimeToolAdapters";
import { getWeatherToolAdapter } from "./tools/adapters/weatherToolAdapter";

export const weatherAgent = new AgentApplicationBuilder().build();

weatherAgent.conversationUpdate(
  "membersAdded",
  async (context: TurnContext) => {
    await context.sendActivity(
      `Hello and Welcome! I'm here to help with all your weather forecast needs!`
    );
  }
);

interface WeatherForecastAgentResponse {
  contentType: "Text" | "AdaptiveCard";
  content: string;
}

// Initialize the Semantic Kernel agent with your dev tunnel URL
const semanticKernelBaseUrl = process.env.SEMANTIC_KERNEL_BASE_URL || 'https://your-devtunnel-url.devtunnels.ms';
const agent = new SemanticKernelAgent(semanticKernelBaseUrl);

// Add tools to the agent
agent.addTool(getWeatherToolAdapter);
agent.addTool(dateToolAdapter);

const sysMessage: Message = {
  role: 'system',
  content: `
You are a friendly assistant that helps people find a weather forecast for a given time and place.
You may ask follow up questions until you have enough information to answer the customers question,
but once you have a forecast forecast, make sure to format it nicely using an adaptive card.

Respond in JSON format with the following JSON schema, and do not use markdown in the response:

{
    "contentType": "'Text' or 'AdaptiveCard' only",
    "content": "{The content of the response, may be plain text, or JSON based adaptive card}"
}`
};

weatherAgent.activity(ActivityTypes.Message, async (context, state) => {
  const userMessage: Message = {
    role: 'user',
    content: context.activity.text!
  };

  const llmResponse = await agent.invoke(
    [sysMessage, userMessage],
    {
      configurable: { thread_id: context.activity.conversation!.id },
    }
  );

  // Get the last assistant message
  const lastMessage = llmResponse.messages[llmResponse.messages.length - 1];
  
  if (lastMessage && lastMessage.role === 'assistant') {
    try {
      const llmResponseContent: WeatherForecastAgentResponse = JSON.parse(lastMessage.content);

      if (llmResponseContent.contentType === "Text") {
        await context.sendActivity(llmResponseContent.content);
      } else if (llmResponseContent.contentType === "AdaptiveCard") {
        const response = MessageFactory.attachment({
          contentType: "application/vnd.microsoft.card.adaptive",
          content: llmResponseContent.content,
        });
        await context.sendActivity(response);
      }
    } catch (error) {
      // If parsing fails, send the raw content
      await context.sendActivity(lastMessage.content);
    }
  } else {
    await context.sendActivity("I'm sorry, I couldn't process your request.");
  }
});
