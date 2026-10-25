import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { 
  Filter, Search, Calendar, RefreshCw, AlertCircle, 
  ChevronLeft, ChevronRight, Eye, X
} from 'lucide-react';
import { inspectionService } from '../services/inspectionService';
import { productService } from '../services/productService';
import { STATIC_BASE_URL } from '../services/api';
import StatusBadge from '../components/StatusBadge';

export default function InspectionHistory() {
  const [reports, setReports] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');

  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedProductId, setSelectedProductId] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('');
  const [isConfirmedFilter, setIsConfirmedFilter] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);

  useEffect(() => {
    productService.getProducts().then(setProducts).catch(() => {});
  }, []);

  const fetchReports = async () => {
    try {
      setLoading(true);
      setErrorMessage('');

      const params = {
        page: currentPage,
        pageSize: 10
      };

      if (searchTerm.trim()) params.search = searchTerm.trim();
      if (selectedProductId) params.productId = selectedProductId;
      if (selectedStatus) params.status = selectedStatus;
      if (isConfirmedFilter !== '') params.isConfirmed = isConfirmedFilter === 'true';
      if (startDate) params.startDate = `${startDate}T00:00:00Z`;
      if (endDate) params.endDate = `${endDate}T23:59:59Z`;

      const data = await inspectionService.getInspections(params);
      setReports(data.items);
      setTotalPages(data.totalPages);
      setTotalItems(data.totalItems);
    } catch (err) {
      setErrorMessage(err.message || 'Failed to load inspection history.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReports();
  }, [currentPage, selectedProductId, selectedStatus, isConfirmedFilter, startDate, endDate]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setCurrentPage(1);
    fetchReports();
  };

  const handleResetFilters = () => {
    setSearchTerm('');
    setSelectedProductId('');
    setSelectedStatus('');
    setIsConfirmedFilter('');
    setStartDate('');
    setEndDate('');
    setCurrentPage(1);
  };

  return (
    <div>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '28px', flexWrap: 'wrap', gap: '16px' }}>
        <div>
          <h1 style={{ fontSize: '1.8rem', fontWeight: 800, letterSpacing: '-0.02em', marginBottom: '6px' }}>
            Inspection History &amp; Audit Log
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
            Search and review all inspection records, inventory confirmation statuses, defect classifications, and date ranges.
          </p>
        </div>

        <button className="btn btn-secondary" onClick={fetchReports} title="Refresh records">
          <RefreshCw size={16} /> Refresh
        </button>
      </div>

      {/* Filter Bar with Date Filters and Search */}
      <div className="glass-panel" style={{ padding: '24px', marginBottom: '24px' }}>
        {/* Search Row */}
        <form onSubmit={handleSearchSubmit} style={{ display: 'flex', gap: '12px', marginBottom: '16px' }}>
          <div style={{ position: 'relative', flex: 1 }}>
            <Search size={18} style={{ position: 'absolute', left: '14px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-dim)' }} />
            <input
              type="text"
              className="form-input"
              placeholder="Search by product name, inspector, defect category, or diagnostic notes..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{ paddingLeft: '40px' }}
            />
          </div>
          <button type="submit" className="btn btn-primary" style={{ padding: '10px 20px' }}>
            Search
          </button>
        </form>

        {/* Dropdowns & Date Pickers Grid */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '16px', alignItems: 'flex-end' }}>
          {/* Product Filter */}
          <div className="form-group" style={{ marginBottom: 0 }}>
            <label className="form-label" style={{ fontSize: '0.8rem' }}>Product</label>
            <select
              className="form-select"
              value={selectedProductId}
              onChange={(e) => { setSelectedProductId(e.target.value); setCurrentPage(1); }}
            >
              <option value="">All products</option>
              {products.map((p) => (
                <option key={p.productId} value={p.productId}>
                  {p.productName}
                </option>
              ))}
            </select>
          </div>

          {/* Status Filter */}
          <div className="form-group" style={{ marginBottom: 0 }}>
            <label className="form-label" style={{ fontSize: '0.8rem' }}>Quality Result</label>
            <select
              className="form-select"
              value={selectedStatus}
              onChange={(e) => { setSelectedStatus(e.target.value); setCurrentPage(1); }}
            >
              <option value="">All results</option>
              <option value="Passed">Passed</option>
              <option value="Failed">Failed</option>
              <option value="NeedsReview">Needs Review</option>
            </select>
          </div>

          {/* Inventory Status */}
          <div className="form-group" style={{ marginBottom: 0 }}>
            <label className="form-label" style={{ fontSize: '0.8rem' }}>Inventory Status</label>
            <select
              className="form-select"
              value={isConfirmedFilter}
              onChange={(e) => { setIsConfirmedFilter(e.target.value); setCurrentPage(1); }}
            >
              <option value="">All</option>
              <option value="true">Confirmed to stock</option>
              <option value="false">Pending confirmation</option>
            </select>
          </div>

          {/* Date Filters (Item 8) */}
          <div className="form-group" style={{ marginBottom: 0 }}>
            <label className="form-label" style={{ fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '4px' }}>
              <Calendar size={13} color="var(--accent-cyan)" />
              <span>From Date</span>
            </label>
            <input
              type="date"
              className="form-input"
              value={startDate}
              onChange={(e) => { setStartDate(e.target.value); setCurrentPage(1); }}
              style={{ fontSize: '0.85rem' }}
            />
          </div>

          <div className="form-group" style={{ marginBottom: 0 }}>
            <label className="form-label" style={{ fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '4px' }}>
              <Calendar size={13} color="var(--accent-cyan)" />
              <span>To Date</span>
            </label>
            <input
              type="date"
              className="form-input"
              value={endDate}
              onChange={(e) => { setEndDate(e.target.value); setCurrentPage(1); }}
              style={{ fontSize: '0.85rem' }}
            />
          </div>

          <div>
            <button className="btn btn-secondary" onClick={handleResetFilters} style={{ width: '100%', padding: '12px' }}>
              Reset Filters
            </button>
          </div>
        </div>
      </div>

      {errorMessage && (
        <div className="alert-banner alert-error">
          <AlertCircle size={20} />
          <div>{errorMessage}</div>
        </div>
      )}

      {/* Reports Table */}
      <div className="glass-panel" style={{ padding: '24px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
          <span style={{ fontSize: '0.9rem', color: 'var(--text-muted)' }}>
            Found <strong>{totalItems}</strong> inspection reports
          </span>
          <span style={{ fontSize: '0.85rem', color: 'var(--text-dim)' }}>
            Page {currentPage} / {totalPages || 1}
          </span>
        </div>

        {loading ? (
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <div className="spinner" style={{ margin: '0 auto 16px auto', width: '28px', height: '28px' }} />
            <p style={{ color: 'var(--text-muted)' }}>Loading inspection records...</p>
          </div>
        ) : reports.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '60px 0', color: 'var(--text-dim)' }}>
            No inspection records match the current filter criteria.
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
                  <th>Date / Time</th>
                  <th>Inventory Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {reports.map((r) => (
                  <tr key={r.reportId}>
                    <td style={{ fontWeight: 700, color: 'var(--text-dim)' }}>#{r.reportId}</td>
                    <td>
                      <img
                        src={`${STATIC_BASE_URL}${r.imageUrl}`}
                        alt="Thumbnail"
                        style={{ width: '44px', height: '44px', objectFit: 'cover', borderRadius: '6px', border: '1px solid var(--border-subtle)' }}
                        onError={(e) => { e.target.style.display = 'none'; }}
                      />
                    </td>
                    <td style={{ fontWeight: 600 }}>{r.productName}</td>
                    <td style={{ color: 'var(--text-muted)' }}>{r.inspectorName}</td>
                    <td style={{ fontSize: '0.85rem' }}>{r.defectCategoryName}</td>
                    <td>
                      <StatusBadge status={r.qualityStatus} />
                    </td>
                    <td>
                      <span style={{ fontWeight: 600 }}>
                        {Math.round(r.confidenceScore * 100)}%
                      </span>
                    </td>
                    <td style={{ fontSize: '0.8rem', color: 'var(--text-dim)' }}>
                      {new Date(r.createdAt).toLocaleString('en-US')}
                    </td>
                    <td>
                      {r.isUserConfirmed ? (
                        <span style={{ color: 'var(--status-pass)', fontSize: '0.8rem', fontWeight: 600 }}>
                          ✓ Stock Updated
                        </span>
                      ) : (
                        <span style={{ color: 'var(--status-review)', fontSize: '0.8rem', fontWeight: 600 }}>
                          ⏳ Pending Confirmation
                        </span>
                      )}
                    </td>
                    <td>
                      <Link
                        to={r.isUserConfirmed ? `/history/${r.reportId}` : `/inspect/preview/${r.reportId}`}
                        className="btn btn-secondary"
                        style={{ padding: '6px 12px', fontSize: '0.8rem' }}
                      >
                        {r.isUserConfirmed ? 'View Details' : 'Confirm'}
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination Controls */}
        {totalPages > 1 && (
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '12px', marginTop: '24px' }}>
            <button
              className="btn btn-secondary"
              onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
              disabled={currentPage <= 1}
              style={{ padding: '6px 12px' }}
            >
              <ChevronLeft size={16} /> Previous
            </button>
            <span style={{ fontSize: '0.9rem', color: 'var(--text-muted)' }}>
              Page {currentPage} / {totalPages}
            </span>
            <button
              className="btn btn-secondary"
              onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
              disabled={currentPage >= totalPages}
              style={{ padding: '6px 12px' }}
            >
              Next <ChevronRight size={16} />
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
