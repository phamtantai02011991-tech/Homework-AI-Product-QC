import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  UploadCloud, AlertCircle, Sparkles, CheckCircle2, 
  Image as ImageIcon, UserCheck, Package, RefreshCw, ShieldAlert 
} from 'lucide-react';
import { productService } from '../services/productService';
import { inspectorService } from '../services/inspectorService';
import { inspectionService } from '../services/inspectionService';

export default function UploadInspection() {
  const navigate = useNavigate();
  const fileInputRef = useRef(null);

  const [products, setProducts] = useState([]);
  const [inspectors, setInspectors] = useState([]);
  const [loadingInitial, setLoadingInitial] = useState(true);

  // Form states
  const [selectedProductId, setSelectedProductId] = useState('');
  const [selectedInspectorId, setSelectedInspectorId] = useState('');
  const [selectedFile, setSelectedFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState('');

  // Processing states
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [errorMessage, setErrorMessage] = useState('');
  const [isDragging, setIsDragging] = useState(false);

  useEffect(() => {
    async function loadData() {
      try {
        setLoadingInitial(true);
        const [prodList, inspList] = await Promise.all([
          productService.getProducts(),
          inspectorService.getInspectors()
        ]);
        setProducts(prodList);
        setInspectors(inspList);

        if (prodList.length > 0) setSelectedProductId(prodList[0].productId);
        if (inspList.length > 0) setSelectedInspectorId(inspList[0].inspectorId);
      } catch (err) {
        setErrorMessage('Failed to load product catalog or inspectors list: ' + err.message);
      } finally {
        setLoadingInitial(false);
      }
    }
    loadData();
  }, []);

  const handleFileChange = (file) => {
    setErrorMessage('');
    if (!file) return;

    // Validate size (10 MB limit)
    if (file.size > 10 * 1024 * 1024) {
      setErrorMessage('File size exceeds the allowed limit (10MB).');
      return;
    }

    // Validate format
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!validTypes.includes(file.type)) {
      setErrorMessage('Only valid image formats are accepted: JPG, PNG, WEBP.');
      return;
    }

    setSelectedFile(file);
    const objectUrl = URL.createObjectURL(file);
    setPreviewUrl(objectUrl);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setIsDragging(false);
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileChange(e.dataTransfer.files[0]);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrorMessage('');

    if (!selectedProductId) {
      setErrorMessage('Please select a product to inspect.');
      return;
    }

    if (!selectedInspectorId) {
      setErrorMessage('Please select the assigned QC inspector.');
      return;
    }

    if (!selectedFile) {
      setErrorMessage('Please upload a product image for inspection.');
      return;
    }

    try {
      setIsAnalyzing(true);
      setUploadProgress(0);

      const formData = new FormData();
      formData.append('productId', selectedProductId);
      formData.append('inspectorId', selectedInspectorId);
      formData.append('file', selectedFile);

      const analysisResult = await inspectionService.analyzeInspection(formData, (progressEvent) => {
        if (progressEvent.total) {
          const percent = Math.round((progressEvent.loaded * 100) / progressEvent.total);
          setUploadProgress(percent);
        }
      });

      // Pass the analysis result to PreviewConfirmation page
      navigate(`/inspect/preview/${analysisResult.reportId}`, {
        state: { analysisResult }
      });
    } catch (err) {
      setErrorMessage(err.message || 'Error uploading image to the AI analysis server.');
    } finally {
      setIsAnalyzing(false);
    }
  };

  if (loadingInitial) {
    return (
      <div style={{ textAlign: 'center', padding: '100px 0' }}>
        <div className="spinner" style={{ margin: '0 auto 16px auto', width: '36px', height: '36px' }}></div>
        <p style={{ color: 'var(--text-muted)' }}>Loading inspection form...</p>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: '840px', margin: '0 auto' }}>
      <div style={{ marginBottom: '28px' }}>
        <h1 style={{ fontSize: '1.8rem', fontWeight: 800, letterSpacing: '-0.02em', marginBottom: '8px' }}>
          New Product Inspection (AI Defect Analysis)
        </h1>
        <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
          Upload a clear product image. Google Gemini Vision AI will detect visible surface and structural defects in real time.
        </p>
      </div>

      {errorMessage && (
        <div className="alert-banner alert-error">
          <AlertCircle size={20} />
          <div>{errorMessage}</div>
        </div>
      )}

      <form onSubmit={handleSubmit} className="glass-panel" style={{ padding: '32px' }}>
        {/* Product & Inspector Selectors */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '20px', marginBottom: '28px' }}>
          <div className="form-group">
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <Package size={15} color="var(--accent-cyan)" />
              <span>Product to Inspect *</span>
            </label>
            <select
              className="form-select"
              value={selectedProductId}
              onChange={(e) => setSelectedProductId(e.target.value)}
              disabled={isAnalyzing}
            >
              {products.map((p) => (
                <option key={p.productId} value={p.productId}>
                  {p.productName} ({p.category}) — Stock: {p.totalStock} (Passed: {p.passedStock}, Failed: {p.failedStock})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <UserCheck size={15} color="var(--accent-cyan)" />
              <span>Assigned QC Inspector *</span>
            </label>
            <select
              className="form-select"
              value={selectedInspectorId}
              onChange={(e) => setSelectedInspectorId(e.target.value)}
              disabled={isAnalyzing}
            >
              {inspectors.map((i) => (
                <option key={i.inspectorId} value={i.inspectorId}>
                  {i.inspectorName} ({i.department})
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Drag and Drop Image Dropzone */}
        <div className="form-group">
          <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
            <ImageIcon size={15} color="var(--accent-cyan)" />
            <span>Product Image * (JPG, PNG, WEBP — Max 10MB)</span>
          </label>

          <input
            type="file"
            ref={fileInputRef}
            onChange={(e) => e.target.files && handleFileChange(e.target.files[0])}
            accept=".jpg,.jpeg,.png,.webp"
            style={{ display: 'none' }}
          />

          {!previewUrl ? (
            <div
              className={`dropzone ${isDragging ? 'active' : ''}`}
              onDragOver={(e) => { e.preventDefault(); setIsDragging(true); }}
              onDragLeave={() => setIsDragging(false)}
              onDrop={handleDrop}
              onClick={() => fileInputRef.current?.click()}
            >
              <div style={{ width: '56px', height: '56px', borderRadius: '50%', background: 'rgba(6, 182, 212, 0.1)', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 16px auto', color: 'var(--accent-cyan)' }}>
                <UploadCloud size={28} />
              </div>
              <h3 style={{ fontSize: '1.05rem', fontWeight: 600, marginBottom: '6px' }}>
                Drag &amp; drop image here, or <span style={{ color: 'var(--accent-cyan)' }}>Browse from computer</span>
              </h3>
              <p style={{ color: 'var(--text-dim)', fontSize: '0.85rem' }}>
                Ensure the image is sharp and well-lit so the AI can accurately identify scratches, fractures, or deformations.
              </p>
            </div>
          ) : (
            <div style={{ position: 'relative' }}>
              <div className="scanner-container" style={{ maxHeight: '380px', display: 'flex', justifyContent: 'center', background: '#000', borderRadius: 'var(--radius-md)' }}>
                <img
                  src={previewUrl}
                  alt="Inspection Preview"
                  style={{ maxHeight: '380px', width: 'auto', objectFit: 'contain' }}
                />
                {isAnalyzing && <div className="scanner-laser" />}
              </div>

              {!isAnalyzing && (
                <div style={{ marginTop: '12px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                    Selected file: <strong>{selectedFile?.name}</strong> ({(selectedFile?.size / 1024).toFixed(1)} KB)
                  </span>
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => { setSelectedFile(null); setPreviewUrl(''); }}
                    style={{ padding: '6px 14px', fontSize: '0.82rem' }}
                  >
                    Choose Different Image
                  </button>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Zero Mutation Notice */}
        <div style={{ marginTop: '16px', padding: '12px 16px', background: 'rgba(255, 255, 255, 0.03)', borderRadius: 'var(--radius-sm)', border: '1px solid var(--border-color)', display: 'flex', alignItems: 'center', gap: '10px' }}>
          <ShieldAlert size={16} color="var(--accent-cyan)" />
          <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
            <strong>Inventory Safety Guarantee:</strong> This analysis phase only generates an unconfirmed draft report. <em>Product inventory is strictly unchanged</em> until explicitly confirmed in the next step.
          </span>
        </div>

        {/* Analyzing Status Indicator */}
        {isAnalyzing && (
          <div style={{ marginTop: '24px', padding: '16px', background: 'rgba(6, 182, 212, 0.08)', borderRadius: 'var(--radius-md)', border: '1px solid rgba(6, 182, 212, 0.25)' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '8px' }}>
              <div className="spinner" style={{ width: '18px', height: '18px' }} />
              <span style={{ fontWeight: 600, color: 'var(--accent-cyan)' }}>
                Uploading image and analyzing defects via Google Gemini Vision AI...
              </span>
            </div>
            <div className="confidence-bar-bg" style={{ height: '6px' }}>
              <div className="confidence-bar-fill" style={{ width: `${uploadProgress || 60}%`, background: 'var(--gradient-brand)' }} />
            </div>
          </div>
        )}

        {/* Submit Button */}
        <div style={{ marginTop: '32px', display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
          <button
            type="submit"
            className="btn btn-primary"
            disabled={isAnalyzing || !selectedFile}
            style={{ minWidth: '240px', padding: '14px 28px' }}
          >
            {isAnalyzing ? (
              <>
                <div className="spinner" />
                <span>Analyzing...</span>
              </>
            ) : (
              <>
                <Sparkles size={18} />
                <span>Start AI Defect Analysis</span>
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
}
