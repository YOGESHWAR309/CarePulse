import { useEffect, useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiFetch } from '../lib/api';
import TopNav from '../components/TopNav';

export default function PatientPortal() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  if (user?.role !== 'Patient') {
    return <Navigate to="/dashboard" replace />;
  }
  const [appointments, setAppointments] = useState([]);
  const [mealOrders, setMealOrders] = useState([]);
  const [menuItems, setMenuItems] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [selectedDoctorCategory, setSelectedDoctorCategory] = useState('All');
  const [selectedFoodCategory, setSelectedFoodCategory] = useState('All');
  const [appointmentDoctorId, setAppointmentDoctorId] = useState('');
  const [slots, setSlots] = useState([]);
  const [selectedSlotId, setSelectedSlotId] = useState('');
  const [appointmentNotes, setAppointmentNotes] = useState('Requested from patient portal.');
  const [menuItemId, setMenuItemId] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [cancelingAppointments, setCancelingAppointments] = useState(new Set());
  const [noteDrafts, setNoteDrafts] = useState({});
  const [savingNotesFor, setSavingNotesFor] = useState(null);
  const [sendingReminderFor, setSendingReminderFor] = useState(null);

  useEffect(() => {
    if (!user) return;

    const loadData = async () => {
      try {
        const appointmentsResult = await apiFetch(user, '/api/Patient/appointments');
        const ordersResult = await apiFetch(user, '/api/Patient/orders');
        const menuResult = await apiFetch(user, '/api/Meal/menu');
        const doctorResult = await apiFetch(user, '/api/Patient/doctors');

        setAppointments(appointmentsResult.appointments ?? []);
        setMealOrders(ordersResult.orders ?? []);
        setMenuItems(Array.isArray(menuResult) ? menuResult : menuResult.menu ?? []);
        setDoctors(doctorResult.doctors ?? []);

        if (!appointmentDoctorId && doctorResult.doctors?.length > 0) {
          setAppointmentDoctorId(String(doctorResult.doctors[0].userId));
        }

        if (!menuItemId && Array.isArray(menuResult) && menuResult.length > 0) {
          setMenuItemId(String(menuResult[0].menuItemId ?? menuResult[0].id));
        }

        setMessage('Loaded patient portal data successfully.');
      } catch (err) {
        setError(err.message || 'Unable to load patient data.');
      }
    };

    loadData();
  }, [user]);

  useEffect(() => {
    if (!user || !appointmentDoctorId) return;

    const loadSlots = async () => {
      try {
        const payload = await apiFetch(user, `/api/PatientAppointment/available-slots/${appointmentDoctorId}`);
        const allSlots = payload.slots ?? [];
        setSlots(allSlots);

        const firstAvailable = allSlots.find((slot) => !slot.isBooked);
        const hasCurrentSelection = selectedSlotId && allSlots.some((slot) => String(slot.doctorAvailabilitySlotId) === selectedSlotId && !slot.isBooked);
        setSelectedSlotId(hasCurrentSelection ? selectedSlotId : firstAvailable ? String(firstAvailable.doctorAvailabilitySlotId) : '');
      } catch (err) {
        setError(err.message || 'Unable to load slots.');
      }
    };

    setSelectedSlotId('');
    loadSlots();
  }, [user, appointmentDoctorId]);

  const handleCreateAppointment = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    if (!selectedSlotId) {
      setError('Please select an available slot before booking.');
      return;
    }

    try {
      const result = await apiFetch(user, '/api/PatientAppointment/book-slot', {
        method: 'POST',
        body: JSON.stringify({
          slotId: Number(selectedSlotId),
          notes: appointmentNotes,
        }),
      });

      setMessage(result.message || 'Appointment requested successfully.');
      setAppointments((prev) => [...prev, result.appointment]);

      const bookedSlotId = selectedSlotId;
      setSlots((prev) => prev.filter((slot) => String(slot.doctorAvailabilitySlotId) !== bookedSlotId));
      setSelectedSlotId('');

      // Refresh slots after successful booking so UI reflects booked status.
      try {
        const slotsPayload = await apiFetch(user, `/api/PatientAppointment/available-slots/${appointmentDoctorId}`);
        const allSlots = slotsPayload.slots ?? [];
        setSlots(allSlots);
        const firstAvailable = allSlots.find((slot) => !slot.isBooked);
        setSelectedSlotId(firstAvailable ? String(firstAvailable.doctorAvailabilitySlotId) : '');
      } catch (reloadErr) {
        // non-fatal: keep existing slots if reload fails
      }
    } catch (err) {
      setError(err.message || 'Appointment request failed.');
    }
  };

  const handleCancelAppointment = async (appointmentId) => {
    if (!window.confirm('Cancel this appointment?')) return;
    setError('');
    setMessage('');
    setCancelingAppointments((prev) => new Set(prev).add(appointmentId));
    try {
      await apiFetch(user, `/api/Appointment/${appointmentId}/cancel`, { method: 'POST' });
      setMessage('Appointment cancelled.');
      setAppointments((prev) => prev.map((a) => (a.appointmentId === appointmentId || a.id === appointmentId) ? { ...a, status: 'Cancelled' } : a));
      apiFetch(user, '/api/Patient/appointments').then((appointmentsResult) => setAppointments(appointmentsResult.appointments ?? [])).catch(() => {});
      if (appointmentDoctorId) {
        const slotsPayload = await apiFetch(user, `/api/PatientAppointment/available-slots/${appointmentDoctorId}`);
        const allSlots = slotsPayload.slots ?? [];
        setSlots(allSlots);
      }
    } catch (err) {
      setError(err.message || 'Unable to cancel appointment.');
    }
    finally {
      setCancelingAppointments((prev) => {
        const copy = new Set(prev);
        copy.delete(appointmentId);
        return copy;
      });
    }
  };

  const handleSaveNotes = async (appointmentId) => {
    const notes = (noteDrafts[appointmentId] || '').trim();
    if (!notes) return;
    setSavingNotesFor(appointmentId);
    try {
      const payload = await apiFetch(user, `/api/Appointment/${appointmentId}/notes`, {
        method: 'PUT',
        body: JSON.stringify({ notes }),
      });
      setAppointments((prev) => prev.map((appointment) => (appointment.appointmentId === appointmentId || appointment.id === appointmentId) ? { ...appointment, notes: payload.notes || notes } : appointment));
      setMessage('Appointment notes updated.');
    } catch (err) {
      setError(err.message || 'Unable to update appointment notes.');
    } finally {
      setSavingNotesFor(null);
    }
  };

  const handleSendReminder = async (appointmentId) => {
    setSendingReminderFor(appointmentId);
    try {
      const payload = await apiFetch(user, `/api/Appointment/${appointmentId}/remind`, { method: 'POST' });
      setMessage(payload.message || 'Reminder sent.');
    } catch (err) {
      setError(err.message || 'Unable to send reminder.');
    } finally {
      setSendingReminderFor(null);
    }
  };

  const handlePlaceMealOrder = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      const result = await apiFetch(user, '/api/Meal/orders', {
        method: 'POST',
        body: JSON.stringify({
          patientId: user.userId,
          menuItemId: Number(menuItemId),
          quantity: Number(quantity),
        }),
      });

      setMessage(result.message || 'Meal order placed successfully.');
      setMealOrders((prev) => [...prev, result.order]);
    } catch (err) {
      setError(err.message || 'Meal order failed.');
    }
  };

  const handleActivateMealOrder = async (orderId) => {
    setError('');
    setMessage('');

    try {
      const result = await apiFetch(user, `/api/Patient/orders/${orderId}/activate`, {
        method: 'PUT',
      });

      setMessage(result.message || 'Meal order activated successfully.');
      setMealOrders((prev) => prev.map((order) => 
        (order.mealOrderId ?? order.id) === orderId ? { ...order, status: 'Confirmed' } : order
      ));
    } catch (err) {
      setError(err.message || 'Unable to activate meal order.');
    }
  };

  const handleRejectMealOrder = async (orderId) => {
    setError('');
    setMessage('');

    try {
      const result = await apiFetch(user, `/api/Patient/orders/${orderId}/reject`, {
        method: 'PUT',
      });

      setMessage(result.message || 'Meal order rejected successfully.');
      setMealOrders((prev) => prev.map((order) => 
        (order.mealOrderId ?? order.id) === orderId ? { ...order, status: 'Rejected' } : order
      ));
    } catch (err) {
      setError(err.message || 'Unable to reject meal order.');
    }
  };

  const groupedSlots = slots.reduce((groups, slot) => {
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
        title="Patient Portal"
        description="Book appointments and order meals from your care dashboard."
      />

      <section className="back-card">
        <div>
          <h2>Quick workflow</h2>
          <p>Manage your appointments and orders here, then return to the dashboard anytime to switch tools.</p>
          <p className="role-badge">Current portal: /patient</p>
        </div>
        <button type="button" onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
      </section>

      <section className="status-section">
        {message && <div className="success-text">{message}</div>}
        {error && <div className="error-text">{error}</div>}
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <h2>Book doctor appointment</h2>
          <form onSubmit={handleCreateAppointment} className="form-grid">
            <label>Doctor Category</label>
            <select value={selectedDoctorCategory} onChange={(e) => setSelectedDoctorCategory(e.target.value)}>
              <option value="All">All</option>
              <option value="General Physician">General Physician</option>
              <option value="Cardiologist">Cardiologist</option>
              <option value="Pediatrician">Pediatrician</option>
              <option value="Dermatologist">Dermatologist</option>
            </select>

            <label>Doctor</label>
            <select
              value={appointmentDoctorId}
              onChange={(e) => setAppointmentDoctorId(e.target.value)}
              required
            >
              <option value="" disabled>Select a doctor</option>
              {doctors.filter((doctor) => selectedDoctorCategory === 'All' || doctor.doctorCategory === selectedDoctorCategory).map((doctor) => (
                <option key={doctor.userId} value={doctor.userId}>
                  {doctor.fullName} · {doctor.doctorCategory || 'General'} · {doctor.doctorQualification || 'Qualification'} · {doctor.doctorSpecialty || 'Specialty'}
                </option>
              ))}
            </select>

            <label>Available slot</label>
            {slots.length === 0 ? (
              <div className="slot-empty">No slots are currently available for this doctor.</div>
            ) : (
              <div className="slot-day-list">
                {Object.entries(groupedSlots).map(([day, daySlots]) => (
                  <div key={day} className="slot-day-group">
                    <div className="slot-day-heading">{day}</div>
                    <div className="slot-grid">
                      {daySlots.map((slot) => {
                        const start = new Date(slot.slotStart);
                        const end = new Date(slot.slotEnd);
                        const isBooked = Boolean(slot.isBooked);
                        const isSelected = String(slot.doctorAvailabilitySlotId) === selectedSlotId;
                        const isAvailable = !isBooked;
                        return (
                          <button
                            key={slot.doctorAvailabilitySlotId}
                            type="button"
                            className={`slot-card ${isBooked ? 'slot-booked' : isSelected ? 'slot-selected' : 'slot-available'}`}
                            onClick={() => isAvailable && setSelectedSlotId(String(slot.doctorAvailabilitySlotId))}
                            disabled={!isAvailable}
                          >
                            <div className="slot-time">
                              {start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - {end.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                            </div>
                            <div className="slot-status">{isBooked ? 'Booked' : isSelected ? 'Selected' : 'Available'}</div>
                          </button>
                        );
                      })}
                    </div>
                  </div>
                ))}
              </div>
            )}
            {slots.length > 0 && !slots.some((slot) => !slot.isBooked) && (
              <div className="error-text">All displayed slots are booked. Please choose another doctor or check back later.</div>
            )}

            <label>Notes</label>
            <input
              type="text"
              value={appointmentNotes}
              onChange={(e) => setAppointmentNotes(e.target.value)}
            />

            <button type="submit">Request Appointment</button>
          </form>

          <h3>My appointments</h3>
          <ul>
            {appointments.length === 0 ? (
              <li>No appointments found.</li>
            ) : (
              appointments.map((appointment) => (
                <li key={appointment.appointmentId ?? appointment.id ?? appointment.patientId + appointment.appointmentDate}>
                  <strong>Date:</strong> {new Date(appointment.appointmentDate).toLocaleString()}<br />
                  <strong>Doctor:</strong> {appointment.doctor?.fullName ?? `ID ${appointment.doctorId}`}<br />
                  <strong>Status:</strong> {appointment.status ?? 'Pending'}<br />
                  <strong>Notes:</strong> {appointment.notes || 'No notes'}
                  <div style={{ marginTop: '8px' }}>
                    <input
                      type="text"
                      value={noteDrafts[appointment.appointmentId ?? appointment.id] ?? appointment.notes ?? ''}
                      onChange={(e) => setNoteDrafts((prev) => ({ ...prev, [appointment.appointmentId ?? appointment.id]: e.target.value }))}
                      placeholder="Update notes"
                      style={{ width: '100%', marginBottom: '6px' }}
                    />
                    <button type="button" onClick={() => handleSaveNotes(appointment.appointmentId ?? appointment.id)} disabled={savingNotesFor === (appointment.appointmentId ?? appointment.id)} style={{ marginRight: '8px' }}>
                      {savingNotesFor === (appointment.appointmentId ?? appointment.id) ? 'Saving...' : 'Save Notes'}
                    </button>
                    <button type="button" onClick={() => handleSendReminder(appointment.appointmentId ?? appointment.id)} disabled={sendingReminderFor === (appointment.appointmentId ?? appointment.id)}>
                      {sendingReminderFor === (appointment.appointmentId ?? appointment.id) ? 'Sending...' : 'Send Reminder'}
                    </button>
                    {appointment.status?.toLowerCase() !== 'cancelled' && (
                      <button type="button" onClick={() => handleCancelAppointment(appointment.appointmentId ?? appointment.id)} style={{ backgroundColor: '#f44336', color: 'white', padding: '6px 10px', borderRadius: '4px', border: 'none', cursor: 'pointer', marginTop: '6px', marginLeft: '8px' }}>
                        Cancel Appointment
                      </button>
                    )}
                  </div>
                </li>
              ))
            )}
          </ul>
        </div>

        <div className="portal-column">
          <h2>Order meal</h2>
          <form onSubmit={handlePlaceMealOrder} className="form-grid">
            <label>Food Category</label>
            <select value={selectedFoodCategory} onChange={(e) => setSelectedFoodCategory(e.target.value)}>
              <option value="All">All</option>
              <option value="Vegetarian">Vegetarian</option>
              <option value="Non-Vegetarian">Non-Vegetarian</option>
              <option value="Diabetic">Diabetic</option>
              <option value="Low-Calorie">Low-Calorie</option>
            </select>

            <label>Menu item</label>
            <select value={menuItemId} onChange={(e) => setMenuItemId(e.target.value)} required>
              <option value="" disabled>Select a menu item</option>
              {menuItems.filter((item) => selectedFoodCategory === 'All' || item.foodCategory === selectedFoodCategory).map((item) => (
                <option key={item.menuItemId ?? item.id} value={item.menuItemId ?? item.id}>
                  {item.name} - ₹{item.price} ({item.foodCategory || 'General'})
                </option>
              ))}
            </select>

            <label>Quantity</label>
            <input
              type="number"
              min="1"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              required
            />

            <button type="submit">Place Meal Order</button>
          </form>

          <h3>My meal orders</h3>
          <ul>
            {mealOrders.length === 0 ? (
              <li>No meal orders found.</li>
            ) : (
              mealOrders.map((order) => (
                <li key={order.mealOrderId ?? order.id ?? order.menuItemId + order.quantity}>
                  <strong>Menu Item:</strong> {order.menuItem?.name ?? `ID ${order.menuItemId}`}<br />
                  <strong>Quantity:</strong> {order.quantity}<br />
                  <strong>Status:</strong> {order.status ?? 'Pending'}
                  <div style={{ marginTop: '8px' }}>
                    {(order.status === 'Pending' || !order.status) && (
                      <>
                        <button
                          type="button"
                          onClick={() => handleActivateMealOrder(order.mealOrderId ?? order.id)}
                          style={{ marginRight: '8px', backgroundColor: '#4CAF50', color: 'white', padding: '4px 8px', borderRadius: '4px', border: 'none', cursor: 'pointer' }}
                        >
                          Activate Order
                        </button>
                        <button
                          type="button"
                          onClick={() => handleRejectMealOrder(order.mealOrderId ?? order.id)}
                          style={{ backgroundColor: '#f44336', color: 'white', padding: '4px 8px', borderRadius: '4px', border: 'none', cursor: 'pointer' }}
                        >
                          Reject Order
                        </button>
                      </>
                    )}
                  </div>
                </li>
              ))
            )}
          </ul>
        </div>
      </section>
    </div>
  );
}
