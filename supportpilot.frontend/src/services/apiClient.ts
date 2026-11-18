const API_URL = "http://localhost:5050/api";

type SessionExpiredListener = () => void;

const sessionExpiredListeners = new Set<SessionExpiredListener>();

let refreshPromise: Promise<boolean> | null = null;

// Oturum sona erdiğinde dinleyicileri bilgilendirir.
function notifySessionExpired(): void {
  sessionExpiredListeners.forEach((listener) => listener());
}

// React bileşenlerinin oturum sona ermesini dinlemesini sağlar.
export function onSessionExpired(
  listener: SessionExpiredListener
): () => void {
  sessionExpiredListeners.add(listener);

  // Listener kaldırma fonksiyonu.
  return () => {
    sessionExpiredListeners.delete(listener);
  };
}

async function refreshAccessToken(): Promise<boolean> {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    try {
      const response = await fetch(
        `${API_URL}/Authentication/refresh`,
        {
          method: "POST",
          credentials: "include",
        }
      );

      return response.ok;
    } catch {
      return false;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

export async function apiRequest(
  endpoint: string,
  options: RequestInit = {}
): Promise<Response> {
  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    credentials: "include",
  });

  if (response.status !== 401) {
    return response;
  }

  const refreshSucceeded = await refreshAccessToken();

  if (!refreshSucceeded) {
    notifySessionExpired();
    return response;
  }

  const retryResponse = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    credentials: "include",
  });

  // Yeni token ile tekrar deneme de 401 döndürürse
  // oturum sona ermiş kabul edilir.
  if (retryResponse.status === 401) {
    notifySessionExpired();
  }

  return retryResponse;
}


