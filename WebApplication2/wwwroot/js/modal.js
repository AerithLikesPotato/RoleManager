function openPopUp(modalId) {
    // If modalId isn't provided, open the first visible modal overlay on the page.
    const modal = document.getElementById(modalId || "createUserModal")
        || document.querySelector(".modal-overlay.modal-open");

    if (modal) {
        modal.classList.add("modal-open");
        modal.style.display = "flex";
    }
}

function closePopUp(modalId) {
    const modal = document.getElementById(modalId || "createUserModal")
        || document.querySelector(".modal-overlay[style*='display: flex']");

    if (modal) {
        modal.classList.remove("modal-open");
        modal.style.display = "none";
    }
}

function openEditUserModal(button) {
    openPopUp("editUserModal");
    const userId = button.getAttribute("data-id") || "";
    const fullName = button.getAttribute("data-fullname") || "";
    const email = button.getAttribute("data-email") || "";
    const roleId = button.getAttribute("data-roleid") || "";
    const status = button.getAttribute("data-status") || "active";

    const editUserId = document.getElementById("editUserId");
    const deleteUserId = document.getElementById("deleteUserId");
    const editFullName = document.getElementById("editFullName");
    const editUserEmail = document.getElementById("editUserEmail");
    const editUserRole = document.getElementById("editUserRole");
    const editUserStatus = document.getElementById("editUserStatus");

    if (editUserId) editUserId.value = userId;
    if (deleteUserId) deleteUserId.value = userId;
    if (editFullName) editFullName.value = fullName;
    if (editUserEmail) editUserEmail.value = email;
    if (editUserRole) editUserRole.value = roleId;
    if (editUserStatus) editUserStatus.value = status;

}

function openEditRoleModal(button) {
    openPopUp("editRoleModal");
    const roleId = button.getAttribute("data-id") || "";
    const roleName = button.getAttribute("data-name") || "";
    const roleDescription = button.getAttribute("data-description") || "";
    const rolePermissions = button.getAttribute("data-permissions") || "";
    const permissionSet = new Set(
        rolePermissions.split(",").map(p => p.trim()).filter(p => p.length > 0)
    );

    const editRoleId = document.getElementById("editRoleId");
    const deleteRoleId = document.getElementById("deleteRoleId");
    const editRoleName = document.getElementById("editRoleName");
    const editRoleDescription = document.getElementById("editRoleDescription");

    if (editRoleId) editRoleId.value = roleId;
    if (deleteRoleId) deleteRoleId.value = roleId;
    if (editRoleName) editRoleName.value = roleName;
    if (editRoleDescription) editRoleDescription.value = roleDescription;

    // Toggle each permission checkbox on/off based on the role's saved permissions.
    document.querySelectorAll('#editRoleForm input[name="Permissions"]').forEach(function (checkbox) {
        checkbox.checked = permissionSet.has(checkbox.value);
    });
}

document.addEventListener("click", function (event) {
    if (event.target.classList.contains("modal-overlay")) {
        closePopUp(event.target.id);
    }
});

(function () {
    const MAX_NOTIFICATIONS = 3;
    const DISPLAY_MS = 3500;

    function createNotificationElement(message, type) {
        const item = document.createElement("div");
        item.className = `notification-item ${type || "info"}`;
        item.innerHTML = `
            <div class="notification-content">
                <div class="notification-icon">
                    <svg aria-hidden="true" fill="none" viewBox="0 0 24 24">
                        <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.5 11.5 11 14l4-4m6 2a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z"></path>
                    </svg>
                </div>
                <div class="notification-text">${message}</div>
            </div>
            <div class="notification-icon notification-close" role="button" tabindex="0" aria-label="Dismiss notification">
                <svg aria-hidden="true" fill="none" viewBox="0 0 24 24">
                    <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18 17.94 6M18 18 6.06 6"></path>
                </svg>
            </div>
            <div class="notification-progress-bar"></div>
        `;

        const closeButton = item.querySelector(".notification-close");
        closeButton.addEventListener("click", () => removeNotification(item));
        closeButton.addEventListener("keydown", (event) => {
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                removeNotification(item);
            }
        });
        return item;
    }

    function removeNotification(item) {
        if (!item || !item.isConnected) return;
        item.style.opacity = "0";
        item.style.transform = "translateY(-8px)";
        setTimeout(() => item.remove(), 180);
    }

    function showNotification(message, type) {
        const root = document.getElementById("notification-root");
        if (!root) return;

        const items = Array.from(root.querySelectorAll(".notification-item"));
        while (items.length >= MAX_NOTIFICATIONS) {
            const oldest = items.shift();
            if (oldest) removeNotification(oldest);
        }

        const item = createNotificationElement(message, type);
        root.appendChild(item);
        setTimeout(() => removeNotification(item), DISPLAY_MS);
    }

    // Expose notification function
    window.showNotification = showNotification;

    // Render queued notifications on DOM ready. Also re-render on a short delay
    // to catch cases where TempData-based scripts were injected after DOMContentLoaded.
    function flushQueuedNotifications() {
        const pending = window.__notificationQueue || [];
        if (!pending.length) return;
        pending.forEach(function (entry) {
            showNotification(entry.message, entry.type);
        });
        window.__notificationQueue = [];
    }

    window.addEventListener("DOMContentLoaded", flushQueuedNotifications);
    setTimeout(flushQueuedNotifications, 50);
    setTimeout(flushQueuedNotifications, 250);
})();