import {
  useEffect,
  useState,
  type FormEvent,
} from "react";
import "./App.css";

import {
  generateSql,
  type GenerateSqlResponse,
} from "./api/aiApi";

import {
  getDatabaseHealth,
  type DatabaseHealthResponse,
} from "./api/databaseApi";

import {
  executeSql,
  type ExecuteSqlResponse,
  type QueryCellValue,
} from "./api/queryApi";

const exampleQuestions = [
  "Which client submitted the most tickets?",
  "Show all open critical tickets.",
  "How many tickets are there for each status?",
  "Which employee resolved the most tickets?",
];

function formatCellValue(
  value: QueryCellValue,
): string {
  if (value === null) {
    return "NULL";
  }

  if (typeof value === "boolean") {
    return value ? "True" : "False";
  }

  return String(value);
}

function App() {
  const [health, setHealth] =
    useState<DatabaseHealthResponse | null>(null);

  const [healthError, setHealthError] =
    useState<string | null>(null);

  const [isHealthLoading, setIsHealthLoading] =
    useState(true);

  const [question, setQuestion] = useState("");

  const [generation, setGeneration] =
    useState<GenerateSqlResponse | null>(null);

  const [generationError, setGenerationError] =
    useState<string | null>(null);

  const [isGenerating, setIsGenerating] =
    useState(false);

  const [execution, setExecution] =
    useState<ExecuteSqlResponse | null>(null);

  const [executionError, setExecutionError] =
    useState<string | null>(null);

  const [isExecuting, setIsExecuting] =
    useState(false);

  useEffect(() => {
    const controller = new AbortController();

    async function loadDatabaseHealth() {
      try {
        setIsHealthLoading(true);
        setHealthError(null);

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

        setHealthError(
          requestError instanceof Error
            ? requestError.message
            : "Could not check the backend connection.",
        );
      } finally {
        setIsHealthLoading(false);
      }
    }

    void loadDatabaseHealth();

    return () => {
      controller.abort();
    };
  }, []);

  async function handleGenerateSql(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    const normalizedQuestion = question.trim();

    if (!normalizedQuestion) {
      return;
    }

    setGeneration(null);
    setExecution(null);
    setGenerationError(null);
    setExecutionError(null);
    setIsGenerating(true);

    try {
      const result = await generateSql(
        normalizedQuestion,
      );

      setGeneration(result);
    } catch (requestError) {
      setGenerationError(
        requestError instanceof Error
          ? requestError.message
          : "SQL generation failed.",
      );
    } finally {
      setIsGenerating(false);
    }
  }

  async function handleExecuteSql() {
    if (
      !generation?.generatedSql ||
      !generation.isValid
    ) {
      return;
    }

    setExecution(null);
    setExecutionError(null);
    setIsExecuting(true);

    try {
      const result = await executeSql(
        generation.generatedSql,
      );

      setExecution(result);
    } catch (requestError) {
      setExecutionError(
        requestError instanceof Error
          ? requestError.message
          : "Query execution failed.",
      );
    } finally {
      setIsExecuting(false);
    }
  }

  function selectExample(example: string) {
    setQuestion(example);
    setGeneration(null);
    setExecution(null);
    setGenerationError(null);
    setExecutionError(null);
  }

  return (
    <main className="app-shell">
      <header className="hero">
        <div>
          <p className="eyebrow">
            Midis ICT Internship Project
          </p>

          <h1>AI SQL Assistant</h1>

          <p className="hero-description">
            Ask a database question in plain English.
            GPT-5 mini generates SQL, the backend
            validates it, and a restricted SQL account
            executes approved queries.
          </p>
        </div>

        <div className="system-status">
          <span
            className={
              health?.canConnect
                ? "status-dot connected"
                : "status-dot"
            }
          />

          <div>
            <span className="status-title">
              {isHealthLoading
                ? "Checking system"
                : health?.canConnect
                  ? "System connected"
                  : "System unavailable"}
            </span>

            <span className="status-detail">
              {health
                ? `${health.databaseName} · ${health.ticketCount} tickets`
                : healthError ?? "Connecting to backend…"}
            </span>
          </div>
        </div>
      </header>

      <section className="workspace">
        <form
          className="question-card"
          onSubmit={handleGenerateSql}
        >
          <div className="section-heading">
            <div>
              <p className="step-label">Step 1</p>
              <h2>Ask a question</h2>
            </div>

            <span className="character-count">
              {question.length}/1000
            </span>
          </div>

          <label
            className="visually-hidden"
            htmlFor="database-question"
          >
            Database question
          </label>

          <textarea
            id="database-question"
            value={question}
            maxLength={1000}
            rows={5}
            placeholder="Example: Which client submitted the most tickets?"
            onChange={(event) => {
              setQuestion(event.target.value);
            }}
          />

          <div className="examples">
            <span>Try an example:</span>

            <div className="example-list">
              {exampleQuestions.map((example) => (
                <button
                  key={example}
                  type="button"
                  className="example-button"
                  onClick={() => {
                    selectExample(example);
                  }}
                >
                  {example}
                </button>
              ))}
            </div>
          </div>

          <button
            className="primary-button"
            type="submit"
            disabled={
              isGenerating ||
              !question.trim() ||
              !health?.canConnect
            }
          >
            {isGenerating
              ? "Generating SQL…"
              : "Generate SQL"}
          </button>
        </form>

        {generationError && (
          <section className="message-card error-card">
            <strong>SQL generation failed</strong>
            <span>{generationError}</span>
          </section>
        )}

        {generation && (
          <section className="sql-card">
            <div className="section-heading">
              <div>
                <p className="step-label">Step 2</p>
                <h2>Generated SQL</h2>
              </div>

              <span
                className={
                  generation.isValid
                    ? "validation-badge valid"
                    : "validation-badge invalid"
                }
              >
                {generation.isValid
                  ? "Validated"
                  : "Rejected"}
              </span>
            </div>

            <p className="model-name">
              Model deployment:{" "}
              <strong>
                {generation.modelDeployment}
              </strong>
            </p>

            {!generation.canAnswer && (
              <div className="message-card warning-card">
                The database schema does not contain
                enough information to answer this
                question.
              </div>
            )}

            {generation.generatedSql && (
              <>
                <pre className="sql-preview">
                  <code>{generation.generatedSql}</code>
                </pre>

                {generation.validationErrors.length > 0 && (
                  <div className="validation-errors">
                    <strong>
                      Validation errors
                    </strong>

                    <ul>
                      {generation.validationErrors.map(
                        (error) => (
                          <li key={error}>{error}</li>
                        ),
                      )}
                    </ul>
                  </div>
                )}

                <button
                  className="execute-button"
                  type="button"
                  disabled={
                    !generation.isValid ||
                    isExecuting
                  }
                  onClick={() => {
                    void handleExecuteSql();
                  }}
                >
                  {isExecuting
                    ? "Executing query…"
                    : "Execute validated query"}
                </button>
              </>
            )}
          </section>
        )}

        {executionError && (
          <section className="message-card error-card">
            <strong>Query execution failed</strong>
            <span>{executionError}</span>
          </section>
        )}

        {execution?.result && (
          <section className="results-card">
            <div className="section-heading">
              <div>
                <p className="step-label">Step 3</p>
                <h2>Query results</h2>
              </div>

              <span className="result-count">
                {execution.result.returnedRowCount} rows
              </span>
            </div>

            <div className="result-metadata">
              <span>
                Duration:{" "}
                {execution.result.durationMilliseconds} ms
              </span>

              <span>
                Maximum rows:{" "}
                {execution.result.maximumRows}
              </span>
            </div>

            {execution.result.wasTruncated && (
              <div className="message-card warning-card">
                The result contained more than{" "}
                {execution.result.maximumRows} rows.
                Only the first{" "}
                {execution.result.maximumRows} were
                returned.
              </div>
            )}

            {execution.result.rows.length === 0 ? (
              <div className="empty-results">
                The query executed successfully but
                returned no rows.
              </div>
            ) : (
              <div className="table-container">
                <table>
                  <thead>
                    <tr>
                      {execution.result.columns.map(
                        (column, index) => (
                          <th
                            key={`${column.name}-${index}`}
                          >
                            <span>{column.name}</span>
                            <small>
                              {column.dataType}
                            </small>
                          </th>
                        ),
                      )}
                    </tr>
                  </thead>

                  <tbody>
                    {execution.result.rows.map(
                      (row, rowIndex) => (
                        <tr key={rowIndex}>
                          {row.map(
                            (value, columnIndex) => (
                              <td
                                key={`${rowIndex}-${columnIndex}`}
                                className={
                                  value === null
                                    ? "null-value"
                                    : undefined
                                }
                              >
                                {formatCellValue(value)}
                              </td>
                            ),
                          )}
                        </tr>
                      ),
                    )}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        )}
      </section>

      <footer>
        Generated SQL is validated and executed through
        a restricted read-only database account.
      </footer>
    </main>
  );
}

export default App;