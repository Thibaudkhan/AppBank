import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost:8083/api/v1',  // Toutes les requêtes seront relatives à cette base URL
});

export const authenticate = async (username, password) => {
    try {
        return  await api.post('/auth', { username, password });
    } catch (error) {
        console.error('Erreur lors de l\'authentificaAtion :', error);
        throw error;
    }
};

export const getServices = async (token) => {
    try {
        return await api.get('/services', {
            headers: { Authorization: `Bearer ${token}` },
        });
    } catch (error) {
        console.error('Erreur lors de la récupération des services :', error);
        throw error;
    }
};

export const getServiceActions = async (serviceName, token) => {
    try {
        return await api.get(`/services/${serviceName}`, {
            headers: { Authorization: `Bearer ${token}` },
        });
    } catch (error) {
        console.error(`Erreur lors de la récupération des actions du service ${serviceName} :`, error);
        throw error;
    }
};

export const executeAction = async (serviceName, action, token) => {
    try {
        return  await api.post(
            `/services/${serviceName}`,
            { ActionName :action },
            {
                headers: { Authorization: `Bearer ${token}` },
            }
        );
    } catch (error) {
        console.error(`Erreur lors de l'exécution de l'action ${action} sur le service ${serviceName} :`, error);
        throw error;
    }
};
