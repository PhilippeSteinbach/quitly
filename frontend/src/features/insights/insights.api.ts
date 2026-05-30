// @ts-ignore Dependency resolution is restored once npm install is available in this environment.
import { useMutation, useQuery } from "@tanstack/react-query";
import { httpClient } from "@/services/httpClient";

export type PromptPayloadDto = {
  showPrompt: boolean;
  message: string;
};

export type PromptPreferenceDto = {
  passivePromptEnabled: boolean;
  promptTone: "gentle" | "neutral";
};

export type WeeklyInsightDto = {
  weekStart: string;
  checkInCount: number;
  abstinentDays: number;
  topTriggers: string[];
  moodTrend: Record<string, number>;
  summaryText: string;
  confidence: "low" | "medium" | "high";
};

async function fetchTodayPrompt() {
  const response = await httpClient.get<PromptPayloadDto>("/prompts/today");
  return response.data;
}

async function fetchWeeklyInsight() {
  const response = await httpClient.get<WeeklyInsightDto>("/insights/weekly");
  return response.data;
}

async function updatePromptPreference(payload: PromptPreferenceDto) {
  const response = await httpClient.put<PromptPreferenceDto>("/prompts/preferences", payload);
  return response.data;
}

export function useTodayPromptQuery() {
  return useQuery({ queryKey: ["prompt", "today"], queryFn: fetchTodayPrompt, retry: false });
}

export function useWeeklyInsightQuery() {
  return useQuery({ queryKey: ["insight", "weekly"], queryFn: fetchWeeklyInsight, retry: false });
}

export function useUpdatePromptPreferenceMutation() {
  return useMutation({ mutationFn: updatePromptPreference });
}
