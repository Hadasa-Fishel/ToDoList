import axios from 'axios';

const apiUrl = "https://todo-api-8mie.onrender.com";

const api = axios.create({
  baseURL: apiUrl,
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
      name: '', // שימי לב: ייתכן שהשרת דורש את השם המקורי, אם זה יוצר בעיה נצטרך לתקן כאן
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