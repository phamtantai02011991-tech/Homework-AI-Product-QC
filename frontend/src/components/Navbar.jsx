import React from 'react';
import { NavLink } from 'react-router-dom';
import { Award, ScanEye } from 'lucide-react';

export default function Navbar() {
  return (
    <header className="site-header">
      <div className="header-top">
        <div className="header-inner">
          <NavLink to="/" className="nav-brand">
            <div className="brand-crest">
              <Award size={22} color="#caa472" />
            </div>
            <div>
              <div className="brand-title">AI PRODUCT QC</div>
              <div className="brand-subtitle">ASSISTANT • GEMINI 3.5 FLASH</div>
            </div>
          </NavLink>

          <nav>
            <ul className="nav-links">
              <li>
                <NavLink to="/" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`} end>
                  <span>Dashboard</span>
                </NavLink>
              </li>
              <li>
                <NavLink to="/inspect" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
                  <span>New Inspection</span>
                </NavLink>
              </li>
              <li>
                <NavLink to="/history" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
                  <span>History</span>
                </NavLink>
              </li>
              <li>
                <NavLink to="/products" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
                  <span>Products &amp; Stock</span>
                </NavLink>
              </li>
            </ul>
          </nav>

          <div className="header-actions">
            <NavLink to="/inspect" className="btn-header-outline">
              <ScanEye size={16} />
              <span>Inspect Now</span>
            </NavLink>
          </div>
        </div>
      </div>
    </header>
  );
}
