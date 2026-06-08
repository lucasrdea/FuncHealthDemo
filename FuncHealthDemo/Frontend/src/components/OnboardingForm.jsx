import { useState } from 'react'
import { apiClient } from '../api/apiClient'

export default function OnboardingForm({ onCompleted }) {
  const [phoneNumber, setPhoneNumber] = useState('')
  const [helperMessage, setHelperMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (event) => {
    event.preventDefault()

    if (!phoneNumber.trim()) {
      setHelperMessage('Please enter your phone number.')
      return
    }

    if (!/^\d{10}$/.test(phoneNumber.trim())) {
      setHelperMessage('Phone number must be exactly 10 digits.')
      return
    }

    setHelperMessage('')
    setIsSubmitting(true)

    try {
      const fullPhoneNumber = `+1${phoneNumber.trim()}`

      const response = await apiClient.put('/users/profile', {
        phoneNumber: fullPhoneNumber,
      })

      const normalizedProfile =
        response?.data && typeof response.data === 'object'
          ? response.data
          : { phoneNumber: fullPhoneNumber }

      onCompleted(normalizedProfile)
    } catch (error) {
      setHelperMessage(error.message || 'Unable to complete setup right now.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section aria-labelledby="onboarding-heading">
      <form className="login-form onboarding-form" onSubmit={handleSubmit}>
        <h1 id="onboarding-heading">Finish your profile</h1>
        <p className="subtext">Add your final details to complete setup.</p>

        <label className="sr-only" htmlFor="phoneNumber">
          Phone Number
        </label>
        <div className="phone-input-wrapper">
          <span className="phone-prefix">+1</span>
          <input
            id="phoneNumber"
            type="tel"
            placeholder="5551234567"
            value={phoneNumber}
            onChange={(event) => {
              const digitsOnly = event.target.value.replace(/\D/g, '').slice(0, 10)
              setPhoneNumber(digitsOnly)
            }}
            inputMode="numeric"
            pattern="\d{10}"
            maxLength={10}
            autoComplete="tel"
            className="text-input"
          />
        </div>

        <button type="submit" className="continue-button" disabled={isSubmitting}>
          Complete Setup
        </button>

        {helperMessage ? (
          <p className="helper-message" role="status" aria-live="polite">
            {helperMessage}
          </p>
        ) : null}
      </form>
    </section>
  )
}
