import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getGuestCheckIns, upsertGuestCheckIn } from "@/features/guest/guestStorage";
import type { GuestCheckIn } from "@/features/guest/guestStorage";

const QUERY_KEY = ["guest", "checkins"] as const;

/** Mirrors the shape of the checkin.api.ts streak/checkin query */
export function useGuestCheckInsQuery() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: getGuestCheckIns,
    staleTime: Infinity
  });
}

/** Mirrors useCheckInMutation from checkin.api.ts */
export function useGuestCheckInMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (checkIn: GuestCheckIn) => {
      upsertGuestCheckIn(checkIn);
      return Promise.resolve(checkIn);
    },
    onSuccess() {
      void queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      void queryClient.invalidateQueries({ queryKey: ["guest", "streak"] });
    }
  });
}
