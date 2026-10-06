let realtimeConnection = null;

async function getAccessToken() {

    const response = await fetch("/Realtime/Token", {
        method: "GET",
        credentials: "include"
    });

    if (!response.ok) {
        throw new Error("Unable to get access token.");
    }

    const data = await response.json();

    return data.token;
}


async function startRealtimeConnection() {

    try {

        const token = await getAccessToken();

        realtimeConnection =
            new signalR.HubConnectionBuilder()
                .withUrl(
                    "https://localhost:7188/hubs/notifications",
                    {
                        accessTokenFactory: () => token
                    })
                .withAutomaticReconnect()
                .build();


        // =========================================================
        // Notifications
        // =========================================================

        realtimeConnection.on(
            "ReceiveNotification",
            notification => {

                console.log(
                    "🔔 Notification received:",
                    notification
                );

                // Add notification to bell
                if (typeof addNotification === "function") {

                    addNotification({
                        title:
                            notification.title
                            ?? "Notification",

                        message:
                            notification.message
                            ?? "",

                        createdAt:
                            notification.createdAt
                            ?? new Date(),

                        isRead:
                            notification.isRead
                            ?? false
                    });
                }

                // Optional browser notification
                showBrowserNotification(notification);
            });


        // =========================================================
        // Chat
        // =========================================================

        realtimeConnection.on(
            "ReceiveChatMessage",
            message => {

                console.log(
                    "💬 Chat message received:",
                    message
                );

                if (typeof addMessageToChat === "function") {

                    addMessageToChat(message);
                }
            });


        // =========================================================
        // Read Receipt
        // =========================================================

        realtimeConnection.on(
            "ChatMessageRead",
            receipt => {

                console.log(
                    "👀 Chat message read:",
                    receipt
                );

                if (
                    typeof markMessagesAsReadInUI ===
                    "function"
                ) {

                    markMessagesAsReadInUI(receipt);
                }
            });


        // =========================================================
        // Connection
        // =========================================================

        realtimeConnection.onreconnecting(error => {

            console.warn(
                "🟡 SignalR reconnecting...",
                error
            );
        });


        realtimeConnection.onreconnected(connectionId => {

            console.log(
                "🟢 SignalR reconnected:",
                connectionId
            );
        });


        realtimeConnection.onclose(error => {

            console.error(
                "🔴 SignalR connection closed:",
                error
            );
        });


        await realtimeConnection.start();

        console.log(
            "🟢 SignalR connected successfully."
        );

    }
    catch (error) {

        console.error(
            "❌ SignalR connection failed:",
            error
        );

        setTimeout(
            startRealtimeConnection,
            5000
        );
    }
}


// =========================================================
// Browser notification
// =========================================================

function showBrowserNotification(notification) {

    if (!("Notification" in window)) {
        return;
    }

    if (Notification.permission !== "granted") {
        return;
    }

    const title =
        notification.title ?? "CuraLink";

    const message =
        notification.message ?? "";

    new Notification(title, {
        body: message,
        icon: "/favicon.ico"
    });
}


// =========================================================
// Start SignalR
// =========================================================

document.addEventListener(
    "DOMContentLoaded",
    startRealtimeConnection
);