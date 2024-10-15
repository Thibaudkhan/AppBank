import axios from 'axios';

const api = axios.create({
    baseURL: '/api/v1',
});

export const authenticate = (username, password) =>
    api.post('/auth', { username, password });

export const getServices = (token) =>
    api.get('/services', {
        headers: { Authorization: `Bearer ${token}` },
    });

export const getServiceActions = (serviceName, token) =>
    api.get(`/services/${serviceName}`, {
        headers: { Authorization: `Bearer ${token}` },
    });

export const executeAction = (serviceName, action, token) =>
    api.post(
        `/services/${serviceName}`,
        { action },
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    );
