import { useMutation, useQuery } from "@tanstack/react-query";
import { httpClient } from "@/services/httpClient";
import { tokenStorage } from "@/services/httpClient";

export type AuthResponse = {
  accessToken: string;
  refreshToken: string;
};

export type UserProfile = {
  id: string;
  email: string;
};

// --- API functions ---

async function login(payload: { email: string; password: string }): Promise<AuthResponse> {
  const { data } = await httpClient.post<AuthResponse>("/auth/login", payload);
  return data;
}

async function register(payload: {
  email: string;
  password: string;
  timezone: string;
}): Promise<AuthResponse> {
  const { data } = await httpClient.post<AuthResponse>("/auth/register", payload);
  return data;
}

async function refresh(refreshToken: string): Promise<AuthResponse> {
  const { data } = await httpClient.post<AuthResponse>("/auth/refresh", { refreshToken });
  return data;
}

async function logout(refreshToken: string): Promise<void> {
  await httpClient.delete("/auth/session", { data: { refreshToken } });
}

async function getCurrentUser(): Promise<UserProfile> {
  const { data } = await httpClient.get<UserProfile>("/auth/me");
  return data;
}

// --- TanStack Query hooks ---

export function useLoginMutation() {
  return useMutation({
    mutationFn: login,
    onSuccess(data) {
      tokenStorage.setTokens(data.accessToken, data.refreshToken);
    }
  });
}

export function useRegisterMutation() {
  return useMutation({
    mutationFn: register,
    onSuccess(data) {
      tokenStorage.setTokens(data.accessToken, data.refreshToken);
    }
  });
}

export function useRefreshMutation() {
  return useMutation({
    mutationFn: refresh,
    onSuccess(data) {
      tokenStorage.setTokens(data.accessToken, data.refreshToken);
    }
  });
}

export function useLogoutMutation() {
  return useMutation({
    mutationFn: () => {
      const refreshToken = tokenStorage.getRefreshToken() ?? "";
      return logout(refreshToken);
    }
  });
}

export function useCurrentUserQuery(enabled = true) {
  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: getCurrentUser,
    enabled,
    retry: false,
    staleTime: 5 * 60 * 1000
  });
}
