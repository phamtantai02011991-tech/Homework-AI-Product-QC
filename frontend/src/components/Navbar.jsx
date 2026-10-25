import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { ShieldCheck, ChevronRight, Award, ScanEye } from 'lucide-react';

export default function Navbar() {
  const location = useLocation();

  const getBreadcrumbTitle = () => {
    switch (location.pathname) {
      case '/': return 'Inspection Dashboard & Defect Distribution';
      case '/inspect': return 'Multimodal AI Vision Defect Inspection Station';
      case '/history': return 'Quality Audit History & Inspection Logs';
      case '/products': return 'Product Catalog & Inventory Stocks';
      default:
        if (location.pathname.startsWith('/inspect/preview')) return 'AI Defect Analysis & Manual Confirmation Review';
        if (location.pathname.startsWith('/history/')) return 'Detailed Inspection Report Audit';
        return 'Industrial Quality Assurance';
    }
  };

  return (
    <header className="site-header">
      {/* 1. Deep Roasted Coffee Brown Header (Matching User Reference Image) */}
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

      {/* 2. Warm Camel / Sand Sub-bar Ribbon (Matching Secondary Band in Image) */}
      <div className="header-subbar">
        <div className="subbar-inner">
          <div className="subbar-crumb">
            <ShieldCheck size={16} />
            <span>Quality Control</span>
            <ChevronRight size={14} />
            <span className="crumb-active">{getBreadcrumbTitle()}</span>
          </div>
          <div className="subbar-tag">
            ISO-9001 Standard
          </div>
        </div>
      </div>
    </header>
  );
}
