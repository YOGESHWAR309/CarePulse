import { useEffect, useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import TopNav from '../components/TopNav';
import { useToast } from '../context/ToastContext';
import { apiFetch } from '../lib/api';

export default function ReceptionistPortal() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [patientId, setPatientId] = useState('');
  const [doctorId, setDoctorId] = useState('');
  const [date, setDate] = useState(new Date().toISOString().slice(0, 16));
  const [notes, setNotes] = useState('Scheduled from receptionist portal.');
  const [waitingPatients, setWaitingPatients] = useState([]);
  const [availableDoctors, setAvailableDoctors] = useState([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [lastUpdated, setLastUpdated] = useState(null);

  if (user?.role !== 'Receptionist') {
    return <Navigate to="/dashboard" replace />;
  }

  const { showToast } = useToast();

  const loadData = async (showMessage = false) => {
    if (!user) return;

    setLoading(true);
    setError('');
    setMessage('');

    try {
      const patientPayload = await apiFetch(user, '/api/Receptionist/patients');
      const doctorPayload = await apiFetch(user, '/api/Receptionist/doctors');

      const patients = patientPayload.patients ?? [];
      const doctors = doctorPayload.doctors ?? [];

      setWaitingPatients(patients);
      setAvailableDoctors(doctors);

      if (!patientId || !patients.some((p) => String(p.patientId ?? p.id) === String(patientId))) {
        setPatientId(patients.length > 0 ? String(patients[0].patientId ?? patients[0].id) : '');
      }
      if (!doctorId || !doctors.some((d) => String(d.userId ?? d.id) === String(doctorId))) {
        setDoctorId(doctors.length > 0 ? String(doctors[0].userId ?? doctors[0].id) : '');
      }

      if (showMessage) {
        showToast('success', 'Receptionist lists refreshed.');
        setLastUpdated(Date.now());
      }
    } catch (err) {
      const msg = err.message || 'Unable to load receptionist data.';
      setError(msg);
      showToast('error', msg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [user]);

  const handleSchedule = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');
    setLoading(true);

    try {
      const payload = await apiFetch(user, '/api/Receptionist/appointments', {
        method: 'POST',
        body: JSON.stringify({
          patientId: Number(patientId),
          doctorId: Number(doctorId),
          appointmentDate: new Date(date).toISOString(),
          notes,
        }),
      });
      const msg = payload.message || 'Appointment scheduled successfully.';
      setMessage(msg);
      showToast('success', msg);
      await loadData();
    } catch (err) {
      const msg = err.message || 'Unable to schedule appointment.';
      setError(msg);
      showToast('error', msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="dashboard-card">
      <TopNav
        title="Receptionist Portal"
        description="Manage appointments and patient check-ins from one place."
      />

      <section className="back-card">
        <div>
          <h2>Quick workflow</h2>
          <p>Schedule appointments and view waiting patients, then head back to the dashboard to manage other requests.</p>
          <p className="role-badge">Current portal: /receptionist</p>
        </div>
        <button type="button" onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
      </section>

      <section className="status-section">
        {message && <div className="success-text">{message}</div>}
        {error && <div className="error-text">{error}</div>}
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <div className="section-header">
            <h2>Schedule Appointment</h2>
            <button
              type="button"
              className="portal-button portal-button-sm"
              onClick={() => loadData(true)}
              disabled={loading}
            >
              {loading ? 'Refreshing…' : 'Refresh lists'}
            </button>
          </div>
          <p className="small-text">
            {lastUpdated
              ? `Last refreshed ${new Date(lastUpdated).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`
              : 'Last refreshed: never'}
          </p>
          <form onSubmit={handleSchedule} className="form-grid">
            <label>Patient</label>
            <select
              value={patientId}
              onChange={(e) => setPatientId(e.target.value)}
              required
            >
              <option value="" disabled>Select a patient</option>
              {waitingPatients.map((patient) => (
                <option key={patient.patientId ?? patient.id} value={patient.patientId ?? patient.id}>
                  {patient.name} ({patient.status})
                </option>
              ))}
            </select>

            <label>Doctor</label>
            <select
              value={doctorId}
              onChange={(e) => setDoctorId(e.target.value)}
              required
            >
              <option value="" disabled>Select a doctor</option>
              {availableDoctors.map((doctor) => (
                <option key={doctor.userId ?? doctor.id} value={doctor.userId ?? doctor.id}>
                  {doctor.fullName} {doctor.doctorCategory ? `- ${doctor.doctorCategory}` : ''}
                </option>
              ))}
            </select>

            <label>Appointment Date</label>
            <input
              type="datetime-local"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              required
            />
            <label>Notes</label>
            <input
              type="text"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
            <button type="submit" className="portal-button portal-button-sm" disabled={loading}>
              {loading ? 'Working…' : 'Schedule Appointment'}
            </button>
          </form>
        </div>

        <div className="portal-column">
          <h2>Current Waiting Patients</h2>
          <ul>
            {waitingPatients.length === 0 ? (
              <li>No waiting patients found.</li>
            ) : (
              waitingPatients.map((patient) => (
                <li key={patient.patientId ?? patient.id}>
                  <strong>Name:</strong> {patient.name}<br />
                  <strong>Status:</strong> {patient.status}
                </li>
              ))
            )}
          </ul>
        </div>
      </section>
    </div>
  );
}
