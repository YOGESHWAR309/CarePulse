import * as signalR from '@microsoft/signalr';

let connection = null;

export const startConnection = async (onReceive) => {
  if (connection) return connection;

  connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/notifications')
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

  connection.on('ReceiveAppointmentUpdate', (appointment) => {
    try {
      onReceive && onReceive(appointment);
    } catch (e) {
      console.error('Error handling appointment update', e);
    }
  });

  await connection.start();
  return connection;
};

export const stopConnection = async () => {
  if (!connection) return;
  try {
    await connection.stop();
  } catch (e) {
    console.error('Error stopping SignalR connection', e);
  }
  connection = null;
};
