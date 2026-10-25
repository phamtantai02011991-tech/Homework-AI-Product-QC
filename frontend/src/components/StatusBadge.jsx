import React from 'react';
import { CheckCircle2, AlertTriangle, XCircle, Clock } from 'lucide-react';

export default function StatusBadge({ status, isConfirmed = false }) {
  const norm = (status || '').toLowerCase();

  if (norm === 'passed') {
    return (
      <span className="badge badge-passed">
        <CheckCircle2 size={13} />
        <span>PASSED</span>
        {isConfirmed && <span style={{ fontSize: '0.68rem', opacity: 0.85 }}>(Confirmed)</span>}
      </span>
    );
  }

  if (norm === 'failed') {
    return (
      <span className="badge badge-failed">
        <XCircle size={13} />
        <span>FAILED</span>
        {isConfirmed && <span style={{ fontSize: '0.68rem', opacity: 0.85 }}>(Confirmed)</span>}
      </span>
    );
  }

  return (
    <span className="badge badge-needsreview">
      <AlertTriangle size={13} />
      <span>NEEDS REVIEW</span>
    </span>
  );
}
