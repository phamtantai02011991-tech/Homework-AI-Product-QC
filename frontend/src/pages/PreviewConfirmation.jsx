import React, { useState, useEffect } from 'react';
import { useParams, useLocation, useNavigate, Link } from 'react-router-dom';
import confetti from 'canvas-confetti';
import { 
  CheckCircle2, XCircle, AlertTriangle, ShieldCheck, 
  ArrowLeft, RotateCcw, RefreshCw, FileText, Package, AlertCircle, ShieldAlert 
} from 'lucide-react';
import { inspectionService } from '../services/inspectionService';
import { defectCategoryService } from '../services/defectCategoryService';
import { STATIC_BASE_URL } from '../services/api';
import StatusBadge from '../components/StatusBadge';

export default function PreviewConfirmation() {
  const { id } = useParams();
  const location = useLocation();
  const navigate = useNavigate();

  const [report, setReport] = useState(location.state?.analysisResult || null);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(!report);
  const [confirming, setConfirming] = useState(false);
  const [confirmedResult, setConfirmedResult] = useState(null);
  const [errorMessage, setErrorMessage] = useState('');

  // Editable QC Override state
  const [selectedStatus, setSelectedStatus] = useState('');
  const [selectedCategoryId, setSelectedCategoryId] = useState('');
  const [notes, setNotes] = useState('');

  useEffect(() => {
    async function loadData() {
      try {
        const catList = await defectCategoryService.getDefectCategories();
        setCategories(catList);

        if (!report && id) {
          setLoading(true);
          const data = await inspectionService.getInspectionById(id);
          // Normalize to match DTO
          const normalized = {
            reportId: data.reportId,
            productId: data.product.productId,
            productName: data.product.productName,
            inspectorId: data.inspector?.inspectorId || 1,
            inspectorName: data.inspector?.inspectorName || 'Người dùng ứng dụng (QC Operator)',
            defectCategoryId: data.defectCategory.defectCategoryId,
            defectCategoryName: data.defectCategory.categoryName,
            severityLevel: data.defectCategory.severityLevel,
            imageUrl: data.imageUrl,
            defectDescription: data.defectDescription,
            qualityStatus: data.qualityStatus,
            confidenceScore: data.confidenceScore,
            isUserConfirmed: data.isUserConfirmed
          };
          setReport(normalized);
          setSelectedStatus(normalized.qualityStatus);
          setSelectedCategoryId(normalized.defectCategoryId);
          setNotes(normalized.defectDescription);
        } else if (report) {
          setSelectedStatus(report.qualityStatus);
          setSelectedCategoryId(report.defectCategoryId);
          setNotes(report.defectDescription);
        }
      } catch (err) {
        setErrorMessage(err.message || 'Không thể tải dữ liệu báo cáo kiểm tra.');
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, [id]);

  const handleConfirm = async () => {
    setErrorMessage('');

    if (selectedStatus === 'NeedsReview') {
      setErrorMessage('Trạng thái "Cần xem xét (Needs Review)" không thể cập nhật tồn kho. Vui lòng chọn "ĐẠT (Passed)" hoặc "LỖI (Failed)" trước khi xác nhận.');
      return;
    }

    try {
      setConfirming(true);
      const result = await inspectionService.confirmInspection(report.reportId, {
        qualityStatus: selectedStatus,
        defectCategoryId: Number(selectedCategoryId) || report.defectCategoryId,
        defectDescription: notes
      });

      setConfirmedResult(result);

      if (selectedStatus === 'Passed') {
        confetti({
          particleCount: 80,
          spread: 70,
          origin: { y: 0.6 }
        });
      }
    } catch (err) {
      setErrorMessage(err.message || 'Lỗi khi xác nhận và cập nhật kho.');
    } finally {
      setConfirming(false);
    }
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '100px 0' }}>
        <div className="spinner" style={{ margin: '0 auto 16px auto', width: '36px', height: '36px' }}></div>
        <p style={{ color: 'var(--text-muted)' }}>Đang tải báo cáo kiểm tra...</p>
      </div>
    );
  }

  if (!report) {
    return (
      <div className="glass-panel" style={{ padding: '40px', textAlign: 'center', maxWidth: '600px', margin: '40px auto' }}>
        <AlertCircle size={48} color="var(--status-fail)" style={{ margin: '0 auto 16px auto' }} />
        <h2>Không tìm thấy báo cáo kiểm định</h2>
        <p style={{ color: 'var(--text-muted)', margin: '12px 0 24px 0' }}>
          Báo cáo này không tồn tại hoặc đã bị xóa khỏi hệ thống.
        </p>
        <Link to="/inspect" className="btn btn-primary">
          Bắt đầu kiểm tra mới
        </Link>
      </div>
    );
  }

  const isAlreadyConfirmed = report.isUserConfirmed || confirmedResult != null;
  const currentConfidence = Math.round((report.confidenceScore || 0) * 100);

  return (
    <div>
      {/* Top Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '28px', flexWrap: 'wrap', gap: '16px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
          <Link to="/history" className="btn btn-secondary" style={{ padding: '8px 12px' }} title="Quay lại danh sách lịch sử">
            <ArrowLeft size={16} />
          </Link>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
              <h1 style={{ fontSize: '1.6rem', fontWeight: 800 }}>
                Kết Quả AI &amp; Xác Nhận Báo Cáo #{report.reportId}
              </h1>
              <StatusBadge status={selectedStatus || report.qualityStatus} isConfirmed={isAlreadyConfirmed} />
            </div>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: '4px' }}>
              Sản phẩm: <strong>{report.productName}</strong> | Người kiểm định: <strong>{report.inspectorName || 'Người dùng ứng dụng (QC Operator)'}</strong>
            </p>
          </div>
        </div>

        <div>
          {isAlreadyConfirmed ? (
            <span style={{ color: 'var(--status-pass)', fontWeight: 700, fontSize: '0.95rem', display: 'flex', alignItems: 'center', gap: '6px' }}>
              <CheckCircle2 size={18} /> ĐÃ CẬP NHẬT TỒN KHO THÀNH CÔNG
            </span>
          ) : (
            <span style={{ color: 'var(--status-review)', fontWeight: 600, fontSize: '0.9rem', display: 'flex', alignItems: 'center', gap: '6px' }}>
              <AlertTriangle size={16} /> CHỜ NGƯỜI DÙNG XÁC NHẬN
            </span>
          )}
        </div>
      </div>

      {errorMessage && (
        <div className="alert-banner alert-error">
          <AlertCircle size={20} />
          <div>{errorMessage}</div>
        </div>
      )}

      {/* Confirmed Success Banner */}
      {confirmedResult && (
        <div className="alert-banner alert-success" style={{ padding: '20px', marginBottom: '28px' }}>
          <CheckCircle2 size={24} color="#10b981" />
          <div style={{ flex: 1 }}>
            <h3 style={{ fontSize: '1.1rem', fontWeight: 700, marginBottom: '4px' }}>
              Xác Nhận Thành Công! Tồn Kho Đã Được Cập Nhật.
            </h3>
            <p style={{ fontSize: '0.9rem' }}>{confirmedResult.message}</p>
            <div style={{ marginTop: '8px', fontSize: '0.85rem', color: '#065f46' }}>
              Tổng tồn kho: <strong>{confirmedResult.updatedInventory.totalStock}</strong> | Đạt chuẩn: <strong>{confirmedResult.updatedInventory.passedStock}</strong> | Hàng lỗi: <strong>{confirmedResult.updatedInventory.failedStock}</strong>
            </div>
          </div>
          <Link to="/" className="btn btn-success" style={{ padding: '8px 16px' }}>
            Xem Dashboard
          </Link>
        </div>
      )}

      {/* Warning for NeedsReview or low confidence */}
      {!isAlreadyConfirmed && (report.qualityStatus === 'NeedsReview' || report.confidenceScore < 0.7) && (
        <div className="alert-banner alert-warning" style={{ marginBottom: '24px' }}>
          <AlertTriangle size={20} />
          <div>
            <strong>Yêu cầu kiểm tra trực tiếp:</strong> Ảnh chụp có thể hơi mờ, thiếu sáng hoặc độ tin cậy AI dưới 70%. 
            Vui lòng xem lại sản phẩm thực tế và chọn <strong>ĐẠT (PASSED)</strong> hoặc <strong>LỖI (FAILED)</strong> trước khi cập nhật kho, hoặc bấm <strong>Tạo lại / Quét lại</strong> để chụp ảnh khác rõ hơn.
          </div>
        </div>
      )}

      {/* Zero Mutation Reminder */}
      {!isAlreadyConfirmed && (
        <div style={{ marginBottom: '20px', padding: '12px 16px', background: 'rgba(56, 189, 248, 0.08)', borderRadius: 'var(--radius-sm)', border: '1px solid rgba(56, 189, 248, 0.25)', display: 'flex', alignItems: 'center', gap: '10px' }}>
          <ShieldAlert size={18} color="var(--accent-cyan)" />
          <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
            <strong>Bảo toàn tồn kho:</strong> Số lượng kho sản phẩm <em>hoàn toàn chưa thay đổi</em>. Tồn kho chỉ cập nhật sau khi bạn bấm <strong>"Đồng ý &amp; Cập nhật vào kho"</strong>.
          </span>
        </div>
      )}

      {/* Split Screen Grid: Image on Left / AI Diagnosis on Right */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(380px, 1fr))', gap: '28px' }}>
        
        {/* Left: Product Image */}
        <div className="glass-panel" style={{ padding: '24px' }}>
          <h2 style={{ fontSize: '1.05rem', fontWeight: 700, marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Package size={18} color="var(--accent-cyan)" />
            <span>Ảnh Chụp Sản Phẩm Kiểm Định</span>
          </h2>

          <div style={{ background: '#000', borderRadius: 'var(--radius-md)', overflow: 'hidden', textAlign: 'center' }}>
            <img 
              src={`${STATIC_BASE_URL}${report.imageUrl}`} 
              alt="Ảnh chụp sản phẩm"
              style={{ width: '100%', maxHeight: '480px', objectFit: 'contain' }}
            />
          </div>

          <div style={{ marginTop: '16px', fontSize: '0.82rem', color: 'var(--text-dim)', textAlign: 'center' }}>
            Đường dẫn tệp lưu trữ: <code>{report.imageUrl}</code>
          </div>
        </div>

        {/* Right: AI Findings & QC Review Controls */}
        <div className="glass-panel" style={{ padding: '24px' }}>
          <h2 style={{ fontSize: '1.05rem', fontWeight: 700, marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <ShieldCheck size={18} color="var(--accent-cyan)" />
            <span>Nhận Xét &amp; Đánh Giá Từ Gemini AI</span>
          </h2>

          {/* AI Metrics Card */}
          <div style={{ background: 'rgba(255,255,255,0.03)', padding: '16px', borderRadius: 'var(--radius-md)', marginBottom: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px', fontSize: '0.88rem' }}>
              <span style={{ color: 'var(--text-muted)' }}>Độ tin cậy của AI (Confidence Score):</span>
              <strong style={{ color: currentConfidence >= 80 ? 'var(--status-pass)' : 'var(--status-review)' }}>
                {currentConfidence}%
              </strong>
            </div>
            <div className="confidence-bar-bg">
              <div 
                className="confidence-bar-fill" 
                style={{ 
                  width: `${currentConfidence}%`, 
                  background: currentConfidence >= 80 
                    ? 'linear-gradient(90deg, #10b981 0%, #059669 100%)' 
                    : 'linear-gradient(90deg, #f59e0b 0%, #d97706 100%)' 
                }} 
              />
            </div>

            <div style={{ marginTop: '12px', fontSize: '0.85rem', display: 'flex', justifyContent: 'space-between' }}>
              <span style={{ color: 'var(--text-muted)' }}>Mức độ khuyết tật:</span>
              <span style={{ fontWeight: 600 }}>{report.severityLevel || 'None'}</span>
            </div>
          </div>

          {/* Quality Decision Toggle */}
          <div className="form-group">
            <label className="form-label">Quyết định chất lượng sản phẩm *</label>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
              <button
                type="button"
                className={`btn ${selectedStatus === 'Passed' ? 'btn-success' : 'btn-secondary'}`}
                onClick={() => {
                  setSelectedStatus('Passed');
                  setSelectedCategoryId(1); // Tự động reset về Không có lỗi
                }}
                disabled={isAlreadyConfirmed}
              >
                <CheckCircle2 size={16} /> ĐẠT (PASSED)
              </button>
              <button
                type="button"
                className={`btn ${selectedStatus === 'Failed' ? 'btn-danger' : 'btn-secondary'}`}
                onClick={() => setSelectedStatus('Failed')}
                disabled={isAlreadyConfirmed}
              >
                <XCircle size={16} /> LỖI (FAILED)
              </button>
            </div>
          </div>

          {/* Defect Category Dropdown */}
          <div className="form-group">
            <label className="form-label">Phân loại lỗi kỹ thuật</label>
            <select
              className="form-select"
              value={selectedCategoryId}
              onChange={(e) => setSelectedCategoryId(e.target.value)}
              disabled={isAlreadyConfirmed}
            >
              {categories.map((c) => (
                <option key={c.defectCategoryId} value={c.defectCategoryId}>
                  {c.categoryName} ({c.severityLevel})
                </option>
              ))}
            </select>
          </div>

          {/* Defect Description */}
          <div className="form-group">
            <label className="form-label">Nhận xét chi tiết từ AI / Ghi chú kiểm định</label>
            <textarea
              className="form-textarea"
              rows={4}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              disabled={isAlreadyConfirmed}
              placeholder="Nhận xét chẩn đoán của Gemini hoặc ghi chú thêm từ bạn..."
            />
          </div>

          {/* Action Buttons: Cho tạo lại / Đồng ý cập nhật vào kho */}
          {!isAlreadyConfirmed ? (
            <div style={{ marginTop: '28px', display: 'flex', gap: '14px', flexWrap: 'wrap' }}>
              <Link 
                to="/inspect" 
                className="btn btn-secondary" 
                style={{ 
                  flex: 1, 
                  minWidth: '180px', 
                  padding: '13px 18px', 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center', 
                  gap: '8px' 
                }}
                title="Hủy kết quả này và chụp/tải lại ảnh khác mà không lưu vào kho"
              >
                <RotateCcw size={17} />
                <span>Tạo lại / Quét lại</span>
              </Link>
              <button
                type="button"
                className="btn btn-primary"
                onClick={handleConfirm}
                disabled={confirming}
                style={{ 
                  flex: 2, 
                  minWidth: '240px', 
                  padding: '13px 24px', 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center', 
                  gap: '8px', 
                  fontWeight: 700 
                }}
              >
                {confirming ? (
                  <>
                    <div className="spinner" />
                    <span>Đang cập nhật tồn kho...</span>
                  </>
                ) : (
                  <>
                    <CheckCircle2 size={18} />
                    <span>Đồng ý &amp; Cập nhật vào kho</span>
                  </>
                )}
              </button>
            </div>
          ) : (
            <div style={{ marginTop: '28px', display: 'flex', gap: '12px' }}>
              <Link 
                to="/inspect" 
                className="btn btn-primary" 
                style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', padding: '12px' }}
              >
                <RotateCcw size={16} />
                <span>Kiểm tra sản phẩm mới</span>
              </Link>
              <Link to="/history" className="btn btn-secondary" style={{ flex: 1, padding: '12px', textAlign: 'center' }}>
                Xem lịch sử kiểm định
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
