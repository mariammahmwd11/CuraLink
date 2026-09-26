async function getAccessToken() {
    const response = await fetch("/Auth/GetAccessToken");

    if (!response.ok) {
        throw new Error("Unable to get access token.");
    }

    const data = await response.json();

    return data.accessToken;
}

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7188/hubs/notifications", {
        accessTokenFactory: getAccessToken
    })
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", (notification) => {
    console.log("🔥 SIGNALR NOTIFICATION RECEIVED", notification);

    alert(`${notification.title}\n${notification.message}`);
});
connection.start()
    .then(() => {
        console.log("SignalR connected successfully.");
    })
    .catch(error => {
        console.error("SignalR connection failed:", error);
    });