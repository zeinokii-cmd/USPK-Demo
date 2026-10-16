import { useEffect, useState } from 'react';
import CreateUser from './CreateUser';
import { apiErrorMessage, listUsers } from '../api/auth';

const roleNames = { admin: 'Администратор', student: 'Студент', parent: 'Родитель', teacher: 'Преподаватель' };

export default function Users() {
    const [users, setUsers] = useState([]);
    const [error, setError] = useState('');
    const [revision, setRevision] = useState(0);

    useEffect(() => {
        let active = true;
        listUsers().then((result) => { if (active) setUsers(result); })
            .catch((requestError) => { if (active) setError(apiErrorMessage(requestError)); });
        return () => { active = false; };
    }, [revision]);

    return <main className="container py-5">
        <h1 className="h2 mb-4">Пользователи</h1>
        <div className="row g-4">
            <div className="col-lg-7">
                <div className="card p-4 shadow-sm">
                    <h2 className="h4">Учётные записи</h2>
                    {error && <p className="alert alert-danger" role="alert">{error}</p>}
                    <div className="table-responsive">
                        <table className="table table-striped align-middle">
                            <thead><tr><th>Имя</th><th>Email</th><th>Роль</th></tr></thead>
                            <tbody>{users.map((user) => <tr key={user.id}>
                                <td>{user.username}</td><td>{user.email}</td><td>{roleNames[user.role] || user.role}</td>
                            </tr>)}</tbody>
                        </table>
                    </div>
                </div>
            </div>
            <div className="col-lg-5"><CreateUser onCreated={() => setRevision((value) => value + 1)} /></div>
        </div>
    </main>;
}
