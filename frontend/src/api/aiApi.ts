import { requestJson } from "./apiClient";

export interface GenerateSqlResponse {
  canAnswer: boolean;
  generatedSql: string | null;
  isValid: boolean;
  validationErrors: string[];
  modelDeployment: string;
}

export function generateSql(
  question: string,
  signal?: AbortSignal,
): Promise<GenerateSqlResponse> {
  return requestJson<GenerateSqlResponse>(
    "/api/ai/generate-sql",
    {
      method: "POST",
      signal,
      body: JSON.stringify({
        question,
      }),
    },
  );
}