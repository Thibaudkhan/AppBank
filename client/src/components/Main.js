import React, { useState, useEffect, useContext } from 'react';
import axios from 'axios';
import { AuthContext } from '../contexts/AuthContext';
import ServiceActions from './ServiceActions';

const Main = () => {
    const { token } = useContext(AuthContext);
    const [services, setServices] = useState([]);
    const [selectedService, setSelectedService] = useState(null);

    useEffect(() => {
        const fetchServices = async () => {
            const response = await axios.get('/api/v1/services', {
                headers: { Authorization: `Bearer ${token}` },
            });
            setServices(response.data);
        };
        fetchServices();
    }, [token]);

    return (
        <div style={{ display: 'flex' }}>
            <aside className="sidebar">
                <h3>Services</h3>
                <ul>
                    {services.length > 0 ? (
                        services.map((service) => (
                            <li key={service} onClick={() => setSelectedService(service)}>
                                {service}
                            </li>
                        ))
                    ) : (
                        <p>Aucun service disponible</p>
                    )}
                </ul>
            </aside>
            <main className="main-content">
                {selectedService ? (
                    <ServiceActions serviceName={selectedService} token={token} />
                ) : (
                    <p>Sélectionnez un service pour voir les actions disponibles.</p>
                )}
            </main>
        </div>
    );
};

export default Main;
