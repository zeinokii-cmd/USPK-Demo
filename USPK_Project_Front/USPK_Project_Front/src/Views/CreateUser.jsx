import { useState } from 'react';
import { apiErrorMessage, createUser } from '../api/auth';

export default function CreateUser({ onCreated }) {
    const [values, setValues] = useState({ email: '', username: '', role: '', initialPassword: '' });
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [loading, setLoading] = useState(false);

    function update(event) {
        setValues((current) => ({ ...current, [event.target.name]: event.target.value }));
    }

    async function handleSubmit(event) {
        event.preventDefault();
        setError('');
        setSuccess('');
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(values.email.trim()) ||
            values.username.trim().length < 2 || !values.role.trim() || values.initialPassword.length < 12) {
            setError('Проверьте Email, имя, роль и пароль длиной от 12 символов.');
            return;
        }
        setLoading(true);
        try {
            const result = await createUser({ ...values, email: values.email.trim(), username: values.username.trim(), role: values.role.trim() });
            setSuccess(`Пользователь ${result.email} создан.`);
            onCreated?.();
            setValues({ email: '', username: '', role: '', initialPassword: '' });
        } catch (requestError) {
            setError(apiErrorMessage(requestError));
        } finally {
            setLoading(false);
        }
    }

    return (
        <section className="card p-4 shadow-sm">
            <h2 className="h4">Создать пользователя</h2>
            <form onSubmit={handleSubmit}>
                <label className="form-label" htmlFor="new-email">Email</label>
                <input className="form-control" id="new-email" name="email" type="email" autoComplete="off" required maxLength="254" value={values.email} onChange={update} />
                <label className="form-label" htmlFor="new-username">Имя пользователя</label>
                <input className="form-control" id="new-username" name="username" required minLength="2" maxLength="100" value={values.username} onChange={update} />
                <label className="form-label" htmlFor="new-role">Роль</label>
                <select className="form-select" id="new-role" name="role" required value={values.role} onChange={update}>
                    <option value="">Выберите роль</option>
                    <option value="student">Студент</option>
                    <option value="parent">Родитель</option>
                    <option value="teacher">Преподаватель</option>
                    <option value="admin">Администратор</option>
                </select>
                <label className="form-label" htmlFor="new-password">Начальный пароль</label>
                <input className="form-control" id="new-password" name="initialPassword" type="password" autoComplete="new-password" required minLength="12" value={values.initialPassword} onChange={update} />
                {error && <p className="alert alert-danger" role="alert">{error}</p>}
                {success && <p className="alert alert-success" role="status">{success}</p>}
                <button className="btn btn-primary" type="submit" disabled={loading}>{loading ? 'Создание…' : 'Создать'}</button>
            </form>
        </section>
    );
}
