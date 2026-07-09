import React from 'react';

export default function ToastContainer({ toasts = [], onRemove = () => {} }) {
  return (
    <div className="toast-root">
      {toasts.map((t) => (
        <div
          key={t.id}
          className={`toast ${t.type === 'error' ? 'toast-error' : 'toast-success'} ${t.leaving ? 'toast-leaving' : ''}`}
        >
          <div className="toast-body">
            <strong className="toast-title">{t.type === 'error' ? 'Error' : 'Success'}</strong>
            <div className="toast-message">{t.message}</div>
          </div>
          <button className="toast-dismiss" onClick={() => onRemove(t.id)} aria-label="dismiss">✕</button>
        </div>
      ))}
    </div>
  );
}
