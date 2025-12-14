import axios from 'axios';

const apiUrl = "https://todo-api-ישן.onrender.com";

const api = axios.create({
  baseURL: apiUrl,
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      console.log('Unauthorized - redirecting to login');
    }
    console.error('API error:', error);
    return Promise.reject(error);
  }
);

export default {
  getTasks: async () => {
    const result = await api.get('/items');
    return result.data;
  },

  addTask: async (name) => {
    console.log('addTask', name);
    const result = await api.post('/items', { name, isComplete: false });
    return result.data;
  },

  // עדכון סטטוס השלמת משימה
  setCompleted: async (id, isCompleted) => {
    console.log('setCompleted', { id, isCompleted });
    const result = await api.put(`/items/${id}`, {
      id,
      name: '', 
      isComplete: isCompleted,
    });
    return result.data;
  },

  deleteTask: async (id) => {
    console.log('deleteTask', id);
    const result = await api.delete(`/items/${id}`);
    return result.data;
  },
};