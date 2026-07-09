import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';

export default function ResetPassword() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  useEffect(() => {
    // Check if token is in URL query params
    const tokenFromUrl = searchParams.get('token');
    if (tokenFromUrl) {
      setToken(tokenFromUrl);
    }
  }, [searchParams]);

  async function handleSubmit(event) {
    event.preventDefault();
    setError('');
    setMessage('');

    // Validate
    if (!token.trim()) {
      setError('Reset token is required.');
      return;
    }

    if (newPassword.length < 6) {
      setError('Password must be at least 6 characters long.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Password and confirmation must match.');
      return;
    }

    setIsLoading(true);

    try {
      const response = await fetch('/api/Auth/reset-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token, newPassword, confirmPassword })
      });

      const result = await response.json();
      
      if (!response.ok) {
        setError(result?.message || 'Unable to reset password.');
        return;
      }

      setMessage(result?.message || 'Your password has been reset successfully.');
      setIsSuccess(true);
      setToken('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err) {
      setError(err.message || 'An error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  }

  if (isSuccess) {
    return (
      <div className="auth-shell">
        <div className="auth-grid">
          <aside className="auth-panel">
            <span className="brand-pill">CarePulse</span>
            <h1>Password reset</h1>
            <p>Your password has been changed successfully.</p>
          </aside>

          <div className="form-card">
            <div className="auth-header">
              <p className="eyebrow">Success</p>
              <h1>Password updated</h1>
              <p className="page-subtitle">Your password has been changed successfully. You can now log in with your new password.</p>
            </div>

            <div style={{ padding: '20px', backgroundColor: '#f1f8f4', borderRadius: '4px', marginBottom: '20px', textAlign: 'center' }}>
              <p style={{ fontSize: '14px', color: '#2e7d32' }}>✓ {message}</p>
            </div>

            <button type="button" onClick={() => navigate('/login')}>
              Go to login
            </button>
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
          <h1>Create new password</h1>
          <p>Use the reset token from your email to set a new password.</p>
        </aside>

        <div className="form-card">
          <div className="auth-header">
            <p className="eyebrow">Reset password</p>
            <h1>Enter new password</h1>
            <p className="page-subtitle">Set a strong password to secure your account.</p>
          </div>

          {message && <div style={{ color: '#4CAF50', padding: '12px', backgroundColor: '#f1f8f4', borderRadius: '4px', marginBottom: '20px' }}>{message}</div>}
          {error && <div style={{ color: '#f44336', padding: '12px', backgroundColor: '#ffebee', borderRadius: '4px', marginBottom: '20px' }}>{error}</div>}

          <form onSubmit={handleSubmit}>
            <div>
              <label>Reset token</label>
              <input
                type="text"
                value={token}
                onChange={(e) => setToken(e.target.value)}
                placeholder="Paste the token from your email"
                required
              />
              <small style={{ color: '#999' }}>You can copy the token from the email we sent you</small>
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
            Don't have a reset token? <Link to="/forgot-password">Request one</Link>
          </p>
        </div>
      </div>
    </div>
  );
}

          <form onSubmit={handleSubmit}>
            <div>
              <label>Reset token</label>
              <input
                value={token}
                onChange={(e) => setToken(e.target.value)}
                required
              />
            </div>
            <div>
              <label>New password</label>
              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
              />
            </div>
            <div>
              <label>Confirm password</label>
              <input
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
            </div>

            {error && <div className="error-text">{error}</div>}
            {message && <div className="success-text">{message}</div>}

            <button type="submit">Reset password</button>
          </form>

          <p style={{ marginTop: '20px', color: '#617d98' }}>
            Back to <Link to="/login">Login</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
