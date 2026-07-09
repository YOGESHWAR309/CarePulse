export const portalRoutes = {
  Admin: '/admin/staff',
  Patient: '/patient',
  Doctor: '/doctor',
  Receptionist: '/receptionist',
  Dietician: '/dietician',
  Nutritionist: '/nutritionist',
  Canteen: '/canteen',
  Pharmacist: '/pharmacist',
  LabTechnician: '/labtechnician',
};

export const normalizeRole = (role) => {
  if (!role) return role;

  const trimmed = String(role).trim();
  const lower = trimmed.toLowerCase();

  switch (lower) {
    case 'admin':
      return 'Admin';
    case 'patient':
      return 'Patient';
    case 'doctor':
      return 'Doctor';
    case 'receptionist':
      return 'Receptionist';
    case 'nutritionist':
      return 'Nutritionist';
    case 'dietician':
      return 'Dietician';
    case 'canteen':
      return 'Canteen';
    case 'pharmacist':
      return 'Pharmacist';
    case 'labtechnician':
      return 'LabTechnician';
    default:
      return trimmed;
  }
};

export const normalizeUserRole = (user) => {
  if (!user) return user;
  return { ...user, role: normalizeRole(user.role) };
};
