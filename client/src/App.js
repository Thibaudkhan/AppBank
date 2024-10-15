import React, { useContext } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthContext, AuthProvider } from './contexts/AuthContext';
import Login from './components/Login';
import Main from './components/Main';
import './App.css'; // Import du fichier CSS

function App() {
    const { token } = useContext(AuthContext);

    return (
        <Router>
            <Routes>
                <Route path="/" element={token ? <Main /> : <Login />} />
                <Route path="/login" element={<Login />} />
                <Route path="/main" element={<Main />} />
            </Routes>
        </Router>
    );
}

const AppWrapper = () => (
    <AuthProvider>
        <App />
    </AuthProvider>
);

export default AppWrapper;
