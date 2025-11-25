import { Tool } from '../../agents/semanticKernelAgent';

export const dateToolAdapter: Tool = {
  name: "Date",
  description: "Get the current date",
  parameters: {
    type: "object",
    properties: {},
    required: [],
  },
  execute: async () => {
    const d = new Date();
    console.log("************Getting the date", d);
    return `${d.getMonth() + 1}/${d.getDate()}/${d.getFullYear()}`;
  },
};

export const todayToolAdapter: Tool = {
  name: "Today",
  description: "Get the current date",
  parameters: {
    type: "object",
    properties: {},
    required: [],
  },
  execute: async () => {
    return new Date().toDateString();
  },
};

export const nowToolAdapter: Tool = {
  name: "Now",
  description: "Get the current date and time in the local time zone",
  parameters: {
    type: "object",
    properties: {},
    required: [],
  },
  execute: async () => {
    return new Date().toLocaleDateString() + " " + new Date().toLocaleTimeString();
  },
};
