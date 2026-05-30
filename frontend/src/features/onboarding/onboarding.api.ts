import { useMutation, useQuery } from "@tanstack/react-query";
import { httpClient } from "@/services/httpClient";

export type HabitMode = "reduce" | "quit";
export type HabitCategory =
  | "smoking"
  | "social_media"
  | "sugar"
  | "impulse_buying"
  | "custom";

export type HabitPayload = {
  category: HabitCategory;
  mode: HabitMode;
  title: string;
  startedOn?: string;
};

export type HabitDto = {
  id: string;
  category: HabitCategory;
  mode: HabitMode;
  title: string;
  active: boolean;
  startedOn: string;
};

async function fetchActiveHabit() {
  const response = await httpClient.get<HabitDto>("/habit");
  return response.data;
}

async function upsertHabit(payload: HabitPayload) {
  const response = await httpClient.put<HabitDto>("/habit", payload);
  return response.data;
}

export function useActiveHabitQuery() {
  return useQuery({
    queryKey: ["habit", "active"],
    queryFn: fetchActiveHabit,
    retry: false
  });
}

export function useUpsertHabitMutation() {
  return useMutation({
    mutationFn: upsertHabit
  });
}
