import React, { useState, useEffect } from 'react';
import { Package, Plus, AlertCircle, RefreshCw, CheckCircle2 } from 'lucide-react';
import { productService } from '../services/productService';

export default function ProductsList() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [showModal, setShowModal] = useState(false);

  // New product form
  const [productName, setProductName] = useState('');
  const [category, setCategory] = useState('');
  const [totalStock, setTotalStock] = useState(100);
  const [saving, setSaving] = useState(false);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      setErrorMessage('');
      const data = await productService.getProducts();
      setProducts(data);
    } catch (err) {
      setErrorMessage(err.message || 'Failed to load product catalog.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const handleCreateProduct = async (e) => {
    e.preventDefault();
    if (!productName.trim() || !category.trim()) return;

    try {
      setSaving(true);
      await productService.createProduct({
        productName: productName.trim(),
        category: category.trim(),
        totalStock: Number(totalStock) || 0
      });

      setShowModal(false);
      setProductName('');
      setCategory('');
      setTotalStock(100);
      fetchProducts();
    } catch (err) {
      alert(err.message || 'Error creating new product.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '28px', flexWrap: 'wrap', gap: '16px' }}>
        <div>
          <h1 style={{ fontSize: '1.8rem', fontWeight: 800, letterSpacing: '-0.02em', marginBottom: '6px' }}>
            Product Catalog &amp; Inventory Management
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
            Track inspection distribution: Total registered units, Quality-approved stock, and Defective-quarantined stock.
          </p>
        </div>

        <div style={{ display: 'flex', gap: '12px' }}>
          <button className="btn btn-secondary" onClick={fetchProducts} title="Refresh records">
            <RefreshCw size={16} /> Refresh
          </button>
          <button className="btn btn-primary" onClick={() => setShowModal(true)}>
            <Plus size={18} /> Add New Product
          </button>
        </div>
      </div>

      {errorMessage && (
        <div className="alert-banner alert-error">
          <AlertCircle size={20} />
          <div>{errorMessage}</div>
        </div>
      )}

      <div className="glass-panel" style={{ padding: '24px' }}>
        {loading ? (
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <div className="spinner" style={{ margin: '0 auto 16px auto', width: '28px', height: '28px' }}></div>
            <p style={{ color: 'var(--text-muted)' }}>Loading product catalog...</p>
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Product Name</th>
                  <th>Category / Product Line</th>
                  <th>Total Stock</th>
                  <th>Passed (Approved)</th>
                  <th>Failed (Defective)</th>
                  <th>Uninspected</th>
                  <th>Pass Rate</th>
                </tr>
              </thead>
              <tbody>
                {products.map((p) => {
                  const inspected = p.passedStock + p.failedStock;
                  const rate = inspected > 0 ? Math.round((p.passedStock / inspected) * 100) : 100;
                  return (
                    <tr key={p.productId}>
                      <td style={{ fontWeight: 700, color: 'var(--text-dim)' }}>#{p.productId}</td>
                      <td style={{ fontWeight: 600 }}>{p.productName}</td>
                      <td>
                        <span style={{ background: 'rgba(255,255,255,0.05)', padding: '3px 8px', borderRadius: '4px', fontSize: '0.8rem' }}>
                          {p.category}
                        </span>
                      </td>
                      <td style={{ fontWeight: 700 }}>{p.totalStock}</td>
                      <td style={{ color: 'var(--status-pass)', fontWeight: 700 }}>{p.passedStock}</td>
                      <td style={{ color: 'var(--status-fail)', fontWeight: 700 }}>{p.failedStock}</td>
                      <td style={{ color: 'var(--text-dim)' }}>{p.uninspectedStock}</td>
                      <td>
                        <span style={{ fontWeight: 700, color: rate >= 90 ? 'var(--status-pass)' : 'var(--status-review)' }}>
                          {rate}%
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal: Create New Product */}
      {showModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(6px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 100, padding: '20px' }}>
          <div className="glass-panel" style={{ width: '100%', maxWidth: '480px', padding: '32px', background: '#0f1523', border: '1px solid rgba(255,255,255,0.15)' }}>
            <h2 style={{ fontSize: '1.25rem', fontWeight: 800, marginBottom: '20px' }}>
              Register New Product
            </h2>

            <form onSubmit={handleCreateProduct}>
              <div className="form-group">
                <label className="form-label">Product Name *</label>
                <input
                  type="text"
                  className="form-input"
                  value={productName}
                  onChange={(e) => setProductName(e.target.value)}
                  placeholder="e.g. Lithium Battery Casing 5000mAh"
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Category / Product Line *</label>
                <input
                  type="text"
                  className="form-input"
                  value={category}
                  onChange={(e) => setCategory(e.target.value)}
                  placeholder="e.g. Energy &amp; Power Components"
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Registered Stock Quantity</label>
                <input
                  type="number"
                  min="0"
                  className="form-input"
                  value={totalStock}
                  onChange={(e) => setTotalStock(e.target.value)}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px', marginTop: '28px' }}>
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowModal(false)}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={saving}
                >
                  {saving ? 'Saving...' : 'Add Product'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
