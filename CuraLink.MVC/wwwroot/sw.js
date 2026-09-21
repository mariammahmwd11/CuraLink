self.addEventListener("push", event => {
    let data = {
        title: "CuraLink",
        message: "You have a new notification."
    };

    if (event.data) {
        try {
            data = event.data.json();
        } catch {
            data.message = event.data.text();
        }
    }

    event.waitUntil(
        self.registration.showNotification(
            data.title || "CuraLink",
            {
                body: data.message || ""
            }
        )
    );
});

self.addEventListener("notificationclick", event => {
    event.notification.close();

    event.waitUntil(
        clients.matchAll({
            type: "window",
            includeUncontrolled: true
        }).then(clientList => {
            for (const client of clientList) {
                if ("focus" in client) {
                    return client.focus();
                }
            }

            if (clients.openWindow) {
                return clients.openWindow("/");
            }
        })
    );
});