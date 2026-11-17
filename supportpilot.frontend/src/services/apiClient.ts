
const API_URL = "http://localhost:5050/api";

// Devam eden refresh işlemini tutar.
let refreshPromise: Promise<boolean> | null = null;

async function refreshAccessToken(): Promise<boolean> {
  // Başka bir istek refresh başlattıysa
  // yeni refresh isteği göndermek yerine onu bekle.
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
      // İşlem bittikten sonra yeni refresh işlemlerine izin ver.
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

export async function apiRequest(
  endpoint: string,
  options: RequestInit = {}
): Promise<Response> {
  // İlk API isteği
  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    credentials: "include",
  });

  // 401 dışında bir cevap geldiyse doğrudan döndür.
  if (response.status !== 401) {
    return response;
  }

  // Refresh işlemini başlat veya devam eden işlemi bekle.
  const refreshSucceeded = await refreshAccessToken();

  // Refresh başarısızsa ilk 401 cevabını döndür.
  if (!refreshSucceeded) {
    return response;
  }

  // Refresh başarılıysa orijinal isteği tekrar gönder.
  return fetch(`${API_URL}${endpoint}`, {
    ...options,
    credentials: "include",
  });
}

