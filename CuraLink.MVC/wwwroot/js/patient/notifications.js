(function () {
    "use strict";

    var bell = document.getElementById("notificationBell");
    var badge = document.getElementById("notificationBadge");
    var panel = document.getElementById("notificationPanel");
    var list = document.getElementById("notificationList");
    var markAllBtn = document.getElementById("markAllReadBtn");

    if (!bell || !badge || !panel || !list) {
        // Layout markup not present on this page — nothing to wire up.
        return;
    }

    var POLL_INTERVAL_MS = 60000;
    var panelOpen = false;

    function setBadge(count) {
        if (count > 0) {
            badge.textContent = count > 99 ? "99+" : String(count);
            badge.classList.remove("d-none");
        } else {
            badge.textContent = "0";
            badge.classList.add("d-none");
        }
    }

    function refreshUnreadCount() {
        fetch("/Patient/UnreadNotificationCount", {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Failed to load unread count.");
                }
                return response.json();
            })
            .then(function (data) {
                setBadge(data.count || 0);
            })
            .catch(function (error) {
                console.error("Could not refresh unread notification count:", error);
            });
    }

    function formatTime(isoString) {
        try {
            var date = new Date(isoString);
            return date.toLocaleString(undefined, {
                month: "short",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit"
            });
        } catch (error) {
            return "";
        }
    }

    function renderNotifications(notifications) {
        list.innerHTML = "";

        if (!notifications || notifications.length === 0) {
            var empty = document.createElement("div");
            empty.className = "patient-notification-empty";
            empty.textContent = "You have no notifications yet.";
            list.appendChild(empty);
            return;
        }

        notifications.forEach(function (notification) {
            var item = document.createElement("div");
            item.className = "patient-notification-item" + (notification.isRead ? "" : " unread");
            item.setAttribute("data-id", notification.id);

            item.innerHTML =
                '<i class="bi bi-capsule"></i>' +
                '<div>' +
                '<div class="patient-notification-item-title"></div>' +
                '<div class="patient-notification-item-message"></div>' +
                '<div class="patient-notification-item-time"></div>' +
                '</div>';

            item.querySelector(".patient-notification-item-title").textContent = notification.title;
            item.querySelector(".patient-notification-item-message").textContent = notification.message;
            item.querySelector(".patient-notification-item-time").textContent = formatTime(notification.createdAt);

            item.addEventListener("click", function () {
                markAsRead(notification.id, item);
            });

            list.appendChild(item);
        });
    }

    function loadNotifications() {
        list.innerHTML = '<div class="patient-notification-empty">Loading…</div>';

        fetch("/Patient/Notifications", {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Failed to load notifications.");
                }
                return response.json();
            })
            .then(function (data) {
                renderNotifications(data);
            })
            .catch(function (error) {
                console.error("Could not load notifications:", error);
                list.innerHTML =
                    '<div class="patient-notification-empty">Notifications could not be loaded.</div>';
            });
    }

    function markAsRead(id, itemEl) {
        if (!itemEl.classList.contains("unread")) {
            return;
        }

        fetch("/Patient/MarkNotificationRead?id=" + encodeURIComponent(id), {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Failed to mark notification as read.");
                }
                itemEl.classList.remove("unread");
                refreshUnreadCount();
            })
            .catch(function (error) {
                console.error("Could not mark notification as read:", error);
            });
    }

    function markAllAsRead() {
        fetch("/Patient/MarkAllNotificationsRead", {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Failed to mark all notifications as read.");
                }
                list.querySelectorAll(".patient-notification-item.unread").forEach(function (el) {
                    el.classList.remove("unread");
                });
                setBadge(0);
            })
            .catch(function (error) {
                console.error("Could not mark all notifications as read:", error);
            });
    }

    function togglePanel() {
        panelOpen = !panelOpen;
        panel.classList.toggle("open", panelOpen);

        if (panelOpen) {
            loadNotifications();
        }
    }

    function closePanelIfOutside(event) {
        if (!panelOpen) {
            return;
        }
        if (panel.contains(event.target) || bell.contains(event.target)) {
            return;
        }
        panelOpen = false;
        panel.classList.remove("open");
    }

    bell.addEventListener("click", function (event) {
        event.stopPropagation();
        togglePanel();
    });

    document.addEventListener("click", closePanelIfOutside);

    if (markAllBtn) {
        markAllBtn.addEventListener("click", function (event) {
            event.stopPropagation();
            markAllAsRead();
        });
    }

    refreshUnreadCount();
    window.setInterval(refreshUnreadCount, POLL_INTERVAL_MS);
})();
