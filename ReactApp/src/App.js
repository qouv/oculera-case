import React, { useEffect, useRef, useState } from 'react';
import './App.css';

function App() {
	const canvasRef = useRef(null);
	const wsRef = useRef(null);
	const pointsRef = useRef([]);
	const [connected, setConnected] = useState(false);
	const [canvasSize, setCanvasSize] = useState({ width: window.innerWidth, height: window.innerHeight });

	// Canvas setup
	useEffect(() => {
		const handleResize = () => {
			setCanvasSize({
				width: window.innerWidth,
				height: window.innerHeight
			});
		};

		window.addEventListener('resize', handleResize);
		return () => window.removeEventListener('resize', handleResize);
	}, []);

	// WebSocket connection
	useEffect(() => {
		connectWebSocket();

		return () => {
			if (wsRef.current) {
				wsRef.current.close();
			}
		};
	}, []);
	const connectWebSocket = () => {
		wsRef.current = new WebSocket('ws://localhost:8080');

		wsRef.current.onopen = () => {
			console.log('Connected to WebSocket server');
			setConnected(true);
		};

		wsRef.current.onclose = () => {
			console.log('Disconnected from WebSocket server');
			setConnected(false);
			// Try to reconnect after a delay
			setTimeout(connectWebSocket, 3000);
		};

		wsRef.current.onmessage = (event) => {
			try {
				const data = JSON.parse(event.data);
				// Add a new point with opacity and timestamp for fading
				pointsRef.current.push({
					x: data.x,
					y: data.y,
					opacity: 1.0,
					timestamp: Date.now()
				});
			} catch (error) {
				console.error('Error parsing message:', error);
			}
		};

		wsRef.current.onerror = (error) => {
			console.error('WebSocket error:', error);
		};
	};

	// Animation loop
	useEffect(() => {
		const canvas = canvasRef.current;
		const ctx = canvas.getContext('2d');

		let animationId;

		const render = () => {
			// Clear canvas
			ctx.clearRect(0, 0, canvas.width, canvas.height);

			// Fade out points over time
			const now = Date.now();
			pointsRef.current = pointsRef.current
				.filter(point => point.opacity > 0.05) // Remove nearly invisible points
				.map(point => {
					// Reduce opacity based on time elapsed
					const age = now - point.timestamp;
					return {
						...point,
						opacity: Math.max(0, point.opacity - 0.005)
					};
				});

			// Draw lines between points
			if (pointsRef.current.length > 1) {
				ctx.lineJoin = 'round';
				ctx.lineCap = 'round';

				for (let i = 1; i < pointsRef.current.length; i++) {
					const prevPoint = pointsRef.current[i-1];
					const point = pointsRef.current[i];

					// Scale Unity screen coordinates to canvas
					const x1 = (prevPoint.x / 1920) * canvas.width; // Assuming Unity screen is 1920x1080
					const y1 = canvas.height - (prevPoint.y / 1080) * canvas.height; // Invert Y axis
					const x2 = (point.x / 1920) * canvas.width;
					const y2 = canvas.height - (point.y / 1080) * canvas.height;

					ctx.beginPath();
					ctx.moveTo(x1, y1);
					ctx.lineTo(x2, y2);
					ctx.strokeStyle = `rgba(65, 105, 225, ${point.opacity})`;
					ctx.lineWidth = 3 * point.opacity;
					ctx.stroke();

					// Draw a dot at each point
					ctx.beginPath();
					ctx.arc(x2, y2, 2 * point.opacity, 0, Math.PI * 2);
					ctx.fillStyle = `rgba(65, 105, 225, ${point.opacity})`;
					ctx.fill();
				}
			}

			animationId = requestAnimationFrame(render);
		};

		render();

		return () => {
			cancelAnimationFrame(animationId);
		};
	}, [canvasSize]);

	return (
		<div className="app">
			<canvas
				ref={canvasRef}
				width={canvasSize.width}
				height={canvasSize.height}
				className="canvas"
			/>
			<div className={`status ${connected ? 'connected' : 'disconnected'}`}>
				{connected ? 'Connected' : 'Disconnected'}
			</div>
		</div>
	);
}

export default App;
