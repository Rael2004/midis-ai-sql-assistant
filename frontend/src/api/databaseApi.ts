import { requestJson } from "./apiClient";

export interface DatabaseHealthResponse {
  canConnect: boolean;
  databaseName: string;
  serverName: string;
  clientCount: number;
  ticketCount: number;
}

export function getDatabaseHealth(
  signal?: AbortSignal,
): Promise<DatabaseHealthResponse> {
  return requestJson<DatabaseHealthResponse>(
    "/api/database/health",
    {
      method: "GET",
      signal,
    },
  );
}