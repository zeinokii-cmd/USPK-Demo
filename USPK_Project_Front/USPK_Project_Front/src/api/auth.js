import axios from 'axios';
import api from './client';

export async function login(email, password) {
    const { data } = await api.post('/auth/login', { email, password });
    return data;
}

export async function getCurrentUser() {
    const { data } = await api.get('/auth/me');
    return data;
}

export async function createUser(values) {
    const { data } = await api.post('/admin/users', values);
    return data;
}

export async function listUsers() {
    const { data } = await api.get('/admin/users');
    return data;
}

export function apiErrorMessage(error) {
    if (!axios.isAxiosError(error) || !error.response) return 'Не удалось связаться с сервером. Проверьте, запущен ли Backend.';
    if (error.response.status === 503) return 'Сервис авторизации пока недоступен. Требуется подключение базы данных и настройка JWT.';
    if (error.response.status === 401) return 'Неверный Email или пароль либо срок сеанса истёк.';
    if (error.response.status === 403) return 'Недостаточно прав для этого действия.';
    return error.response.data?.title || 'Не удалось выполнить запрос.';
}
