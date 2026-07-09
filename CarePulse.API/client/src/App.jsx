import { Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import ForgotPassword from './pages/ForgotPassword';
import Dashboard from './pages/Dashboard';
import StaffManagement from './pages/StaffManagement';
import PatientPortal from './pages/PatientPortal';
import DoctorPortal from './pages/DoctorPortal';
import ReceptionistPortal from './pages/ReceptionistPortal';
import DieticianPortal from './pages/DieticianPortal';
import NutritionistPortal from './pages/NutritionistPortal';
import CanteenPortal from './pages/CanteenPortal';
import StaffPortal from './pages/StaffPortal';
import ProtectedRoute from './components/ProtectedRoute';
import SidebarNav from './components/SidebarNav';
import { useAuth } from './context/AuthContext';
import { useNotifications } from './context/NotificationsContext';
import { portalRoutes } from './lib/roles';
import SessionTimer from './components/SessionTimer';
import ChatWidget from './components/ChatWidget';

function App() {
  const { user } = useAuth();
  const { notifications, clearNotifications } = useNotifications();

  const routes = (
    <Routes>
      <Route path="/" element={user ? <Navigate to={portalRoutes[user.role] ?? '/dashboard'} /> : <Navigate to="/login" />} />
      <Route path="/login" element={user ? <Navigate to={portalRoutes[user.role] ?? '/dashboard'} /> : <Login />} />
<Route path="/register" element={user ? <Navigate to={portalRoutes[user.role] ?? '/dashboard'} /> : <Register />} />

      <Route path="/forgot-password" element={<ForgotPassword />} />
      <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
      <Route path="/admin" element={<ProtectedRoute><Navigate to="/admin/staff" replace /></ProtectedRoute>} />
      <Route path="/admin/staff" element={<ProtectedRoute><StaffManagement /></ProtectedRoute>} />
      <Route path="/patient" element={<ProtectedRoute><PatientPortal /></ProtectedRoute>} />
      <Route path="/doctor" element={<ProtectedRoute><DoctorPortal /></ProtectedRoute>} />
      <Route path="/receptionist" element={<ProtectedRoute><ReceptionistPortal /></ProtectedRoute>} />
      <Route path="/dietician" element={<ProtectedRoute><DieticianPortal /></ProtectedRoute>} />
      <Route path="/nutritionist" element={<ProtectedRoute><NutritionistPortal /></ProtectedRoute>} />
      <Route path="/canteen" element={<ProtectedRoute><CanteenPortal /></ProtectedRoute>} />
      <Route path="/pharmacist" element={<ProtectedRoute><StaffPortal /></ProtectedRoute>} />
      <Route path="/labtechnician" element={<ProtectedRoute><StaffPortal /></ProtectedRoute>} />
      <Route path="*" element={<Navigate to={user ? '/dashboard' : '/login'} />} />
    </Routes>
  );

  if (!user) {
    return <main className="content-shell">{routes}</main>;
  }

return (
  <>
    <SessionTimer />
    <ChatWidget />
    <div className="app-shell">
      <SidebarNav />
      <main className="content-shell">
        <div style={{ marginBottom: '16px', padding: '12px 16px', border: '1px solid #dce7f3', borderRadius: '8px', backgroundColor: '#f7fbff' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '12px' }}>
            <strong>Notification center</strong>
            <button type="button" onClick={clearNotifications} style={{ padding: '4px 8px' }}>Clear</button>
          </div>
          {notifications.length === 0 ? (
            <div style={{ marginTop: '8px', color: '#5b6b7a' }}>No recent updates yet.</div>
          ) : (
            <ul style={{ marginTop: '8px', paddingLeft: '18px' }}>
              {notifications.map((item) => (
                <li key={item.id} style={{ marginBottom: '4px' }}>
                  <strong>{item.message}</strong> {item.detail ? `- ${item.detail}` : ''}
                </li>
              ))}
            </ul>
          )}
        </div>
        {routes}
      </main>
    </div>
  </>
);
}

export default App;
