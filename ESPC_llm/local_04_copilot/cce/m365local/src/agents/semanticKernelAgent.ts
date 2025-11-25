import { SemanticKernelService, SemanticKernelRequest } from '../services/semanticKernelService';

export interface Tool {
  name: string;
  description: string;
  parameters: any;
  execute: (args: any) => Promise<any>;
}

export interface Message {
  role: 'system' | 'user' | 'assistant';
  content: string;
}

export class SemanticKernelAgent {
  private semanticKernel: SemanticKernelService;
  private tools: Map<string, Tool> = new Map();
  private conversationHistory: Map<string, Message[]> = new Map();

  constructor(baseUrl: string) {
    this.semanticKernel = new SemanticKernelService(baseUrl);
  }

  addTool(tool: Tool) {
    this.tools.set(tool.name, tool);
  }

  private convertToolsToOpenAIFormat() {
    return Array.from(this.tools.values()).map(tool => ({
      type: 'function',
      function: {
        name: tool.name,
        description: tool.description,
        parameters: tool.parameters,
      },
    }));
  }

  async invoke(
    messages: Message[],
    config: { configurable: { thread_id: string } }
  ): Promise<{ messages: Message[] }> {
    const threadId = config.configurable.thread_id;
    
    // Get or initialize conversation history
    let conversationHistory = this.conversationHistory.get(threadId) || [];
    
    // Add new messages to history
    conversationHistory.push(...messages);
    
    // Prepare request for Semantic Kernel
    const request: SemanticKernelRequest = {
      messages: conversationHistory.map(msg => ({
        role: msg.role,
        content: msg.content,
      })),
      tools: this.tools.size > 0 ? this.convertToolsToOpenAIFormat() : undefined,
      temperature: 0,
    };

    let maxIterations = 5;
    let currentIteration = 0;

    while (currentIteration < maxIterations) {
      const response = await this.semanticKernel.chatCompletion(request);
      const choice = response.choices[0];
      
      if (!choice) {
        throw new Error('No response from Semantic Kernel');
      }

      const assistantMessage: Message = {
        role: 'assistant',
        content: choice.message.content,
      };

      conversationHistory.push(assistantMessage);

      // Check if the model wants to call tools
      if (choice.message.tool_calls && choice.message.tool_calls.length > 0) {
        for (const toolCall of choice.message.tool_calls) {
          const tool = this.tools.get(toolCall.function.name);
          if (tool) {
            try {
              const args = JSON.parse(toolCall.function.arguments);
              const toolResult = await tool.execute(args);
              
              // Add tool result to conversation
              const toolMessage: Message = {
                role: 'assistant',
                content: `Tool ${toolCall.function.name} result: ${JSON.stringify(toolResult)}`,
              };
              conversationHistory.push(toolMessage);
              
              // Update request for next iteration
              request.messages = conversationHistory.map(msg => ({
                role: msg.role,
                content: msg.content,
              }));
            } catch (error) {
              console.error(`Error executing tool ${toolCall.function.name}:`, error);
              const errorMessage: Message = {
                role: 'assistant',
                content: `Error executing tool ${toolCall.function.name}: ${error}`,
              };
              conversationHistory.push(errorMessage);
            }
          }
        }
        currentIteration++;
        continue;
      }

      // No more tool calls, we're done
      break;
    }

    // Update conversation history
    this.conversationHistory.set(threadId, conversationHistory);

    return { messages: conversationHistory };
  }
}
