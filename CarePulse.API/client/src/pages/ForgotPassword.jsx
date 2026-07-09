import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

export default function ForgotPassword() {
  const navigate = useNavigate();
  const [stage, setStage] = useState('email'); // 'email', 'otp', 'reset'
  const [email, setEmail] = useState('');
  const [otp, setOtp] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  async function handleRequestOtp(event) {
    event.preventDefault();
    setError('');
    setMessage('');
    setIsLoading(true);

    try {
      const response = await fetch('/api/Auth/forgot-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email })
      });

      const result = await response.json();

      if (!response.ok) {
        setError(result?.message || 'Unable to process password reset request.');
        return;
      }

      setMessage(result?.message || 'OTP sent to your email.');
      setStage('otp');
    } catch (err) {
      setError(err.message || 'An error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  }

  async function handleResetPassword(event) {
    event.preventDefault();
    setError('');
    setMessage('');

    if (newPassword.length < 6) {
      setError('Password must be at least 6 characters.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    setIsLoading(true);

    try {
      const response = await fetch('/api/Auth/verify-otp-and-reset', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email,
          otp,
          newPassword,
          confirmPassword
        })
      });

      const result = await response.json();

      if (!response.ok) {
        setError(result?.message || 'Unable to reset password.');
        return;
      }

      setMessage(result?.message || 'Password reset successfully!');
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(err.message || 'An error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  }

  if (stage === 'email') {
    return (
      <div className="auth-shell">
        <div className="auth-grid">
          <aside className="auth-panel">
            <span className="brand-pill">CarePulse</span>
            <h1>Forgot your password?</h1>
            <p>Don't worry! We'll help you get back into your account.</p>
          </aside>

          <div className="form-card">
            <div className="auth-header">
              <p className="eyebrow">Password recovery</p>
              <h1>Reset your password</h1>
              <p className="page-subtitle">Enter your email address to receive a one-time password (OTP).</p>
            </div>

            {message && <div style={{ color: '#4CAF50', padding: '12px', backgroundColor: '#f1f8f4', borderRadius: '4px', marginBottom: '20px' }}>{message}</div>}
            {error && <div style={{ color: '#f44336', padding: '12px', backgroundColor: '#ffebee', borderRadius: '4px', marginBottom: '20px' }}>{error}</div>}

            <form onSubmit={handleRequestOtp}>
              <div>
                <label>Email address</label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="your.email@example.com"
                  required
                />
              </div>
              <button type="submit" disabled={isLoading}>
                {isLoading ? 'Sending OTP...' : 'Send OTP'}
              </button>
            </form>

            <p style={{ textAlign: 'center', marginTop: '20px' }}>
              Remember your password? <Link to="/login">Log in</Link>
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-shell">
      <div className="auth-grid">
        <aside className="auth-panel">
          <span className="brand-pill">CarePulse</span>
          <h1>Check your email</h1>
          <p>We've sent a one-time password to your email address.</p>
        </aside>

        <div className="form-card">
          <div className="auth-header">
            <p className="eyebrow">Password reset</p>
            <h1>Enter OTP and new password</h1>
            <p className="page-subtitle">Check your email for the 6-digit code and create a new password.</p>
          </div>

          {message && <div style={{ color: '#4CAF50', padding: '12px', backgroundColor: '#f1f8f4', borderRadius: '4px', marginBottom: '20px' }}>{message}</div>}
          {error && <div style={{ color: '#f44336', padding: '12px', backgroundColor: '#ffebee', borderRadius: '4px', marginBottom: '20px' }}>{error}</div>}

          <form onSubmit={handleResetPassword}>
            <div>
              <label>One-Time Password (OTP)</label>
              <input
                type="text"
                value={otp}
                onChange={(e) => setOtp(e.target.value.replace(/\D/g, '').slice(0, 6))}
                placeholder="000000"
                maxLength="6"
                required
                style={{ fontSize: '24px', letterSpacing: '8px', textAlign: 'center' }}
              />
              <small style={{ color: '#999' }}>Enter the 6-digit code from your email</small>
            </div>

            <div>
              <label>New password</label>
              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Enter new password (min 6 characters)"
                required
              />
            </div>

            <div>
              <label>Confirm password</label>
              <input
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Confirm your new password"
                required
              />
            </div>

            <button type="submit" disabled={isLoading}>
              {isLoading ? 'Resetting password...' : 'Reset password'}
            </button>
          </form>

          <p style={{ textAlign: 'center', marginTop: '20px' }}>
            <button 
              type="button" 
              onClick={() => setStage('email')}
              style={{ background: 'none', border: 'none', color: '#1976d2', cursor: 'pointer', textDecoration: 'underline' }}
            >
              Back to email entry
            </button>
          </p>
        </div>
      </div>
    </div>
  );
}
