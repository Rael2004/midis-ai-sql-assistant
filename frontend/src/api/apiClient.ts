const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ??
  "https://localhost:7238";

async function readErrorMessage(
  response: Response,
): Promise<string> {
  const responseText = await response.text();

  if (!responseText) {
    return `Request failed with status ${response.status}.`;
  }

  try {
    const parsedBody = JSON.parse(responseText) as {
      message?: string;
    };

    return parsedBody.message ?? responseText;
  } catch {
    return responseText;
  }
}

export async function requestJson<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers);

  headers.set("Accept", "application/json");

  if (options.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(
    `${API_BASE_URL}${path}`,
    {
      ...options,
      headers,
    },
  );

  if (!response.ok) {
    const message = await readErrorMessage(response);
    throw new Error(message);
  }

  return (await response.json()) as T;
}