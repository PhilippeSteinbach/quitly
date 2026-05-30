import { useQuery } from "@tanstack/react-query";
import { getGuestCheckIns } from "@/features/guest/guestStorage";
import { computeGuestStreak } from "@/features/guest/guestStreak";

/** Returns the same shape as the server streak DTO (currentStreakDays / longestStreakDays). */
export function useGuestStreakQuery() {
  return useQuery({
    queryKey: ["guest", "streak"],
    queryFn: () => {
      const checkIns = getGuestCheckIns();
      const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;
      const today = new Date().toLocaleDateString("en-CA", { timeZone: timezone }); // YYYY-MM-DD
      const { currentDays, longestDays } = computeGuestStreak(checkIns, today, timezone);
      return {
        currentStreakDays: currentDays,
        longestStreakDays: longestDays
      };
    },
    staleTime: 0 // re-compute on every mount
  });
}
