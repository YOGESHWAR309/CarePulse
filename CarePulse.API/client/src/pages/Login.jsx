import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiFetch } from '../lib/api';
import { normalizeUserRole, portalRoutes } from '../lib/roles';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  async function handleSubmit(e) {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      const result = await apiFetch(null, '/api/Auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password })
      });

      const normalizedUser = normalizeUserRole(result.data);
      login(normalizedUser);
      navigate(portalRoutes[normalizedUser.role] || '/dashboard');
    } catch (err) {
      setError(err?.message || 'Login failed.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="auth-shell">
      <div className="auth-grid">
        <aside className="auth-panel">
          <span className="brand-pill">CarePulse</span>
          <h1>Hospital care, simplified.</h1>
          <p>Securely access patient dashboards, appointments, and hospital workflows from a modern clinician-first portal.</p>
          <div className="auth-stats">
            <div className="stat-card">
              <strong>24/7 access</strong>
              <span>Stay connected with the care team anytime.</span>
            </div>
            <div className="stat-card">
              <strong>Patient-first</strong>
              <span>Designed for fast, accurate hospital operations.</span>
            </div>
          </div>
        </aside>

        <div className="form-card">
          <div className="auth-header">
            <p className="eyebrow">Welcome back</p>
            <h1>CarePulse Login</h1>
            <p className="page-subtitle">Access your role-based dashboard and keep hospital operations running smoothly.</p>
          </div>

          <form onSubmit={handleSubmit}>
            <div>
              <label>Email</label>
              <input value={email} onChange={(e) => setEmail(e.target.value)} type="email" required />
            </div>

            <div>
              <label>Password</label>
              <input value={password} onChange={(e) => setPassword(e.target.value)} type="password" required />
            </div>

            {error && <div className="error-text">{error}</div>}
            <button type="submit">Login</button>
          </form>

          <p style={{ marginTop: '12px', color: '#617d98' }}>
            <Link to="/forgot-password">Forgot password?</Link>
          </p>

          <p style={{ marginTop: '20px', color: '#617d98' }}>
            New user? <Link to="/register">Register</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
