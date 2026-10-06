// =========================================================
// CuraLink — Doctor Dashboard Layout
// Scope: Views/Shared/_DoctorLayout.cshtml only.
// =========================================================

(function () {
    "use strict";

    // -----------------------------------------------------
    // Sidebar (off-canvas on tablet/mobile)
    // -----------------------------------------------------
    function initSidebar() {
        var sidebar = document.getElementById("doctorSidebar");
        var toggleBtn = document.getElementById("sidebarToggle");
        var backdrop = document.getElementById("sidebarBackdrop");

        if (!sidebar || !toggleBtn || !backdrop) return;

        function openSidebar() {
            sidebar.classList.add("open");
            backdrop.classList.add("open");
        }

        function closeSidebar() {
            sidebar.classList.remove("open");
            backdrop.classList.remove("open");
        }

        toggleBtn.addEventListener("click", function () {
            sidebar.classList.contains("open") ? closeSidebar() : openSidebar();
        });

        backdrop.addEventListener("click", closeSidebar);

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") closeSidebar();
        });
    }

    // -----------------------------------------------------
    // Notifications bell
    // -----------------------------------------------------
    function initNotifications() {
        var button = document.getElementById("notificationButton");
        var dropdown = document.getElementById("notificationDropdown");
        var badge = document.getElementById("notificationBadge");
        var list = document.getElementById("notificationList");
        var markAllRead = document.getElementById("markAllNotificationsRead");

        if (!button || !dropdown || !badge || !list) return;

        var notifications = [];

        // Toggle dropdown
        button.addEventListener("click", function (e) {
            e.stopPropagation();
            dropdown.classList.toggle("show");
        });

        // Close when clicking outside
        document.addEventListener("click", function (e) {
            if (!dropdown.contains(e.target) && !button.contains(e.target)) {
                dropdown.classList.remove("show");
            }
        });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") dropdown.classList.remove("show");
        });

        function escapeHtml(value) {
            var div = document.createElement("div");
            div.textContent = value == null ? "" : value;
            return div.innerHTML;
        }

        function formatTime(value) {
            if (!value) return "";
            var date = new Date(value);
            if (Number.isNaN(date.getTime())) return "";

            var diff = Math.floor((Date.now() - date.getTime()) / 1000);
            if (diff < 60) return "Just now";
            if (diff < 3600) return Math.floor(diff / 60) + " min ago";
            if (diff < 86400) return Math.floor(diff / 3600) + " hr ago";
            return date.toLocaleDateString();
        }

        function updateBadge() {
            var unread = notifications.filter(function (n) { return !n.isRead; }).length;

            if (unread > 0) {
                badge.textContent = unread > 99 ? "99+" : unread;
                badge.style.display = "flex";
            } else {
                badge.style.display = "none";
            }
        }

        function render() {
            list.innerHTML = "";
            updateBadge();

            if (notifications.length === 0) {
                list.innerHTML =
                    '<div class="doctor-no-notifications">' +
                    '<i class="bi bi-bell-slash"></i>' +
                    '<span>No new notifications</span>' +
                    '</div>';
                return;
            }

            notifications.forEach(function (n) {
                var item = document.createElement("div");
                item.className = "doctor-notification-item" + (n.isRead ? "" : " unread");

                item.innerHTML =
                    '<div class="doctor-notification-icon"><i class="bi bi-bell"></i></div>' +
                    '<div class="doctor-notification-content">' +
                    '<div class="doctor-notification-title">' + escapeHtml(n.title) + '</div>' +
                    '<div class="doctor-notification-message">' + escapeHtml(n.message) + '</div>' +
                    '<div class="doctor-notification-time">' + formatTime(n.createdAt) + '</div>' +
                    '</div>';

                item.addEventListener("click", function () {
                    n.isRead = true;
                    render();
                });

                list.appendChild(item);
            });
        }

        // Called from realtime.js
        window.addNotification = function (notification) {
            notification = notification || {};

            notifications.unshift({
                id: notification.id || (window.crypto && crypto.randomUUID
                    ? crypto.randomUUID()
                    : String(Date.now() + Math.random())),
                title: notification.title ?? "Notification",
                message: notification.message ?? "",
                createdAt: notification.createdAt ?? new Date(),
                isRead: notification.isRead ?? false
            });

            notifications = notifications.slice(0, 20);
            render();
        };

        if (markAllRead) {
            markAllRead.addEventListener("click", function () {
                notifications.forEach(function (n) { n.isRead = true; });
                render();
            });
        }

        render();
    }

    document.addEventListener("DOMContentLoaded", function () {
        initSidebar();
        initNotifications();
    });
})();