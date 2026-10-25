import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { 
  Package, CheckCircle2, XCircle, AlertCircle, 
  TrendingUp, ArrowRight, ScanEye, RefreshCw, ShieldCheck 
} from 'lucide-react';
import { dashboardService } from '../services/dashboardService';
import { STATIC_BASE_URL } from '../services/api';
import StatCard from '../components/StatCard';
import StatusBadge from '../components/StatusBadge';
import useReveal from '../hooks/useReveal';

export default function Dashboard() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useReveal([data]);

  const fetchDashboard = async () => {
    try {
      setLoading(true);
      setError('');
      const summary = await dashboardService.getDashboardSummary();
      setData(summary);
    } catch (err) {
      setError(err.message || 'Failed to load dashboard metrics.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboard();
  }, []);

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '100px 0' }}>
        <div className="spinner" style={{ margin: '0 auto 16px auto', width: '36px', height: '36px' }}></div>
        <p style={{ color: 'var(--text-muted)' }}>Synchronizing QC inspection metrics...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="alert-banner alert-error">
        <AlertCircle size={20} />
        <div style={{ flex: 1 }}>{error}</div>
        <button className="btn btn-secondary" onClick={fetchDashboard} style={{ padding: '6px 12px' }}>
          <RefreshCw size={14} /> Retry
        </button>
      </div>
    );
  }

  if (!data) return null;

  return (
    <div>
      {/* Top Banner & Quick Actions */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '32px', flexWrap: 'wrap', gap: '16px' }}>
        <div>
          <h1 style={{ fontSize: '1.8rem', fontWeight: 800, letterSpacing: '-0.02em', marginBottom: '6px' }}>
            Quality Control Dashboard
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
            Real-time monitoring of inspection counts, pass rates, and visual defect distribution.
          </p>
        </div>

        <div style={{ display: 'flex', gap: '12px' }}>
          <button className="btn btn-secondary" onClick={fetchDashboard} title="Refresh metrics">
            <RefreshCw size={16} />
            <span>Refresh</span>
          </button>
          <Link to="/inspect" className="btn btn-primary">
            <ScanEye size={18} />
            <span>New Inspection</span>
          </Link>
        </div>
      </div>

      {/* Summary KPI Cards Grid */}
      <div className="stat-grid ck-stagger">
        <StatCard
          title="Total Managed Products"
          value={data.totalProducts}
          icon={Package}
          color="#38bdf8"
          iconBg="rgba(56, 189, 248, 0.15)"
          meta="Product lines in catalog"
        />
        <StatCard
          title="Total Inspections"
          value={data.totalInspections}
          icon={ScanEye}
          color="#818cf8"
          iconBg="rgba(129, 140, 248, 0.15)"
          meta={`${data.totalConfirmed} confirmed / ${data.totalPendingConfirmation} pending`}
        />
        <StatCard
          title="Passed (Quality Approved)"
          value={data.totalPassed}
          icon={CheckCircle2}
          color="#34d399"
          iconBg="rgba(52, 211, 153, 0.15)"
          meta="Added to passed stock inventory"
        />
        <StatCard
          title="Failed (Defect Detected)"
          value={data.totalFailed}
          icon={XCircle}
          color="#fb7185"
          iconBg="rgba(251, 113, 133, 0.15)"
          meta="Isolated in quarantine stock"
        />
        <StatCard
          title="Pass Rate"
          value={`${data.passRate}%`}
          icon={TrendingUp}
          color={data.passRate >= 90 ? '#34d399' : '#fbbf24'}
          iconBg="rgba(251, 191, 36, 0.15)"
          meta="Based on confirmed inspection reports"
        />
      </div>

      {/* Main Content Layout: Defect Breakdown & Recent Inspections */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))', gap: '24px' }}>
        
        {/* Defect Categories Distribution */}
        <div className="glass-panel" style={{ padding: '24px' }}>
          <h2 style={{ fontSize: '1.15rem', fontWeight: 700, marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <span>Visual Defect Category Distribution</span>
          </h2>

          {data.defectDistribution.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '40px 0', color: 'var(--text-dim)' }}>
              No defects have been recorded yet in the system.
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
              {data.defectDistribution.map((item, idx) => (
                <div key={idx}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.88rem', marginBottom: '6px' }}>
                    <span style={{ fontWeight: 600 }}>{item.categoryName}</span>
                    <span style={{ color: 'var(--text-muted)' }}>
                      {item.count} cases ({item.percentage}%)
                    </span>
                  </div>
                  <div className="confidence-bar-bg">
                    <div 
                      className="confidence-bar-fill" 
                      style={{ 
                        width: `${item.percentage}%`, 
                        background: 'linear-gradient(90deg, #f43f5e 0%, #fb7185 100%)' 
                      }} 
                    />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Data Integrity & Inventory Policy Card */}
        <div className="glass-panel" style={{ padding: '24px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
          <div>
            <h2 style={{ fontSize: '1.15rem', fontWeight: 700, marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <ShieldCheck size={20} color="var(--accent-cyan)" />
              <span>Inventory Data Integrity Policy</span>
            </h2>
            <div style={{ fontSize: '0.9rem', color: 'var(--text-muted)', lineHeight: 1.6 }}>
              <p style={{ marginBottom: '12px' }}>
                🛡️ <strong>Zero Mutation Principle:</strong> The AI provides an objective diagnostic proposal. Factory inventory is <strong>strictly unchanged</strong> until a QC inspector reviews and explicitly clicks confirm.
              </p>
              <p style={{ marginBottom: '12px' }}>
                ⚡ <strong>Atomic Transaction:</strong> Upon confirmation, report locking and stock adjustments are executed within a single <code>IDbContextTransaction</code>, preventing duplicates (HTTP 409) and guaranteeing 100% data consistency.
              </p>
            </div>
          </div>

          <div style={{ marginTop: '20px', padding: '16px', background: 'rgba(6, 182, 212, 0.08)', borderRadius: 'var(--radius-md)', border: '1px solid rgba(6, 182, 212, 0.2)' }}>
            <div style={{ fontSize: '0.85rem', fontWeight: 600, color: 'var(--accent-cyan)', marginBottom: '4px' }}>
              Draft reports awaiting QC confirmation:
            </div>
            <div style={{ fontSize: '1.2rem', fontWeight: 800 }}>
              {data.totalPendingConfirmation} unconfirmed reports
            </div>
          </div>
        </div>
      </div>

      {/* Recent Inspections Table */}
      <div className="glass-panel" style={{ marginTop: '32px', padding: '24px' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '20px' }}>
          <h2 style={{ fontSize: '1.15rem', fontWeight: 700 }}>Recent Inspections</h2>
          <Link to="/history" style={{ color: 'var(--accent-cyan)', fontSize: '0.9rem', textDecoration: 'none', display: 'flex', alignItems: 'center', gap: '4px', fontWeight: 600 }}>
            View full history <ArrowRight size={16} />
          </Link>
        </div>

        {data.recentInspections.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '40px 0', color: 'var(--text-dim)' }}>
            No inspection reports recorded yet. Perform your first inspection!
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>Report #</th>
                  <th>Image</th>
                  <th>Product</th>
                  <th>QC Inspector</th>
                  <th>Defect Category</th>
                  <th>AI Result</th>
                  <th>Confidence</th>
                  <th>Inventory Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {data.recentInspections.map((item) => (
                  <tr key={item.reportId}>
                    <td style={{ fontWeight: 700, color: 'var(--text-dim)' }}>#{item.reportId}</td>
                    <td>
                      <img 
                        src={`${STATIC_BASE_URL}${item.imageUrl}`} 
                        alt="qc preview" 
                        style={{ width: '48px', height: '48px', objectFit: 'cover', borderRadius: '6px', border: '1px solid var(--border-subtle)' }}
                        onError={(e) => { e.target.style.display = 'none'; }}
                      />
                    </td>
                    <td style={{ fontWeight: 600 }}>{item.productName}</td>
                    <td style={{ color: 'var(--text-muted)' }}>{item.inspectorName}</td>
                    <td>
                      <span style={{ fontSize: '0.85rem' }}>{item.defectCategoryName}</span>
                    </td>
                    <td>
                      <StatusBadge status={item.qualityStatus} />
                    </td>
                    <td>
                      <span style={{ fontWeight: 600 }}>
                        {Math.round(item.confidenceScore * 100)}%
                      </span>
                    </td>
                    <td>
                      {item.isUserConfirmed ? (
                        <span style={{ color: 'var(--status-pass)', fontSize: '0.82rem', fontWeight: 600 }}>
                          ✓ Stock Updated
                        </span>
                      ) : (
                        <span style={{ color: 'var(--status-review)', fontSize: '0.82rem', fontWeight: 600 }}>
                          ⏳ Pending Confirmation
                        </span>
                      )}
                    </td>
                    <td>
                      <Link 
                        to={item.isUserConfirmed ? `/history/${item.reportId}` : `/inspect/preview/${item.reportId}`} 
                        className="btn btn-secondary"
                        style={{ padding: '6px 12px', fontSize: '0.8rem' }}
                      >
                        {item.isUserConfirmed ? 'View Details' : 'Confirm'}
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
