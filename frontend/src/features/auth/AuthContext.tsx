import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState
} from "react";

export type AuthMode = "unauthenticated" | "guest" | "authenticated";

export type AuthUser = {
  id: string;
  email: string;
};

export type AuthState = {
  mode: AuthMode;
  user: AuthUser | null;
};

type AuthContextValue = AuthState & {
  setMode: (mode: AuthMode) => void;
  setUser: (user: AuthUser | null) => void;
  signOut: () => void;
};

const AUTH_MODE_KEY = "quitly.auth-mode";
const AUTH_USER_KEY = "quitly.auth-user";

function readPersistedMode(): AuthMode {
  try {
    const value = localStorage.getItem(AUTH_MODE_KEY);
    if (value === "guest" || value === "authenticated") return value;
  } catch {
    // localStorage unavailable — default to unauthenticated
  }
  return "unauthenticated";
}

function readPersistedUser(): AuthUser | null {
  try {
    const raw = localStorage.getItem(AUTH_USER_KEY);
    if (raw) return JSON.parse(raw) as AuthUser;
  } catch {
    // ignore
  }
  return null;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [mode, setModeState] = useState<AuthMode>(readPersistedMode);
  const [user, setUserState] = useState<AuthUser | null>(readPersistedUser);
  const navigateRef = useRef<((path: string) => void) | null>(null);

  const setMode = useCallback((next: AuthMode) => {
    setModeState(next);
    try {
      if (next === "unauthenticated") {
        localStorage.removeItem(AUTH_MODE_KEY);
      } else {
        localStorage.setItem(AUTH_MODE_KEY, next);
      }
    } catch {
      // ignore
    }
  }, []);

  const setUser = useCallback((next: AuthUser | null) => {
    setUserState(next);
    try {
      if (next === null) {
        localStorage.removeItem(AUTH_USER_KEY);
      } else {
        localStorage.setItem(AUTH_USER_KEY, JSON.stringify(next));
      }
    } catch {
      // ignore
    }
  }, []);

  const signOut = useCallback(() => {
    setMode("unauthenticated");
    setUser(null);
  }, [setMode, setUser]);

  // Listen for session-expired event emitted by httpClient interceptor (T014)
  useEffect(() => {
    const handler = () => {
      signOut();
      navigateRef.current?.("/welcome");
    };
    window.addEventListener("auth:session-expired", handler);
    return () => window.removeEventListener("auth:session-expired", handler);
  }, [signOut]);

  return (
    <AuthContext.Provider value={{ mode, user, setMode, setUser, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}

/** Exposed so App.tsx can wire React Router's navigate() into the context. */
export function useRegisterNavigate(navigate: (path: string) => void) {
  // This is a lightweight way to inject navigate without requiring router context inside AuthProvider
  useEffect(() => {
    const handler = (event: Event) => {
      const path = (event as CustomEvent<string>).detail ?? "/welcome";
      navigate(path);
    };
    window.addEventListener("auth:navigate", handler);
    return () => window.removeEventListener("auth:navigate", handler);
  }, [navigate]);
}
