import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { portalRoutes } from '../lib/roles';

export default function TopNav({ title, description }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const currentPortal = user?.role ? portalRoutes[user.role] : undefined;

  return (
    <header className="top-nav">
      <div className="top-nav-brand">
        <div className="nav-icon">✚</div>
        <div>
          <h1>{title}</h1>
          {description && <p className="top-nav-description">{description}</p>}
          {user && (
            <p className="top-nav-user">
              Welcome, {user.fullName} · Role: <strong>{user.role}</strong>
            </p>
          )}
        </div>
      </div>

      <div className="top-nav-actions">
        <button onClick={() => navigate('/dashboard')}>Dashboard</button>
        {currentPortal && pathname !== currentPortal && (
          <button onClick={() => navigate(currentPortal)}>{user.role} Portal</button>
        )}
        <button className="logout-button" onClick={logout}>Logout</button>
      </div>
    </header>
  );
}
