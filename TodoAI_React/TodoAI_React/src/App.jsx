import { useState, useEffect } from "react";
import {
  getTodos,
  createTodo,
  deleteTodo,
  completeTodo,
  sendToAgent,
} from "./api/todoApi";
import TodoItem from "./components/TodoItem";
import styles from "./App.module.css";

export default function App() {
  const [todos, setTodos] = useState([]);
  const [input, setInput] = useState("");
  const [aiInput, setAiInput] = useState("");
  const [loading, setLoading] = useState(true);
  const [adding, setAdding] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);
  const [aiResult, setAiResult] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadTodos();
  }, []);

  async function loadTodos() {
    try {
      setLoading(true);
      const data = await getTodos();
      setTodos(data);
    } catch {
      setError(
        "Cannot connect to API. Make sure ASP.NET is running on port 7012.",
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleAdd(e) {
    e.preventDefault();
    if (!input.trim()) return;
    setAdding(true);
    try {
      await createTodo(input.trim());
      setInput("");
      await loadTodos();
    } catch {
      setError("Failed to create todo.");
    } finally {
      setAdding(false);
    }
  }

  async function handleAiSubmit(e) {
    e.preventDefault();
    if (!aiInput.trim()) return;
    setAiLoading(true);
    setAiResult(null);
    try {
      const res = await sendToAgent(aiInput.trim());
      setAiInput("");
      setAiResult(res);
      await loadTodos();
    } catch {
      setError("AI agent failed.");
    } finally {
      setAiLoading(false);
    }
  }

  async function handleComplete(id) {
    try {
      await completeTodo(id);
      await loadTodos();
    } catch {
      setError("Failed to complete todo.");
    }
  }

  async function handleDelete(id) {
    try {
      await deleteTodo(id);
      setTodos((prev) => prev.filter((t) => t.id !== id));
    } catch {
      setError("Failed to delete todo.");
    }
  }

  const pending = todos.filter((t) => !t.completed);
  const completed = todos.filter((t) => t.completed);

  return (
    <div className={styles.layout}>
      <header className={styles.header}>
        <div className={styles.headerInner}>
          <div className={styles.logo}>
            <span className={styles.logoAccent}>TODO</span>
            <span className={styles.logoSub}>AI</span>
          </div>
          <div className={styles.stats}>
            <span className={styles.statItem}>
              <span className={styles.statNum}>{pending.length}</span>
              <span className={styles.statLabel}>pending</span>
            </span>
            <span className={styles.statDivider}>/</span>
            <span className={styles.statItem}>
              <span className={styles.statNum}>{completed.length}</span>
              <span className={styles.statLabel}>done</span>
            </span>
          </div>
        </div>
      </header>

      <main className={styles.main}>
        <form className={styles.form} onSubmit={handleAdd}>
          <input
            className={styles.input}
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Add a new task now..."
            disabled={adding}
            autoFocus
          />
          <button
            className={styles.addBtn}
            type="submit"
            disabled={adding || !input.trim()}
          >
            {adding ? "..." : "+ ADD"}
          </button>
        </form>

        <div className={styles.aiSection}>
          <div className={styles.aiLabel}>
            <span className={styles.aiDot} />
            THAO TÁC VỚI AI
          </div>
          <form className={styles.aiForm} onSubmit={handleAiSubmit}>
            <input
              className={styles.aiInput}
              value={aiInput}
              onChange={(e) => setAiInput(e.target.value)}
              placeholder='vd: "create learn React", "complete todo abc-123"'
              disabled={aiLoading}
            />
            <button
              className={styles.aiBtn}
              type="submit"
              disabled={aiLoading || !aiInput.trim()}
            >
              {aiLoading ? <span className={styles.aiSpinner}>⟳</span> : "SEND"}
            </button>
          </form>
          {aiResult && (
            <div className={styles.aiResponse}>
              {aiResult.action === "created" && (
                <span>
                  ✅ AI đã tạo: <strong>{aiResult.title}</strong>
                </span>
              )}
              {aiResult.action === "none" && <span>💬 {aiResult.message}</span>}
              {aiResult.action === "deleted" && <span>🗑️ AI đã xóa todo</span>}
              {aiResult.action === "completed" && (
                <span>✔️ AI đã hoàn thành todo</span>
              )}
            </div>
          )}
        </div>

        {error && (
          <div className={styles.error}>
            ⚠ {error}
            <button
              onClick={() => setError(null)}
              className={styles.errorClose}
            >
              ×
            </button>
          </div>
        )}

        {loading ? (
          <div className={styles.loading}>
            <span className={styles.loadingDot} />
            <span className={styles.loadingDot} />
            <span className={styles.loadingDot} />
          </div>
        ) : (
          <div className={styles.list}>
            {pending.length === 0 && completed.length === 0 && (
              <div className={styles.empty}>
                <span className={styles.emptyIcon}>◎</span>
                <p>No tasks yet. Add one above.</p>
              </div>
            )}
            {pending.map((todo) => (
              <TodoItem
                key={todo.id}
                todo={todo}
                onComplete={handleComplete}
                onDelete={handleDelete}
              />
            ))}
            {completed.length > 0 && pending.length > 0 && (
              <div className={styles.divider}>
                <span>COMPLETED</span>
              </div>
            )}
            {completed.map((todo) => (
              <TodoItem
                key={todo.id}
                todo={todo}
                onComplete={handleComplete}
                onDelete={handleDelete}
              />
            ))}
          </div>
        )}
      </main>

      <footer className={styles.footer}>
        <span>TODO_AGENT — powered by OneBit Solutions 🚀</span>
      </footer>
    </div>
  );
}
