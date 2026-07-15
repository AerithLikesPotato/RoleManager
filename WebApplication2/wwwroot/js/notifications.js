function toggleNotifDropdown(event) {
    event.stopPropagation();
    const menu = document.getElementById("notifDropdownMenu");
    if (!menu) return;

    const isOpen = menu.classList.contains("show");

    // Close any other open dropdowns (e.g. the profile menu) first.
    document.querySelectorAll(".dropdown-menu.show").forEach((item) => item.classList.remove("show"));

    if (!isOpen) {
        menu.classList.add("show");
        loadRecentActivity();
    }
}

function formatRelativeTime(isoString) {
    const then = new Date(isoString).getTime();
    const now = Date.now();
    const seconds = Math.max(0, Math.floor((now - then) / 1000));

    if (seconds < 60) return "just now";
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return minutes + (minutes === 1 ? " minute ago" : " minutes ago");
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return hours + (hours === 1 ? " hour ago" : " hours ago");
    const days = Math.floor(hours / 24);
    if (days < 7) return days + (days === 1 ? " day ago" : " days ago");
    return new Date(isoString).toLocaleDateString();
}

async function loadRecentActivity() {
    const list = document.getElementById("notifList");
    if (!list) return;

    try {
        const response = await fetch("/Home/RecentActivity", {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        if (!response.ok) {
            list.innerHTML = '<div class="notif-empty">Couldn\'t load activity.</div>';
            return;
        }

        const items = await response.json();

        if (!items || items.length === 0) {
            list.innerHTML = '<div class="notif-empty">No recent activity.</div>';
            return;
        }

        list.innerHTML = items.map(function (item) {
            const description = escapeHtml(item.description);
            const timeAgo = escapeHtml(formatRelativeTime(item.createdAt));
            return '<div class="notif-item">' +
                '<div class="notif-item-text">' + description + '</div>' +
                '<div class="notif-item-time">' + timeAgo + '</div>' +
                '</div>';
        }).join("");
    } catch (err) {
        list.innerHTML = '<div class="notif-empty">Couldn\'t load activity.</div>';
    }
}

function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value == null ? "" : String(value);
    return div.innerHTML;
}

// Close the notification dropdown on any outside click (mirrors the profile dropdown behavior).
document.addEventListener("click", function () {
    const menu = document.getElementById("notifDropdownMenu");
    if (menu) menu.classList.remove("show");
});
