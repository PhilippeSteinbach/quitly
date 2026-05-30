import { useMutation } from "@tanstack/react-query";
import { httpClient } from "@/services/httpClient";

export type RelapseDto = {
  id: string;
  occurredAt: string;
  contextNote?: string | null;
  createdAt: string;
};

export type RecoveryStepDto = {
  id: string;
  relapseId: string;
  stepText: string;
  dueWithinHours: number;
  completedAt?: string | null;
  createdAt: string;
};

export async function createRelapse(contextNote?: string) {
  const response = await httpClient.post<RelapseDto>("/relapse", {
    occurredAt: new Date().toISOString(),
    contextNote
  });

  return response.data;
}

export async function upsertRecoveryStep(payload: { relapseId: string; stepText: string; completed?: boolean }) {
  const response = await httpClient.post<RecoveryStepDto>("/recovery-steps", {
    relapseId: payload.relapseId,
    stepText: payload.stepText,
    completed: payload.completed ?? false
  });

  return response.data;
}

export function useCreateRelapseMutation() {
  return useMutation({ mutationFn: createRelapse });
}

export function useUpsertRecoveryStepMutation() {
  return useMutation({ mutationFn: upsertRecoveryStep });
}
