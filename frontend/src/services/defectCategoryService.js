import api from './api';

export const defectCategoryService = {
  async getDefectCategories() {
    const response = await api.get('/defect-categories');
    return response.data;
  }
};
