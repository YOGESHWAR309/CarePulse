import { createContext, useContext, useState, useEffect } from 'react';
import { normalizeUserRole } from '../lib/roles';
import { apiFetch } from '../lib/api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const stored = localStorage.getItem('carepulse_user');
    return stored ? JSON.parse(stored) : null;
  });

  useEffect(() => {
    if (user) {
      localStorage.setItem('carepulse_user', JSON.stringify(user));
    } else {
      localStorage.removeItem('carepulse_user');
    }
  }, [user]);

  const login = (data) => setUser(normalizeUserRole(data));
  const logout = () => setUser(null);

  const refreshSession = async () => {
    if (!user?.refreshToken) return false;
    try {
      const result = await apiFetch(null, '/api/Auth/refresh', {
        method: 'POST',
        body: JSON.stringify({ refreshToken: user.refreshToken }),
      });
      setUser(normalizeUserRole(result.data));
      return true;
    } catch {
      return false;
    }
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, refreshSession }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);