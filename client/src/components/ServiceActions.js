import React, { useState, useEffect } from 'react';
import axios from 'axios';
import ActionLogs from './ActionLogs';

const ServiceActions = ({ serviceName, token }) => {
    const [actions, setActions] = useState([]);
    const [selectedAction, setSelectedAction] = useState(null);
    const [uuid, setUuid] = useState(null);

    useEffect(() => {
        const fetchActions = async () => {
            const response = await axios.get(`/api/v1/services/${serviceName}`, {
                headers: { Authorization: `Bearer ${token}` },
            });
            setActions(response.data);
        };
        fetchActions();
    }, [serviceName, token]);

    const triggerAction = async (action) => {
        const response = await axios.post(
            `/api/v1/services/${serviceName}`,
            { ActionName: action },
            {
                headers: { Authorization: `Bearer ${token}` },
            }
        );
        console.log("response.data.taskId");
        console.log(response.data.taskId);
        setUuid(response.data.taskId);
        setSelectedAction(action);
    };

    return (
        <div>
            <h3>Actions pour {serviceName}</h3>
            <ul>
                {actions.map((action) => (
                    <li key={action}>
                        <button className="action-button" onClick={() => triggerAction(action)}>
                            {action}
                        </button>
                    </li>
                ))}
            </ul>
            {uuid && (
                <ActionLogs uuid={uuid} token={token} action={selectedAction} />
            )}
        </div>
    );
};

export default ServiceActions;
