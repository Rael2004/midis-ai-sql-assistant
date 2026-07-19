import { requestJson } from "./apiClient";

export interface QueryColumn {
  name: string;
  dataType: string;
}

export type QueryCellValue =
  | string
  | number
  | boolean
  | null;

export interface QueryExecutionResult {
  columns: QueryColumn[];
  rows: QueryCellValue[][];
  returnedRowCount: number;
  wasTruncated: boolean;
  maximumRows: number;
  durationMilliseconds: number;
}

export interface ExecuteSqlResponse {
  executed: boolean;
  validationErrors: string[];
  result: QueryExecutionResult | null;
}

export function executeSql(
  sql: string,
  signal?: AbortSignal,
): Promise<ExecuteSqlResponse> {
  return requestJson<ExecuteSqlResponse>(
    "/api/query/execute",
    {
      method: "POST",
      signal,
      body: JSON.stringify({
        sql,
      }),
    },
  );
}