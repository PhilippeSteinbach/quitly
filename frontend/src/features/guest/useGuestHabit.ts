import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getGuestHabit, saveGuestHabit } from "@/features/guest/guestStorage";
import type { GuestHabit } from "@/features/guest/guestStorage";

const QUERY_KEY = ["guest", "habit"] as const;

/** Mirrors the shape of useActiveHabitQuery from onboarding.api.ts */
export function useGuestHabitQuery() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getGuestHabit,
    staleTime: Infinity
  });
}

/** Mirrors the shape of useUpsertHabitMutation from onboarding.api.ts */
export function useUpsertGuestHabitMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (habit: GuestHabit) => {
      saveGuestHabit(habit);
      return Promise.resolve(habit);
    },
    onSuccess() {
      void queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    }
  });
}
