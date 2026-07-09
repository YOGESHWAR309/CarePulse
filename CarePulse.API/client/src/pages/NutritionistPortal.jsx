import { useEffect, useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import TopNav from '../components/TopNav';
import { apiFetch } from '../lib/api';

export default function NutritionistPortal() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [patientId, setPatientId] = useState('1');
  const [dietPlans, setDietPlans] = useState([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  if (user?.role !== 'Nutritionist') {
    return <Navigate to="/dashboard" replace />;
  }

  const handleGetPlans = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      const payload = await apiFetch(user, `/api/Nutritionist/plans/patient?patientId=${patientId}`);
      setDietPlans(payload.dietPlans ?? []);
      setMessage('Loaded patient diet plans successfully.');
    } catch (err) {
      setError(err.message || 'Unable to load diet plans.');
    }
  };

  return (
    <div className="dashboard-card">
      <TopNav
        title="Nutritionist Portal"
        description="Review patient nutrition plans and follow up on progress."
      />

      <section className="back-card">
        <div>
          <h2>Quick workflow</h2>
          <p>Review patient plans here, then go back to the dashboard to continue with other care activities.</p>
          <p className="role-badge">Current portal: /nutritionist</p>
        </div>
        <button type="button" onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
      </section>

      <section className="status-section">
        {message && <div className="success-text">{message}</div>}
        {error && <div className="error-text">{error}</div>}
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <h2>View Patient Diet Plans</h2>
          <form onSubmit={handleGetPlans} className="form-grid">
            <label>Patient ID</label>
            <input
              type="number"
              value={patientId}
              min="1"
              onChange={(e) => setPatientId(e.target.value)}
              required
            />
            <button type="submit">Load Diet Plans</button>
          </form>
        </div>

        <div className="portal-column">
          <h2>Diet Plans</h2>
          <ul>
            {dietPlans.length === 0 ? (
              <li>No diet plans loaded.</li>
            ) : (
              dietPlans.map((plan) => (
                <li key={plan.planId ?? plan.dietPlanId ?? `${plan.patientId}-${plan.createdAt}`}>
                  <strong>Patient:</strong> {plan.patient?.fullName ?? `ID ${plan.patientId}`}<br />
                  <strong>Goal:</strong> {plan.goal}<br />
                  <strong>Summary:</strong> {plan.dietSummary}
                </li>
              ))
            )}
          </ul>
        </div>
      </section>
    </div>
  );
}
