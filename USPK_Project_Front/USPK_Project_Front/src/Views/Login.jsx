import { useState } from 'react';
import { apiErrorMessage, getCurrentUser, login } from '../api/auth';
import { clearAccessToken, setAccessToken } from '../api/client';

export default function Login({ onSuccess }) {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    async function handleSubmit(event) {
        event.preventDefault();
        setError('');
        if (!email.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim()) || !password) {
            setError('Укажите корректный Email и пароль.');
            return;
        }
        setLoading(true);
        try {
            const response = await login(email.trim(), password);
            if (!response?.token) throw new Error('Missing token');
            setAccessToken(response.token);
            const user = await getCurrentUser();
            onSuccess(user);
        } catch (requestError) {
            clearAccessToken();
            setError(apiErrorMessage(requestError));
        } finally {
            setLoading(false);
        }
    }

    return (
        <main className="auth-card card shadow-sm">
            <h1 className="h3">Выполнить вход</h1>
            <form onSubmit={handleSubmit}>
                <label className="form-label" htmlFor="login-email">Email</label>
                <input className="form-control" id="login-email" type="email" autoComplete="username" required maxLength="254"
                    value={email} onChange={(event) => setEmail(event.target.value)} />
                <label className="form-label" htmlFor="login-password">Пароль</label>
                <input className="form-control" id="login-password" type="password" autoComplete="current-password" required
                    value={password} onChange={(event) => setPassword(event.target.value)} />
                {error && <p className="alert alert-danger" role="alert">{error}</p>}
                <button className="btn btn-primary" type="submit" disabled={loading}>{loading ? 'Выполняется вход…' : 'Войти'}</button>
            </form>
        </main>
    );
}
