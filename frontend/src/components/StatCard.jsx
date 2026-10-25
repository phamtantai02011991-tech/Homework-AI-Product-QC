import React from 'react';

export default function StatCard({ title, value, icon: Icon, color, meta, iconBg, className = '' }) {
  return (
    <div className={`glass-panel stat-card ck-reveal ${className}`}>
      <div className="stat-header">
        <span className="stat-title">{title}</span>
        <div className="stat-icon" style={{ background: iconBg || 'rgba(255,255,255,0.05)', color: color || '#fff' }}>
          {Icon && <Icon size={20} />}
        </div>
      </div>
      <div className="stat-value tabular-nums" style={{ color: color || '#fff' }}>
        {value}
      </div>
      {meta && <div className="stat-meta">{meta}</div>}
    </div>
  );
}
