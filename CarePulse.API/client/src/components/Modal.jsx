import React from 'react';

export default function Modal({ title, children, onClose, onConfirm, confirmLabel = 'Confirm', confirmDisabled = false }) {
  return (
    <div className="modal-overlay">
      <div className="modal-dialog">
        <div className="modal-header">
          <h3>{title}</h3>
          <button onClick={onClose} className="modal-close">✕</button>
        </div>
        <div className="modal-body">{children}</div>
        <div className="modal-actions">
          <button onClick={onClose} className="portal-button" type="button">Cancel</button>
          <button onClick={onConfirm} className="portal-button" type="button" disabled={confirmDisabled}>{confirmLabel}</button>
        </div>
      </div>
    </div>
  );
}
