import { useEffect } from 'react';

/**
 * Gắn class "is-visible" cho mọi phần tử .ck-reveal khi nó vào khung nhìn.
 * Gọi hook này SAU khi dữ liệu đã render (nếu render bất đồng bộ), ví dụ:
 *
 *   const [data, setData] = useState(null);
 *   useReveal([data]);
 *
 * Tham số `deps` để observer được tạo lại khi nội dung mới xuất hiện.
 */
export default function useReveal(deps = []) {
  useEffect(() => {
    const nodes = document.querySelectorAll('.ck-reveal:not(.is-visible)');

    // Trình duyệt cũ hoặc môi trường không hỗ trợ: hiện luôn, không ẩn nội dung.
    if (typeof IntersectionObserver === 'undefined') {
      nodes.forEach((el) => el.classList.add('is-visible'));
      return undefined;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('is-visible');
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.15, rootMargin: '0px 0px -10% 0px' }
    );

    nodes.forEach((el) => observer.observe(el));
    return () => observer.disconnect();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);
}
