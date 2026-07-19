import { useEffect, useState } from "react";
import "./App.css";
import {
  getDatabaseHealth,
  type DatabaseHealthResponse,
} from "./api/databaseApi";

function App() {
  const [health, setHealth] =
    useState<DatabaseHealthResponse | null>(null);

  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const controller = new AbortController();

    async function loadDatabaseHealth() {
      try {
        const result = await getDatabaseHealth(
          controller.signal,
        );

        setHealth(result);
      } catch (requestError) {
        if (
          requestError instanceof DOMException &&
          requestError.name === "AbortError"
        ) {
          return;
        }

        setError(
          requestError instanceof Error
            ? requestError.message
            : "An unknown error occurred.",
        );
      } finally {
        setIsLoading(false);
      }
    }

    void loadDatabaseHealth();

    return () => {
      controller.abort();
    };
  }, []);

  return (
    <main className="app-shell">
      <section className="status-card">
        <p className="eyebrow">Midis ICT Internship Project</p>

        <h1>AI SQL Assistant</h1>

        <p className="description">
          Ask questions in plain English, generate safe SQL,
          and display results from the support-ticket database.
        </p>

        <div className="connection-panel">
          <h2>Backend connection</h2>

          {isLoading && <p>Checking database connection…</p>}

          {error && (
            <p className="error-message">{error}</p>
          )}

          {health && (
            <dl className="health-grid">
              <div>
                <dt>Status</dt>
                <dd className="healthy">
                  {health.canConnect
                    ? "Connected"
                    : "Disconnected"}
                </dd>
              </div>

              <div>
                <dt>Database</dt>
                <dd>{health.databaseName}</dd>
              </div>

              <div>
                <dt>Clients</dt>
                <dd>{health.clientCount}</dd>
              </div>

              <div>
                <dt>Tickets</dt>
                <dd>{health.ticketCount}</dd>
              </div>
            </dl>
          )}
        </div>
      </section>
    </main>
  );
}

export default App;