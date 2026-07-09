import React, { createContext, useContext, useEffect, useState } from 'react';
import { startConnection, stopConnection } from '../lib/signalR';
import { useToast } from './ToastContext';

const NotificationsContext = createContext({
  latestAppointment: null,
  notifications: [],
  clearNotifications: () => {},
});

export const NotificationsProvider = ({ children }) => {
  const [latestAppointment, setLatestAppointment] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const { showToast } = useToast();

  useEffect(() => {
    let mounted = true;
    startConnection((appointment) => {
      if (!mounted) return;
      const message = appointment?.status ? `Appointment ${appointment.status}` : 'Appointment updated';
      setLatestAppointment(appointment);
      setNotifications((prev) => [{ id: Date.now(), message, detail: appointment?.notes || 'Appointment updated' }, ...prev].slice(0, 10));
      showToast(message, 'info');
    }).catch((err) => {
      console.error('SignalR connection failed', err);
    });

    return () => {
      mounted = false;
      stopConnection();
    };
  }, []);

  const clearNotifications = () => setNotifications([]);

  return (
    <NotificationsContext.Provider value={{ latestAppointment, notifications, clearNotifications }}>
      {children}
    </NotificationsContext.Provider>
  );
};

export const useNotifications = () => useContext(NotificationsContext);
