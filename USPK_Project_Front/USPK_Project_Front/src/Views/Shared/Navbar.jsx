export default function Navbar({ onNavigate, user, canManageUsers, onLogout }) {
    return <nav className="navbar navbar-expand-lg navbar-dark">
        <div className="container">
            <a className="navbar-brand" href={user ? '#home' : '#login'} onClick={() => onNavigate(user ? 'home' : 'login')}>USPK Project</a>
            <button className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Открыть меню">
                <span className="navbar-toggler-icon" />
            </button>
            <div className="collapse navbar-collapse" id="navbarNav">
                <ul className="navbar-nav ms-auto align-items-lg-center">
                    {user && <li className="nav-item"><a className="nav-link" href="#home" onClick={() => onNavigate('home')}>Главная</a></li>}
                    {user && <li className="nav-item"><a className="nav-link" href="#timetable" onClick={() => onNavigate('timetable')}>Расписание</a></li>}
                    {canManageUsers && <li className="nav-item"><a className="nav-link" href="#admin-users" onClick={() => onNavigate('admin-users')}>Пользователи</a></li>}
                    {user ? <>
                        <li className="nav-item"><span className="nav-link">{user.username}</span></li>
                        <li className="nav-item"><button className="btn btn-outline-light btn-sm ms-lg-2" type="button" onClick={onLogout}>Выйти</button></li>
                    </> : <li className="nav-item"><a className="nav-link" href="#login" onClick={() => onNavigate('login')}>Войти</a></li>}
                </ul>
            </div>
        </div>
    </nav>;
}
