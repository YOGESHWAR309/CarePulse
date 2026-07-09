import { NavLink } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { portalRoutes } from '../lib/roles';

export default function SidebarNav() {
  const { user } = useAuth();
  const portalRoute = user?.role ? portalRoutes[user.role] : null;

  return (
    <aside className="sidebar-nav">
      <div className="sidebar-card">
        <h2>Navigation</h2>
        <nav className="sidebar-links">
          <NavLink to="/dashboard" className={({ isActive }) => isActive ? 'sidebar-link active' : 'sidebar-link'}>
            Dashboard
          </NavLink>
          {portalRoute && (
            <NavLink to={portalRoute} className={({ isActive }) => isActive ? 'sidebar-link active' : 'sidebar-link'}>
              {user.role} Portal
            </NavLink>
          )}
        </nav>
      </div>
    </aside>
  );
}
