async function registerForPushNotifications() {
    try {
        if (!("serviceWorker" in navigator)) {
            alert("Your browser does not support notifications.");
            return;
        }

        if (!("PushManager" in window)) {
            alert("Push notifications are not supported by your browser.");
            return;
        }

        const permission = await Notification.requestPermission();

        if (permission !== "granted") {
            console.log("Notification permission was not granted.");
            return;
        }

        const registration =
            await navigator.serviceWorker.register("/sw.js");

        await navigator.serviceWorker.ready;

        let subscription =
            await registration.pushManager.getSubscription();

        if (!subscription) {
            subscription =
                await registration.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey:
                        urlBase64ToUint8Array(
                            window.vapidPublicKey
                        )
                });
        }

        const json = subscription.toJSON();

        const response = await fetch(
            "/Patient/RegisterNotificationSubscription",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    endpoint: json.endpoint,
                    p256DH: json.keys.p256dh,
                    auth: json.keys.auth
                })
            }
        );

        if (!response.ok) {
            throw new Error(
                "Failed to register notification subscription."
            );
        }

        alert("Medication reminders have been enabled.");

        console.log(
            "Push subscription registered successfully."
        );
    }
    catch (error) {
        console.error(
            "Push notification registration failed:",
            error
        );
    }
}

function urlBase64ToUint8Array(base64String) {
    const padding =
        "=".repeat(
            (4 - base64String.length % 4) % 4
        );

    const base64 =
        (base64String + padding)
            .replace(/-/g, "+")
            .replace(/_/g, "/");

    const rawData =
        window.atob(base64);

    return Uint8Array.from(
        [...rawData].map(
            character => character.charCodeAt(0)
        )
    );
}