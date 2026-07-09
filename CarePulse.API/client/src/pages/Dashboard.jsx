import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiFetch } from '../lib/api';
import TopNav from '../components/TopNav';
import Modal from '../components/Modal';
import { useToast } from '../context/ToastContext';

export default function Dashboard() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [receptionistPatientId, setReceptionistPatientId] = useState('');
  const [receptionistDoctorId, setReceptionistDoctorId] = useState('');
  const [receptionistDate, setReceptionistDate] = useState(new Date().toISOString().slice(0, 16));
  const [receptionistNotes, setReceptionistNotes] = useState('Scheduled from dashboard');
  const [receptionistPatients, setReceptionistPatients] = useState([]);
  const [receptionistDoctors, setReceptionistDoctors] = useState([]);
  const [receptionistLoading, setReceptionistLoading] = useState(false);
  const [receptionistLastUpdated, setReceptionistLastUpdated] = useState(null);
  const [receptionistAppointments, setReceptionistAppointments] = useState([]);
  const [receptionistAppointmentsLoading, setReceptionistAppointmentsLoading] = useState(false);
  const [dieticianPatientId, setDieticianPatientId] = useState('1');
  const [dietSummary, setDietSummary] = useState('Balanced nutrition plan');
  const [dietGoal, setDietGoal] = useState('Maintain healthy weight');
  const [dietNotes, setDietNotes] = useState('Created from dashboard');
  const [nutritionistPatientId, setNutritionistPatientId] = useState('1');
  const [canteenPatientId, setCanteenPatientId] = useState('1');

  const { showToast } = useToast();

  const handleError = (error) => {
    const msg = error?.message || 'An error occurred';
    setError(msg);
    setSuccess('');
    setResult(null);
    setLoading(false);
    try { showToast('error', msg); } catch {}
  };

  const [rescheduleModal, setRescheduleModal] = useState(null);
  const [rescheduleIso, setRescheduleIso] = useState('');
  const [rescheduleSubmitting, setRescheduleSubmitting] = useState(false);

  const loadReceptionistAppointments = async () => {
    if (user?.role !== 'Receptionist') return;
    setReceptionistAppointmentsLoading(true);
    try {
      const payload = await apiFetch(user, '/api/Receptionist/appointments');
      const appts = payload.appointments ?? [];
      setReceptionistAppointments(appts);
    } catch (err) {
      handleError(err);
    } finally {
      setReceptionistAppointmentsLoading(false);
    }
  };

  const loadReceptionistData = async (showMessage = false) => {
    if (user?.role !== 'Receptionist') return;
    setReceptionistLoading(true);

    try {
      const patientsPayload = await apiFetch(user, '/api/Receptionist/patients');
      const doctorsPayload = await apiFetch(user, '/api/Receptionist/doctors');

      const patients = patientsPayload.patients ?? [];
      const doctors = doctorsPayload.doctors ?? [];

      setReceptionistPatients(patients);
      setReceptionistDoctors(doctors);

      const currentPatientId = String(receptionistPatientId);
      const currentDoctorId = String(receptionistDoctorId);

      if (!currentPatientId || !patients.some((p) => String(p.patientId ?? p.id) === currentPatientId)) {
        setReceptionistPatientId(patients.length > 0 ? String(patients[0].patientId ?? patients[0].id) : '');
      }

      if (!currentDoctorId || !doctors.some((d) => String(d.userId ?? d.id) === currentDoctorId)) {
        setReceptionistDoctorId(doctors.length > 0 ? String(doctors[0].userId ?? doctors[0].id) : '');
      }

      if (showMessage) {
        setSuccess('Receptionist counts refreshed.');
      }
      setReceptionistLastUpdated(Date.now());
    } catch (err) {
      handleError(err);
    } finally {
      setReceptionistLoading(false);
    }
  };

  useEffect(() => {
    loadReceptionistData();
    loadReceptionistAppointments();
  }, [user]);

  const handleRoleAction = async (action) => {
    if (!user) return;
    setError('');
    setLoading(true);

    try {
      let payload;

      switch (action) {
        case 'adminUsers':
          payload = await apiFetch(user, '/api/Admin/users');
          break;
        case 'adminRolePatients':
          payload = await apiFetch(user, '/api/Admin/users/role?role=Patient');
          break;
        case 'patientAppointments':
          payload = await apiFetch(user, `/api/Patient/appointments?patientId=${user.userId}`);
          break;
        case 'patientMealOrders':
          payload = await apiFetch(user, `/api/Patient/orders?patientId=${user.userId}`);
          break;
        case 'doctorSchedule':
          payload = await apiFetch(user, `/api/Doctor/schedule?doctorId=${user.userId}`);
          break;
        case 'doctorPrescriptions':
          payload = await apiFetch(user, `/api/Doctor/prescriptions?patientId=${nutritionistPatientId}`);
          break;
        case 'receptionistSchedule':
          payload = await apiFetch(user, '/api/Receptionist/appointments', {
            method: 'POST',
            body: JSON.stringify({
              patientId: Number(receptionistPatientId),
              doctorId: Number(receptionistDoctorId),
              appointmentDate: new Date(receptionistDate).toISOString(),
              notes: receptionistNotes,
            }),
          });
          showToast('success', payload?.message ?? 'Appointment scheduled successfully.');
          await loadReceptionistData();
          break;
        case 'dieticianCreatePlan':
          payload = await apiFetch(user, '/api/Dietician/diet-plans', {
            method: 'POST',
            body: JSON.stringify({
              patientId: Number(dieticianPatientId),
              dietSummary,
              goal: dietGoal,
              notes: dietNotes,
            }),
          });
          break;
        case 'nutritionistPlans':
          payload = await apiFetch(user, `/api/Nutritionist/plans/patient?patientId=${nutritionistPatientId}`);
          break;
        case 'canteenMenu':
          payload = await apiFetch(user, '/api/Canteen/menu');
          break;
        case 'canteenOrders':
          payload = await apiFetch(user, `/api/Canteen/orders?patientId=${canteenPatientId}`);
          break;
        default:
          throw new Error('Unknown action');
      }

      setResult(payload);
    } catch (err) {
      handleError(err);
    } finally {
      setLoading(false);
    }
  };

  const portalRoutes = {
    Admin: '/admin/staff',
    Patient: '/patient',
    Doctor: '/doctor',
    Receptionist: '/receptionist',
    Dietician: '/dietician',
    Nutritionist: '/nutritionist',
    Canteen: '/canteen',
  };

  const portalDescriptions = {
    Patient: 'Book appointments, track orders and manage your care from one place.',
    Doctor: 'Review your schedule, create prescriptions, and manage patient care.',
    Receptionist: 'Schedule appointments and view the patient waiting list.',
    Dietician: 'Create diet plans and manage meal guidance for patients.',
    Nutritionist: 'Review patient nutrition plans and follow up on progress.',
    Canteen: 'Manage the meal menu and review patient orders.',
  };

  const rolePortalRoute = user?.role ? portalRoutes[user.role] : undefined;
  const rolePortalDescription = user?.role ? portalDescriptions[user.role] : undefined;

  const getPayloadField = (payload, key) => {
    if (!payload || typeof payload !== 'object') return undefined;
    return payload[key] ?? payload[key.charAt(0).toLowerCase() + key.slice(1)];
  };

  const renderOrders = (orders) => {
    if (!Array.isArray(orders) || orders.length === 0) {
      return <p>No orders found.</p>;
    }

    return (
      <table className="result-table">
        <thead>
          <tr>
            <th>Order ID</th>
            <th>Patient</th>
            <th>Item</th>
            <th>Quantity</th>
            <th>Status</th>
            <th>Created</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => (
            <tr key={order.mealOrderId ?? order.id ?? `${order.menuItemId}-${order.quantity}-${order.patientId}`}>
              <td>{order.mealOrderId ?? order.id}</td>
              <td>{order.patient?.fullName ?? order.patient?.email ?? `#${order.patientId}`}</td>
              <td>{order.menuItem?.name ?? `Item ${order.menuItemId}`}</td>
              <td>{order.quantity}</td>
              <td>{order.status ?? 'Pending'}</td>
              <td>{order.createdAt ? new Date(order.createdAt).toLocaleString() : '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  };

  const renderResultDisplay = () => {
    if (!result) return null;

    const orders = getPayloadField(result, 'Orders') || getPayloadField(result, 'orders');
    if (Array.isArray(orders)) {
      return (
        <div>
          <p>{result.message || 'Orders returned.'}</p>
          {renderOrders(orders)}
        </div>
      );
    }

    if (result.Appointments) {
      return (
        <table className="result-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Doctor</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {result.Appointments.length === 0 ? (
              <tr>
                <td colSpan="3">No appointments found.</td>
              </tr>
            ) : (
              result.Appointments.map((appointment) => (
                <tr key={appointment.appointmentId ?? appointment.id}>
                  <td>{new Date(appointment.appointmentDate).toLocaleString()}</td>
                  <td>{appointment.doctorName ?? appointment.doctor?.fullName ?? appointment.doctorId}</td>
                  <td>{appointment.status ?? 'Pending'}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      );
    }

    if (result.MenuItems) {
      return (
        <table className="result-table">
          <thead>
            <tr>
              <th>Item</th>
              <th>Price</th>
              <th>Description</th>
            </tr>
          </thead>
          <tbody>
            {result.MenuItems.length === 0 ? (
              <tr>
                <td colSpan="3">No menu items found.</td>
              </tr>
            ) : (
              result.MenuItems.map((item) => (
                <tr key={item.menuItemId ?? item.id}>
                  <td>{item.name}</td>
                  <td>₹{item.price}</td>
                  <td>{item.description}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      );
    }

    if (result.Doctors) {
      return (
        <ul>
          {result.Doctors.length === 0 ? (
            <li>No doctors found.</li>
          ) : (
            result.Doctors.map((doctor) => (
              <li key={doctor.userId ?? doctor.id}>
                <strong>{doctor.fullName}</strong> ({doctor.email})<br />
                <small>{doctor.phone}</small>
              </li>
            ))
          )}
        </ul>
      );
    }

    if (typeof result === 'object') {
      return <pre>{JSON.stringify(result, null, 2)}</pre>;
    }

    return <p>{String(result)}</p>;
  };

  return (
    <div className="dashboard-card">
      <TopNav
        title="CarePulse Dashboard"
        description="Your central workflow hub across roles."
      />

      <section className="dashboard-hero">
        <div className="hero-grid">
          <div className="hero-summary">
            <div className="hero-badge-row">
              <span className="hero-pill">🩺 CarePulse Intelligence</span>
              <span className="hero-pill hero-pill-soft">⚡ Live care workflows</span>
            </div>
            <p className="eyebrow">CarePulse Overview</p>
            <h2 className="page-title">Hello, {user?.fullName}</h2>
            <p className="page-subtitle">Your secure hospital coordination center for appointments, meals, and patient care workflows.</p>
            <div className="hero-metrics">
              <div className="metric-chip">
                <strong>24/7</strong>
                <span>Operational</span>
              </div>
              <div className="metric-chip">
                <strong>HIPAA</strong>
                <span>Ready</span>
              </div>
              <div className="metric-chip">
                <strong>Fast</strong>
                <span>Responses</span>
              </div>
            </div>
          </div>

          <div className="hero-visual">
            <div className="hero-orb" />
            <div className="hero-panel">
              <div className="hero-panel-header">
                <span className="hero-dot" />
                <span className="hero-dot" />
                <span className="hero-dot" />
              </div>
              <div className="hero-panel-body">
                <div className="mini-stat">
                  <span>Patients</span>
                  <strong>128</strong>
                </div>
                <div className="mini-stat">
                  <span>Today</span>
                  <strong>14</strong>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="hero-card-grid">
          <div className="hero-card">
            <h4>Assigned role</h4>
            <strong>{user?.role}</strong>
          </div>
          <div className="hero-card">
            <h4>Account</h4>
            <strong>{user?.email}</strong>
          </div>
          <div className="hero-card">
            <h4>Portal</h4>
            <strong>{rolePortalRoute ? rolePortalRoute.replace('/', '') : 'Dashboard'}</strong>
          </div>
        </div>
      </section>

      {user?.role === 'Receptionist' && (
        <section className="portal-card">
          <h3>Receptionist Summary</h3>
          <div className="hero-card-grid">
            <div className="hero-card">
              <h4>Waiting patients</h4>
              <strong>{receptionistPatients.length}</strong>
            </div>
            <div className="hero-card">
              <h4>Available doctors</h4>
              <strong>{receptionistDoctors.length}</strong>
            </div>
          </div>
          <div className="hero-card-meta">
            <small>
              {receptionistLastUpdated
                ? `Last refreshed ${new Date(receptionistLastUpdated).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`
                : 'Last refreshed: never'}
            </small>
          </div>
          <button
            type="button"
            className="portal-button portal-button-sm"
            onClick={() => loadReceptionistData(true)}
            disabled={receptionistLoading}
          >
            {receptionistLoading ? 'Refreshing…' : 'Refresh counts'}
          </button>
        </section>
      )}

      {rolePortalRoute && (
        <section className="portal-card">
          <h3>{user.role} Portal</h3>
          <p>{rolePortalDescription}</p>
          <button className="portal-button" onClick={() => navigate(rolePortalRoute)}>
            Open {user.role} Portal
          </button>
        </section>
      )}

      {user?.role === 'Receptionist' && (
        <section className="portal-card">
          <h3>Quick Receptionist Scheduler</h3>
          <p>Schedule appointments using live patient and doctor lists.</p>
          <form
            className="form-grid"
            onSubmit={(e) => {
              e.preventDefault();
              handleRoleAction('receptionistSchedule');
            }}
          >
            <label>Patient</label>
            <select
              value={receptionistPatientId}
              onChange={(e) => setReceptionistPatientId(e.target.value)}
              required
            >
              <option value="" disabled>Select a patient</option>
              {receptionistPatients.map((patient) => (
                <option key={patient.patientId ?? patient.id} value={patient.patientId ?? patient.id}>
                  {patient.name} ({patient.status})
                </option>
              ))}
            </select>

            <label>Doctor</label>
            <select
              value={receptionistDoctorId}
              onChange={(e) => setReceptionistDoctorId(e.target.value)}
              required
            >
              <option value="" disabled>Select a doctor</option>
              {receptionistDoctors.map((doctor) => (
                <option key={doctor.userId ?? doctor.id} value={doctor.userId ?? doctor.id}>
                  {doctor.fullName}
                </option>
              ))}
            </select>

            <label>Appointment Date</label>
            <input
              type="datetime-local"
              value={receptionistDate}
              onChange={(e) => setReceptionistDate(e.target.value)}
              required
            />

            <label>Notes</label>
            <input
              type="text"
              value={receptionistNotes}
              onChange={(e) => setReceptionistNotes(e.target.value)}
            />

            <button type="submit" className="portal-button portal-button-sm">
              Schedule Appointment
            </button>
          </form>
        </section>
      )}

      {user?.role === 'Receptionist' && (
        <section className="portal-card">
          <h3>Scheduled Appointments</h3>
          {receptionistAppointmentsLoading ? (
            <p>Loading appointments...</p>
          ) : receptionistAppointments.length === 0 ? (
            <p>No appointments found.</p>
          ) : (
            <table className="result-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Patient</th>
                  <th>Doctor</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {receptionistAppointments.map((a) => (
                  <tr key={a.appointmentId ?? a.id}>
                    <td>{new Date(a.appointmentDate).toLocaleString()}</td>
                    <td>{a.patientName ?? a.patient?.fullName ?? `#${a.patientId}`}</td>
                    <td>{a.doctorName ?? a.doctor?.fullName ?? a.doctorId}</td>
                    <td>{a.status}</td>
                    <td>
                      <button
                        type="button"
                        className="portal-button portal-button-sm"
                        onClick={() => {
                          setRescheduleModal(a);
                          setRescheduleIso(new Date(a.appointmentDate).toISOString().slice(0,16));
                        }}
                      >
                        Reschedule
                      </button>
                      <button
                        type="button"
                        className="portal-button portal-button-sm"
                        onClick={async () => {
                          if (!window.confirm('Cancel this appointment?')) return;
                          try {
                            await apiFetch(user, `/api/Appointment/${a.appointmentId}/cancel`, { method: 'POST' });
                            showToast('success', 'Appointment cancelled.');
                            await loadReceptionistData();
                            await loadReceptionistAppointments();
                          } catch (err) {
                            handleError(err);
                          }
                        }}
                      >
                        Cancel
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </section>
      )}

      {rescheduleModal && (
        <Modal
          title={`Reschedule appointment #${rescheduleModal.appointmentId}`}
          onClose={() => setRescheduleModal(null)}
          onConfirm={async () => {
            if (!rescheduleIso) return;
            setRescheduleSubmitting(true);
            try {
              await apiFetch(user, `/api/Appointment/${rescheduleModal.appointmentId}/reschedule`, {
                method: 'POST',
                body: JSON.stringify({ appointmentDate: new Date(rescheduleIso).toISOString() }),
              });
              showToast('success', 'Appointment rescheduled.');
              setRescheduleModal(null);
              await loadReceptionistData();
              await loadReceptionistAppointments();
            } catch (err) {
              handleError(err);
            } finally {
              setRescheduleSubmitting(false);
            }
          }}
          confirmLabel={rescheduleSubmitting ? 'Saving…' : 'Save'}
          confirmDisabled={rescheduleSubmitting}
        >
          <label>New date</label>
          <input
            type="datetime-local"
            value={rescheduleIso}
            onChange={(e) => setRescheduleIso(e.target.value)}
          />
        </Modal>
      )}

      <section className="result-section">
        <h3>API response</h3>
        {loading && <p>Loading...</p>}
        {error && <div className="error-text">{error}</div>}
        {success && <div className="success-text">{success}</div>}
        {result && renderResultDisplay()}
      </section>
    </div>
  );
}
