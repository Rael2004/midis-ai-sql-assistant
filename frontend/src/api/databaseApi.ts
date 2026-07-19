const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ??
  "https://localhost:7238";

export interface DatabaseHealthResponse {
  canConnect: boolean;
  databaseName: string;
  serverName: string;
  clientCount: number;
  ticketCount: number;
}

export async function getDatabaseHealth(
  signal?: AbortSignal,
): Promise<DatabaseHealthResponse> {
  const response = await fetch(
    `${API_BASE_URL}/api/database/health`,
    { signal },
  );

  if (!response.ok) {
    const responseBody = await response.text();

    throw new Error(
      `Database health request failed with status ` +
        `${response.status}. ${responseBody}`,
    );
  }

  return (await response.json()) as DatabaseHealthResponse;
}