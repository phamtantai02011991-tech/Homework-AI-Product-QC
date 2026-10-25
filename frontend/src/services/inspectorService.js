import api from './api';

export const inspectorService = {
  async getInspectors() {
    const response = await api.get('/inspectors');
    return response.data;
  }
};
