import { useMutation, useQuery } from "@tanstack/react-query";
import { httpClient } from "@/services/httpClient";

export type CheckInStatus = "abstinent" | "non_abstinent" | "unsure";
export type MoodValue = "very_low" | "low" | "neutral" | "good" | "very_good";

export type CheckInDto = {
  id: string;
  day: string;
  status: CheckInStatus;
  mood?: MoodValue | null;
  triggers: string[];
  note?: string | null;
  createdAt: string;
};

export type StreakDto = {
  currentStreakDays: number;
  lastAbstinentDay?: string | null;
  lastNonAbstinentDay?: string | null;
};

export type CheckInPayload = {
  day: string;
  status: CheckInStatus;
  mood?: MoodValue;
  triggers: string[];
  note?: string;
};

async function fetchStreak() {
  const response = await httpClient.get<StreakDto>("/streak");
  return response.data;
}

async function upsertCheckIn(payload: CheckInPayload) {
  const response = await httpClient.post<CheckInDto>("/check-ins", payload);
  return response.data;
}

export function useStreakQuery() {
  return useQuery({
    queryKey: ["streak"],
    queryFn: fetchStreak,
    retry: false
  });
}

export function useCheckInMutation() {
  return useMutation({
    mutationFn: upsertCheckIn
  });
}
