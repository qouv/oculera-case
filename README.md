# Real-Time Mouse Movement Integration

This project creates a system where mouse movements captured in a Unity application are transmitted in real-time to a React application for visualization.

![alt text](https://raw.githubusercontent.com/qouv/oculera-case/refs/heads/main/Documentation/mobile-react-screen.png )
![alt text](https://raw.githubusercontent.com/qouv/oculera-case/refs/heads/main/Documentation/unity-app-screen.png)



## System Components

1. **Unity Application**: Captures mouse position data and sends it to the WebSocket server
2. **WebSocket Server**: Acts as a broker for real-time communication
3. **React Application**: Receives and visualizes the mouse movement data

![alt text](https://raw.githubusercontent.com/qouv/oculera-case/d498f364ceb4d17c05c11571ae51072499f5d71b/Documentation/system-diagram.svg)

## Technical Decisions

### Communication Protocol: WebSockets
- **Rationale**: WebSockets provide a persistent, low-latency, bidirectional connection perfect for real-time applications. Unlike HTTP polling, WebSockets maintain a single connection, reducing overhead and enabling true real-time updates.

### Messaging Protocol: JSON
- **Rationale**: JSON is universally supported, human-readable, and efficiently parsed across platforms. It provides a good balance between message size and ease of development.

### Visualization Method: Canvas with Fading Trail
- **Rationale**: HTML Canvas offers high-performance rendering capabilities ideal for real-time visualizations. The fading trail approach visually indicates both current and recent movements, providing context to the user.

### Message Queue Implementation
- **Rationale**: A client-side message queue ensures no data is lost during connection interruptions. Messages are stored locally and transmitted when connection is restored.

## Setup Instructions

### Prerequisites
- Node.js (v14 or later)
- Unity 2021.3 or later
- npm or yarn

### WebSocket Server Setup
1. Navigate to the `SocketApp` directory
2. Install dependencies:
   ```
   npm install
   ```
3. Start the server:
   ```
   node app.js
   ```

### React Application Setup
1. Navigate to the `ReactApp` directory
2. Install dependencies:
   ```
   npm install
   ```
3. Start the development server:
   ```
   npm start
   ```
4. The application will be available at `http://localhost:3000`

### Unity Application Start
1. Open the Unity App


## Additional Features

1. **Automatic Reconnection**: Both the Unity and React applications automatically attempt to reconnect if the WebSocket connection is lost.

2. **Message Queueing**: The Unity application queues messages when the connection is interrupted and sends them when the connection is restored.

3. **Responsive Design**: The React visualization adapts to different screen sizes and maintains proper aspect ratio.

4. **Visual Feedback**: Connection status is displayed in the React app for immediate feedback.

## Usage

1. Start the WebSocket server
2. Launch the React application
3. Run the Unity application
4. Move your mouse in the Unity application window and observe the movements visualized in real-time on the React application

## Troubleshooting

- **Connection Issues**: Ensure the WebSocket server is running before launching the client applications
- **Visualization Scale**: If the mouse movements appear scaled incorrectly, adjust the screen resolution settings in the Unity MouseTracker script
