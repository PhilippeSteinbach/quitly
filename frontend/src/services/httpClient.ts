import axios from "axios";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5200/api/v1";

const ACCESS_TOKEN_KEY = "quitly.access-token";
const REFRESH_TOKEN_KEY = "quitly.refresh-token";

export const tokenStorage = {
  getAccessToken() {
    return window.localStorage.getItem(ACCESS_TOKEN_KEY);
  },
  getRefreshToken() {
    return window.localStorage.getItem(REFRESH_TOKEN_KEY);
  },
  setTokens(accessToken: string, refreshToken: string) {
    window.localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    window.localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },
  clear() {
    window.localStorage.removeItem(ACCESS_TOKEN_KEY);
    window.localStorage.removeItem(REFRESH_TOKEN_KEY);
  }
};

export const httpClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json"
  }
});

httpClient.interceptors.request.use((config) => {
  const accessToken = tokenStorage.getAccessToken();

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});

let isRefreshing = false;
let pendingRetries: Array<(token: string) => void> = [];

httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as (typeof error.config) & { _retry?: boolean };

    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    const refreshToken = tokenStorage.getRefreshToken();
    if (!refreshToken) {
      tokenStorage.clear();
      window.dispatchEvent(new Event("auth:session-expired"));
      return Promise.reject(error);
    }

    if (isRefreshing) {
      // Queue the retry until the refresh finishes
      return new Promise((resolve, reject) => {
        pendingRetries.push((newToken) => {
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          originalRequest._retry = true;
          resolve(httpClient(originalRequest));
        });
      });
    }

    isRefreshing = true;
    originalRequest._retry = true;

    try {
      const { data } = await axios.post<{ accessToken: string; refreshToken: string }>(
        `${API_BASE_URL}/auth/refresh`,
        { refreshToken }
      );

      tokenStorage.setTokens(data.accessToken, data.refreshToken);

      // Unblock queued requests
      pendingRetries.forEach((cb) => cb(data.accessToken));
      pendingRetries = [];

      originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
      return httpClient(originalRequest);
    } catch {
      tokenStorage.clear();
      pendingRetries = [];
      window.dispatchEvent(new Event("auth:session-expired"));
      return Promise.reject(error);
    } finally {
      isRefreshing = false;
    }
  }
);
