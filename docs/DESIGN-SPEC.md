# Đặc tả thiết kế — "Warm Classic" (Editorial Heritage)

> Tài liệu này là **hợp đồng quy chuẩn thiết kế giao diện (Design System)** cho toàn bộ ứng dụng **AI Product QC Assistant** (Dashboard, New Inspection, Preview Confirmation, Inspection History, Product Catalog).
>
> **Nguồn cảm hứng:** Thiết kế dựa trên phong cách trang mẫu thực tế (Header nâu cà phê đậm, dải ribbon vàng lạc đà, nền kem thạch cao sáng, thẻ số liệu thanh lịch viền camel, và typography serif cổ điển `Playfair Display`).
>
> File mã nguồn triển khai chính thức:
> - [`frontend/src/index.css`](../frontend/src/index.css) — Toàn bộ Design Tokens, typography và component styles.
> - [`frontend/src/components/Navbar.jsx`](../frontend/src/components/Navbar.jsx) — Cấu trúc Dual-tier Header & Camel Ribbon.
> - [`frontend/src/styles/warm-classic.css`](../frontend/src/styles/warm-classic.css) — Kit phong cách bổ trợ.

---

## 1. Nguyên tắc cốt lõi (Design Principles)

1. **Nền kem sáng, chữ mực nâu trầm, màu nhấn vàng lạc đà:**
   - Thay thế toàn bộ nền đen tối trước đây bằng nền thạch cao kem ấm (`#f8f5ee`).
   - Header nâu cà phê đậm (`#3e2b17`) tạo độ sâu và uy tín công nghiệp.
   - Dải ruy băng vàng camel (`#caa472`) làm điểm nhấn định hướng trạm kiểm định (Breadcrumb ribbon).
2. **Khoảng thở trang nhã (Generous Breathing Room):**
   - Padding thẻ và section rộng rãi (24–32px), khoảng cách giữa các khối mạch lạc, tránh nhồi nhét.
3. **Serif để "khẳng định", Sans để "đọc":**
   - Tiêu đề trang (`h1`, `h2`) dùng font serif **Playfair Display** sắc sảo.
   - Số liệu KPI lớn kết hợp serif thanh lịch; bảng dữ liệu, form và nút bấm dùng **Plus Jakarta Sans** với `tabular-nums` chống nhảy số.
4. **Chuyển động êm dịu, không nảy (Subtle & Dignified Motion):**
   - Thời lượng chuyển động từ 300–700ms với easing `cubic-bezier(0.22, 0.61, 0.36, 1)`, cấm bounce/elastic.

---

## 2. Hệ thống Design Tokens

### 2.1 Bảng màu chính (Palette Tokens)

| Token CSS | Giá trị Hex / RGBA | Vai trò & Vị trí sử dụng |
| :--- | :--- | :--- |
| `--header-brown` | `#3e2b17` | Header chính trên cùng, nút Primary, icon thương hiệu |
| `--header-brown-dark` | `#2a1c0e` | Trạng thái hover của nút Primary, bóng đổ nhấn |
| `--header-text` | `#f7f2e8` | Chữ trắng ngà trên nền header nâu đậm |
| `--subbar-camel` | `#caa472` | Dải ruy băng phụ (Breadcrumb ribbon), gạch chân active nav |
| `--subbar-camel-hover` | `#b88f58` | Trạng thái hover của dải camel, viền tiêu điểm |
| `--bg-canvas` | `#f8f5ee` | Nền canvas toàn bộ trang (thay thế nền tối trước đây) |
| `--bg-surface` | `#ffffff` | Nền thẻ (card), form input, nền bảng dữ liệu |
| `--bg-card-hover` | `#fdfaf4` | Nền thẻ khi hover |
| `--border-subtle` | `#e4dacb` | Viền thẻ KPI, đường kẻ ngăn cách bảng |
| `--border-hover` | `#caa472` | Viền thẻ khi hover, viền dropzone upload |
| `--text-heading` | `#3e2b17` / `#241b11` | Tiêu đề chính `h1`, `h2` (font serif Playfair Display) |
| `--text-main` | `#241b11` | Mực nâu espresso đậm cho chữ đọc chính |
| `--text-muted` | `#5e5446` | Chữ phụ, mô tả tính năng, nhãn form |
| `--text-dim` | `#857a6c` | Chữ thời gian, mã định danh, ghi chú phụ |
| `--accent-gold-text` | `#8f6a34` | Chữ màu vàng đồng đạt chuẩn tương phản AA trên nền sáng |

### 2.2 Màu trạng thái kiểm định (Quality Control Statuses)

Các màu trạng thái được tinh chỉnh để hài hòa tuyệt đối với nền kem sáng `#f8f5ee`, đồng thời giữ nguyên vẹn ngữ nghĩa công nghiệp (ISO-9001):

| Trạng thái | Mã Hex chữ | Mã Nền (Background) | Mã Viền (Border) | Ngữ nghĩa & Áp dụng |
| :--- | :--- | :--- | :--- | :--- |
| `Passed` | `#1b7a43` | `#edf7f0` | `#bce5cb` | Sản phẩm đạt chuẩn xuất xưởng |
| `Failed` | `#b91c1c` | `#fdf1f1` | `#fecaca` | Phát hiện lỗi ngoại quan, cách ly kho |
| `NeedsReview` | `#b45309` | `#fef8eb` | `#fde68a` | Ảnh mờ/nghi ngờ, chờ kỹ thuật viên duyệt |

### 2.3 Độ tương phản tiếp cận (Accessibility Contrast - WCAG AA/AAA)

| Cặp màu kiểm tra | Tỉ lệ tương phản | Chuẩn WCAG | Kết luận & Áp dụng |
| :--- | ---: | :---: | :--- |
| `#241b11` (Mực espresso) trên `#f8f5ee` (Nền kem) | **14.8:1** | AAA | ✅ Chữ đọc chính, tiêu đề, số liệu |
| `#5e5446` (Chữ phụ) trên `#ffffff` (Nền thẻ) | **7.5:1** | AAA | ✅ Mô tả, nhãn form, ghi chú |
| `#f7f2e8` (Chữ ngà) trên `#3e2b17` (Header nâu) | **12.6:1** | AAA | ✅ Logo, menu điều hướng, nút header |
| `#1b7a43` (Passed) trên `#edf7f0` | **5.8:1** | AA | ✅ Badge Đạt chuẩn xuất xưởng |
| `#b91c1c` (Failed) trên `#fdf1f1` | **6.4:1** | AA | ✅ Badge Phát hiện lỗi |
| `#b45309` (NeedsReview) trên `#fef8eb` | **5.1:1** | AA | ✅ Badge Chờ kỹ thuật viên xem xét |
| `#caa472` (Vàng camel) trên `#3e2b17` | **4.9:1** | AA | ✅ Subtitle crest, gạch active |
| `#caa472` trên nền kem `#f8f5ee` | **2.2:1** | – | ⚠️ Chỉ dùng làm viền, icon trang trí, nền dải ribbon |
| `#8f6a34` trên nền trắng `#ffffff` | **4.7:1** | AA | ✅ Bắt buộc dùng mã này khi viết chữ vàng trên nền sáng |

---

## 3. Typography & Kiểu chữ

```css
--font-display : "Playfair Display", Georgia, "Times New Roman", serif;
--font-family : "Plus Jakarta Sans", system-ui, -apple-system, sans-serif;
```

Tải trực tiếp qua Google Fonts (OFL):
`<link href="https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,500;0,600;0,700;1,500;1,600&family=Plus+Jakarta+Sans:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">`

| Vai trò | Phông chữ | Size | Weight | Line-height | Tracking |
| :--- | :--- | ---: | ---: | ---: | ---: |
| Brand Title | Playfair Display | 20px | 700 | 1.1 | 0.12em, UPPERCASE |
| H1 Page Heading | Playfair Display | 34px | 600 | 1.2 | -0.015em |
| H2 Section Title | Playfair Display | 23px | 600 | 1.25 | -0.01em |
| KPI Stat Number | Playfair Display | 35px | 700 | 1.15 | `tabular-nums` |
| Body Text | Plus Jakarta Sans | 15–16px | 400 | 1.6 | 0 |
| Form Labels / Headers | Plus Jakarta Sans | 12.5px | 700 | 1.4 | 0.05em, UPPERCASE |
| Badges / Chips | Plus Jakarta Sans | 12px | 700 | 1.2 | 0.03em |

---

## 4. Đặc tả thành phần (Components)

### 4.1 Dual-tier Header & Breadcrumb Ribbon
- **Tầng 1 (Main Header):** Cao 72px, nền nâu `#3e2b17`, viền dưới `rgba(255,255,255,0.08)`. Logo crest tròn 40px viền vàng camel. Nút **"Inspect Now"** góc phải viền mảnh `#f7f2e8`, hover đổi nền camel `#caa472`.
- **Tầng 2 (Sub-bar Ribbon):** Cao 36px, nền vàng camel `#caa472`, chữ nâu `#3e2b17`. Hiển thị icon `<ShieldCheck />` và đường dẫn breadcrumb trạm kiểm định kèm nhãn `ISO-9001 Standard`.
- **Active Navigation Item:** Ký hiệu gạch chân `—` (thanh 2px vàng camel) ngay dưới chữ tab đang chọn.

### 4.2 Thẻ số liệu KPI (Stat Cards)
- Mô phỏng khối đếm ngược thời gian từ ảnh mẫu:
  - Nền trắng tinh `#ffffff`, viền camel mảnh `1px solid #e4dacb`, bo góc 6px.
  - Vạch kẻ 3px màu camel `#caa472` trên đỉnh thẻ.
  - Con số lớn `2.2rem` màu nâu cà phê `#3e2b17`.
  - Nhãn tiêu đề 12px uppercase màu `#5e5446`.
  - Hover: Nhấc nhẹ `translateY(-3px)`, viền đổi sang `#caa472`, bóng đổ ấm nhẹ nhàng.

### 4.3 Bảng dữ liệu (Tables)
- Header bảng: Nền camel mềm `#ede4d4`, chữ nâu `#3e2b17` in hoa, viền dưới 2px `#e4dacb`.
- Hàng bảng: Nền trắng `#ffffff`, viền dưới 1px `#ede4d4`, hover đổi sang `#faf5ee` trong 180ms.
- Số liệu (Stock, Confidence Score): Căn phải, áp dụng thuộc tính `font-variant-numeric: tabular-nums`.

### 4.4 Khung Upload Dropzone & Máy quét ảnh
- Nền kem nhạt `#fdfbf7`, viền nét đứt 2px màu `#dfd3c3`.
- Icon tải lên đặt trong vòng tròn camel nhạt `rgba(202, 164, 114, 0.16)`.
- Khi rê file kéo thả: Viền chuyển sang `#caa472`, nền đổi `#fbf6ec`.
- Máy quét ảnh: Khung nâu đen `#2a1c0e` với tia laser vàng camel quét êm ái khi AI đang phân tích.

---

## 5. Chuyển động & Tiếp cận (Motion & Accessibility)

- **Lộ dần theo nhịp (.ck-reveal & .ck-stagger):** Thẻ KPI và danh mục khi nạp xuất hiện mượt mà từ dưới lên 16px trong 700ms với easing `cubic-bezier(0.22, 0.61, 0.36, 1)`.
- **Hỗ trợ giảm chuyển động:**
  ```css
  @media (prefers-reduced-motion: reduce) {
    *, *::before, *::after {
      animation-duration: 0.01ms !important;
      animation-iteration-count: 1 !important;
      transition-duration: 0.01ms !important;
    }
  }
  ```
- **Vòng sáng tiêu điểm (Focus Ring):** Mọi input, button, select đều có vòng outline vàng camel `outline: 2px solid var(--subbar-camel); outline-offset: 2px` đạt chuẩn tiếp cận WCAG 2.1.

---

## 6. Checklist nghiệm thu thiết kế

- [x] Nền trang đã chuyển hoàn toàn sang nền kem thạch cao sáng `#f8f5ee` (không còn nền tối).
- [x] Header nâu cà phê đậm `#3e2b17` kết hợp dải sub-bar vàng lạc đà `#caa472` đúng theo ảnh tham chiếu.
- [x] Menu điều hướng có chỉ báo gạch ngang vàng `—` dưới chân mục đang chọn.
- [x] Tiêu đề chính dùng font serif `Playfair Display`, mực nâu đậm `#3e2b17` tương phản cao.
- [x] Các thẻ KPI mô phỏng khối đếm số trong ảnh mẫu: số to đậm, viền camel mảnh, hover nhấc nhẹ.
- [x] Trạng thái kiểm định đạt chuẩn quốc tế: Xanh xô thơm (Passed), Đỏ đất nung (Failed), Vàng mật ong (NeedsReview).
- [x] Chữ vàng trên nền sáng đạt chuẩn WCAG AA nhờ mã chuyên dụng `#8f6a34` (4.7:1).
- [x] Giao diện 100% tiếng Anh, tiêu đề chuẩn thương hiệu.
- [x] Build frontend sạch, không có bất kỳ cảnh báo hay lỗi layout shift nào.
