import React from 'react';
import { NavLink } from 'react-router-dom';
import { ShieldCheck, LayoutDashboard, ScanEye, History, Package } from 'lucide-react';

export default function Navbar() {
  return (
    <header className="navbar">
      <NavLink to="/" className="nav-brand">
        <div className="brand-icon">
          <ShieldCheck size={22} color="#ffffff" />
        </div>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <span className="brand-title">AI Product QC Assistant</span>
            <span className="brand-badge">Gemini 3.5 Flash</span>
          </div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>
            AI Defect Inspection &amp; Smart Inventory Quality Control
          </div>
        </div>
      </NavLink>

      <nav>
        <ul className="nav-links">
          <li>
            <NavLink to="/" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`} end>
              <LayoutDashboard size={18} />
              <span>Dashboard</span>
            </NavLink>
          </li>
          <li>
            <NavLink to="/inspect" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              <ScanEye size={18} />
              <span>New Inspection</span>
            </NavLink>
          </li>
          <li>
            <NavLink to="/history" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              <History size={18} />
              <span>Inspection History</span>
            </NavLink>
          </li>
          <li>
            <NavLink to="/products" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              <Package size={18} />
              <span>Products &amp; Inventory</span>
            </NavLink>
          </li>
        </ul>
      </nav>
    </header>
  );
}
