import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  UploadCloud, AlertCircle, Sparkles, CheckCircle2, 
  Image as ImageIcon, UserCheck, Package, RotateCcw, ShieldAlert 
} from 'lucide-react';
import { productService } from '../services/productService';
import { inspectionService } from '../services/inspectionService';

export default function UploadInspection() {
  const navigate = useNavigate();
  const fileInputRef = useRef(null);

  const [products, setProducts] = useState([]);
  const [loadingInitial, setLoadingInitial] = useState(true);

  // Form states
  const [selectedProductId, setSelectedProductId] = useState('');
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
        const prodList = await productService.getProducts();
        setProducts(prodList);
        if (prodList.length > 0) {
          setSelectedProductId(prodList[0].productId);
        }
      } catch (err) {
        setErrorMessage('Không thể tải danh sách sản phẩm: ' + err.message);
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
      setErrorMessage('Dung lượng tệp vượt quá giới hạn cho phép (tối đa 10MB).');
      return;
    }

    // Validate format
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!validTypes.includes(file.type)) {
      setErrorMessage('Chỉ chấp nhận các định dạng ảnh hợp lệ: JPG, PNG, WEBP.');
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
      setErrorMessage('Vui lòng chọn sản phẩm cần kiểm định.');
      return;
    }

    if (!selectedFile) {
      setErrorMessage('Vui lòng tải lên ảnh chụp sản phẩm để AI kiểm tra.');
      return;
    }

    try {
      setIsAnalyzing(true);
      setUploadProgress(0);

      const formData = new FormData();
      formData.append('productId', selectedProductId);
      // Mặc định InspectorId = 1 đại diện cho Người dùng ứng dụng (QC Operator)
      formData.append('inspectorId', '1');
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
      setErrorMessage(err.message || 'Lỗi khi gửi ảnh lên máy chủ phân tích AI.');
    } finally {
      setIsAnalyzing(false);
    }
  };

  if (loadingInitial) {
    return (
      <div style={{ textAlign: 'center', padding: '100px 0' }}>
        <div className="spinner" style={{ margin: '0 auto 16px auto', width: '36px', height: '36px' }}></div>
        <p style={{ color: 'var(--text-muted)' }}>Đang tải danh mục sản phẩm...</p>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: '840px', margin: '0 auto' }}>
      <div style={{ marginBottom: '28px' }}>
        <h1 style={{ fontSize: '1.8rem', fontWeight: 800, letterSpacing: '-0.02em', marginBottom: '8px' }}>
          Kiểm Tra Chất Lượng Sản Phẩm (AI Vision QC)
        </h1>
        <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
          Tải ảnh chụp sản phẩm thực tế. Google Gemini AI sẽ phân tích bề mặt, cấu trúc và đưa ra nhận xét chi tiết.
        </p>
      </div>

      {errorMessage && (
        <div className="alert-banner alert-error">
          <AlertCircle size={20} />
          <div>{errorMessage}</div>
        </div>
      )}

      <form onSubmit={handleSubmit} className="glass-panel" style={{ padding: '32px' }}>
        {/* Product & Current QC Operator info */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '20px', marginBottom: '28px' }}>
          <div className="form-group">
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <Package size={15} color="var(--accent-cyan)" />
              <span>Sản phẩm kiểm định *</span>
            </label>
            <select
              className="form-select"
              value={selectedProductId}
              onChange={(e) => setSelectedProductId(e.target.value)}
              disabled={isAnalyzing}
            >
              {products.map((p) => (
                <option key={p.productId} value={p.productId}>
                  {p.productName} ({p.category}) — Tồn: {p.totalStock} (Đạt: {p.passedStock}, Lỗi: {p.failedStock})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <UserCheck size={15} color="var(--accent-cyan)" />
              <span>Người thực hiện kiểm định</span>
            </label>
            <div style={{ 
              padding: '11px 16px', 
              background: 'rgba(255, 255, 255, 0.04)', 
              border: '1px solid var(--border-color)', 
              borderRadius: 'var(--radius-sm)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between'
            }}>
              <span style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: '0.9rem' }}>
                Người dùng ứng dụng (QC Operator)
              </span>
              <span style={{ fontSize: '0.75rem', padding: '3px 8px', borderRadius: '4px', background: 'rgba(56, 189, 248, 0.15)', color: 'var(--accent-cyan)', fontWeight: 600 }}>
                Trực tiếp
              </span>
            </div>
          </div>
        </div>

        {/* Drag and Drop Image Dropzone */}
        <div className="form-group">
          <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
            <ImageIcon size={15} color="var(--accent-cyan)" />
            <span>Ảnh chụp sản phẩm * (JPG, PNG, WEBP — Tối đa 10MB)</span>
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
              <div style={{ width: '56px', height: '56px', borderRadius: '50%', background: 'rgba(202, 164, 114, 0.16)', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 16px auto', color: 'var(--subbar-camel)' }}>
                <UploadCloud size={28} />
              </div>
              <h3 style={{ fontSize: '1.05rem', fontWeight: 600, marginBottom: '6px' }}>
                Kéo thả ảnh vào đây, hoặc <span style={{ color: 'var(--accent-gold-text)' }}>Chọn từ máy tính</span>
              </h3>
              <p style={{ color: 'var(--text-dim)', fontSize: '0.85rem' }}>
                Đảm bảo ảnh rõ nét và đủ sáng để Gemini Vision AI nhận diện chính xác các vết trầy xước, nứt vỡ hay móp méo.
              </p>
            </div>
          ) : (
            <div style={{ position: 'relative' }}>
              <div className="scanner-container" style={{ maxHeight: '380px', display: 'flex', justifyContent: 'center', background: '#000', borderRadius: 'var(--radius-md)' }}>
                <img
                  src={previewUrl}
                  alt="Ảnh kiểm tra xem trước"
                  style={{ maxHeight: '380px', width: 'auto', objectFit: 'contain' }}
                />
                {isAnalyzing && <div className="scanner-laser" />}
              </div>

              {!isAnalyzing && (
                <div style={{ marginTop: '12px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                    Tệp đã chọn: <strong>{selectedFile?.name}</strong> ({(selectedFile?.size / 1024).toFixed(1)} KB)
                  </span>
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => { setSelectedFile(null); setPreviewUrl(''); }}
                    style={{ padding: '6px 14px', fontSize: '0.82rem', display: 'flex', alignItems: 'center', gap: '6px' }}
                  >
                    <RotateCcw size={14} />
                    <span>Chọn ảnh khác</span>
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
            <strong>Nguyên tắc an toàn kho:</strong> Bước phân tích AI chỉ tạo bản nháp kiểm định. <em>Tồn kho sản phẩm hoàn toàn chưa bị thay đổi</em> cho đến khi bạn xem và bấm đồng ý xác nhận.
          </span>
        </div>

        {/* Analyzing Status Indicator */}
        {isAnalyzing && (
          <div style={{ marginTop: '24px', padding: '16px', background: 'rgba(6, 182, 212, 0.08)', borderRadius: 'var(--radius-md)', border: '1px solid rgba(6, 182, 212, 0.25)' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '8px' }}>
              <div className="spinner" style={{ width: '18px', height: '18px' }} />
              <span style={{ fontWeight: 600, color: 'var(--accent-cyan)' }}>
                Đang tải ảnh và phân tích khuyết tật bằng Google Gemini Vision AI...
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
            style={{ minWidth: '260px', padding: '14px 28px' }}
          >
            {isAnalyzing ? (
              <>
                <div className="spinner" />
                <span>Đang phân tích...</span>
              </>
            ) : (
              <>
                <Sparkles size={18} />
                <span>Phân Tích Bằng AI (Gemini Vision)</span>
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
}
