import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

export default function Register() {
  const [form, setForm] = useState({ fullName: '', email: '', password: '', confirmPassword: '', phone: '' });
  const [error, setError] = useState('');
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setError('');

    const trimmedName = form.fullName.trim();
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const namePattern = /^[A-Za-zÀ-ÖØ-öø-ÿ\s]+$/;
    const phonePattern = /^\d{10}$/;

    if (!trimmedName) {
      setError('Full name is required.');
      return;
    }

    if (!namePattern.test(trimmedName)) {
      setError('Full name may only contain letters and spaces.');
      return;
    }

    if (!emailPattern.test(form.email)) {
      setError('Enter a valid email address.');
      return;
    }

    if (form.password.length < 8) {
      setError('Password must be at least 8 characters long.');
      return;
    }

    if (form.password !== form.confirmPassword) {
      setError('Password and confirmation must match.');
      return;
    }

    if (!phonePattern.test(form.phone)) {
      setError('Phone number must be exactly 10 digits.');
      return;
    }

    const response = await fetch('/api/Auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ...form,
        fullName: trimmedName,
        phone: form.phone.replace(/\D/g, '')
      })
    });

    const result = await response.json();
    if (!response.ok) {
      const validationMessage = result?.message ||
        (result?.errors ? Object.values(result.errors).flat().join(' ') : undefined);
      setError(validationMessage || 'Registration failed.');
      return;
    }

    navigate('/login');
  }

  return (
    <div className="auth-shell">
      <div className="auth-grid">
        <aside className="auth-panel">
          <span className="brand-pill">CarePulse</span>
          <h1>Create your clinical account</h1>
          <p>Join the hospital platform with a role-based login that supports patients, clinicians, nutritionists, and support staff.</p>
          <div className="auth-stats">
            <div className="stat-card">
              <strong>Secure access</strong>
              <span>Strong authentication for hospital workflows.</span>
            </div>
            <div className="stat-card">
              <strong>Multiple roles</strong>
              <span>Built for administrators, doctors, nurses, and staff.</span>
            </div>
          </div>
        </aside>

        <div className="form-card">
          <div className="auth-header">
            <p className="eyebrow">Create account</p>
            <h1>Register with CarePulse</h1>
            <p className="page-subtitle">Enter your information to join the hospital care platform.</p>
          </div>

          <form onSubmit={handleSubmit}>
            <div>
              <label>Full name</label>
              <input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} required />
            </div>
            <div>
              <label>Email</label>
              <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
            </div>
            <div>
              <label>Password</label>
              <input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required />
            </div>
            <div>
              <label>Confirm password</label>
              <input type="password" value={form.confirmPassword} onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })} required />
            </div>
            <div>
              <label>Phone</label>
              <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} required />
            </div>

            {error && <div className="error-text">{error}</div>}
            <button type="submit">Register</button>
          </form>

          <p style={{ marginTop: '20px', color: '#617d98' }}>
            Already registered? <Link to="/login">Login</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
