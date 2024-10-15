import React, { useEffect, useState } from 'react';

const ActionLogs = ({ uuid, token, action }) => {
    const [logs, setLogs] = useState([]);

    useEffect(() => {
        let ws;
        const connectWebSocket = () => {
            ws = new WebSocket(`ws://localhost:5094/api/v1/ws/${uuid}?token=${token}`);

            ws.onopen = () => console.log("WebSocket connection established");

            ws.onmessage = (event) => {
                setLogs((prevLogs) => [...prevLogs, event.data]);
            };

            ws.onerror = (error) => console.error("WebSocket error:", error);

            ws.onclose = () => {
                console.log("WebSocket connection closed, attempting to reconnect...");
                setTimeout(connectWebSocket, 3000);  // Reconnexion après 3 secondes
            };
        };

        connectWebSocket();

        return () => {
            if (ws) ws.close();
        };
    }, [uuid, token]);



    return (
        <div>
            <h4>Logs pour l'action {action}</h4>
            <div className="logs-container">
                {logs.map((log, index) => (
                    <p key={index}>{log}</p>
                ))}
            </div>
        </div>
    );
};

export default ActionLogs;
