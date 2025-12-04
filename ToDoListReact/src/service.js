import axios from 'axios';

// יצירת instance של axios עם Base URL ל-API שלנו
const api = axios.create({
  baseURL: 'http://localhost:5053',
});

// interceptor לטיפול בשגיאות (לדוגמה 401)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      console.log('Unauthorized - redirecting to login');
      // כאן אפשר לשלב ניווט ל-login אם תוסיפי ראוטינג
    }
    console.error('API error:', error);
    return Promise.reject(error);
  }
);

export default {
  // שליפת כל המשימות
  getTasks: async () => {
    const result = await api.get('/items');
    return result.data;
  },

  // הוספת משימה חדשה
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

  // מחיקת משימה
  deleteTask: async (id) => {
    console.log('deleteTask', id);
    const result = await api.delete(`/items/${id}`);
    return result.data;
  },
};
