/**
 * Streak API hooks — US1, US2, US3 (Feature 008).
 * Uses monotonic clock snapshot to display a manipulation-resistant second counter.
 * Manipulation detection (T035): client-side, compares serverUtcMs vs monotonic estimate.
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useEffect, useRef } from 'react';
import httpClient from '@/services/httpClient';
import { saveSnapshot, loadSnapshot } from '@/lib/streak-calc/monotonic-store';
import { detectManipulation } from '@/lib/streak-calc';

export interface StreakDto {
  habitId: string;
  currentStreakSeconds: number;
  serverUtcMs: number;
}

async function fetchStreak(habitId: string): Promise<StreakDto> {
  const { data } = await httpClient.get<StreakDto>(`/habits/${habitId}/streak`);
  return data;
}

/**
 * useStreak — fetches streak seconds and anchors a monotonic snapshot.
 *
 * On each successful fetch:
 * 1. Saves serverUtcMs snapshot for manipulation detection.
 * 2. Compares new serverUtcMs against the monotonic estimate derived from
 *    the previous snapshot. If the device clock was set back by > 5 min,
 *    calls onManipulationDetected (which should show a non-shaming toast).
 *
 * Visibility change listener re-fetches when the tab becomes visible again
 * so the streak counter stays current after device sleep.
 */
export function useStreak(
  habitId: string | undefined,
  options?: {
    onManipulationDetected?: () => void;
  }
) {
  const onManipulationDetectedRef = useRef(options?.onManipulationDetected);
  onManipulationDetectedRef.current = options?.onManipulationDetected;

  const query = useQuery({
    queryKey: ['streak', habitId],
    queryFn: () => fetchStreak(habitId!),
    enabled: !!habitId,
    staleTime: 30_000,
    refetchOnWindowFocus: true,
  });

  // Manipulation detection on each new server response
  useEffect(() => {
    if (!query.data) return;

    const prev = loadSnapshot();
    if (prev) {
      const elapsed = performance.now() - prev.performanceNowMs;
      const offlineDeltaMs = elapsed; // how much monotonic time passed
      const serverDeltaMs = query.data.serverUtcMs - prev.serverUtcMs;

      if (detectManipulation(offlineDeltaMs, serverDeltaMs)) {
        onManipulationDetectedRef.current?.();
      }
    }

    saveSnapshot(query.data.serverUtcMs);
  }, [query.data]);

  // Visibility change — refetch when tab becomes active again
  const refetch = query.refetch;
  useEffect(() => {
    const handleVisibility = () => {
      if (document.visibilityState === 'visible') {
        void refetch();
      }
    };

    document.addEventListener('visibilitychange', handleVisibility);
    return () => document.removeEventListener('visibilitychange', handleVisibility);
  }, [refetch]);

  return query;
}

// ── US2: useRecordRelapse ────────────────────────────────────────────────────

export interface RecordRelapseInput {
  occurredAt: string; // ISO 8601 with offset
  contextNote?: string;
}

export interface RelapseCreatedDto {
  id: string;
  occurredAt: string;
  previousStreakSeconds: number;
}

export function useRecordRelapse(habitId: string | undefined) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: RecordRelapseInput) => {
      const { data } = await httpClient.post<RelapseCreatedDto>(
        `/habits/${habitId}/relapses`,
        input
      );
      return data;
    },
    onSuccess: () => {
      // Invalidate streak + stats so they refresh after a relapse
      void queryClient.invalidateQueries({ queryKey: ['streak', habitId] });
      void queryClient.invalidateQueries({ queryKey: ['monthStats', habitId] });
      void queryClient.invalidateQueries({ queryKey: ['yearStats', habitId] });
    },
  });
}

// ── US2: useMonthStats / useYearStats ─────────────────────────────────────────

export interface MonthStatsDto {
  year: number;
  month: number;
  abstinentDays: number;
  relevantDays: number;
  relapseCount: number;
  isCurrentMonth: boolean;
}

export interface YearStatsDto {
  year: number;
  totalAbstinentDays: number;
  totalRelevantDays: number;
  months: MonthStatsDto[];
}

export function useMonthStats(habitId: string | undefined, year: number, month: number) {
  return useQuery({
    queryKey: ['monthStats', habitId, year, month],
    queryFn: async () => {
      const { data } = await httpClient.get<MonthStatsDto>(
        `/habits/${habitId}/stats/${year}/${month}`
      );
      return data;
    },
    enabled: !!habitId,
    staleTime: 60_000,
  });
}

export function useYearStats(habitId: string | undefined, year: number) {
  return useQuery({
    queryKey: ['yearStats', habitId, year],
    queryFn: async () => {
      const { data } = await httpClient.get<YearStatsDto>(
        `/habits/${habitId}/stats/${year}`
      );
      return data;
    },
    enabled: !!habitId,
    staleTime: 60_000,
  });
}

// ── US3: useCalendarMonth ────────────────────────────────────────────────────

export interface CalendarDayDto {
  date: string; // YYYY-MM-DD
  status: 'abstinent' | 'relapse' | 'paused' | 'neutral';
  notes?: string[];
}

export interface CalendarMonthDto {
  year: number;
  month: number;
  days: CalendarDayDto[];
}

export function useCalendarMonth(habitId: string | undefined, year: number, month: number) {
  return useQuery({
    queryKey: ['calendarMonth', habitId, year, month],
    queryFn: async () => {
      const { data } = await httpClient.get<CalendarMonthDto>(
        `/habits/${habitId}/calendar/${year}/${month}`
      );
      return data;
    },
    enabled: !!habitId,
    staleTime: 60_000,
  });
}
