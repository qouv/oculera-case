const WebSocket = require('ws');

const wss = new WebSocket.Server({ port: 8080 });
const clients = new Set();

wss.on('connection', (ws) => {
	console.log('Client connected');
	clients.add(ws);

	ws.on('message', (message) => {
		// Broadcast to all connected clients except sender
		clients.forEach(client => {
			if (client !== ws && client.readyState === WebSocket.OPEN) {
				client.send(message.toString());
			}
		});
	});

	ws.on('close', () => {
		console.log('Client disconnected');
		clients.delete(ws);
	});
});

console.log('WebSocket server started on port 8080');