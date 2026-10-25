import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
export const STATIC_BASE_URL = import.meta.env.VITE_STATIC_BASE_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 45000,
  headers: {
    'Accept': 'application/json'
  }
});

// Response interceptor to normalize error messages
api.interceptors.response.use(
  (response) => response,
  (error) => {
    let formattedError = 'An unexpected error occurred. Please try again.';

    if (error.response) {
      const data = error.response.data;
      if (data && typeof data === 'object') {
        formattedError = data.detail || data.title || data.message || formattedError;
        if (data.errors && typeof data.errors === 'object') {
          const fieldErrors = Object.values(data.errors).flat().join(' ');
          if (fieldErrors) {
            formattedError = `${formattedError} (${fieldErrors})`;
          }
        }
      }
    } else if (error.request) {
      formattedError = 'Unable to connect to the backend API server. Please verify that the server is running.';
    } else {
      formattedError = error.message;
    }

    return Promise.reject(new Error(formattedError));
  }
);

export default api;
