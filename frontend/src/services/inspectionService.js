import api from './api';

export const inspectionService = {
  async getInspections(params = {}) {
    const response = await api.get('/inspections', { params });
    return response.data;
  },

  async getInspectionById(id) {
    const response = await api.get(`/inspections/${id}`);
    return response.data;
  },

  async analyzeInspection(formData, onUploadProgress) {
    const response = await api.post('/inspections/analyze', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress
    });
    return response.data;
  },

  async confirmInspection(id, confirmData = {}) {
    const response = await api.post(`/inspections/${id}/confirm`, confirmData);
    return response.data;
  }
};
