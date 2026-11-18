import { apiRequest } from "./apiClient";

const API_URL = "http://localhost:5050/api";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: number;
  message: string;
}

export interface ProfileResponse {
  userId: string;
  email: string;
  role: string;
  message: string;
}

export async function login(
  request: LoginRequest
): Promise<LoginResponse> {
  const response = await fetch(`${API_URL}/Authentication/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(request),
  });

  const data = await response.json();

  if (!response.ok) {
    throw new Error(data.error ?? "Login failed.");
  }

  return data as LoginResponse;
}

export async function getMyProfile(): Promise<ProfileResponse> {
  const response = await apiRequest("/Profile/me", {
    method: "GET",
  });

  if (response.status === 401) {
    throw new Error(
      "Your session has expired. Please sign in again."
    );
  }

  if (!response.ok) {
    throw new Error("Could not retrieve profile.");
  }

  return response.json() as Promise<ProfileResponse>;
}

export async function logout(): Promise<void> {
  const response = await fetch(`${API_URL}/Authentication/logout`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("Logout failed.");
  }
}

export async function refreshSession(): Promise<void> {
  const response = await fetch(
    `${API_URL}/Authentication/refresh`,
    {
      method: "POST",
      credentials: "include",
    }
  );

  if (!response.ok) {
    throw new Error("Session refresh failed.");
  }
}

