import { useState } from 'react'
import styles from './TodoItem.module.css'

export default function TodoItem({ todo, onComplete, onDelete }) {
  const [deleting, setDeleting] = useState(false)
  const [completing, setCompleting] = useState(false)

  async function handleComplete() {
    if (todo.completed) return
    setCompleting(true)
    await onComplete(todo.id)
    setCompleting(false)
  }

  async function handleDelete() {
    setDeleting(true)
    await onDelete(todo.id)
  }

  return (
    <div className={`${styles.item} ${todo.completed ? styles.done : ''} ${deleting ? styles.removing : ''}`}>
      <button
        className={`${styles.check} ${todo.completed ? styles.checked : ''}`}
        onClick={handleComplete}
        disabled={todo.completed || completing}
        title="Mark complete"
      >
        {todo.completed ? '✓' : ''}
      </button>

      <span className={styles.title}>{todo.title}</span>

      {todo.completed && (
        <span className={styles.badge}>DONE</span>
      )}

      <button
        className={styles.delete}
        onClick={handleDelete}
        title="Delete"
      >
        ×
      </button>
    </div>
  )
}
