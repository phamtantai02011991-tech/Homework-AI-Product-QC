import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { 
  ArrowLeft, ShieldCheck, CheckCircle2, User, Package, 
  Calendar, AlertCircle, FileText, ExternalLink 
} from 'lucide-react';
import { inspectionService } from '../services/inspectionService';
import { STATIC_BASE_URL } from '../services/api';
import StatusBadge from '../components/StatusBadge';

export default function InspectionDetail() {
  const { id } = useParams();
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    async function loadDetail() {
      try {
        setLoading(true);
        const data = await inspectionService.getInspectionById(id);
        setReport(data);
      } catch (err) {
        setErrorMessage(err.message || 'Failed to load inspection report details.');
      } finally {
        setLoading(false);
      }
    }
    loadDetail();
  }, [id]);

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '100px 0' }}>
        <div className="spinner" style={{ margin: '0 auto 16px auto', width: '36px', height: '36px' }}></div>
        <p style={{ color: 'var(--text-muted)' }}>Loading inspection record details...</p>
      </div>
    );
  }

  if (errorMessage || !report) {
    return (
      <div className="glass-panel" style={{ padding: '40px', textAlign: 'center', maxWidth: '600px', margin: '40px auto' }}>
        <AlertCircle size={48} color="var(--status-fail)" style={{ margin: '0 auto 16px auto' }} />
        <h2>Report Not Found</h2>
        <p style={{ color: 'var(--text-muted)', margin: '12px 0 24px 0' }}>
          {errorMessage || 'This inspection record does not exist in the system.'}
        </p>
        <Link to="/history" className="btn btn-secondary">
          Back to Inspection History
        </Link>
      </div>
    );
  }

  return (
    <div>
      {/* Top Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginBottom: '28px' }}>
        <Link to="/history" className="btn btn-secondary" style={{ padding: '8px 12px' }} title="Back to history">
          <ArrowLeft size={16} />
        </Link>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <h1 style={{ fontSize: '1.6rem', fontWeight: 800 }}>
              Inspection Report Details #{report.reportId}
            </h1>
            <StatusBadge status={report.qualityStatus} isConfirmed={report.isUserConfirmed} />
          </div>
          <p style={{ color: 'var(--text-dim)', fontSize: '0.88rem', marginTop: '4px' }}>
            Timestamp: {new Date(report.createdAt).toLocaleString('en-US')}
          </p>
        </div>
      </div>

      {/* Grid: Image and Metadata */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(400px, 1fr))', gap: '28px' }}>
        
        {/* Left: High-resolution Inspection Photo */}
        <div className="glass-panel" style={{ padding: '24px' }}>
          <h2 style={{ fontSize: '1.05rem', fontWeight: 700, marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Package size={18} color="var(--accent-cyan)" />
            <span>Product Inspection Photo</span>
          </h2>

          <div style={{ background: '#000', borderRadius: 'var(--radius-md)', overflow: 'hidden', textAlign: 'center' }}>
            <a href={`${STATIC_BASE_URL}${report.imageUrl}`} target="_blank" rel="noopener noreferrer" title="Click to view full-size original image">
              <img
                src={`${STATIC_BASE_URL}${report.imageUrl}`}
                alt="Product Defect Inspection"
                style={{ width: '100%', maxHeight: '520px', objectFit: 'contain' }}
              />
            </a>
          </div>

          <div style={{ marginTop: '12px', textAlign: 'center' }}>
            <a
              href={`${STATIC_BASE_URL}${report.imageUrl}`}
              target="_blank"
              rel="noopener noreferrer"
              style={{ color: 'var(--accent-gold-text)', fontSize: '0.85rem', textDecoration: 'none', display: 'inline-flex', alignItems: 'center', gap: '4px', fontWeight: 600 }}
            >
              View original full-resolution image <ExternalLink size={14} />
            </a>
          </div>
        </div>

        {/* Right: Inspection Audit Details */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          
          {/* Product & Inspector Info Card */}
          <div className="glass-panel" style={{ padding: '24px' }}>
            <h2 style={{ fontSize: '1.05rem', fontWeight: 700, marginBottom: '16px' }}>
              Product &amp; Personnel Information
            </h2>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', fontSize: '0.9rem' }}>
              <div>
                <span style={{ color: 'var(--text-muted)', display: 'block', fontSize: '0.8rem' }}>Product:</span>
                <strong>{report.product.productName}</strong>
                <span style={{ color: 'var(--text-dim)', display: 'block', fontSize: '0.82rem' }}>
                  Category: {report.product.category}
                </span>
              </div>

              <div>
                <span style={{ color: 'var(--text-muted)', display: 'block', fontSize: '0.8rem' }}>QC Inspector:</span>
                <strong>{report.inspector.inspectorName}</strong>
                <span style={{ color: 'var(--text-dim)', display: 'block', fontSize: '0.82rem' }}>
                  {report.inspector.department} ({report.inspector.email})
                </span>
              </div>
            </div>
          </div>

          {/* AI Analysis & Quality Conclusion */}
          <div className="glass-panel" style={{ padding: '24px' }}>
            <h2 style={{ fontSize: '1.05rem', fontWeight: 700, marginBottom: '16px' }}>
              AI Analysis &amp; Quality Assessment
            </h2>

            <div style={{ marginBottom: '16px' }}>
              <span style={{ color: 'var(--text-muted)', display: 'block', fontSize: '0.8rem', marginBottom: '4px' }}>
                Defect Category:
              </span>
              <div style={{ fontSize: '1.05rem', fontWeight: 700 }}>
                {report.defectCategory.categoryName}
              </div>
              <p style={{ color: 'var(--text-dim)', fontSize: '0.85rem', marginTop: '4px' }}>
                {report.defectCategory.description} (Severity Level: {report.defectCategory.severityLevel})
              </p>
            </div>

            <div style={{ marginBottom: '16px' }}>
              <span style={{ color: 'var(--text-muted)', display: 'block', fontSize: '0.8rem', marginBottom: '4px' }}>
                AI Confidence Score:
              </span>
              <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                <div className="confidence-bar-bg" style={{ flex: 1 }}>
                  <div
                    className="confidence-bar-fill"
                    style={{
                      width: `${Math.round(report.confidenceScore * 100)}%`,
                      background: 'var(--gradient-brand)'
                    }}
                  />
                </div>
                <strong style={{ fontSize: '0.95rem' }}>{Math.round(report.confidenceScore * 100)}%</strong>
              </div>
            </div>

            <div style={{ marginBottom: '16px' }}>
              <span style={{ color: 'var(--text-muted)', display: 'block', fontSize: '0.8rem', marginBottom: '4px' }}>
                Defect Description (AI Generated):
              </span>
              <div style={{ background: 'rgba(255,255,255,0.03)', padding: '14px', borderRadius: 'var(--radius-md)', fontSize: '0.92rem', lineHeight: 1.6, borderLeft: '3px solid var(--accent-cyan)' }}>
                {report.defectDescription}
              </div>
            </div>

            <div style={{ borderTop: '1px solid var(--border-subtle)', paddingTop: '16px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>Inventory Status:</span>
                <div style={{ fontWeight: 700, color: report.isUserConfirmed ? 'var(--status-pass)' : 'var(--status-review)' }}>
                  {report.isUserConfirmed ? '✓ Synced to inventory stock' : '⏳ Draft — awaiting confirmation'}
                </div>
              </div>

              {!report.isUserConfirmed && (
                <Link to={`/inspect/preview/${report.reportId}`} className="btn btn-primary" style={{ padding: '8px 16px', fontSize: '0.85rem' }}>
                  Proceed to Confirm
                </Link>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
