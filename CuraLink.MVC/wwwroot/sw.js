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

    // Opens the patient dashboard, where the in-app notification (same
    // title/message) is also waiting in the bell dropdown. Falls back to
    // focusing an already-open CuraLink tab if there is one.
    const targetUrl = "/Patient/Dashboard";

    event.waitUntil(
        clients.matchAll({
            type: "window",
            includeUncontrolled: true
        }).then(clientList => {
            for (const client of clientList) {
                if ("focus" in client) {
                    if ("navigate" in client) {
                        client.navigate(targetUrl);
                    }
                    return client.focus();
                }
            }

            if (clients.openWindow) {
                return clients.openWindow(targetUrl);
            }
        })
    );
});
