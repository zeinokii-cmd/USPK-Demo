import axios from 'axios';

const api = axios.create({ baseURL: '/api', timeout: 10000 });
let accessToken = null;

api.interceptors.request.use((config) => {
    if (accessToken) config.headers.Authorization = `Bearer ${accessToken}`;
    return config;
});

api.interceptors.response.use((response) => response, (error) => {
    if (error.response?.status === 401 && accessToken) {
        accessToken = null;
        window.dispatchEvent(new Event('auth-expired'));
    }
    return Promise.reject(error);
});

export function setAccessToken(token) {
    accessToken = token;
}

export function clearAccessToken() {
    accessToken = null;
}

export default api;
