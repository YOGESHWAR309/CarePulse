import { useEffect, useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiFetch } from '../lib/api';
import TopNav from '../components/TopNav';
import Modal from '../components/Modal';
import { useToast } from '../context/ToastContext';

export default function StaffManagement() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [users, setUsers] = useState([]);
  const [selectedUser, setSelectedUser] = useState(null);
  const doctorCategories = ['General Physician', 'Cardiologist', 'Pediatrician', 'Dermatologist'];
  const [editForm, setEditForm] = useState({
    fullName: '',
    email: '',
    phone: '',
    role: 'Doctor',
    isActive: true,
    doctorCategory: '',
    doctorQualification: '',
    doctorSpecialty: ''
  });
  const [searchRole, setSearchRole] = useState('All');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [staffForm, setStaffForm] = useState({
    fullName: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: '',
    role: 'Doctor',
    doctorCategory: '',
    doctorQualification: '',
    doctorSpecialty: ''
  });
  const [isCreatingStaff, setIsCreatingStaff] = useState(false);
  const [isUpdatingUser, setIsUpdatingUser] = useState(false);
  const [isDeletingUser, setIsDeletingUser] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [isLoadingUsers, setIsLoadingUsers] = useState(false);
  const [isResettingPassword, setIsResettingPassword] = useState(false);
  const [selectedUserDependencies, setSelectedUserDependencies] = useState(null);
  const [isLoadingDependencies, setIsLoadingDependencies] = useState(false);
  const [dashboardStats, setDashboardStats] = useState(null);
  const [isLoadingDashboard, setIsLoadingDashboard] = useState(false);

  if (user?.role !== 'Admin') {
    return <Navigate to="/dashboard" replace />;
  }

  const loadUsers = async () => {
    if (!user) return;
    setError('');
    setMessage('');
    setIsLoadingUsers(true);

    try {
      const response = await apiFetch(user, '/api/Admin/users');
      setUsers(Array.isArray(response) ? response : response.users || []);
      setMessage('Users loaded successfully.');
    } catch (err) {
      setError(err.message || 'Unable to load users.');
    } finally {
      setIsLoadingUsers(false);
    }
  };

  const loadDashboardStats = async () => {
    if (!user) return;
    setIsLoadingDashboard(true);
    try {
      const payload = await apiFetch(user, '/api/Admin/dashboard');
      setDashboardStats(payload);
    } catch (err) {
      setError(err.message || 'Unable to load admin dashboard data.');
    } finally {
      setIsLoadingDashboard(false);
    }
  };

  useEffect(() => {
    if (!user) return;
    loadUsers();
    loadDashboardStats();
  }, [user]);

  const loadUserDependencies = async (userId) => {
    setIsLoadingDependencies(true);
    setError('');

    try {
      const payload = await apiFetch(user, `/api/Admin/users/${userId}/dependency-info`);
      setSelectedUserDependencies(payload);
    } catch (err) {
      setSelectedUserDependencies(null);
      setError(err.message || 'Unable to load dependency info.');
    } finally {
      setIsLoadingDependencies(false);
    }
  };

  const { showToast } = useToast();

  const handleSelectUser = (selectedUser) => {
    setSelectedUser(selectedUser);
    setEditForm({
      fullName: selectedUser.fullName,
      email: selectedUser.email,
      phone: selectedUser.phone,
      role: selectedUser.role,
      isActive: selectedUser.isActive,
      doctorCategory: selectedUser.doctorCategory || '',
      doctorQualification: selectedUser.doctorQualification || '',
      doctorSpecialty: selectedUser.doctorSpecialty || ''
    });
    setNewPassword('');
    setConfirmPassword('');
    setMessage('');
    setError('');
    loadUserDependencies(selectedUser.userId);
  };

  const handleUpdateUser = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    if (!selectedUser) {
      setError('Please select a user to update.');
      return;
    }

    if (!editForm.fullName || !editForm.email || !editForm.phone) {
      setError('Full name, email, and phone are required.');
      return;
    }

    if (editForm.role === 'Doctor' && (!editForm.doctorCategory || !editForm.doctorQualification || !editForm.doctorSpecialty)) {
      setError('Doctor accounts require category, qualification, and specialty.');
      return;
    }

    setIsUpdatingUser(true);

    try {
      await apiFetch(user, `/api/Admin/users/${selectedUser.userId}`, {
        method: 'PUT',
        body: JSON.stringify(editForm),
      });

      setMessage('User updated successfully.');
      await loadUsers();
    } catch (err) {
      setError(err.message || 'Unable to update user.');
    } finally {
      setIsUpdatingUser(false);
    }
  };

  const handleDeleteUser = async () => {
    if (!selectedUser) {
      setError('Please select a user to delete.');
      return;
    }

    if (selectedUser.role === 'Admin') {
      setError('Admin user cannot be deleted.');
      return;
    }

    setError('');
    setMessage('');
    setIsDeletingUser(true);

    try {
      // If dependencies present, offer soft-delete (deactivate) instead of hard delete
      // At this point delete should only be attempted when no clinical dependencies exist.

      await apiFetch(user, `/api/Admin/users/${selectedUser.userId}`, {
        method: 'DELETE'
      });

      setMessage('User deleted successfully.');
      setSelectedUser(null);
      setSelectedUserDependencies(null);
      await loadUsers();
    } catch (err) {
      const message = err.message || 'Unable to delete user.';
      setError(message);

      if (message.includes('related clinical data')) {
        await loadUserDependencies(selectedUser.userId);
      }
    } finally {
      setIsDeletingUser(false);
    }
  };

  const handleResetPassword = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    if (!selectedUser) {
      setError('Please select a user first.');
      return;
    }

    if (newPassword.length < 6) {
      setError('Password must be at least 6 characters long.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Password and confirmation do not match.');
      return;
    }

    setIsResettingPassword(true);

    try {
      const response = await apiFetch(user, `/api/Admin/users/${selectedUser.userId}/reset-password`, {
        method: 'PUT',
        body: JSON.stringify({
          newPassword,
          confirmPassword,
        }),
      });

      setMessage(response.message || 'Password reset successfully!');
      setNewPassword('');
      setConfirmPassword('');
      
      // Reload users list to show updated data
      await loadUsers();
      setSelectedUser(null);
    } catch (err) {
      setError(err.message || 'Unable to reset password.');
    } finally {
      setIsResettingPassword(false);
    }
  };

  const handleSetUserStatus = async (userId, isActive) => {
    setError('');
    setMessage('');

    const newState = !isActive;

    try {
      const response = await apiFetch(user, `/api/Admin/users/${userId}/status`, {
        method: 'PUT',
        body: JSON.stringify(newState),
      });

      setMessage(`User status updated to ${newState ? 'Active' : 'Inactive'}.`);
      await loadUsers();
      if (selectedUser?.userId === userId) {
        setSelectedUser({ ...selectedUser, isActive: newState });
      }
    } catch (err) {
      setError(err.message || 'Unable to update user status.');
    }
  };

  const handleCreateStaff = async (event) => {
    event.preventDefault();
    setError('');
    setMessage('');

    if (!staffForm.fullName || !staffForm.email || !staffForm.phone || !staffForm.password || !staffForm.confirmPassword) {
      setError('All fields are required.');
      return;
    }

    if (staffForm.password.length < 8) {
      setError('Password must be at least 8 characters long.');
      return;
    }

    if (staffForm.password !== staffForm.confirmPassword) {
      setError('Password and confirmation do not match.');
      return;
    }

    if (staffForm.role === 'Doctor' && (!staffForm.doctorCategory || !staffForm.doctorQualification || !staffForm.doctorSpecialty)) {
      setError('Doctor accounts require category, qualification, and specialty.');
      return;
    }

    setIsCreatingStaff(true);

    try {
      await apiFetch(user, '/api/Admin/create-staff', {
        method: 'POST',
        body: JSON.stringify(staffForm),
      });

      setMessage('Staff account created successfully.');
      setStaffForm({
        fullName: '',
        email: '',
        phone: '',
        password: '',
        confirmPassword: '',
        role: 'Doctor',
        doctorCategory: '',
        doctorQualification: '',
        doctorSpecialty: ''
      });
      await loadUsers();
    } catch (err) {
      setError(err.message || 'Unable to create staff account.');
    } finally {
      setIsCreatingStaff(false);
    }
  };

  const filteredUsers = users.filter(
    (u) => searchRole === 'All' || u.role === searchRole
  );

  return (
    <div className="dashboard-card">
      <TopNav
        title="Staff Management"
        description="Create and manage hospital staff accounts, reset passwords, and control access."
      />

      <section className="back-card">
        <div>
          <h2>Admin Management</h2>
          <p>View all users, reset forgotten passwords, and manage user status and roles.</p>
          <p className="role-badge">Current portal: /admin</p>
        </div>
        <button type="button" onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
      </section>

      <section className="status-section">
        {message && <div className="success-text">{message}</div>}
        {error && <div className="error-text">{error}</div>}
      </section>

      <section className="portal-section">
        <div className="portal-column" style={{ flex: 1 }}>
          <h2>Admin Dashboard</h2>
          {isLoadingDashboard ? <p>Loading dashboard summary...</p> : dashboardStats ? (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '12px', marginBottom: '16px' }}>
              <div style={{ padding: '12px', backgroundColor: '#f5f9ff', borderRadius: '6px' }}>
                <strong>Total users</strong><br />{dashboardStats.totalUsers ?? 0}
              </div>
              <div style={{ padding: '12px', backgroundColor: '#f5f9ff', borderRadius: '6px' }}>
                <strong>Active users</strong><br />{dashboardStats.activeUsers ?? 0}
              </div>
              <div style={{ padding: '12px', backgroundColor: '#f5f9ff', borderRadius: '6px' }}>
                <strong>Patients</strong><br />{dashboardStats.patientCount ?? 0}
              </div>
              <div style={{ padding: '12px', backgroundColor: '#f5f9ff', borderRadius: '6px' }}>
                <strong>Staff</strong><br />{dashboardStats.staffCount ?? 0}
              </div>
            </div>
          ) : <p>No dashboard metrics available.</p>}
        </div>
      </section>

      <section className="portal-section">
        <div className="portal-column">
          <h2>User Management</h2>
          <div style={{ marginBottom: '16px' }}>
            <button type="button" onClick={loadUsers} disabled={isLoadingUsers}>
              {isLoadingUsers ? 'Refreshing...' : 'Refresh Users'}
            </button>
          </div>

          <label>Filter by Role</label>
          <select value={searchRole} onChange={(e) => setSearchRole(e.target.value)} style={{ marginBottom: '16px' }}>
            <option value="All">All Roles</option>
            <option value="Admin">Admin</option>
            <option value="Patient">Patient</option>
            <option value="Doctor">Doctor</option>
            <option value="Receptionist">Receptionist</option>
            <option value="Nutritionist">Nutritionist</option>
            <option value="Dietician">Dietician</option>
            <option value="Canteen">Canteen</option>
          </select>

          <div style={{ maxHeight: '500px', overflowY: 'auto', border: '1px solid #ddd', borderRadius: '4px', padding: '8px' }}>
            {filteredUsers.length === 0 ? (
              <p>No users found.</p>
            ) : (
              <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
                {filteredUsers.map((u) => (
                  <li
                    key={u.userId}
                    onClick={() => handleSelectUser(u)}
                    style={{
                      padding: '12px',
                      marginBottom: '8px',
                      cursor: 'pointer',
                      backgroundColor: selectedUser?.userId === u.userId ? '#e3f2fd' : '#f5f5f5',
                      borderRadius: '4px',
                      border: selectedUser?.userId === u.userId ? '2px solid #1976d2' : '1px solid #ddd',
                    }}
                  >
                    <strong>{u.fullName}</strong> ({u.role})<br />
                    <small>{u.email}</small><br />
                    <small>Status: {u.isActive ? '✓ Active' : '✗ Inactive'}</small>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>

        <div className="portal-column">
          <h2>Reset User Password</h2>
          {selectedUser ? (
            <div>
              <div style={{ marginBottom: '16px', padding: '12px', backgroundColor: '#f5f5f5', borderRadius: '4px' }}>
                <strong>Selected User:</strong><br />
                Name: {selectedUser.fullName}<br />
                Email: {selectedUser.email}<br />
                Role: {selectedUser.role}<br />
                Status: {selectedUser.isActive ? '✓ Active' : '✗ Inactive'}
              </div>

              <form onSubmit={handleResetPassword} className="form-grid">
                <label>New Password</label>
                <input
                  type="password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  placeholder="Enter new password (min 6 characters)"
                  required
                />

                <label>Confirm Password</label>
                <input
                  type="password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  placeholder="Confirm new password"
                  required
                />

                <button type="submit" disabled={isResettingPassword}>
                  {isResettingPassword ? 'Resetting...' : 'Reset Password'}
                </button>
              </form>

              <div style={{ marginTop: '24px', paddingTop: '16px', borderTop: '1px solid #ddd' }}>
                <h3>User Management</h3>
                <button
                  type="button"
                  onClick={() => handleSetUserStatus(selectedUser.userId, selectedUser.isActive)}
                  style={{
                    backgroundColor: selectedUser.isActive ? '#f44336' : '#4CAF50',
                    color: 'white',
                    padding: '8px 16px',
                    borderRadius: '4px',
                    border: 'none',
                    cursor: 'pointer',
                    marginTop: '8px',
                    marginRight: '8px'
                  }}
                >
                  {selectedUser.isActive ? 'Deactivate User' : 'Activate User'}
                </button>
                <button
                  type="button"
                  onClick={handleDeleteUser}
                  disabled={isDeletingUser || selectedUser.role === 'Admin' || !!selectedUserDependencies?.appointmentCount || !!selectedUserDependencies?.prescriptionCount || !!selectedUserDependencies?.dietPlanCount || !!selectedUserDependencies?.mealOrderCount}
                  style={{
                    backgroundColor: selectedUser.role === 'Admin' || !!selectedUserDependencies?.appointmentCount || !!selectedUserDependencies?.prescriptionCount || !!selectedUserDependencies?.dietPlanCount || !!selectedUserDependencies?.mealOrderCount ? '#999' : '#d32f2f',
                    color: 'white',
                    padding: '8px 16px',
                    borderRadius: '4px',
                    border: 'none',
                    cursor: selectedUser.role === 'Admin' || !!selectedUserDependencies?.appointmentCount || !!selectedUserDependencies?.prescriptionCount || !!selectedUserDependencies?.dietPlanCount || !!selectedUserDependencies?.mealOrderCount ? 'not-allowed' : 'pointer',
                    marginTop: '8px'
                  }}
                >
                  {isDeletingUser ? 'Deleting...' : 'Delete User'}
                </button>
                {(selectedUserDependencies?.appointmentCount || selectedUserDependencies?.prescriptionCount || selectedUserDependencies?.dietPlanCount || selectedUserDependencies?.mealOrderCount) && (
                  <div style={{ marginTop: '12px', color: '#d32f2f' }}>
                    This user has active clinical records and cannot be deleted. Deactivate the account instead.
                  </div>
                )}
              </div>

              <div style={{ marginTop: '24px', paddingTop: '16px', borderTop: '1px solid #ddd' }}>
                <h3>Edit Selected User</h3>
                {isLoadingDependencies && <p>Loading selected user dependencies...</p>}
                {selectedUserDependencies && (
                  <div style={{ marginBottom: '16px', padding: '12px', backgroundColor: '#fff4e5', borderRadius: '4px' }}>
                    <strong>Dependency summary</strong><br />
                    Appointments: {selectedUserDependencies.appointmentCount}<br />
                    Prescriptions: {selectedUserDependencies.prescriptionCount}<br />
                    Diet plans: {selectedUserDependencies.dietPlanCount}<br />
                    Meal orders: {selectedUserDependencies.mealOrderCount}<br />
                    Refresh tokens: {selectedUserDependencies.refreshTokenCount}<br />
                  </div>
                )}
                  <form onSubmit={handleUpdateUser} className="form-grid">
                  <label>Full Name</label>
                  <input
                    value={editForm.fullName}
                    onChange={(e) => setEditForm({ ...editForm, fullName: e.target.value })}
                    required
                  />

                  <label>Email</label>
                  <input
                    type="email"
                    value={editForm.email}
                    onChange={(e) => setEditForm({ ...editForm, email: e.target.value })}
                    required
                  />

                  <label>Phone</label>
                  <input
                    value={editForm.phone}
                    onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })}
                    required
                  />

                  <label>Role</label>
                  <select
                    value={editForm.role}
                    onChange={(e) => setEditForm({ ...editForm, role: e.target.value })}
                    required
                  >
                    <option value="Doctor">Doctor</option>
                    <option value="Receptionist">Receptionist</option>
                    <option value="Nutritionist">Nutritionist</option>
                    <option value="Dietician">Dietician</option>
                    <option value="Canteen">Canteen</option>
                    <option value="Pharmacist">Pharmacist</option>
                    <option value="LabTechnician">Lab Technician</option>
                  </select>

                  <label>Active</label>
                  <select
                    value={editForm.isActive ? 'true' : 'false'}
                    onChange={(e) => setEditForm({ ...editForm, isActive: e.target.value === 'true' })}
                  >
                    <option value="true">Active</option>
                    <option value="false">Inactive</option>
                  </select>

                  {editForm.role === 'Doctor' && (
                    <>
                      <label>Doctor Category</label>
                      <select
                        value={editForm.doctorCategory}
                        onChange={(e) => setEditForm({ ...editForm, doctorCategory: e.target.value })}
                        required
                      >
                        <option value="">Select a category</option>
                        {doctorCategories.map((category) => (
                          <option key={category} value={category}>{category}</option>
                        ))}
                      </select>

                      <label>Qualification</label>
                      <input
                        value={editForm.doctorQualification}
                        onChange={(e) => setEditForm({ ...editForm, doctorQualification: e.target.value })}
                        required
                      />

                      <label>Specialty</label>
                      <input
                        value={editForm.doctorSpecialty}
                        onChange={(e) => setEditForm({ ...editForm, doctorSpecialty: e.target.value })}
                        required
                      />
                    </>
                  )}

                  <div style={{ gridColumn: 'span 2' }}>
                    <button type="submit" disabled={isUpdatingUser}>
                      {isUpdatingUser ? 'Updating...' : 'Update User'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          ) : (
            <div style={{ padding: '24px', textAlign: 'center', color: '#999' }}>
              <p>Select a user from the list to reset their password.</p>
            </div>
          )}
        </div>
      </section>

      <section className="portal-section" style={{ marginTop: '24px' }}>
        <div className="portal-column" style={{ flex: 1 }}>
          <h2>Create Staff Account</h2>
          <form onSubmit={handleCreateStaff} className="form-grid">
            <label>Full Name</label>
            <input
              value={staffForm.fullName}
              onChange={(e) => setStaffForm({ ...staffForm, fullName: e.target.value })}
              required
            />

            <label>Email</label>
            <input
              type="email"
              value={staffForm.email}
              onChange={(e) => setStaffForm({ ...staffForm, email: e.target.value })}
              required
            />

            <label>Phone</label>
            <input
              value={staffForm.phone}
              onChange={(e) => setStaffForm({ ...staffForm, phone: e.target.value })}
              required
            />

            <label>Password</label>
            <input
              type="password"
              value={staffForm.password}
              onChange={(e) => setStaffForm({ ...staffForm, password: e.target.value })}
              required
            />

            <label>Confirm Password</label>
            <input
              type="password"
              value={staffForm.confirmPassword}
              onChange={(e) => setStaffForm({ ...staffForm, confirmPassword: e.target.value })}
              required
            />

            <label>Role</label>
            <select
              value={staffForm.role}
              onChange={(e) => setStaffForm({ ...staffForm, role: e.target.value })}
              required
            >
              <option value="Doctor">Doctor</option>
              <option value="Receptionist">Receptionist</option>
              <option value="Nutritionist">Nutritionist</option>
              <option value="Dietician">Dietician</option>
              <option value="Canteen">Canteen</option>
              <option value="Pharmacist">Pharmacist</option>
              <option value="LabTechnician">Lab Technician</option>
            </select>

            {staffForm.role === 'Doctor' && (
              <>
                <label>Doctor Category</label>
                <select
                  value={staffForm.doctorCategory}
                  onChange={(e) => setStaffForm({ ...staffForm, doctorCategory: e.target.value })}
                  required
                >
                  <option value="">Select a category</option>
                  {doctorCategories.map((category) => (
                    <option key={category} value={category}>{category}</option>
                  ))}
                </select>

                <label>Qualification</label>
                <input
                  value={staffForm.doctorQualification}
                  onChange={(e) => setStaffForm({ ...staffForm, doctorQualification: e.target.value })}
                  required
                />

                <label>Specialty</label>
                <input
                  value={staffForm.doctorSpecialty}
                  onChange={(e) => setStaffForm({ ...staffForm, doctorSpecialty: e.target.value })}
                  required
                />
              </>
            )}

            <div style={{ gridColumn: 'span 2' }}>
              {message && <div className="success-text">{message}</div>}
              {error && <div className="error-text">{error}</div>}
              <button type="submit" disabled={isCreatingStaff}>
                {isCreatingStaff ? 'Creating...' : 'Create Staff Account'}
              </button>
            </div>
          </form>
        </div>
      </section>

      <section style={{ marginTop: '24px', padding: '16px', backgroundColor: '#f9f9f9', borderRadius: '4px' }}>
        <h3>Password Reset Instructions</h3>
        <ul>
          <li>Select a user from the list on the left</li>
          <li>Enter a new temporary password (minimum 6 characters)</li>
          <li>Confirm the password and click "Reset Password"</li>
          <li>The user can now login with the new password</li>
          <li>Recommended: Ask the user to change their password after first login</li>
        </ul>
      </section>
    </div>
  );
}
