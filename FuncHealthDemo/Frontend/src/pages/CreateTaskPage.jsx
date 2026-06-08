import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { apiClient } from '../api/apiClient'
import { useAuth } from '../context/AuthContext'
import { hasCompletedProfile, ROUTES } from '../lib/auth'

// Must match C# TaskPriority enum order: Low=0, Medium=1, High=2, Urgent=3
const PRIORITY_OPTIONS = [
  { value: 0, label: 'Low' },
  { value: 1, label: 'Medium' },
  { value: 2, label: 'High' },
  { value: 3, label: 'Urgent' },
]

// Must match C# TaskCategory enum order: Personal=0, Work=1, Shopping=2, Health=3, Finance=4, Education=5, Fitness=6, Other=7
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

export default function CreateTaskPage() {
  const navigate = useNavigate()
  const { activeUser } = useAuth()

  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState(1)
  const [category, setCategory] = useState(0)
  const [dueDate, setDueDate] = useState('')
  const [helperMessage, setHelperMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [showPastDateWarning, setShowPastDateWarning] = useState(false)

  if (!activeUser) {
    return <Navigate to={ROUTES.login} replace />
  }

  if (!hasCompletedProfile(activeUser)) {
    return <Navigate to={ROUTES.onboarding} replace />
  }

  const handleSubmit = async (event) => {
    event.preventDefault()

    if (!title.trim()) {
      setHelperMessage('Task title is required.')
      return
    }

    if (title.trim().length > 200) {
      setHelperMessage('Title must be 200 characters or less.')
      return
    }

    if (description.trim().length > 2000) {
      setHelperMessage('Description must be 2000 characters or less.')
      return
    }

    if (!dueDate) {
      setHelperMessage('Due date is required.')
      return
    }

    const today = new Date()
    today.setHours(0, 0, 0, 0)
    const selected = new Date(`${dueDate}T00:00:00`)
    if (selected < today && !showPastDateWarning) {
      setShowPastDateWarning(true)
      return
    }

    setShowPastDateWarning(false)
    setHelperMessage('')
    setIsSubmitting(true)

    try {
      await apiClient.post('/tasks', {
        title: title.trim(),
        description: description.trim() || null,
        priority,
        category,
        dueDate,
      })

      navigate(ROUTES.dashboard)
    } catch (error) {
      setHelperMessage(error.message || 'Unable to create task right now.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="page-wrapper">
      <div className="create-task-container">
        <div className="create-task-header">
          <button
            type="button"
            className="back-button"
            onClick={() => navigate(ROUTES.dashboard)}
          >
            ← Back
          </button>
          <h1>Create Task</h1>
        </div>

        <form className="create-task-form" onSubmit={handleSubmit}>
          <div className="form-field">
            <label htmlFor="taskTitle">Title</label>
            <input
              id="taskTitle"
              type="text"
              placeholder="Enter task title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="text-input"
              autoFocus
              maxLength={200}
            />
          </div>

          <div className="form-field">
            <label htmlFor="taskDescription">Description</label>
            <textarea
              id="taskDescription"
              placeholder="Enter task description (optional)"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="text-input textarea-input"
              rows={3}
              maxLength={2000}
            />
          </div>

          <div className="form-row">
            <div className="form-field">
              <label htmlFor="taskPriority">Priority</label>
              <select
                id="taskPriority"
                value={priority}
                onChange={(e) => setPriority(Number(e.target.value))}
                className="text-input select-input"
              >
                {PRIORITY_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label htmlFor="taskCategory">Category</label>
              <select
                id="taskCategory"
                value={category}
                onChange={(e) => setCategory(Number(e.target.value))}
                className="text-input select-input"
              >
                {CATEGORY_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-field">
            <label htmlFor="taskDueDate">Due Date</label>
            <input
              id="taskDueDate"
              type="date"
              value={dueDate}
              onChange={(e) => {
                setDueDate(e.target.value)
                setShowPastDateWarning(false)
              }}
              className="text-input"
              required
            />
          </div>

          {showPastDateWarning ? (
            <div className="past-date-warning">
              <p>⚠️ Due date is in the past. Are you sure?</p>
              <div className="past-date-warning-actions">
                <button
                  type="button"
                  className="sign-out-button"
                  onClick={() => setShowPastDateWarning(false)}
                >
                  Cancel
                </button>
                <button type="submit" className="continue-button" disabled={isSubmitting}>
                  {isSubmitting ? 'Creating…' : 'Yes, create anyway'}
                </button>
              </div>
            </div>
          ) : null}

          {helperMessage ? (
            <p className="helper-message" role="status" aria-live="polite">
              {helperMessage}
            </p>
          ) : null}

          <button type="submit" className="continue-button" disabled={isSubmitting}>
            {isSubmitting ? 'Creating…' : 'Create Task'}
          </button>
        </form>
      </div>
    </div>
  )
}
