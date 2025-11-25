import { Tool } from '../../agents/semanticKernelAgent';

export const getWeatherToolAdapter: Tool = {
  name: "GetWeather",
  description: "Retrieve the weather forecast for a specific date. This is a placeholder for a real implementation and currently only returns a random temperature. This would typically call a weather service API.",
  parameters: {
    type: "object",
    properties: {
      date: {
        type: "string",
        description: "The date for which to get the weather forecast",
      },
      location: {
        type: "string",
        description: "The location for which to get the weather forecast",
      },
    },
    required: ["date", "location"],
  },
  execute: async ({ date, location }) => {
    console.log("************Getting random weather for", date, location);
    const min = -22;
    const max = 55;
    return {
      Date: date,
      TemperatureC: Math.floor(Math.random() * (max - min + 1)) + min,
    };
  },
};
