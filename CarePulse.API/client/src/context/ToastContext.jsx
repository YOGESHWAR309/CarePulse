import React, { createContext, useContext, useState, useCallback } from 'react';
import ToastContainer from '../components/ToastContainer';

const ToastContext = createContext(null);

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const removeToast = useCallback((id) => {
    // mark leaving then remove after small delay for animation
    setToasts((t) => t.map((x) => (x.id === id ? { ...x, leaving: true } : x)));
    setTimeout(() => setToasts((t) => t.filter((x) => x.id !== id)), 180);
  }, []);

  const showToast = useCallback((type, message, timeout = 4000) => {
    const id = Date.now() + Math.random();
    setToasts((t) => [...t, { id, type, message, leaving: false }]);
    // schedule graceful removal
    setTimeout(() => {
      removeToast(id);
    }, timeout);
    return id;
  }, [removeToast]);

  const value = { showToast, removeToast };

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within ToastProvider');
  return ctx;
}

export default ToastContext;
