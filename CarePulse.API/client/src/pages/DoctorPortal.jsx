import { Fragment, useEffect, useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import TopNav from '../components/TopNav';
import { apiFetch } from '../lib/api';

const toLocalDateTimeString = (date) => {
  const pad = (value) => String(value).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

const buildTimeOptions = () => {
  const options = [];
  for (let hour = 6; hour <= 20; hour += 1) {
    options.push(`${String(hour).padStart(2, '0')}:00`);
    options.push(`${String(hour).padStart(2, '0')}:30`);
  }
  options.push('21:00');
  return options;
};

const formatTime = (value) => {
  const [hour, minute] = value.split(':').map(Number);
  const date = new Date();
  date.setHours(hour, minute, 0, 0);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

const getDateForDay = (weekStart, dayOfWeek) => {
  const date = new Date(`${weekStart}T00:00:00`);
  const startDay = date.getDay();
  const diff = ((dayOfWeek - startDay) + 7) % 7;
  date.setDate(date.getDate() + diff);
  return date;
};

const formatDateLabel = (date) => date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });

export default function DoctorPortal() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [schedule, setSchedule] = useState([]);
  const [availabilitySlots, setAvailabilitySlots] = useState([]);
  const [weekStart, setWeekStart] = useState(() => {
    const date = new Date();
    const day = date.getDay();
    const daysUntilNextMonday = ((8 - day) % 7) || 7;
    date.setDate(date.getDate() + daysUntilNextMonday);
    return date.toISOString().slice(0, 10);
  });
  const timeOptions = buildTimeOptions();
  const defaultDaySchedules = [
    { dayOfWeek: 1, label: 'Monday', isEnabled: false, startTime: '09:00', endTime: '12:00' },
    { dayOfWeek: 2, label: 'Tuesday', isEnabled: false, startTime: '09:00', endTime: '12:00' },
    { dayOfWeek: 3, label: 'Wednesday', isEnabled: false, startTime: '09:00', endTime: '12:00' },
    { dayOfWeek: 4, label: 'Thursday', isEnabled: false, startTime: '09:00', endTime: '12:00' },
    { dayOfWeek: 5, label: 'Friday', isEnabled: false, startTime: '09:00', endTime: '12:00' },
    { dayOfWeek: 6, label: 'Saturday', isEnabled: false, startTime: '09:00', endTime: '12:00' },
    { dayOfWeek: 0, label: 'Sunday', isEnabled: false, startTime: '09:00', endTime: '12:00' },
  ];
  const [daySchedules, setDaySchedules] = useState(defaultDaySchedules);
  const enabledDaySchedules = daySchedules.filter((day) => day.isEnabled);
  const [patientId, setPatientId] = useState('1');
  const [prescriptions, setPrescriptions] = useState([]);
  const [medication, setMedication] = useState('');
  const [dosage, setDosage] = useState('1 pill daily');
  const [instructions, setInstructions] = useState('Take after meals.');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [updatingAppointmentId, setUpdatingAppointmentId] = useState(null);
  const [cancelingAppointments, setCancelingAppointments] = useState(new Set());
  const [isLoadingSchedule, setIsLoadingSchedule] = useState(false);
  const [doctorNoteDrafts, setDoctorNoteDrafts] = useState({});
  const [savingDoctorNotesFor, setSavingDoctorNotesFor] = useState(null);
  const [sendingDoctorReminderFor, setSendingDoctorReminderFor] = useState(null);

  if (user?.role !== 'Doctor') {
    return <Navigate to="/dashboard" replace />;
  }

  const loadSchedule = async () => {
    if (!user) return;
    setError('');
    setMessage('');
    setIsLoadingSchedule(true);

    try {
      const payload = await apiFetch(user, '/api/Doctor/schedule');
      setSchedule(payload.schedule ?? []);
    } catch (err) {
      setError(err.message || 'Unable to load schedule.');
    } finally {
      setIsLoadingSchedule(false);
    }
  };

  useEffect(() => {
    if (!user) return;
    loadSchedule();
    loadDoctorSlots();

    const intervalId = setInterval(() => {
      loadSchedule();
      loadDoctorSlots();
    }, 30000);

    return () => clearInterval(intervalId);
  }, [user]);

  const loadDoctorSlots = async () => {
    if (!user) return;
    setError('');

    try {
      const payload = await apiFetch(user, '/api/DoctorAvailability/slots');
      setAvailabilitySlots(payload.slots ?? []);
    } catch (err) {
      setError(err.message || 'Unable to load availability slots.');
    }
  };

  const handleCreateSlot = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    const selectedDays = daySchedules.filter((day) => day.isEnabled);
    if (selectedDays.length === 0) {
      setError('Please enable at least one day for the weekly schedule.');
      return;
    }

    try {
      const payload = await apiFetch(user, '/api/DoctorAvailability/weekly-slots', {
        method: 'POST',
        body: JSON.stringify({
          weekStart: `${weekStart}T00:00:00`,
          daySchedules: daySchedules.map((day) => ({
            dayOfWeek: day.dayOfWeek,
            isEnabled: day.isEnabled,
            startTime: day.startTime,
            endTime: day.endTime,
          })),
        }),
      });

      setMessage(payload.message || 'Weekly availability slots created successfully.');
      setAvailabilitySlots((prev) => [...prev, ...(payload.slots ?? [])].sort((a, b) => new Date(a.slotStart) - new Date(b.slotStart)));
    } catch (err) {
      setError(err.message || 'Unable to create weekly availability slots.');
    }
  };

  const handleCreateStandardSlots = async () => {
    setError('');
    setMessage('');

    const selectedDays = daySchedules.filter((day) => day.isEnabled);
    if (selectedDays.length === 0) {
      setError('Please enable at least one day to create daily availability slots.');
      return;
    }

    const pad = (value) => String(value).padStart(2, '0');
    const getDateForDay = (weekStartDate, dayOfWeek) => {
      const target = new Date(`${weekStartDate}T00:00:00`);
      const startDay = target.getDay();
      const diff = ((dayOfWeek - startDay) + 7) % 7;
      target.setDate(target.getDate() + diff);
      return target;
    };

    try {
      const createdSlots = [];
      for (const day of selectedDays) {
        const targetDate = getDateForDay(weekStart, day.dayOfWeek);
        const targetDateStr = `${targetDate.getFullYear()}-${pad(targetDate.getMonth() + 1)}-${pad(targetDate.getDate())}`;
        const startTime = day.startTime || '11:00';
        const endTime = day.endTime || '18:00';

        const payload = await apiFetch(user, '/api/DoctorAvailability/slots', {
          method: 'POST',
          body: JSON.stringify({
            slotStart: `${targetDateStr}T${startTime}:00`,
            slotEnd: `${targetDateStr}T${endTime}:00`,
          }),
        });
        createdSlots.push(...(payload.slots ?? []));
      }

      setMessage('Daily availability slots created successfully.');
      setAvailabilitySlots((prev) => [...prev, ...createdSlots].sort((a, b) => new Date(a.slotStart) - new Date(b.slotStart)));
    } catch (err) {
      setError(err.message || 'Unable to create daily availability slots.');
    }
  };

  const handleGetPrescriptions = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      const payload = await apiFetch(user, `/api/Doctor/prescriptions?patientId=${patientId}`);
      setPrescriptions(payload.prescriptions ?? []);
      setMessage('Loaded prescriptions successfully.');
    } catch (err) {
      setError(err.message || 'Unable to load prescriptions.');
    }
  };

  const handleCreatePrescription = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      const payload = await apiFetch(user, '/api/Doctor/prescription', {
        method: 'POST',
        body: JSON.stringify({
          patientId: Number(patientId),
          medication,
          dosage,
          instructions,
        }),
      });
      setMessage(payload.message || 'Prescription saved successfully.');
    } catch (err) {
      setError(err.message || 'Unable to create prescription.');
    }
  };

  const handleApproveAppointment = async (appointmentId) => {
    setError('');
    setMessage('');
    setUpdatingAppointmentId(appointmentId);

    try {
      const payload = await apiFetch(user, `/api/Appointment/${appointmentId}/status`, {
        method: 'PUT',
        body: JSON.stringify({ status: 'Approved' }),
      });
      setMessage('Appointment approved successfully.');
      setSchedule((prev) => prev.map((item) => (
        item.appointmentId === appointmentId ? { ...item, status: payload.status } : item
      )));
    } catch (err) {
      setError(err.message || 'Unable to approve appointment.');
    } finally {
      setUpdatingAppointmentId(null);
    }
  };

  const handleCancelAppointmentDoctor = async (appointmentId) => {
    if (!window.confirm('Cancel this appointment for the patient?')) return;
    setError('');
    setMessage('');
    setCancelingAppointments((prev) => new Set(prev).add(appointmentId));
    try {
      await apiFetch(user, `/api/Appointment/${appointmentId}/cancel`, { method: 'POST' });
      setMessage('Appointment cancelled.');
      setSchedule((prev) => prev.map((it) => (it.appointmentId === appointmentId ? { ...it, status: 'Cancelled' } : it)));
      apiFetch(user, '/api/Doctor/schedule').then((payload) => setSchedule(payload.schedule ?? [])).catch(() => {});
      apiFetch(user, '/api/DoctorAvailability/slots').then((payload) => setAvailabilitySlots(payload.slots ?? [])).catch(() => {});
    } catch (err) {
      setError(err.message || 'Unable to cancel appointment.');
    } finally {
      setCancelingAppointments((prev) => {
        const copy = new Set(prev);
        copy.delete(appointmentId);
        return copy;
      });
    }
  };

  const handleSaveDoctorNotes = async (appointmentId) => {
    const notes = (doctorNoteDrafts[appointmentId] || '').trim();
    if (!notes) return;
    setSavingDoctorNotesFor(appointmentId);
    try {
      const payload = await apiFetch(user, `/api/Appointment/${appointmentId}/notes`, {
        method: 'PUT',
        body: JSON.stringify({ notes }),
      });
      setSchedule((prev) => prev.map((item) => (item.appointmentId === appointmentId ? { ...item, notes: payload.notes || notes } : item)));
      setMessage('Appointment notes updated.');
    } catch (err) {
      setError(err.message || 'Unable to update appointment notes.');
    } finally {
      setSavingDoctorNotesFor(null);
    }
  };

  const handleSendDoctorReminder = async (appointmentId) => {
    setSendingDoctorReminderFor(appointmentId);
    try {
      const payload = await apiFetch(user, `/api/Appointment/${appointmentId}/remind`, { method: 'POST' });
      setMessage(payload.message || 'Reminder sent.');
    } catch (err) {
      setError(err.message || 'Unable to send reminder.');
    } finally {
      setSendingDoctorReminderFor(null);
    }
  };

  const groupedAvailabilitySlots = availabilitySlots
    .slice()
    .sort((a, b) => new Date(a.slotStart) - new Date(b.slotStart))
    .reduce((groups, slot) => {
      const day = new Date(slot.slotStart).toLocaleDateString(undefined, {
        weekday: 'long',
        month: 'long',
        day: 'numeric',
      });
      if (!groups[day]) groups[day] = [];
      groups[day].push(slot);
      return groups;
    }, {});

  return (
    <div className="dashboard-card">
      <TopNav
        title="Doctor Portal"
        description="Review your appointments, patients, and prescription workflow."
      />

      <section className="back-card">
        <div>
          <h2>Quick workflow</h2>
          <p>Use this portal to manage patient care and prescriptions, then return to the dashboard to review other tasks.</p>
          <p className="role-badge">Current portal: /doctor</p>
        </div>
        <button type="button" onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
      </section>

      <section className="status-section">
        {message && <div className="success-text">{message}</div>}
        {error && <div className="error-text">{error}</div>}
      </section>
      <section className="status-section">
        <button type="button" onClick={loadSchedule} disabled={isLoadingSchedule}>
          {isLoadingSchedule ? 'Refreshing schedule...' : 'Refresh schedule'}
        </button>
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <h2>Doctor Availability</h2>
          <p>Set your weekly availability once and create 30-minute slots automatically for the selected days. This saves manual entry for each day.</p>
          <form onSubmit={handleCreateSlot} className="form-grid">
            <label>Week start (Monday)</label>
            <input
              type="date"
              value={weekStart}
              onChange={(e) => setWeekStart(e.target.value)}
              required
            />

            <div className="weekly-calendar">
              <div className="calendar-day-grid">
                {daySchedules.map((day, index) => {
                  const dayDate = getDateForDay(weekStart, day.dayOfWeek);
                  return (
                    <div key={day.dayOfWeek} className="calendar-day-card">
                      <label className="day-checkbox">
                        <input
                          type="checkbox"
                          checked={day.isEnabled}
                          onChange={(e) => {
                            const next = [...daySchedules];
                            next[index] = { ...next[index], isEnabled: e.target.checked };
                            setDaySchedules(next);
                          }}
                        />
                        {day.label} · {formatDateLabel(dayDate)}
                      </label>
                      <div className="form-grid calendar-day-controls">
                        <div className="time-control">
                          <label>Start</label>
                          <select
                            value={day.startTime}
                            onChange={(e) => {
                              const next = [...daySchedules];
                              next[index] = { ...next[index], startTime: e.target.value };
                              setDaySchedules(next);
                            }}
                            disabled={!day.isEnabled}
                            required={day.isEnabled}
                          >
                            {timeOptions.map((time) => (
                              <option key={time} value={time}>{formatTime(time)}</option>
                            ))}
                          </select>
                        </div>
                        <div className="time-control">
                          <label>End</label>
                          <select
                            value={day.endTime}
                            onChange={(e) => {
                              const next = [...daySchedules];
                              next[index] = { ...next[index], endTime: e.target.value };
                              setDaySchedules(next);
                            }}
                            disabled={!day.isEnabled}
                            required={day.isEnabled}
                          >
                            {timeOptions.map((time) => (
                              <option key={time} value={time}>{formatTime(time)}</option>
                            ))}
                          </select>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>

              <div className="schedule-summary">
                <h3>Selected availability</h3>
                {enabledDaySchedules.length === 0 ? (
                  <div className="slot-empty">Enable one or more days above to create daily slots.</div>
                ) : (
                  <div className="schedule-summary-grid">
                    {enabledDaySchedules.map((day) => {
                      const dayDate = getDateForDay(weekStart, day.dayOfWeek);
                      return (
                        <div key={day.dayOfWeek} className="schedule-summary-day">
                          <div className="schedule-summary-day-label">{day.label} · {formatDateLabel(dayDate)}</div>
                          <div className="schedule-summary-time">{formatTime(day.startTime)} - {formatTime(day.endTime)}</div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            </div>

            <div className="form-actions">
              <button type="submit">Create weekly slots</button>
              <button type="button" className="secondary-button portal-button" onClick={handleCreateStandardSlots}>
                Create daily slots for selected days
              </button>
            </div>
          </form>

          <h3>Existing availability slots</h3>
          {availabilitySlots.length === 0 ? (
            <div className="slot-empty">No availability slots found.</div>
          ) : (
            Object.entries(groupedAvailabilitySlots).map(([day, daySlots]) => (
              <div key={day} className="slot-day-group">
                <div className="slot-day-heading">{day}</div>
                <div className="slot-grid">
                  {daySlots.map((slot) => {
                    const isBooked = Boolean(slot.isBooked);
                    return (
                      <div key={slot.doctorAvailabilitySlotId ?? slot.id ?? `${slot.doctorId}-${slot.slotStart}`} className={`slot-card ${isBooked ? 'slot-booked' : 'slot-available'}`}>
                        <div><strong>Start:</strong> {new Date(slot.slotStart).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
                        <div><strong>End:</strong> {new Date(slot.slotEnd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
                        <div><strong>Status:</strong> {isBooked ? 'Booked' : 'Available'}</div>
                      </div>
                    );
                  })}
                </div>
              </div>
            ))
          )}
        </div>
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <h2>My Schedule</h2>
          <ul>
            {schedule.length === 0 ? (
              <li>No appointments scheduled.</li>
            ) : (
              schedule.map((item) => (
                <li key={item.appointmentId ?? item.id ?? `${item.doctorId}-${item.appointmentDate}`}>
                  <strong>Date:</strong> {new Date(item.appointmentDate).toLocaleString()}<br />
                  <strong>Patient:</strong> {item.patient?.fullName ?? `ID ${item.patientId}`}<br />
                  <strong>Status:</strong> {item.status}<br />
                  <strong>Notes:</strong> {item.notes || 'No notes'}
                  <div style={{ marginTop: '8px' }}>
                    <input
                      type="text"
                      value={doctorNoteDrafts[item.appointmentId ?? item.id] ?? item.notes ?? ''}
                      onChange={(e) => setDoctorNoteDrafts((prev) => ({ ...prev, [item.appointmentId ?? item.id]: e.target.value }))}
                      placeholder="Update notes"
                      style={{ width: '100%', marginBottom: '6px' }}
                    />
                    <button type="button" onClick={() => handleSaveDoctorNotes(item.appointmentId)} disabled={savingDoctorNotesFor === item.appointmentId} style={{ marginRight: '8px' }}>
                      {savingDoctorNotesFor === item.appointmentId ? 'Saving...' : 'Save Notes'}
                    </button>
                    <button type="button" onClick={() => handleSendDoctorReminder(item.appointmentId)} disabled={sendingDoctorReminderFor === item.appointmentId}>
                      {sendingDoctorReminderFor === item.appointmentId ? 'Sending...' : 'Send Reminder'}
                    </button>
                  </div>
                  {item.status?.toLowerCase() === 'requested' && (
                    <div style={{ marginTop: '8px' }}>
                      <button
                        type="button"
                        onClick={() => handleApproveAppointment(item.appointmentId)}
                        disabled={updatingAppointmentId === item.appointmentId}
                      >
                        {updatingAppointmentId === item.appointmentId ? 'Approving...' : 'Approve'}
                      </button>
                    </div>
                  )}
                  <div style={{ marginTop: '8px' }}>
                    {item.status?.toLowerCase() !== 'cancelled' && (
                      <button
                        type="button"
                        onClick={() => handleCancelAppointmentDoctor(item.appointmentId)}
                        style={{ marginLeft: '8px', backgroundColor: '#f44336', color: 'white', padding: '6px 10px', borderRadius: '4px', border: 'none', cursor: 'pointer' }}
                      >
                        Cancel
                      </button>
                    )}
                  </div>
                </li>
              ))
            )}
          </ul>
        </div>

        <div className="portal-column">
          <h2>Create Prescription</h2>
          <form onSubmit={handleCreatePrescription} className="form-grid">
            <label>Patient ID</label>
            <input
              type="number"
              value={patientId}
              min="1"
              onChange={(e) => setPatientId(e.target.value)}
              required
            />
            <label>Medication</label>
            <input
              type="text"
              value={medication}
              onChange={(e) => setMedication(e.target.value)}
              required
            />
            <label>Dosage</label>
            <input
              type="text"
              value={dosage}
              onChange={(e) => setDosage(e.target.value)}
              required
            />
            <label>Instructions</label>
            <textarea
              value={instructions}
              onChange={(e) => setInstructions(e.target.value)}
            />
            <button type="submit">Save Prescription</button>
          </form>

          <h3>View prescriptions for a patient</h3>
          <form onSubmit={handleGetPrescriptions} className="form-grid">
            <label>Patient ID</label>
            <input
              type="number"
              value={patientId}
              min="1"
              onChange={(e) => setPatientId(e.target.value)}
              required
            />
            <button type="submit">Load Prescriptions</button>
          </form>

          <ul>
            {prescriptions.length === 0 ? (
              <li>No prescriptions loaded.</li>
            ) : (
              prescriptions.map((prescription) => (
                <li key={prescription.prescriptionId ?? prescription.id ?? `${prescription.patientId}-${prescription.createdAt}`}>
                  <strong>Medication:</strong> {prescription.medication}<br />
                  <strong>Dosage:</strong> {prescription.dosage}<br />
                  <strong>Instructions:</strong> {prescription.instructions}
                </li>
              ))
            )}
          </ul>
        </div>
      </section>
    </div>
  );
}
