import { useEffect, useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import TopNav from '../components/TopNav';
import { apiFetch } from '../lib/api';

export default function DieticianPortal() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [patientId, setPatientId] = useState('1');
  const [dietSummary, setDietSummary] = useState('Balanced nutrition plan');
  const [goal, setGoal] = useState('Maintain healthy weight');
  const [notes, setNotes] = useState('Created from dietician portal.');
  const [dietPlans, setDietPlans] = useState([]);
  const [refreshing, setRefreshing] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  if (user?.role !== 'Dietician') {
    return <Navigate to="/dashboard" replace />;
  }

  useEffect(() => {
    if (!user) return;

    const loadPlans = async () => {
      try {
        const payload = await apiFetch(user, '/api/Dietician/diet-plans');
        setDietPlans(payload.dietPlans ?? []);
      } catch (err) {
        setError(err.message || 'Unable to load diet plans.');
      }
    };

    loadPlans();
  }, [user]);

  const handleRefresh = async () => {
    if (!user) return;
    setRefreshing(true);
    setError('');
    try {
      const payload = await apiFetch(user, '/api/Dietician/diet-plans');
      setDietPlans(payload.dietPlans ?? []);
    } catch (err) {
      setError(err.message || 'Unable to refresh diet plans.');
    }
    setRefreshing(false);
  };

  const handleCreatePlan = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      const payload = await apiFetch(user, '/api/Dietician/diet-plans', {
        method: 'POST',
        body: JSON.stringify({
          patientId: Number(patientId),
          dietSummary,
          goal,
          notes,
        }),
      });
      setMessage(payload.message || 'Diet plan created successfully.');
    } catch (err) {
      setError(err.message || 'Unable to create diet plan.');
    }
  };

  return (
    <div className="dashboard-card">
      <TopNav
        title="Dietician Portal"
        description="Create tailored diet plans and coordinate patient nutrition."
      />

      <section className="back-card">
        <div>
          <h2>Quick workflow</h2>
          <p>Build diet plans and monitor patient nutrition, then return to the dashboard to access other care tools.</p>
          <p className="role-badge">Current portal: /dietician</p>
        </div>
        <button type="button" onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
      </section>

      <section className="status-section">
        {message && <div className="success-text">{message}</div>}
        {error && <div className="error-text">{error}</div>}
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <h2>Create Diet Plan</h2>
          <form onSubmit={handleCreatePlan} className="form-grid">
            <label>Patient ID</label>
            <input
              type="number"
              value={patientId}
              min="1"
              onChange={(e) => setPatientId(e.target.value)}
              required
            />
            <label>Diet summary</label>
            <input
              type="text"
              value={dietSummary}
              onChange={(e) => setDietSummary(e.target.value)}
              required
            />
            <label>Goal</label>
            <input
              type="text"
              value={goal}
              onChange={(e) => setGoal(e.target.value)}
              required
            />
            <label>Notes</label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
            <button type="submit">Create Diet Plan</button>
          </form>
        </div>

        <div className="portal-column">
          <h2>Diet Plans</h2>
          <div style={{ marginBottom: 8 }}>
            <button type="button" onClick={handleRefresh} disabled={refreshing}>{refreshing ? 'Refreshing...' : 'Refresh'}</button>
          </div>
          <ul>
            {dietPlans.length === 0 ? (
              <li>No diet plans loaded.</li>
            ) : (
              dietPlans.map((plan) => (
                <li key={plan.planId ?? plan.dietPlanId ?? `${plan.patientId}-${plan.createdAt}`} style={{ marginBottom: 12 }}>
                  <strong>Patient:</strong> {plan.patient?.fullName ?? `ID ${plan.patientId}`}<br />
                  <strong>Created By:</strong> {plan.createdBy?.fullName ?? `ID ${plan.createdById}`}<br />
                  <strong>Goal:</strong> {plan.goal ?? plan.focus}<br />
                  <strong>Summary:</strong> {plan.dietSummary ?? plan.calories}<br />
                  {plan.notes && (<><strong>Notes:</strong> <span style={{ color: '#526273' }}>{plan.notes}</span><br /></>)}
                  <small style={{ color: '#8899a6' }}>{new Date(plan.createdAt).toLocaleString()}</small>
                </li>
              ))
            )}
          </ul>
        </div>
      </section>
    </div>
  );
}
