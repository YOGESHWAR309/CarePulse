import { useEffect, useRef, useState, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import Modal from './Modal';

const WARNING_THRESHOLD_SECONDS = 120; // show "stay logged in?" modal at 2 minutes left

export default function SessionTimer() {
  const { user, logout, refreshSession } = useAuth();
  const [secondsLeft, setSecondsLeft] = useState(null);
  const [showWarning, setShowWarning] = useState(false);
  const hasWarnedRef = useRef(false);

  const tokenExpiresAt = user?.tokenExpiresAt;

  const handleLogout = useCallback(() => {
    setShowWarning(false);
    logout();
  }, [logout]);

  const handleStayLoggedIn = useCallback(async () => {
    const ok = await refreshSession();
    if (ok) {
      hasWarnedRef.current = false;
      setShowWarning(false);
    } else {
      handleLogout();
    }
  }, [refreshSession, handleLogout]);

  useEffect(() => {
    if (!user || !tokenExpiresAt) {
      setSecondsLeft(null);
      return;
    }

    const expiryTime = new Date(tokenExpiresAt).getTime();

    const tick = () => {
      const diff = Math.floor((expiryTime - Date.now()) / 1000);
      setSecondsLeft(diff > 0 ? diff : 0);

      if (diff <= 0) {
        handleLogout();
      } else if (diff <= WARNING_THRESHOLD_SECONDS && !hasWarnedRef.current) {
        hasWarnedRef.current = true;
        setShowWarning(true);
      }
    };

    tick();
    const interval = setInterval(tick, 1000);
    return () => clearInterval(interval);
  }, [user, tokenExpiresAt, handleLogout]);

  if (!user || secondsLeft === null) return null;

  const minutes = Math.floor(secondsLeft / 60);
  const seconds = secondsLeft % 60;
  const formatted = `${minutes}:${seconds.toString().padStart(2, '0')}`;

  return (
    <>
      <div className="session-timer" title="Time remaining before you're logged out">
        Session expires in <strong>{formatted}</strong>
      </div>

      {showWarning && (
        <Modal
          title="Your session is about to expire"
          onClose={handleLogout}
          onConfirm={handleStayLoggedIn}
          confirmLabel="Stay signed in"
        >
          <p>
            You'll be logged out in <strong>{formatted}</strong> due to inactivity.
            Click "Stay signed in" to continue your session.
          </p>
        </Modal>
      )}
    </>
  );
}