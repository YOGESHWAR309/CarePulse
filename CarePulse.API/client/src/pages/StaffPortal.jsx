import { useMemo } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import TopNav from '../components/TopNav';

const roleDescriptions = {
  Pharmacist: 'Review prescriptions, manage medication dispensing, and support patient pharmacy needs.',
  LabTechnician: 'Track lab sample processing, review test orders, and support diagnostic workflows.',
};

export default function StaffPortal() {
  const { user } = useAuth();

  const role = user?.role;
  const validRoles = ['Pharmacist', 'LabTechnician'];
  const description = role ? roleDescriptions[role] : undefined;

  const isAuthorized = useMemo(() => validRoles.includes(role), [role]);

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (!isAuthorized) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="dashboard-card">
      <TopNav
        title={`${role} Portal`}
        description={description || 'Access your assigned staff portal.'}
      />

      <section className="back-card">
        <div>
          <h2>{role} workspace</h2>
          <p>
            Welcome to your {role.toLowerCase()} portal. The CarePulse admin has assigned your account a staff role. You can now use your assigned login and access the general dashboard actions while your dedicated portal is coming online.
          </p>
        </div>
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <h3>Assigned role</h3>
          <p>{role}</p>
        </div>
      </section>
    </div>
  );
}
