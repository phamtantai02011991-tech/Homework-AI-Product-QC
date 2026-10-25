import React from 'react';

export default function StatCard({ title, value, icon: Icon, color, meta, iconBg }) {
  return (
    <div className="glass-panel stat-card">
      <div className="stat-header">
        <span className="stat-title">{title}</span>
        <div className="stat-icon" style={{ background: iconBg || 'rgba(255,255,255,0.05)', color: color || '#fff' }}>
          {Icon && <Icon size={20} />}
        </div>
      </div>
      <div className="stat-value" style={{ color: color || '#fff' }}>
        {value}
      </div>
      {meta && <div className="stat-meta">{meta}</div>}
    </div>
  );
}
