import { useEffect, useState } from "react";

import {
  login,
  logout,
  getMyProfile,
} from "./services/authService";

import {
  apiRequest,
  onSessionExpired,
} from "./services/apiClient";

import type { ProfileResponse } from "./services/authService";

import "./App.css";

function App() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [profile, setProfile] = useState<ProfileResponse | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Merkezi oturum sona erme dinleyicisi.
  useEffect(() => {
    const unsubscribe = onSessionExpired(() => {
      setProfile(null);
      setPassword("");
      setError(
        "Your session has expired. Please sign in again."
      );
    });

    return unsubscribe;
  }, []);

  async function handleLogin(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setError("");
    setLoading(true);

    try {
      await login({ email, password });

      const profileData = await getMyProfile();
      setProfile(profileData);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "An unexpected error occurred."
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleLogout() {
    setError("");
    setLoading(true);

    try {
      await logout();

      setProfile(null);
      setEmail("");
      setPassword("");
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Logout failed."
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleProfileTest() {
    setError("");
    setLoading(true);

    try {
      const profileData = await getMyProfile();
      setProfile(profileData);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Profile request failed."
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleConcurrentTest() {
    setError("");
    setLoading(true);

    try {
      const endpoints = [
        "/Profile/me",
        "/Tickets",
        "/Tickets?page=1&pageSize=10",
      ];

      const responses = await Promise.all(
        endpoints.map((endpoint) =>
          apiRequest(endpoint, {
            method: "GET",
          })
        )
      );

      const results = await Promise.all(
        responses.map(async (response) => ({
          status: response.status,
          body: await response.text(),
        }))
      );

      console.log("Concurrent API results:", results);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Concurrent test failed."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <main>
      <h1>SupportPilot</h1>

      {!profile ? (
        <form onSubmit={handleLogin}>
          <h2>Login</h2>

          <div>
            <label htmlFor="email">Email</label>

            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </div>

          <div>
            <label htmlFor="password">Password</label>

            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </div>

          <button type="submit" disabled={loading}>
            {loading ? "Signing in..." : "Login"}
          </button>

          {error && <p style={{ color: "red" }}>{error}</p>}
        </form>
      ) : (
        <section>
          <h2>Welcome!</h2>

          <p>User ID: {profile.userId}</p>
          <p>Email: {profile.email}</p>
          <p>Role: {profile.role}</p>
          <p>{profile.message}</p>

          <button
            type="button"
            onClick={handleProfileTest}
            disabled={loading}
          >
            {loading ? "Loading..." : "Test Profile Request"}
          </button>

          <button
            type="button"
            onClick={handleConcurrentTest}
            disabled={loading}
          >
            {loading ? "Testing..." : "Test Concurrent Requests"}
          </button>

          <button
            type="button"
            onClick={handleLogout}
            disabled={loading}
          >
            {loading ? "Please wait..." : "Logout"}
          </button>

          {error && <p style={{ color: "red" }}>{error}</p>}
        </section>
      )}
    </main>
  );
}

export default App;


