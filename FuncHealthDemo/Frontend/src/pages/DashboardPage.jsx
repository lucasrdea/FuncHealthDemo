import { useEffect, useMemo, useState } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { apiClient } from '../api/apiClient'
import { useAuth } from '../context/AuthContext'
import { hasCompletedProfile, ROUTES } from '../lib/auth'

const PRIORITY_LABEL = { 0: 'Low', 1: 'Medium', 2: 'High', 3: 'Urgent' }
const CATEGORY_LABEL = {
  0: 'Personal',
  1: 'Work',
  2: 'Shopping',
  3: 'Health',
  4: 'Finance',
  5: 'Education',
  6: 'Fitness',
  7: 'Other',
}

const STATUS_LABEL = {
  0: 'Backlog',
  1: 'In Progress',
  2: 'Canceled',
  3: 'Completed',
}

const BOARD_COLUMNS = [
  { key: 'backlog', label: 'Backlog' },
  { key: 'in-progress', label: 'In Progress' },
  { key: 'canceled', label: 'Canceled' },
  { key: 'completed', label: 'Completed' },
]

const PRIORITY_KEY = { 0: 'low', 1: 'medium', 2: 'high', 3: 'urgent' }

const STATUS_OPTIONS = [
  { value: 0, label: 'Backlog' },
  { value: 1, label: 'In Progress' },
  { value: 2, label: 'Canceled' },
  { value: 3, label: 'Completed' },
]

const PRIORITY_OPTIONS = [
  { value: 0, label: 'Low' },
  { value: 1, label: 'Medium' },
  { value: 2, label: 'High' },
  { value: 3, label: 'Urgent' },
]

const CATEGORY_OPTIONS = [
  { value: 0, label: 'Personal' },
  { value: 1, label: 'Work' },
  { value: 2, label: 'Shopping' },
  { value: 3, label: 'Health' },
  { value: 4, label: 'Finance' },
  { value: 5, label: 'Education' },
  { value: 6, label: 'Fitness' },
  { value: 7, label: 'Other' },
]

function normalizeTaskStage(status) {
  if (typeof status === 'number') {
    if (status === 1) return 'in-progress'
    if (status === 2) return 'canceled'
    if (status === 3) return 'completed'
    return 'backlog'
  }

  const normalized = String(status ?? '')
    .trim()
    .toLowerCase()
    .replace(/[_\s]+/g, '-')

  if (normalized === 'in-progress' || normalized === 'inprogress') return 'in-progress'
  if (normalized === 'canceled' || normalized === 'cancelled') return 'canceled'
  if (normalized === 'completed' || normalized === 'done') return 'completed'
  return 'backlog'
}

function formatDueDate(value) {
  if (!value) return null
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  return new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' }).format(
    date
  )
}

function formatInputDate(value) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export default function DashboardPage() {
  const navigate = useNavigate()
  const { activeUser, signOut } = useAuth()
  const displayName = activeUser?.name || activeUser?.firstName || 'there'
  const [tasks, setTasks] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [editingTaskId, setEditingTaskId] = useState(null)
  const [isSavingEdit, setIsSavingEdit] = useState(false)
  const [editHelperMessage, setEditHelperMessage] = useState('')
  const [editForm, setEditForm] = useState({
    title: '',
    description: '',
    status: 0,
    priority: 1,
    category: 0,
    dueDate: '',
  })

  const groupedTasks = useMemo(() => {
    const grouped = {
      backlog: [],
      'in-progress': [],
      canceled: [],
      completed: [],
    }

    tasks.forEach((task) => {
      grouped[normalizeTaskStage(task.status)].push(task)
    })

    return grouped
  }, [tasks])

  const handleSignOut = () => {
    signOut()
    navigate(ROUTES.login, { replace: true })
  }

  const handleDeleteTask = async (taskId) => {
    try {
      setErrorMessage('')
      await apiClient.delete(`/tasks/${encodeURIComponent(String(taskId))}`)
      setTasks((current) => current.filter((task) => task.id !== taskId))
    } catch (error) {
      setErrorMessage(error.message || 'Unable to delete task right now.')
    }
  }

  const handleOpenEdit = (task) => {
    setEditHelperMessage('')
    setEditingTaskId(task.id)
    setEditForm({
      title: task.title ?? '',
      description: task.description ?? '',
      status: typeof task.status === 'number' ? task.status : 0,
      priority: typeof task.priority === 'number' ? task.priority : 1,
      category: typeof task.category === 'number' ? task.category : 0,
      dueDate: formatInputDate(task.dueDate),
    })
  }

  const handleCloseEdit = () => {
    if (isSavingEdit) return
    setEditingTaskId(null)
    setEditHelperMessage('')
  }

  const handleUpdateTask = async (event) => {
    event.preventDefault()
    if (!editingTaskId) return

    if (!editForm.title.trim()) {
      setEditHelperMessage('Title is required.')
      return
    }

    if (editForm.title.trim().length > 200) {
      setEditHelperMessage('Title must be 200 characters or less.')
      return
    }

    if (editForm.description.trim().length > 2000) {
      setEditHelperMessage('Description must be 2000 characters or less.')
      return
    }

    if (!editForm.dueDate) {
      setEditHelperMessage('Due date is required.')
      return
    }

    setEditHelperMessage('')
    setIsSavingEdit(true)

    const payload = {
      title: editForm.title.trim(),
      description: editForm.description.trim() || null,
      status: editForm.status,
      priority: editForm.priority,
      category: editForm.category,
      dueDate: editForm.dueDate,
    }

    try {
      const response = await apiClient.put(`/tasks/${encodeURIComponent(String(editingTaskId))}`, payload)
      const updatedTask =
        response?.data && typeof response.data === 'object'
          ? response.data
          : { id: editingTaskId, ...payload }

      setTasks((current) => current.map((task) => (task.id === editingTaskId ? { ...task, ...updatedTask } : task)))
      setEditingTaskId(null)
    } catch (error) {
      setEditHelperMessage(error.message || 'Unable to update task right now.')
    } finally {
      setIsSavingEdit(false)
    }
  }

  useEffect(() => {
    let isMounted = true

    const loadTasks = async () => {
      setIsLoading(true)
      setErrorMessage('')

      try {
        const response = await apiClient.get('/tasks')
        const data = Array.isArray(response?.data) ? response.data : []

        if (isMounted) {
          setTasks(data)
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error.message || 'Unable to load tasks right now.')
          setTasks([])
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadTasks()

    return () => {
      isMounted = false
    }
  }, [])

  if (!activeUser) {
    return <Navigate to={ROUTES.login} replace />
  }

  if (!hasCompletedProfile(activeUser)) {
    return <Navigate to={ROUTES.onboarding} replace />
  }

  return (
    <main className="dashboard-shell">
      <header className="dashboard-header">
        <div>
          <p className="dashboard-eyebrow">Your Task Dashboard</p>
          <h1>Welcome back, {displayName}</h1>
        </div>
        <div className="dashboard-actions">
          <button
            type="button"
            className="book-appointment-button"
            onClick={() => navigate(ROUTES.createTask)}
          >
            + New Task
          </button>
          <button type="button" className="sign-out-button" onClick={handleSignOut}>
            Sign out
          </button>
        </div>
      </header>

      <section className="appointments-section" aria-labelledby="tasks-heading">
        <h2 id="tasks-heading">My Tasks</h2>

        {isLoading ? <p className="appointments-loading">Loading tasks...</p> : null}

        {!isLoading && errorMessage ? (
          <p className="appointments-error" role="status" aria-live="polite">
            {errorMessage}
          </p>
        ) : null}

        {!isLoading && !errorMessage && tasks.length === 0 ? (
          <div className="appointments-empty-state">
            <p>You have no tasks yet.</p>
            <button
              type="button"
              className="schedule-first-test-button"
              onClick={() => navigate(ROUTES.createTask)}
            >
              Create your first task
            </button>
          </div>
        ) : null}

        {!isLoading && !errorMessage && tasks.length > 0 ? (
          <div className="task-board-grid">
            {BOARD_COLUMNS.map((column) => (
              <article key={column.key} className="task-column">
                <header className={`task-column-header task-column-header--${column.key}`}>
                  <span>{column.label}</span>
                  <span className="task-column-count">{groupedTasks[column.key].length}</span>
                </header>
                <div className="task-column-body">
                  {groupedTasks[column.key].map((task) => (
                    <div key={task.id} className="task-card">
                      <div className="task-card-top">
                        <h3>{task.title}</h3>
                        <div className="task-card-actions">
                          <button
                            type="button"
                            className="edit-task-button"
                            aria-label="Edit task"
                            title="Edit task"
                            onClick={() => handleOpenEdit(task)}
                          >
                            <svg viewBox="0 0 24 24" aria-hidden="true" className="delete-appointment-icon">
                              <path d="M3 21h18" />
                              <path d="M14.5 4.5a2.1 2.1 0 0 1 3 3L8 17l-4 1 1-4z" />
                            </svg>
                          </button>
                          <button
                            type="button"
                            className="delete-appointment-button"
                            aria-label="Delete task"
                            title="Delete task"
                            onClick={() => handleDeleteTask(task.id)}
                          >
                            <svg viewBox="0 0 24 24" aria-hidden="true" className="delete-appointment-icon">
                              <path d="M3 6h18" />
                              <path d="M8 6V4h8v2" />
                              <path d="M19 6l-1 14H6L5 6" />
                              <path d="M10 11v6" />
                              <path d="M14 11v6" />
                            </svg>
                          </button>
                        </div>
                      </div>

                      {task.description ? <p className="task-description">{task.description}</p> : null}

                      <div className="task-meta-row">
                        <span className="task-category-tag">{CATEGORY_LABEL[task.category] ?? 'Other'}</span>
                        <span
                          className={`task-priority-badge task-priority-badge--${
                            PRIORITY_KEY[task.priority] ?? 'low'
                          }`}
                        >
                          {PRIORITY_LABEL[task.priority] ?? 'Low'}
                        </span>
                      </div>

                      <p className="task-status-text">Status: {STATUS_LABEL[task.status] ?? 'Backlog'}</p>

                      <p className="task-due-text">Due: {formatDueDate(task.dueDate) ?? 'No date'}</p>
                    </div>
                  ))}
                </div>
              </article>
            ))}
          </div>
        ) : null}
      </section>

      {editingTaskId ? (
        <div className="task-edit-modal-overlay" role="presentation" onClick={handleCloseEdit}>
          <section
            className="task-edit-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="task-edit-heading"
            onClick={(event) => event.stopPropagation()}
          >
            <header className="task-edit-modal-header">
              <h2 id="task-edit-heading">Update Task</h2>
              <button type="button" className="task-edit-close" onClick={handleCloseEdit}>
                ×
              </button>
            </header>

            <form className="task-edit-form" onSubmit={handleUpdateTask}>
              <label htmlFor="editTitle">Title</label>
              <input
                id="editTitle"
                className="text-input"
                value={editForm.title}
                onChange={(event) => setEditForm((current) => ({ ...current, title: event.target.value }))}
                maxLength={200}
              />

              <label htmlFor="editDescription">Description</label>
              <textarea
                id="editDescription"
                className="text-input textarea-input"
                rows={3}
                value={editForm.description}
                onChange={(event) =>
                  setEditForm((current) => ({ ...current, description: event.target.value }))
                }
                maxLength={2000}
              />

              <div className="task-edit-grid">
                <div>
                  <label htmlFor="editStatus">Status</label>
                  <select
                    id="editStatus"
                    className="text-input"
                    value={editForm.status}
                    onChange={(event) =>
                      setEditForm((current) => ({ ...current, status: Number(event.target.value) }))
                    }
                  >
                    {STATUS_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label htmlFor="editPriority">Priority</label>
                  <select
                    id="editPriority"
                    className="text-input"
                    value={editForm.priority}
                    onChange={(event) =>
                      setEditForm((current) => ({ ...current, priority: Number(event.target.value) }))
                    }
                  >
                    {PRIORITY_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label htmlFor="editCategory">Category</label>
                  <select
                    id="editCategory"
                    className="text-input"
                    value={editForm.category}
                    onChange={(event) =>
                      setEditForm((current) => ({ ...current, category: Number(event.target.value) }))
                    }
                  >
                    {CATEGORY_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label htmlFor="editDueDate">Due Date</label>
                  <input
                    id="editDueDate"
                    type="date"
                    className="text-input"
                    value={editForm.dueDate}
                    onChange={(event) =>
                      setEditForm((current) => ({ ...current, dueDate: event.target.value }))
                    }
                    required
                  />
                </div>
              </div>

              {editHelperMessage ? (
                <p className="appointments-error" role="status" aria-live="polite">
                  {editHelperMessage}
                </p>
              ) : null}

              <div className="task-edit-actions">
                <button type="button" className="sign-out-button" onClick={handleCloseEdit}>
                  Cancel
                </button>
                <button type="submit" className="book-appointment-button" disabled={isSavingEdit}>
                  {isSavingEdit ? 'Saving...' : 'Save Changes'}
                </button>
              </div>
            </form>
          </section>
        </div>
      ) : null}
    </main>
  )
}
