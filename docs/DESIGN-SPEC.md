# Đặc tả thiết kế — "Warm Classic"

> Tài liệu này là **hợp đồng giữa người thiết kế và người viết code**. Coder đọc mục 1 → 6 rồi code; không cần (và không được) copy HTML/CSS/ảnh từ site tham khảo.
>
> Phạm vi: hệ thống màu, typography, layout, component, chuyển động — áp cho web app QC hiện tại và cho các trang landing phụ.
>
> File tham chiếu (không bắt buộc dùng): [warm-classic.css](../frontend/src/styles/warm-classic.css) là bản cài đặt mẫu của mục 2 và 5.

---

## 1. Nguyên tắc

Mượn **chất**, không mượn **hình**. Bốn câu để tự kiểm tra mọi quyết định:

1. **Nền sáng, chữ trầm, một màu nhấn.** Không dùng quá 1 màu nhấn (vàng đồng) cho hành động; trạng thái (đạt/lỗi/chờ) là ngoại lệ vì chúng mang nghĩa.
2. **Khoảng thở lớn hơn bạn nghĩ.** Section cách nhau 96–120px, tiêu đề cách nội dung 16px, không phải 40px.
3. **Serif chỉ để "nói", sans để "đọc".** Tiêu đề trang/mục dùng serif; số liệu, bảng, form, nút dùng sans.
4. **Ít chuyển động, chậm, không nảy.** Một trang tối đa 2 hiệu ứng; mọi thứ 300–800ms; cấm bounce/elastic.

Câu chốt khi phân vân: *nếu phải bỏ một thứ cho trang nhẹ hơn, bỏ hiệu ứng trước, bỏ màu nhấn sau.*

---

## 2. Design tokens

### 2.1 Màu — bảng "sáng" (landing, trang giới thiệu)

| Token | Hex | Dùng cho |
| :--- | :--- | :--- |
| `--ck-bar` | `#1f1f1f` | thanh thông báo trên cùng |
| `--ck-brown` | `#4a3820` | header, footer, nền khối đậm |
| `--ck-brown-deep` | `#3a2b16` | viền, hover của nền nâu |
| `--ck-badge` | `#664626` | vòng tròn icon, chip đậm |
| `--ck-cream` | `#f7f2e8` | nền trang |
| `--ck-cream-card` | `#fdfaf4` | nền card |
| `--ck-cream-deep` | `#efe6d6` | section xen kẽ, nền bảng |
| `--ck-heading` | `#f6efe2` | tiêu đề trên ảnh/nền nâu |
| `--ck-ink` | `#3f3a33` | chữ chính |
| `--ck-ink-soft` | `#5a544b` | chữ phụ, mô tả |
| `--ck-gold-deco` | `#c9a97d` | **chỉ** viền, icon, gạch trang trí |
| `--ck-gold-text` | `#8f6a34` | **chữ** màu vàng (tiêu đề card, link) |
| `--ck-line-light` | `rgba(247,242,232,.45)` | viền trên nền nâu |
| `--ck-line-dark` | `rgba(63,58,51,.12)` | viền trên nền kem |

> ⚠️ **Bắt buộc:** vàng `#c9a97d` chữ trên nền kem chỉ đạt ~2.1:1 — **không dùng làm màu chữ**. Chữ vàng phải là `#8f6a34` (~4.7:1). Đây là lỗi phổ biến nhất khi bắt chước phong cách này.

### 2.2 Màu — bảng "tối" (app QC hiện tại)

Giữ nguyên nền công nghiệp, chỉ thêm một màu nhấn:

| Token | Hex | Ghi chú |
| :--- | :--- | :--- |
| `--bg-dark` | `#080b12` | nền chính tối |
| `--bg-surface` | `#0f1523` | nền container / header |
| `--bg-card` | `rgba(19, 27, 44, 0.75)` | nền card kính (glassmorphism) |
| `--accent` | `#c9a97d` | **vàng đồng** thay cyan ở đúng 3 chỗ: nav active, nút chính, focus ring |
| `--text-main` | `#f8fafc` | chữ chính sáng |
| `--text-soft` | `#e8e3d8` | thay trắng tinh, dịu hơn |
| `--status-pass` | `#10b981` | **không đổi** (chuẩn ISO chất lượng) |
| `--status-fail` | `#f43f5e` | **không đổi** (chuẩn ISO chất lượng) |
| `--status-review` | `#f59e0b` | **không đổi** (chuẩn ISO chất lượng) |

Lý do giữ màu trạng thái: đỏ/xanh/amber đã được người dùng đọc theo nghĩa. Đổi sang nâu–vàng sẽ làm mất khả năng phân biệt Passed/Failed trong 0.2 giây.

### 2.3 Độ tương phản (đã tính, dùng để tự kiểm)

| Cặp | Tỉ lệ | Kết luận |
| :--- | ---: | :--- |
| `#f7f2e8` trên `#4a3820` | ≈ 10.0:1 | ✅ mọi cỡ chữ |
| `#5a544b` trên `#fdfaf4` | ≈ 7.2:1 | ✅ mọi cỡ chữ |
| `#8f6a34` trên `#fdfaf4` | ≈ 4.7:1 | ✅ chữ thường |
| `#c9a97d` trên `#fdfaf4` | ≈ 2.1:1 | ❌ chỉ trang trí |
| `#ffffff` trên `#664626` | ≈ 8.5:1 | ✅ icon/chữ trong badge |
| `#c9a97d` trên `#0f172a` | ≈ 8.0:1 | ✅ dùng cho app tối |

### 2.4 Typography

```
--font-display : "Playfair Display", "Cormorant Garamond", Georgia, serif
--font-body    : "Lato", "Inter", system-ui, "Segoe UI", sans-serif
--font-mono    : ui-monospace, "Cascadia Code", Consolas, monospace
```

Tải bằng Google Fonts (OFL). **Không** dùng file font của site tham khảo.

| Vai trò | Font | Size | Line-height | Weight | Tracking |
| :--- | :--- | ---: | ---: | ---: | ---: |
| Display (hero) | serif | `clamp(2.6rem, 5.6vw, 5rem)` | 1.08 | 500 | -0.01em |
| H1 trang | serif | 40px | 1.15 | 500 | -0.01em |
| H2 mục | serif | 30px | 1.2 | 500 | 0 |
| H3 card | serif | 24px | 1.25 | 500 | 0 |
| Body | sans | 16px | 1.75 | 300–400 | 0 |
| Body nhỏ | sans | 14px | 1.65 | 400 | 0 |
| Nhãn/table header | sans | 12px | 1.4 | 700 | 0.08em, UPPERCASE |
| Số liệu KPI | sans | 32–40px | 1.1 | 700 | `tabular-nums` |

Quy tắc:
- Heading tối đa **2 dòng** ở desktop; dùng `text-wrap: balance`.
- Số liệu, mã báo cáo, % → **luôn** sans + `font-variant-numeric: tabular-nums` (tránh nhảy cột).
- Không dùng serif cho: bảng, form, nút, badge, số.

### 2.5 Khoảng cách, bo góc, bóng

```
--space: 4px  (thang: 4 8 12 16 24 32 48 64 96 128)
--container: 1180px (nội dung) / 1320px (khung rộng)
--gutter: 20px (mobile) / 32px (tablet) / 56px (desktop)
--radius: 0 / 2px   (phong cách này vuông; chỉ chip dùng pill)
--shadow-1: 0 18px 40px -24px rgba(60,40,20,.35)
--shadow-2: 0 34px 60px -34px rgba(60,40,20,.38)
```

Không dùng `border-radius: 16px+`. Nếu thấy tròn quá → sai chất.

### 2.6 Chuyển động (motion tokens)

```
--ease     : cubic-bezier(.22,.61,.36,1)   /* vào nhanh, ra chậm */
--ease-out : cubic-bezier(.16,1,.3,1)      /* cho khối lớn */
--dur-fast : 200ms    /* hover, focus */
--dur-base : 350ms    /* đổi màu, nhấc card */
--dur-slow : 700ms    /* xuất hiện khi scroll */
```

---

## 3. Layout

### 3.1 Breakpoint

| Tên | Min-width | Cột | Gutter |
| :--- | ---: | ---: | ---: |
| mobile | 0 | 4 | 20px |
| tablet | 768px | 8 | 32px |
| desktop | 1024px | 12 | 40px |
| wide | 1280px | 12 | 56px |

### 3.2 Nhịp dọc

- Giữa các section: **96px** (mobile 56px), **120px** cho section lớn.
- Tiêu đề mục → nội dung: **16px**. Nội dung → CTA: **32px**.
- Trong card: padding **32px** (mobile 24px).
- Không bao giờ đặt 2 khối đậm cạnh nhau không có 96px đệm.

### 3.3 Các khuôn bố cục được phép

| Khuôn | Dùng khi | Ghi chú |
| :--- | :--- | :--- |
| **Hero toàn màn** | trang giới thiệu | ảnh phủ + tiêu đề serif, cao 70–80vh, **không** quá 90vh |
| **3 cột thẻ nổi** | 3 điểm nhấn ngang hàng | thẻ chồng lên hero 96px; badge tròn nhô lên 112px |
| **Split 1:1** | ảnh + nội dung | gap 64px, ảnh luôn `object-fit: cover` |
| **Danh sách + lọc** | lịch sử, bảng | bộ lọc trên, bảng dưới, không đặt lọc bên trái ở mobile |
| **Lưới 2 cột KPI** | dashboard | 5 thẻ: 3 + 2, không để thẻ lẻ cuối hàng |

### 3.4 App QC — bố cục không đổi

Giữ nguyên cấu trúc hiện có ([Dashboard](../frontend/src/pages/Dashboard.jsx), [InspectionHistory](../frontend/src/pages/InspectionHistory.jsx)). Chỉ thay token và thêm chuyển động ở mục 5.3. **Không** đổi bảng sang card, không đổi vị trí filter.

---

## 4. Component

### 4.1 Header
- Cao 72px (mobile 64px), nền `--ck-brown`, chữ `--ck-cream`.
- Logo: serif, `letter-spacing: .18em`.
- Nav: 17px, mục active có gạch **18×2px** dưới chân chữ (không đổi màu chữ).
- CTA phải cùng: viền 1px `--ck-line-light`, hover **đảo màu** (nền kem, chữ nâu), 350ms.
- Sticky chỉ khi đã cuộn > 80px; khi sticky thêm `shadow-1`, giảm cao còn 64px.

### 4.2 Button

| Loại | Nền | Chữ | Viền | Hover |
| :--- | :--- | :--- | :--- | :--- |
| Primary | `--ck-brown` | `--ck-cream` | – | nền `--ck-brown-deep`, nhấc 1px |
| Outline | trong suốt | `--ck-cream` | 1px `--ck-line-light` | đảo màu |
| Ghost | trong suốt | `--ck-ink` | – | nền `rgba(63,58,51,.06)` |

- Padding: 14px 28px; radius 2px; font sans 16px.
- Focus: `outline: 2px solid var(--ck-gold-deco); outline-offset: 2px` — **bắt buộc**, không bỏ.
- Disabled: `opacity: .45; cursor: not-allowed` — không đổi màu.

### 4.3 Card
- Nền `--ck-cream-card`, padding 64/32/42, bóng `--shadow-2`, radius 2px.
- Badge tròn 96px, nền `--ck-badge`, icon trắng 36px, nhô lên trên 112px.
- Tiêu đề: serif 24–30px, màu `--ck-gold-text`.
- Mô tả: sans 16px/1.85, `--ck-ink-soft`, tối đa 3 dòng (`line-clamp`).

### 4.4 Bảng (app QC)
- Header: 12px, 700, uppercase, tracking .08em, nền `--ck-cream-deep` (bảng sáng) hoặc `--surface` (bảng tối).
- Hàng cao 56px; hover đổi nền 4% độ sáng, 200ms.
- Ảnh thumbnail 44×44, radius 2px.
- Cột số (confidence, stock): phải, `tabular-nums`.
- Không kẻ dọc; chỉ kẻ ngang 1px `--line-dark`.

### 4.5 Trạng thái
Chỉ 3 màu, giữ đúng ngữ nghĩa: `Passed` xanh `#10b981`, `Failed` đỏ `#f43f5e`, `NeedsReview` amber `#f59e0b`. Badge dạng pill, chữ 12px 700 uppercase, kèm icon — **không** dùng màu vàng đồng cho trạng thái.

### 4.6 Trạng thái tải / rỗng / lỗi
- Loading: skeleton (không spinner toàn trang), shimmer 1.4s.
- Rỗng: icon 48px + 1 câu giải thích + 1 CTA.
- Lỗi: banner trên cùng, nền đỏ nhạt, có nút "Thử lại" — không dùng modal.

---

## 5. Chuyển động

### 5.1 Bảng hiệu ứng được phép

| # | Hiệu ứng | Áp cho | Thời lượng | Easing | Trigger |
| ---: | :--- | :--- | ---: | :--- | :--- |
| 1 | Fade-up 20–26px | tiêu đề hero, card, section | 700ms | `--ease` | vào khung nhìn |
| 2 | Stagger | danh sách card | +100ms/ phần tử, tối đa 4 | `--ease` | cùng #1 |
| 3 | Hover lift 4–8px | card, thẻ KPI | 350–400ms | `--ease` | hover |
| 4 | Ảnh zoom ≤ 1.06 | ảnh trong khung | 800–900ms | `--ease` | hover |
| 5 | Gạch chân vẽ ra | nav active, link | 350ms | `--ease` | hover/active |
| 6 | Crossfade slider | hero | 1200ms | `--ease` | autoplay 6–8s |
| 7 | Ken Burns | ảnh nền hero | 12–16s | linear | autoplay |
| 8 | Arrow slide 6px | nút có mũi tên | 300ms | `--ease` | hover |
| 9 | Toast trượt lên | sau khi xác nhận | 250ms | `--ease` | sự kiện |
| 10 | Shimmer skeleton | khi tải dữ liệu | 1400ms | linear | lặp |

### 5.2 Bị cấm

- Bounce, elastic, `cubic-bezier` có overshoot.
- Parallax nhiều lớp, hiệu ứng theo con trỏ (tilt 3D).
- Animate **bảng dữ liệu** hoặc số liệu KPI đếm lên (gây nhiễu khi đọc).
- Quá **2 hiệu ứng tự chạy** trên một trang.
- `transition: all` (luôn khai báo thuộc tính cụ thể).
- Chuyển động dài hơn 1.5s cho thành phần tương tác.

### 5.3 Bắt buộc

```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: .01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: .01ms !important;
  }
}
```

- Nội dung **không** được ẩn vĩnh viễn nếu JS lỗi: `.ck-reveal` chỉ ẩn khi có class `js-ready` trên `<html>`.
- Không dùng hiệu ứng để che nội dung đang tải (không "giả" dữ liệu).

---

## 6. Áp vào app QC — việc cụ thể

1. Thêm token vào [index.css](../frontend/src/index.css): `--accent: #c9a97d` (vàng đồng), `--text-soft: #e8e3d8`, `--font-display: 'Playfair Display', serif`, giữ nguyên 3 màu trạng thái.
2. Đổi **đúng 3 chỗ** dùng accent: `.nav-link.active`, `.btn-primary`, `:focus-visible`. Không đổi màu biểu đồ.
3. Heading trang (h1, h2) → `--font-display`; **không** đổi heading trong bảng/card số liệu.
4. Thêm serif font qua [index.html](../frontend/index.html) — giữ nguyên `lang="en"` (theo quy chuẩn hệ thống 100% tiếng Anh) và tiêu đề thương hiệu chuẩn `<title>AI Product QC Assistant — Defect Inspection & Quality Control</title>`.
5. Thêm `useReveal` cho danh sách card ở Dashboard; **không** thêm cho bảng.
6. Hover hàng bảng: `background: rgba(255,255,255,.04)` + 200ms.
7. Focus ring vàng cho mọi input/select/button (`outline: 2px solid var(--accent); outline-offset: 2px`).

---

## 7. Checklist nghiệm thu

Trước khi báo xong, tự kiểm 10 điểm:

- [ ] Không có chữ nào dùng `#c9a97d` trên nền sáng (phải là `#8f6a34`).
- [ ] Body text đạt ≥ 4.5:1; tiêu đề lớn ≥ 3:1 (kiểm bằng DevTools contrast).
- [ ] Serif chỉ xuất hiện ở heading; bảng/số/nút là sans.
- [ ] Mỗi trang ≤ 2 hiệu ứng; không có `transition: all`.
- [ ] `prefers-reduced-motion` tắt được toàn bộ chuyển động.
- [ ] Focus ring hiển thị rõ trên nền tối và nền sáng.
- [ ] Không có layout shift khi ảnh tải (đặt `width/height` hoặc `aspect-ratio`).
- [ ] Số liệu dùng `tabular-nums`, không nhảy cột khi cập nhật.
- [ ] Radius ≤ 2px (trừ chip/pill).
- [ ] Không có asset (ảnh, font, icon, audio) lấy từ site tham khảo.

---

## 8. Ghi chú nguồn

Bảng màu và bố cục ở trên được **phân tích từ một ảnh chụp** site tham khảo, rồi viết lại thành hệ thống riêng cho dự án này. Không có file CSS/JS/ảnh/font nào của site đó được sử dụng. Nếu cần đối chiếu: ảnh chụp màn hình là nguồn duy nhất, và mọi giá trị ở đây đã được điều chỉnh để đạt tương phản và tính nhất quán — kể cả khi điều đó khác với bản gốc (ví dụ màu chữ vàng).
