const BASE = '/api/todo'

export async function getTodos() {
  const res = await fetch(BASE)
  if (!res.ok) throw new Error('Failed to fetch todos')
  return res.json()
}

export async function createTodo(title) {
  const res = await fetch(BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ title })
  })
  if (!res.ok) throw new Error('Failed to create todo')
}

export async function deleteTodo(id) {
  const res = await fetch(`${BASE}/${id}`, { method: 'DELETE' })
  if (!res.ok) throw new Error('Failed to delete todo')
}

export async function completeTodo(id) {
  const res = await fetch(`${BASE}/${id}/complete`, { method: 'PATCH' })
  if (!res.ok) throw new Error('Failed to complete todo')
}

export async function sendToAgent(message) {
  const res = await fetch('/api/agent', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ message })
  })
  if (!res.ok) throw new Error('Agent failed')
  return res.json()
}
