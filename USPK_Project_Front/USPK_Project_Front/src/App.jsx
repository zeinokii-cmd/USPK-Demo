import { useEffect, useState } from 'react';
import Navbar from './Views/Shared/Navbar';
import Login from './Views/Login';
import Users from './Views/Users';
import { clearAccessToken } from './api/client';

const roleNames = { admin: 'администратор', student: 'студент', parent: 'родитель', teacher: 'преподаватель' };
const protectedViews = new Set(['home', 'timetable', 'admin-users']);
const currentView = () => window.location.hash.slice(1) || 'login';

function App() {
    const [view, setView] = useState(currentView);
    const [user, setUser] = useState(null);
    const canManageUsers = user?.role === import.meta.env.VITE_ADMIN_ROLE;
    const visibleView = user ? (protectedViews.has(view) ? view : 'home') : 'login';

    useEffect(() => {
        const onHashChange = () => setView(currentView());
        const onAuthExpired = () => {
            clearAccessToken();
            setUser(null);
            window.history.replaceState(null, '', '#login');
            setView('login');
        };
        window.addEventListener('hashchange', onHashChange);
        window.addEventListener('auth-expired', onAuthExpired);
        return () => {
            window.removeEventListener('hashchange', onHashChange);
            window.removeEventListener('auth-expired', onAuthExpired);
        };
    }, []);

    useEffect(() => {
        const allowed = user ? protectedViews.has(view) : view === 'login';
        if (!allowed) {
            const destination = user ? 'home' : 'login';
            window.history.replaceState(null, '', `#${destination}`);
        }
    }, [user, view]);

    function navigate(destination) {
        window.location.hash = destination;
        setView(destination);
    }

    function logout() {
        clearAccessToken();
        setUser(null);
        window.history.replaceState(null, '', '#login');
        setView('login');
    }

    return <div className="App">
        <Navbar onNavigate={navigate} user={user} canManageUsers={canManageUsers} onLogout={logout} />
        {visibleView === 'login' && <Login onSuccess={(currentUser) => { setUser(currentUser); navigate('home'); }} />}
        {visibleView === 'admin-users' && (canManageUsers ? <Users /> : <main className="container py-5"><div className="alert alert-warning">Доступ к пользователям есть только у администратора.</div></main>)}
        {visibleView === 'timetable' && <main className="container py-5"><h1>Расписание</h1><p className="alert alert-info">Раздел разрабатывается командой. Демонстрационные данные расписания не подключены.</p></main>}
        {visibleView === 'home' && <main className="container py-5">
            <div className="p-5 bg-light rounded-3 border">
                <span className="badge text-bg-primary mb-3">Демонстрация авторизации</span>
                <h1 className="display-6">USPK Project</h1>
                <p className="lead mb-0">Вход по Email и роли студента, родителя, преподавателя или администратора.</p>
            </div>
            <div className="card mt-4 p-4 shadow-sm">
                <h2 className="h4">Вы вошли как {user.username}</h2>
                <p className="mb-1">Email: {user.email}</p>
                <p className="mb-0">Роль: {roleNames[user.role] || user.role}</p>
                {canManageUsers && <button className="btn btn-primary mt-3 align-self-start" onClick={() => navigate('admin-users')}>Открыть пользователей</button>}
            </div>
        </main>}
    </div>;
}

export default App;
