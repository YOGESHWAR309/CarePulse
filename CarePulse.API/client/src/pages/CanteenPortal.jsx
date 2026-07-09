import { useEffect, useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import TopNav from '../components/TopNav';
import { apiFetch } from '../lib/api';

export default function CanteenPortal() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [patientId, setPatientId] = useState('');
  const [menuItems, setMenuItems] = useState([]);
  const [showAvailableOnly, setShowAvailableOnly] = useState(true);
  const [selectedMenuItem, setSelectedMenuItem] = useState(null);
  const [menuForm, setMenuForm] = useState({
    id: '',
    name: '',
    description: '',
    price: '',
    foodCategory: 'General',
    isAvailable: true,
  });
  const [orders, setOrders] = useState([]);
  const [orderId, setOrderId] = useState('1');
  const [status, setStatus] = useState('Ready');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const getPayloadField = (payload, key) => {
    if (!payload || typeof payload !== 'object') return undefined;
    return payload[key] ?? payload[key.charAt(0).toLowerCase() + key.slice(1)];
  };

  const renderOrders = (orderList) => {
    if (!Array.isArray(orderList) || orderList.length === 0) {
      return <p>No orders loaded.</p>;
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
          {orderList.map((order) => (
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

  if (user?.role !== 'Canteen') {
    return <Navigate to="/dashboard" replace />;
  }

  useEffect(() => {
    if (!user) return;

    const loadMenu = async () => {
      try {
        const payload = await apiFetch(user, '/api/Canteen/menu');
        setMenuItems(getPayloadField(payload, 'MenuItems') || []);
      } catch (err) {
        setError(err.message || 'Unable to load menu.');
      }
    };

    loadMenu();
  }, [user]);

  const resetMenuForm = () => {
    setSelectedMenuItem(null);
    setMenuForm({
      id: '',
      name: '',
      description: '',
      price: '',
      foodCategory: 'General',
      isAvailable: true,
    });
  };

  const handleEditMenuItem = (item) => {
    setSelectedMenuItem(item);
    setMenuForm({
      id: item.menuItemId ?? item.id ?? '',
      name: item.name ?? '',
      description: item.description ?? '',
      price: item.price ?? '',
      foodCategory: item.foodCategory ?? 'General',
      isAvailable: item.isAvailable ?? true,
    });
    setMessage('');
    setError('');
  };

  const handleSubmitMenu = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      const payloadBody = {
        name: menuForm.name,
        description: menuForm.description,
        price: Number(menuForm.price),
        foodCategory: menuForm.foodCategory,
        isAvailable: menuForm.isAvailable,
      };

      let payload;
      if (selectedMenuItem) {
        payload = await apiFetch(user, `/api/Canteen/menu/${menuForm.id}`, {
          method: 'PUT',
          body: JSON.stringify(payloadBody),
        });
      } else {
        payload = await apiFetch(user, '/api/Canteen/menu', {
          method: 'POST',
          body: JSON.stringify(payloadBody),
        });
      }

      const item = getPayloadField(payload, 'MenuItem') || getPayloadField(payload, 'menuItem');
      if (item) {
        setMenuItems((prev) => {
          if (selectedMenuItem) {
            return prev.map((existing) => ((existing.menuItemId ?? existing.id) === (item.menuItemId ?? item.id) ? item : existing));
          }
          return [...prev, item];
        });
      }

      setMessage(payload.message || (selectedMenuItem ? 'Menu item updated successfully.' : 'Menu item added successfully.'));
      resetMenuForm();
    } catch (err) {
      setError(err.message || 'Unable to save menu item.');
    }
  };

  const handleDeleteMenuItem = async (item) => {
    setError('');
    setMessage('');

    try {
      const payload = await apiFetch(user, `/api/Canteen/menu/${item.menuItemId ?? item.id}`, {
        method: 'DELETE',
      });

      setMenuItems((prev) => prev.filter((existing) => (existing.menuItemId ?? existing.id) !== (item.menuItemId ?? item.id)));
      if (selectedMenuItem && (selectedMenuItem.menuItemId ?? selectedMenuItem.id) === (item.menuItemId ?? item.id)) {
        resetMenuForm();
      }

      setMessage(payload.message || 'Menu item deleted successfully.');
    } catch (err) {
      setError(err.message || 'Unable to delete menu item.');
    }
  };

  const handleGetOrders = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      const query = patientId ? `?patientId=${patientId}` : '';
      const payload = await apiFetch(user, `/api/Canteen/orders${query}`);
      setOrders(getPayloadField(payload, 'Orders') || []);
      setMessage(payload.message || 'Loaded patient orders successfully.');
    } catch (err) {
      setError(err.message || 'Unable to load orders.');
    }
  };

  const handleUpdateOrder = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      const payload = await apiFetch(user, `/api/Canteen/orders`, {
        method: 'POST',
        body: JSON.stringify({ orderId: Number(orderId), status }),
      });
      setMessage(payload.message || 'Order status updated successfully.');
      const updatedOrder = getPayloadField(payload, 'Order');
      if (updatedOrder) {
        setOrders((prev) => {
          const existingIndex = prev.findIndex((o) => (o.mealOrderId ?? o.id) === (updatedOrder.mealOrderId ?? updatedOrder.id));
          if (existingIndex !== -1) {
            const newOrders = [...prev];
            newOrders[existingIndex] = updatedOrder;
            return newOrders;
          }
          return [...prev, updatedOrder];
        });
      }
    } catch (err) {
      setError(err.message || 'Unable to update order status.');
    }
  };

  return (
    <div className="dashboard-card">
      <TopNav
        title="Canteen Portal"
        description="Manage meal menu items and patient food orders."
      />

      <section className="back-card">
        <div>
          <h2>Quick workflow</h2>
          <p>Manage menus and orders here, then return to the dashboard for other role actions.</p>
          <p className="role-badge">Current portal: /canteen</p>
        </div>
        <button type="button" onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
      </section>

      <section className="status-section">
        {message && <div className="success-text">{message}</div>}
        {error && <div className="error-text">{error}</div>}
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <h2>Menu Items</h2>
          <div style={{ marginBottom: 8 }}>
            <label style={{ fontSize: 14 }}>
              <input type="checkbox" checked={showAvailableOnly} onChange={(e) => setShowAvailableOnly(e.target.checked)} />{' '}
              Show available only
            </label>
          </div>

          <form onSubmit={handleSubmitMenu} className="form-grid" style={{ marginBottom: 16 }}>
            <h3>{selectedMenuItem ? 'Edit Menu Item' : 'Add Menu Item'}</h3>
            <label>Name</label>
            <input
              type="text"
              value={menuForm.name}
              onChange={(e) => setMenuForm((prev) => ({ ...prev, name: e.target.value }))}
              required
            />
            <label>Description</label>
            <textarea
              value={menuForm.description}
              onChange={(e) => setMenuForm((prev) => ({ ...prev, description: e.target.value }))}
              rows={3}
              required
            />
            <label>Price</label>
            <input
              type="number"
              min="0"
              step="0.01"
              value={menuForm.price}
              onChange={(e) => setMenuForm((prev) => ({ ...prev, price: e.target.value }))}
              required
            />
            <label>Food Category</label>
            <select value={menuForm.foodCategory} onChange={(e) => setMenuForm((prev) => ({ ...prev, foodCategory: e.target.value }))}>
              <option value="General">General</option>
              <option value="Vegetarian">Vegetarian</option>
              <option value="Non-Vegetarian">Non-Vegetarian</option>
              <option value="Diabetic">Diabetic</option>
              <option value="Low-Calorie">Low-Calorie</option>
            </select>
            <label>Available</label>
            <input
              type="checkbox"
              checked={menuForm.isAvailable}
              onChange={(e) => setMenuForm((prev) => ({ ...prev, isAvailable: e.target.checked }))}
            />
            <div />
            <div className="button-row">
              <button type="submit">{selectedMenuItem ? 'Update Item' : 'Add Item'}</button>
              {selectedMenuItem && (
                <button type="button" className="secondary-button" onClick={resetMenuForm}>
                  Cancel Edit
                </button>
              )}
            </div>
          </form>

          <ul>
            {menuItems.length === 0 ? (
              <li>No menu items available.</li>
            ) : (
              (menuItems.filter(i => !showAvailableOnly || i.isAvailable)).map((item) => (
                <li key={item.menuItemId ?? item.id}>
                  <strong>{item.name}</strong> - ₹{item.price} {' '}
                  <span style={{ color: item.isAvailable ? 'green' : 'crimson' }}>
                    ({item.isAvailable ? 'Available' : 'Unavailable'})
                  </span>
                  <br />
                  <small style={{ color: '#526273' }}>{item.description}</small>
                  <br />
                  <small style={{ color: '#8899a6' }}>Category: {item.foodCategory || 'General'}</small>
                  <br />
                  <small style={{ color: '#8899a6' }}>{new Date(item.createdAt).toLocaleString()}</small>
                  <br />
                  <div style={{ marginTop: 8, display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                    <button type="button" onClick={() => handleEditMenuItem(item)}>
                      Edit item
                    </button>
                    <button type="button" className="secondary-button" onClick={() => handleDeleteMenuItem(item)}>
                      Delete item
                    </button>
                  </div>
                </li>
              ))
            )}
          </ul>
        </div>

        <div className="portal-column">
          <h2>Patient Orders</h2>
          <form onSubmit={handleGetOrders} className="form-grid">
            <label>Patient ID (optional)</label>
            <input
              type="number"
              value={patientId}
              min="1"
              onChange={(e) => setPatientId(e.target.value)}
              placeholder="Leave blank for all orders"
            />
            <button type="submit">Load Orders</button>
          </form>

          <h3>Update Order Status</h3>
          <form onSubmit={handleUpdateOrder} className="form-grid">
            <label>Order ID</label>
            <input
              type="number"
              value={orderId}
              min="1"
              onChange={(e) => setOrderId(e.target.value)}
              required
            />
            <label>Status</label>
            <input
              type="text"
              value={status}
              onChange={(e) => setStatus(e.target.value)}
              required
            />
            <button type="submit">Update Order</button>
          </form>

          {renderOrders(orders)}
        </div>
      </section>
    </div>
  );
}
